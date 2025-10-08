using HtmlAgilityPack;
using AngleSharp;
using AngleSharp.Html.Dom;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace c2_eskolar.Services.WebScraping
{
    public class WebScrapingService : IWebScrapingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebScrapingService> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public WebScrapingService(HttpClient httpClient, ILogger<WebScrapingService> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            
            // Set user agent to avoid being blocked
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<List<ScrapedScholarship>> ScrapeScholarshipsAsync(string sourceUrl)
        {
            var scholarships = new List<ScrapedScholarship>();
            
            try
            {
                _logger.LogInformation($"Starting to scrape scholarships from: {sourceUrl}");
                
                var html = await _httpClient.GetStringAsync(sourceUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Try multiple common selectors for scholarship containers
                var possibleContainerSelectors = new[]
                {
                    "//div[contains(@class, 'scholarship')]",
                    "//div[contains(@class, 'opportunity')]", 
                    "//div[contains(@class, 'listing')]",
                    "//div[contains(@class, 'card')]",
                    "//article",
                    "//div[contains(@class, 'item')]"
                };
                
                HtmlNodeCollection? scholarshipNodes = null;
                
                foreach (var selector in possibleContainerSelectors)
                {
                    scholarshipNodes = doc.DocumentNode.SelectNodes(selector);
                    if (scholarshipNodes != null && scholarshipNodes.Count > 0)
                    {
                        _logger.LogInformation($"Found {scholarshipNodes.Count} potential scholarship containers using selector: {selector}");
                        break;
                    }
                }
                
                if (scholarshipNodes != null)
                {
                    foreach (var node in scholarshipNodes.Take(20)) // Limit to 20 for demo
                    {
                        var scholarship = new ScrapedScholarship
                        {
                            Title = ExtractTitle(node),
                            Description = ExtractDescription(node),
                            Amount = ExtractAmount(node),
                            ApplicationDeadline = ExtractDeadline(node),
                            SourceUrl = sourceUrl
                        };
                        
                        // Only add if we found at least a title
                        if (!string.IsNullOrWhiteSpace(scholarship.Title))
                        {
                            scholarships.Add(scholarship);
                        }
                    }
                }
                
                _logger.LogInformation($"Successfully scraped {scholarships.Count} scholarships from {sourceUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scraping scholarships from {sourceUrl}");
                throw; // Re-throw to let caller handle
            }
            
            return scholarships;
        }
        
        private string ExtractTitle(HtmlNode node)
        {
            // Try multiple selectors for title
            var titleSelectors = new[]
            {
                ".//h1", ".//h2", ".//h3", ".//h4",
                ".//*[contains(@class, 'title')]",
                ".//*[contains(@class, 'name')]",
                ".//*[contains(@class, 'heading')]"
            };
            
            foreach (var selector in titleSelectors)
            {
                var titleNode = node.SelectSingleNode(selector);
                if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
                {
                    return titleNode.InnerText.Trim();
                }
            }
            
            return "";
        }
        
        private string ExtractDescription(HtmlNode node)
        {
            var descSelectors = new[]
            {
                ".//*[contains(@class, 'description')]",
                ".//*[contains(@class, 'summary')]",
                ".//*[contains(@class, 'content')]",
                ".//p"
            };
            
            foreach (var selector in descSelectors)
            {
                var descNode = node.SelectSingleNode(selector);
                if (descNode != null && !string.IsNullOrWhiteSpace(descNode.InnerText))
                {
                    var text = descNode.InnerText.Trim();
                    return text.Length > 200 ? text.Substring(0, 200) + "..." : text;
                }
            }
            
            return "";
        }
        
        private string ExtractAmount(HtmlNode node)
        {
            var amountSelectors = new[]
            {
                ".//*[contains(@class, 'amount')]",
                ".//*[contains(@class, 'value')]",
                ".//*[contains(@class, 'prize')]",
                ".//*[contains(text(), '$')]",
                ".//*[contains(text(), 'PHP')]",
                ".//*[contains(text(), '₱')]"
            };
            
            foreach (var selector in amountSelectors)
            {
                var amountNode = node.SelectSingleNode(selector);
                if (amountNode != null && !string.IsNullOrWhiteSpace(amountNode.InnerText))
                {
                    return amountNode.InnerText.Trim();
                }
            }
            
            return "";
        }
        
        private string ExtractDeadline(HtmlNode node)
        {
            var deadlineSelectors = new[]
            {
                ".//*[contains(@class, 'deadline')]",
                ".//*[contains(@class, 'date')]",
                ".//*[contains(@class, 'due')]",
                ".//*[contains(text(), 'deadline')]",
                ".//*[contains(text(), 'due')]"
            };
            
            foreach (var selector in deadlineSelectors)
            {
                var deadlineNode = node.SelectSingleNode(selector);
                if (deadlineNode != null && !string.IsNullOrWhiteSpace(deadlineNode.InnerText))
                {
                    return deadlineNode.InnerText.Trim();
                }
            }
            
            return "";
        }

        public async Task<InstitutionVerificationResult> VerifyInstitutionAsync(string institutionName, string website)
        {
            var result = new InstitutionVerificationResult();
            
            try
            {
                if (string.IsNullOrWhiteSpace(website))
                {
                    // Try to find official website through search
                    website = await SearchInstitutionWebsiteAsync(institutionName);
                }
                
                if (!string.IsNullOrWhiteSpace(website))
                {
                    var html = await _httpClient.GetStringAsync(website);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    
                    // Extract institution information
                    result.OfficialName = ExtractText(doc.DocumentNode, "//title") ?? 
                                        ExtractText(doc.DocumentNode, "//h1") ?? institutionName;
                    
                    result.Location = ExtractText(doc.DocumentNode, ".//*[contains(text(), 'Address')]" ?? 
                                                 ".//*[contains(@class, 'address')]");
                    
                    // Look for accreditation information
                    var accreditationText = ExtractText(doc.DocumentNode, ".//*[contains(text(), 'accredited')]" ??
                                                       ".//*[contains(text(), 'accreditation')]");
                    result.AccreditationStatus = !string.IsNullOrWhiteSpace(accreditationText) ? "Accredited" : "Unknown";
                    
                    result.IsVerified = !string.IsNullOrWhiteSpace(result.OfficialName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying institution: {institutionName}");
            }
            
            return result;
        }

        public async Task<List<ScholarshipNews>> ScrapeScholarshipNewsAsync()
        {
            var newsList = new List<ScholarshipNews>();
            
            // Define news sources
            var newsSources = new[]
            {
                "https://www.scholarships.com/news",
                "https://www.fastweb.com/college-scholarships/articles"
                // Add more news sources as needed
            };
            
            foreach (var source in newsSources)
            {
                try
                {
                    var html = await _httpClient.GetStringAsync(source);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    
                    var articleNodes = doc.DocumentNode.SelectNodes("//article" ?? "//div[contains(@class, 'news-item')]");
                    
                    if (articleNodes != null)
                    {
                        foreach (var node in articleNodes.Take(10)) // Limit to 10 articles per source
                        {
                            var news = new ScholarshipNews
                            {
                                Title = ExtractText(node, ".//h2" ?? ".//h3" ?? ".//h1"),
                                Content = ExtractText(node, ".//p"),
                                SourceUrl = source
                            };
                            
                            if (!string.IsNullOrWhiteSpace(news.Title))
                            {
                                newsList.Add(news);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error scraping news from {source}");
                }
            }
            
            return newsList;
        }

        public async Task<bool> VerifyOrganizationAsync(string organizationName, string website)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(website))
                {
                    return false;
                }
                
                var html = await _httpClient.GetStringAsync(website);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // Check if organization name appears on the website
                var pageText = doc.DocumentNode.InnerText.ToLower();
                var orgNameLower = organizationName.ToLower();
                
                return pageText.Contains(orgNameLower);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying organization: {organizationName}");
                return false;
            }
        }

        private string ExtractText(HtmlNode node, string xpath)
        {
            try
            {
                var targetNode = node.SelectSingleNode(xpath);
                return targetNode?.InnerText?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private async Task<string> SearchInstitutionWebsiteAsync(string institutionName)
        {
            try
            {
                // This is a simplified example - in production, you might use Google Custom Search API
                var searchQuery = $"{institutionName} official website";
                var encodedQuery = Uri.EscapeDataString(searchQuery);
                var searchUrl = $"https://www.google.com/search?q={encodedQuery}";
                
                // Note: Google may block automated requests, consider using their Custom Search API instead
                var html = await _httpClient.GetStringAsync(searchUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // Extract first search result URL (simplified)
                var firstResult = doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'http')]/@href");
                return firstResult?.GetAttributeValue("href", "") ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
using HtmlAgilityPack;
using AngleSharp;
using AngleSharp.Html.Dom;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using c2_eskolar.Services;
using c2_eskolar.Models;
using System.Text;
using System.Globalization;
using System.Text.Json;

namespace c2_eskolar.Services.WebScraping
{
    public class EnhancedWebScrapingService : IEnhancedWebScrapingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EnhancedWebScrapingService> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly OpenAIService _openAIService;

        public EnhancedWebScrapingService(
            HttpClient httpClient, 
            ILogger<EnhancedWebScrapingService> logger, 
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            OpenAIService openAIService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _openAIService = openAIService;
            
            // Enhanced headers to avoid being blocked - don't specify compression as HttpClient handles it automatically
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", 
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            // Remove Accept-Encoding to let HttpClient handle compression automatically
            _httpClient.DefaultRequestHeaders.Add("DNT", "1");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
            
            // Set timeout to 30 seconds
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<EnhancedScrapedScholarship>> ScrapeAndParseScholarshipsAsync(string sourceUrl)
        {
            var scholarships = new List<EnhancedScrapedScholarship>();
            
            try
            {
                _logger.LogInformation($"Starting enhanced scraping from: {sourceUrl}");
                
                // Validate URL format first
                if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var validUri))
                {
                    throw new ArgumentException($"Invalid URL format: {sourceUrl}");
                }
                
                // Test connectivity first
                _logger.LogInformation($"Testing connectivity to: {sourceUrl}");
                
                HttpResponseMessage response;
                try 
                {
                    response = await _httpClient.GetAsync(sourceUrl);
                    _logger.LogInformation($"HTTP Response: {response.StatusCode} - {response.ReasonPhrase}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var statusCode = (int)response.StatusCode;
                        var errorMessage = statusCode switch
                        {
                            404 => $"Website not found (404). The URL '{sourceUrl}' does not exist or is not accessible. Please check the URL and try again.",
                            403 => $"Access forbidden (403). The website '{sourceUrl}' is blocking automated requests. Try a different URL or contact the website administrator.",
                            429 => $"Too many requests (429). The website '{sourceUrl}' is rate limiting. Wait a few minutes and try again.",
                            500 => $"Server error (500). The website '{sourceUrl}' is experiencing technical difficulties. Try again later.",
                            503 => $"Service unavailable (503). The website '{sourceUrl}' is temporarily down. Try again later.",
                            _ => $"HTTP Error {statusCode}: {response.ReasonPhrase}. Unable to access '{sourceUrl}'."
                        };
                        
                        _logger.LogError($"HTTP Error accessing {sourceUrl}: {statusCode} - {response.ReasonPhrase}");
                        throw new HttpRequestException(errorMessage);
                    }
                    
                    // Enhanced content reading - let HttpClient handle decompression automatically
                    var html = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogInformation($"Successfully retrieved {html.Length} characters from {sourceUrl}");
                    
                    // Check if content looks corrupted (too many control characters)
                    if (html.Length > 100)
                    {
                        var controlCharCount = html.Take(100).Count(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
                        if (controlCharCount > 20)
                        {
                            _logger.LogWarning($"Content appears corrupted - {controlCharCount} control characters in first 100 chars");
                            
                            // Try reading as bytes and decoding manually
                            var contentBytes = await response.Content.ReadAsByteArrayAsync();
                            
                            // Try common encodings
                            var encodingsToTry = new[] { 
                                Encoding.UTF8, 
                                Encoding.GetEncoding("iso-8859-1"), 
                                Encoding.ASCII,
                                Encoding.Unicode 
                            };
                            
                            foreach (var encoding in encodingsToTry)
                            {
                                try
                                {
                                    var testHtml = encoding.GetString(contentBytes);
                                    var testControlCount = testHtml.Take(100).Count(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
                                    
                                    if (testControlCount < 10)
                                    {
                                        html = testHtml;
                                        _logger.LogInformation($"Successfully decoded using {encoding.EncodingName}");
                                        break;
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // Extract main content and individual scholarship blocks
                    var scholarshipTexts = ExtractScholarshipTexts(doc, sourceUrl);
                    
                    _logger.LogInformation($"Found {scholarshipTexts.Count} potential scholarship texts");

                    // Process each text block with AI
                    var tasks = scholarshipTexts.Select(text => 
                        ParseScholarshipWithAIAsync(text, sourceUrl)).ToList();
                    
                    var results = await Task.WhenAll(tasks);
                    var baseScholarships = results.Where(s => !string.IsNullOrWhiteSpace(s.Title)).ToList();
                    
                    _logger.LogInformation($"Successfully parsed {baseScholarships.Count} base scholarships with AI");
                    
                    // Enhanced: Check for external URLs and scrape additional details
                    var enhancedScholarships = new List<EnhancedScrapedScholarship>();
                    
                    foreach (var scholarship in baseScholarships)
                    {
                        var enhancedScholarship = scholarship;
                        
                        // Check if this scholarship has an external application URL
                        if (!string.IsNullOrWhiteSpace(scholarship.ExternalApplicationUrl))
                        {
                            _logger.LogInformation($"Found external URL for '{scholarship.Title}': {scholarship.ExternalApplicationUrl}");
                            
                            try
                            {
                                var externalData = await ScrapeExternalUrlAsync(scholarship.ExternalApplicationUrl, scholarship);
                                if (externalData != null)
                                {
                                    enhancedScholarship = externalData;
                                    _logger.LogInformation($"Successfully enhanced '{scholarship.Title}' with external data");
                                }
                                else
                                {
                                    _logger.LogInformation($"No additional data found for external URL: {scholarship.ExternalApplicationUrl}");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Failed to enhance scholarship '{scholarship.Title}' with external URL: {ex.Message}");
                                // Keep the original scholarship if external enhancement fails
                            }
                        }
                        
                        enhancedScholarships.Add(enhancedScholarship);
                    }
                    
                    scholarships.AddRange(enhancedScholarships);
                    
                    _logger.LogInformation($"Final result: {scholarships.Count} scholarships (enhanced: {enhancedScholarships.Count(s => s.ParsingNotes.Any(n => n.Contains("Enhanced with external")))})");
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    throw new HttpRequestException($"Request timeout: The website '{sourceUrl}' took too long to respond. Please try again or use a different URL.");
                }
                catch (HttpRequestException)
                {
                    throw; // Re-throw HttpRequestException with our custom message
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in enhanced scraping from {sourceUrl}");
                throw;
            }
            
            return scholarships;
        }

        public async Task<EnhancedScrapedScholarship> ParseScholarshipWithAIAsync(string rawText, string sourceUrl)
        {
            try
            {
                _logger.LogInformation($"Starting AI parsing for text of {rawText.Length} characters");
                
                var aiPrompt = CreateScholarshipParsingPrompt(rawText);
                
                _logger.LogInformation("Sending prompt to AI service...");
                var aiResponse = await _openAIService.GetChatCompletionAsync(aiPrompt);
                
                _logger.LogInformation($"AI Response received ({aiResponse.Length} characters)");
                _logger.LogInformation($"AI Response: {aiResponse}");
                
                var result = ParseAIResponse(aiResponse, rawText, sourceUrl);
                
                _logger.LogInformation($"Parsed scholarship: '{result.Title}' with confidence {result.ParsingConfidence:P0}");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing scholarship with AI");
                
                // Return basic scholarship with raw text if AI fails
                return new EnhancedScrapedScholarship
                {
                    Title = ExtractBasicTitle(rawText),
                    RawText = rawText,
                    SourceUrl = sourceUrl,
                    ParsingConfidence = 0.1,
                    ParsingNotes = new List<string> { "AI parsing failed, using fallback extraction" }
                };
            }
        }

        public Task<string> GenerateScholarshipCsvAsync(List<EnhancedScrapedScholarship> scholarships)
        {
            var csv = new StringBuilder();
            
            // CSV Headers matching Scholarship model
            var headers = new[]
            {
                "Title", "Description", "Benefits", "MonetaryValue", "ApplicationDeadline",
                "Requirements", "SlotsAvailable", "MinimumGPA", "RequiredCourse", 
                "RequiredYearLevel", "RequiredUniversity", "IsActive", "IsInternal",
                "ExternalApplicationUrl", "SourceUrl", "ScrapedAt", "ParsingConfidence",
                "RawText", "ParsingNotes"
            };
            
            csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));
            
            foreach (var scholarship in scholarships)
            {
                var values = new object[]
                {
                    EscapeCsvValue(scholarship.Title),
                    EscapeCsvValue(scholarship.Description ?? ""),
                    EscapeCsvValue(scholarship.Benefits),
                    scholarship.MonetaryValue?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                    scholarship.ApplicationDeadline?.ToString("yyyy-MM-dd") ?? "",
                    EscapeCsvValue(scholarship.Requirements ?? ""),
                    scholarship.SlotsAvailable?.ToString() ?? "",
                    scholarship.MinimumGPA?.ToString("F2", CultureInfo.InvariantCulture) ?? "",
                    EscapeCsvValue(scholarship.RequiredCourse ?? ""),
                    scholarship.RequiredYearLevel?.ToString() ?? "",
                    EscapeCsvValue(scholarship.RequiredUniversity ?? ""),
                    scholarship.IsActive.ToString(),
                    scholarship.IsInternal.ToString(),
                    EscapeCsvValue(scholarship.ExternalApplicationUrl ?? ""),
                    EscapeCsvValue(scholarship.SourceUrl),
                    scholarship.ScrapedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    scholarship.ParsingConfidence.ToString("F2", CultureInfo.InvariantCulture),
                    EscapeCsvValue(scholarship.RawText.Length > 500 ? scholarship.RawText.Substring(0, 500) + "..." : scholarship.RawText),
                    EscapeCsvValue(string.Join("; ", scholarship.ParsingNotes))
                };
                
                csv.AppendLine(string.Join(",", values));
            }
            
            return Task.FromResult(csv.ToString());
        }

        private List<string> ExtractScholarshipTexts(HtmlDocument doc, string sourceUrl)
        {
            var texts = new List<string>();
            
            _logger.LogInformation("Starting scholarship text extraction...");
            
            // Enhanced selectors for better scholarship detection - prioritize table structures
            var possibleContainerSelectors = new[]
            {
                // HTML Table selectors - PRIORITY for structured scholarship data
                "//table//tr[td]", // All table rows with cells
                "//tbody//tr", // Table body rows  
                "//tr[td and position() > 1]", // Skip header rows
                "//table//tr[contains(., 'Scholarship') or contains(., 'Grant') or contains(., 'Award')]",
                
                // Specific scholarship-related selectors
                "//div[contains(@class, 'scholarship')]",
                "//div[contains(@class, 'opportunity')]", 
                "//div[contains(@class, 'listing')]",
                "//div[contains(@class, 'program')]",
                "//div[contains(@class, 'card')]",
                "//article",
                "//div[contains(@class, 'item')]",
                "//div[contains(@class, 'post')]",
                "//div[contains(@class, 'content')]",
                "//div[contains(@class, 'entry')]",
                "//div[contains(@class, 'news')]",
                "//div[contains(@class, 'announcement')]",
                "//section",
                "//div[contains(@id, 'content')]",
                "//div[contains(@id, 'main')]",
                // Try paragraphs with substantial content
                "//p[string-length(text()) > 100]",
                // Try divs with substantial text content
                "//div[string-length(text()) > 200]",
                // Try list items that might contain scholarship info
                "//li[string-length(text()) > 100]"
            };
            
            HtmlNodeCollection? nodes = null;
            string usedSelector = "";
            
            foreach (var selector in possibleContainerSelectors)
            {
                try
                {
                    nodes = doc.DocumentNode.SelectNodes(selector);
                    if (nodes != null && nodes.Count > 0)
                    {
                        usedSelector = selector;
                        _logger.LogInformation($"Found {nodes.Count} nodes using selector: {selector}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error with selector: {selector}");
                }
            }
            
            if (nodes != null)
            {
                _logger.LogInformation($"Processing {nodes.Count} nodes from selector: {usedSelector}");
                
                int processedCount = 0;
                foreach (var node in nodes.Take(50)) // Increased limit for table rows
                {
                    var text = CleanText(node.InnerText);
                    
                    // Enhanced content filtering for table rows
                    if (!string.IsNullOrWhiteSpace(text) && 
                        text.Length > 20 && // More lenient for table rows
                        text.Length < 15000 && // Increased limit for table content
                        (ContainsScholarshipKeywords(text) || IsTableRow(node))) // Accept table rows or scholarship content
                    {
                        // Special handling for table rows - combine row data more intelligently  
                        if (IsTableRow(node))
                        {
                            var tableRowText = ProcessTableRow(node);
                            if (!string.IsNullOrWhiteSpace(tableRowText) && tableRowText.Length > 30)
                            {
                                texts.Add(tableRowText);
                                processedCount++;
                                _logger.LogInformation($"Added table row {processedCount}: {tableRowText.Substring(0, Math.Min(100, tableRowText.Length))}...");
                            }
                            continue;
                        }
                        
                        // Extract URLs from non-table nodes and append them to the text
                        var urls = ExtractUrlsFromNode(node);
                        var enhancedText = text;
                        if (urls.Any())
                        {
                            enhancedText += $"\nExternal URLs: {string.Join(", ", urls)}";
                            _logger.LogInformation($"Found {urls.Count} external URLs in node: {string.Join(", ", urls)}");
                        }
                        
                        // ENHANCED: Check if this is a large text block that might contain multiple scholarships
                        if (enhancedText.Length > 2000 && ContainsScholarshipKeywords(enhancedText))
                        {
                            _logger.LogInformation($"Large text block detected ({enhancedText.Length} characters), checking for scholarship list...");
                            
                            // Try to detect and split scholarship lists
                            if (DetectScholarshipList(enhancedText, out var listChunks))
                            {
                                _logger.LogInformation($"Scholarship list detected! Split into {listChunks.Count} individual scholarships");
                                texts.AddRange(listChunks);
                                processedCount += listChunks.Count;
                                
                                // Log first few entries for verification
                                for (int i = 0; i < Math.Min(3, listChunks.Count); i++)
                                {
                                    _logger.LogInformation($"List entry {i + 1}: {listChunks[i].Substring(0, Math.Min(100, listChunks[i].Length))}...");
                                }
                                
                                continue; // Skip normal processing for this block
                            }
                            else
                            {
                                _logger.LogInformation("No scholarship list pattern detected, applying chunking...");
                                
                                // Apply enhanced chunking to break down large blocks
                                var chunks = SplitIntoScholarshipChunks(enhancedText);
                                if (chunks.Count > 1)
                                {
                                    _logger.LogInformation($"Text chunked into {chunks.Count} parts");
                                    texts.AddRange(chunks);
                                    processedCount += chunks.Count;
                                    continue;
                                }
                            }
                        }
                        
                        // Normal processing for smaller blocks or when chunking didn't help
                        texts.Add(enhancedText);
                        processedCount++;
                        _logger.LogInformation($"Added text block {processedCount}: {enhancedText.Substring(0, Math.Min(100, enhancedText.Length))}...");
                    }
                }
                
                _logger.LogInformation($"Extracted {texts.Count} scholarship texts from {processedCount} processed elements");
            }
            else
            {
                _logger.LogWarning("No nodes found with standard selectors, trying fallback extraction");
                
                // Enhanced fallback: extract main content
                var mainSelectors = new[] { "//main", "//body", "//div[@id='content']", "//div[@class='content']" };
                HtmlNode? mainContent = null;
                
                foreach (var selector in mainSelectors)
                {
                    mainContent = doc.DocumentNode.SelectSingleNode(selector);
                    if (mainContent != null)
                    {
                        _logger.LogInformation($"Using fallback selector: {selector}");
                        break;
                    }
                }
                
                if (mainContent != null)
                {
                    var text = CleanText(mainContent.InnerText);
                    _logger.LogInformation($"Fallback extracted {text.Length} characters of content");
                    
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Enhanced chunking for better scholarship detection
                        var chunks = SplitIntoScholarshipChunks(text);
                        
                        // Filter chunks for scholarship content
                        var scholarshipChunks = chunks.Where(chunk => 
                            chunk.Length > 50 && 
                            IsHighQualityScholarshipContent(chunk)).ToList();
                            
                        texts.AddRange(scholarshipChunks);
                        _logger.LogInformation($"Added {scholarshipChunks.Count} scholarship chunks from fallback extraction");
                    }
                }
                else
                {
                    _logger.LogError("Could not find any content containers");
                    _logger.LogInformation("Starting final fallback extraction...");
                    
                    // Final fallback: try to extract ALL text content and look for scholarship keywords
                    _logger.LogInformation("Attempting final fallback: extract all body text");
                    var bodyText = CleanText(doc.DocumentNode.InnerText);
                    
                    if (!string.IsNullOrWhiteSpace(bodyText) && bodyText.Length > 500)
                    {
                        _logger.LogInformation($"Extracted {bodyText.Length} characters from entire document");
                        
                        // Log a sample of the content to help debug
                        var sample = bodyText.Length > 1000 ? bodyText.Substring(0, 1000) : bodyText;
                        _logger.LogInformation($"Content sample: {sample}...");
                        
                        if (ContainsScholarshipKeywords(bodyText))
                        {
                            var chunks = SplitIntoScholarshipChunks(bodyText);
                            var scholarshipChunks = chunks.Where(chunk => 
                                chunk.Length > 100 && 
                                IsHighQualityScholarshipContent(chunk)).ToList();
                                
                            texts.AddRange(scholarshipChunks);
                            _logger.LogInformation($"Added {scholarshipChunks.Count} scholarship chunks from final fallback");
                        }
                        else
                        {
                            _logger.LogWarning("Document content does not contain sufficient scholarship keywords");
                            
                            // Even more lenient fallback - look for education-related terms in case scholarship keywords are encoded
                            var educationKeywords = new[] { "education", "student", "university", "college", "program", "course", "degree", "academic", "tuition", "fee" };
                            var hasEducationTerms = educationKeywords.Any(keyword => bodyText.ToLowerInvariant().Contains(keyword));
                            
                            if (hasEducationTerms)
                            {
                                _logger.LogInformation("Found education-related terms, attempting to extract content anyway");
                                var chunks = SplitIntoScholarshipChunks(bodyText);
                                var educationChunks = chunks.Where(chunk => chunk.Length > 200).Take(5).ToList();
                                texts.AddRange(educationChunks);
                                _logger.LogInformation($"Added {educationChunks.Count} education-related chunks from ultra-fallback");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Final fallback failed - body text too short: {bodyText?.Length ?? 0} characters");
                    }
                }
            }
            
            _logger.LogInformation($"Final result: {texts.Count} scholarship texts extracted");
            return texts;
        }
        
        private bool ContainsScholarshipKeywords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
                
            var lowerText = text.ToLowerInvariant();
            
            // Keywords that indicate scholarship content
            var scholarshipKeywords = new[]
            {
                "scholarship", "grant", "financial aid", "tuition", "student support",
                "educational assistance", "academic", "university", "college", "study",
                "application", "deadline", "requirement", "gpa", "grade", "student",
                "funding", "award", "benefit", "stipend", "allowance", "fees",
                "undergraduate", "graduate", "program", "course", "degree",
                "scholarship program", "financial assistance", "educational grant",
                "merit", "need-based", "academic excellence", "scholar", "education",
                "learning", "school", "institution", "research", "academic year",
                "semester", "matriculation", "enrollment", "admission", "faculty",
                "department", "curriculum", "tuition fee", "educational support"
            };
            
            // Check if text contains scholarship-related keywords
            var keywordCount = scholarshipKeywords.Count(keyword => lowerText.Contains(keyword));
            
            // For initial detection, require only 1 keyword, but for chunk filtering require 2
            // This helps catch more content initially
            return keywordCount >= 1;
        }
        
        private bool IsHighQualityScholarshipContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
                
            var lowerText = text.ToLowerInvariant();
            
            // High-quality scholarship indicators
            var highQualityKeywords = new[]
            {
                "scholarship", "grant", "financial aid", "application", "deadline",
                "requirement", "eligibility", "benefit", "award", "funding"
            };
            
            var highQualityCount = highQualityKeywords.Count(keyword => lowerText.Contains(keyword));
            
            // Require at least 2 high-quality keywords for chunk selection
            return highQualityCount >= 2;
        }

        private string CreateScholarshipParsingPrompt(string rawText)
        {
            // Detect if this looks like a single scholarship or a list entry
            var isListEntry = Regex.IsMatch(rawText.Trim(), @"^\d+\s") || 
                             rawText.Contains("Scholarship Name") || 
                             rawText.Contains("Application Period") ||
                             rawText.Contains("View website") ||
                             rawText.Contains("Discipline") ||
                             rawText.Contains("Website");
            
            // Detect if this is table-formatted data with tabs or consistent spacing
            var isTableData = rawText.Contains("\t") || 
                             Regex.IsMatch(rawText, @"\d+\s+[A-Z].*?[A-Z].*?[A-Z]") ||
                             rawText.Split('\n').Count(line => line.Trim().Split().Length > 5) > 2;
            
            if (isListEntry || isTableData)
            {
                return $@"You are an AI assistant that extracts scholarship information from structured data.
The following text contains scholarship information, possibly in table or list format. Extract the MAIN scholarship information.

CRITICAL: Return ONLY a SINGLE JSON object (not an array) for the FIRST/MAIN scholarship mentioned.

IMPORTANT INSTRUCTIONS:
- If text starts with a number, that's likely the scholarship entry number - ignore it for the title
- Look for the scholarship program name (usually the first major text after the number)
- Extract discipline/field information from any mentions of study areas
- If you see 'Multiple' disciplines, look for specific fields mentioned in parentheses or nearby
- Convert month names to 2025 dates (e.g., 'February' = '2025-02-28', 'January' = '2025-01-31')
- Extract website URLs if present (look for 'View website', 'Website:', 'External URLs:', or any URL patterns like http/https)
- Pay special attention to 'External URLs:' lines which contain the actual scholarship provider websites
- Be aggressive in extracting data - if information appears to be there, include it

Extract these fields:
- Title (required - extract the main scholarship/program name, NOT the entry number)
- Description (combine discipline and program description)
- Benefits (what the scholarship provides)
- MonetaryValue (extract any monetary amounts as decimal)
- ApplicationDeadline (convert dates to YYYY-MM-DD format, assume 2025 if year missing)
- Requirements (eligibility criteria if mentioned)
- SlotsAvailable (number of slots if mentioned)
- MinimumGPA (GPA requirements if any)
- RequiredCourse (specific courses/disciplines mentioned)
- RequiredYearLevel (undergraduate=1-4, graduate=5-8)
- RequiredUniversity (specific university if mentioned)
- ExternalApplicationUrl (PRIORITY: extract from 'External URLs:' lines or any website/URL mentioned - this is the official scholarship provider website)

Return ONLY a valid JSON object (NOT an array), no additional text.

Text to analyze:
{rawText}

JSON Response:";
            }
            else
            {
                return $@"You are an AI assistant that extracts scholarship information from web content. 
Please analyze the following text and extract scholarship details in JSON format.

CRITICAL: Return ONLY a SINGLE JSON object (not an array).

Extract these fields if available:
- Title (required)
- Description (summary of the scholarship)
- Benefits (what the scholarship provides, including monetary amounts)
- MonetaryValue (extract main monetary value as decimal, e.g., 50000.00)
- ApplicationDeadline (convert to ISO date format YYYY-MM-DD if found)
- Requirements (eligibility criteria and application requirements)
- SlotsAvailable (number of available slots if mentioned)
- MinimumGPA (extract GPA requirement as decimal if mentioned)
- RequiredCourse (specific course/program requirements)
- RequiredYearLevel (extract year level as integer 1-8)
- RequiredUniversity (specific university requirements)
- ExternalApplicationUrl (extract any external website URLs - these are the official scholarship provider websites, not the current page URL)

IMPORTANT: For ExternalApplicationUrl, look for:
- Links that say 'View website', 'Apply here', 'More information'
- URLs that point to external domains (not the current site)
- Official scholarship provider websites
- Application portals

Return ONLY a JSON object with these fields. Use null for missing values. Be accurate and conservative.

Text to analyze:
{rawText}

JSON Response:";
            }
        }

        private EnhancedScrapedScholarship ParseAIResponse(string aiResponse, string rawText, string sourceUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to parse AI response as JSON...");
                
                // Enhanced JSON extraction - handle multiple common formats
                string jsonText = aiResponse;
                
                // Try to extract JSON between curly braces or square brackets
                var jsonMatch = Regex.Match(aiResponse, @"[\{\[].*[\}\]]", RegexOptions.Singleline);
                if (jsonMatch.Success)
                {
                    jsonText = jsonMatch.Value;
                }
                else
                {
                    // Try to find JSON that might be wrapped in code blocks
                    var codeBlockMatch = Regex.Match(aiResponse, @"```(?:json)?\s*([\{\[].*?[\}\]])\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (codeBlockMatch.Success)
                    {
                        jsonText = codeBlockMatch.Groups[1].Value;
                    }
                }
                
                // Clean up common JSON formatting issues
                jsonText = jsonText
                    .Replace("\\n", " ")
                    .Replace("\\r", " ")
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Trim();
                
                // Fix common trailing comma issues
                jsonText = Regex.Replace(jsonText, @",\s*}", "}");
                jsonText = Regex.Replace(jsonText, @",\s*]", "]");
                
                _logger.LogInformation($"Cleaned JSON for parsing: {jsonText.Substring(0, Math.Min(200, jsonText.Length))}...");
                
                JsonElement parsed;
                try
                {
                    parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"First JSON parse attempt failed: {ex.Message}");
                    
                    // Try more aggressive cleaning
                    jsonText = Regex.Replace(jsonText, @"[^\x20-\x7E]", ""); // Remove non-ASCII
                    jsonText = Regex.Replace(jsonText, @"\s+", " "); // Normalize whitespace
                    
                    // Try to fix missing quotes around property names
                    jsonText = Regex.Replace(jsonText, @"(\w+):", "\"$1\":");
                    
                    _logger.LogInformation($"Attempting second parse with cleaned JSON: {jsonText.Substring(0, Math.Min(100, jsonText.Length))}...");
                    parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
                }
                
                // Handle both single objects and arrays
                JsonElement scholarshipElement;
                if (parsed.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation($"AI returned an array with {parsed.GetArrayLength()} scholarships. Taking the first one.");
                    
                    if (parsed.GetArrayLength() > 0)
                    {
                        scholarshipElement = parsed[0];
                        
                        // Log information about the array for debugging
                        _logger.LogInformation($"Array contains {parsed.GetArrayLength()} scholarships. Additional scholarships will be processed separately.");
                        
                        // TODO: In the future, we could process all scholarships in the array
                        // For now, we'll just take the first one to maintain compatibility
                    }
                    else
                    {
                        throw new JsonException("AI returned an empty array");
                    }
                }
                else if (parsed.ValueKind == JsonValueKind.Object)
                {
                    scholarshipElement = parsed;
                    _logger.LogInformation("AI returned a single scholarship object");
                }
                else
                {
                    throw new JsonException($"AI returned unexpected JSON type: {parsed.ValueKind}");
                }
                
                var scholarship = new EnhancedScrapedScholarship
                {
                    Title = GetJsonString(scholarshipElement, "Title") ?? 
                           GetJsonString(scholarshipElement, "title") ?? 
                           ExtractBasicTitle(rawText),
                    Description = GetJsonString(scholarshipElement, "Description") ?? 
                                 GetJsonString(scholarshipElement, "description"),
                    Benefits = GetJsonString(scholarshipElement, "Benefits") ?? 
                              GetJsonString(scholarshipElement, "benefits") ?? 
                              "Benefits to be determined",
                    MonetaryValue = GetJsonDecimal(scholarshipElement, "MonetaryValue") ?? 
                                   GetJsonDecimal(scholarshipElement, "monetaryValue"),
                    ApplicationDeadline = GetJsonDateTime(scholarshipElement, "ApplicationDeadline") ?? 
                                        GetJsonDateTime(scholarshipElement, "applicationDeadline"),
                    Requirements = GetJsonString(scholarshipElement, "Requirements") ?? 
                                  GetJsonString(scholarshipElement, "requirements"),
                    SlotsAvailable = GetJsonInt(scholarshipElement, "SlotsAvailable") ?? 
                                    GetJsonInt(scholarshipElement, "slotsAvailable"),
                    MinimumGPA = GetJsonDecimal(scholarshipElement, "MinimumGPA") ?? 
                                GetJsonDecimal(scholarshipElement, "minimumGPA"),
                    RequiredCourse = GetJsonString(scholarshipElement, "RequiredCourse") ?? 
                                    GetJsonString(scholarshipElement, "requiredCourse"),
                    RequiredYearLevel = GetJsonInt(scholarshipElement, "RequiredYearLevel") ?? 
                                       GetJsonInt(scholarshipElement, "requiredYearLevel"),
                    RequiredUniversity = GetJsonString(scholarshipElement, "RequiredUniversity") ?? 
                                        GetJsonString(scholarshipElement, "requiredUniversity"),
                    ExternalApplicationUrl = GetJsonString(scholarshipElement, "ExternalApplicationUrl") ?? 
                                           GetJsonString(scholarshipElement, "externalApplicationUrl"),
                    SourceUrl = sourceUrl,
                    RawText = rawText,
                    ParsingConfidence = CalculateParsingConfidence(scholarshipElement),
                    ParsingNotes = new List<string> { "Parsed successfully with AI" }
                };
                
                _logger.LogInformation($"Successfully parsed scholarship: {scholarship.Title} (confidence: {scholarship.ParsingConfidence:P0})");
                return scholarship;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse AI response as JSON. Response length: {aiResponse.Length}");
                _logger.LogError($"AI Response content: {aiResponse.Substring(0, Math.Min(500, aiResponse.Length))}...");
                
                return new EnhancedScrapedScholarship
                {
                    Title = ExtractBasicTitle(rawText),
                    Benefits = "Benefits to be determined",
                    RawText = rawText,
                    SourceUrl = sourceUrl,
                    ParsingConfidence = 0.3,
                    ParsingNotes = new List<string> { $"AI JSON parsing failed: {ex.Message}" }
                };
            }
        }

        private string? GetJsonString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
            return null;
        }

        private decimal? GetJsonDecimal(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var value))
                    return value;
                if (prop.ValueKind == JsonValueKind.String && decimal.TryParse(prop.GetString(), out var parsedValue))
                    return parsedValue;
            }
            return null;
        }

        private int? GetJsonInt(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value))
                    return value;
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var parsedValue))
                    return parsedValue;
            }
            return null;
        }

        private DateTime? GetJsonDateTime(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(prop.GetString(), out var date))
                    return date;
            }
            return null;
        }

        private double CalculateParsingConfidence(JsonElement parsed)
        {
            var fields = new[] { "Title", "Description", "Benefits", "ApplicationDeadline" };
            var filledFields = fields.Count(field => 
                parsed.TryGetProperty(field, out var prop) && 
                prop.ValueKind == JsonValueKind.String && 
                !string.IsNullOrWhiteSpace(prop.GetString()));
            
            return (double)filledFields / fields.Length;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
                
            // Remove extra whitespace and normalize
            text = Regex.Replace(text, @"\s+", " ");
            text = text.Trim();
            
            return text;
        }

        private bool DetectScholarshipList(string text, out List<string> scholarshipChunks)
        {
            scholarshipChunks = new List<string>();
            
            _logger.LogInformation($"Analyzing text of {text.Length} characters for scholarship list patterns");
            
            // Pattern 1: Numbered list (1 Title, 2 Title, etc.) - Enhanced
            var numberedPattern = @"(?:^|\n)\s*(\d+)\s+([^\d\n][^\n]+(?:\n(?!\s*\d+\s)[^\n]*)*?)(?=\n\s*\d+\s|\n\s*$|$)";
            var numberedMatches = Regex.Matches(text, numberedPattern, RegexOptions.Multiline);
            
            if (numberedMatches.Count >= 3) // At least 3 scholarships to consider it a list
            {
                _logger.LogInformation($"Detected numbered scholarship list with {numberedMatches.Count} entries");
                
                foreach (Match match in numberedMatches)
                {
                    var scholarshipText = $"{match.Groups[1].Value} {match.Groups[2].Value}".Trim();
                    if (scholarshipText.Length > 30) // More lenient length requirement
                    {
                        scholarshipChunks.Add(CleanText(scholarshipText));
                    }
                }
                
                if (scholarshipChunks.Count >= 3)
                {
                    _logger.LogInformation($"Successfully extracted {scholarshipChunks.Count} numbered scholarships");
                    return true;
                }
            }
            
            // Pattern 2: Bulleted list (• Title, - Title, etc.)
            var bulletPattern = @"(?:^|\n)\s*[•\-\*]\s*([^\n]+(?:\n(?!\s*[•\-\*]\s)[^\n]*)*?)(?=\n\s*[•\-\*]\s|\n\s*$|$)";
            var bulletMatches = Regex.Matches(text, bulletPattern, RegexOptions.Multiline);
            
            if (bulletMatches.Count >= 3)
            {
                _logger.LogInformation($"Detected bulleted scholarship list with {bulletMatches.Count} entries");
                
                scholarshipChunks.Clear();
                foreach (Match match in bulletMatches)
                {
                    var scholarshipText = match.Groups[1].Value.Trim();
                    if (scholarshipText.Length > 30)
                    {
                        scholarshipChunks.Add(CleanText(scholarshipText));
                    }
                }
                
                if (scholarshipChunks.Count >= 3)
                {
                    _logger.LogInformation($"Successfully extracted {scholarshipChunks.Count} bulleted scholarships");
                    return true;
                }
            }
            
            // Pattern 3: Enhanced tabular format detection - look for scholarship names in structured data
            var tableRowPattern = @"(?:^|\n)\s*(\d+)\s+(.+?)(?=\n\s*\d+\s|\n\s*$|$)";
            var tableMatches = Regex.Matches(text, tableRowPattern, RegexOptions.Multiline | RegexOptions.Singleline);
            
            if (tableMatches.Count >= 3)
            {
                _logger.LogInformation($"Detected table format with {tableMatches.Count} potential entries");
                
                scholarshipChunks.Clear();
                foreach (Match match in tableMatches)
                {
                    var rowNumber = match.Groups[1].Value;
                    var rowContent = match.Groups[2].Value.Trim();
                    
                    // Split row content and look for scholarship name
                    var fields = rowContent.Split(new[] { '\t', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (fields.Length > 0)
                    {
                        var scholarshipName = fields[0].Trim();
                        var fullContent = $"{rowNumber} {rowContent}";
                        
                        if (scholarshipName.Length > 10 && !scholarshipName.ToLower().Contains("scholarship name"))
                        {
                            scholarshipChunks.Add(CleanText(fullContent));
                        }
                    }
                }
                
                if (scholarshipChunks.Count >= 3)
                {
                    _logger.LogInformation($"Successfully extracted {scholarshipChunks.Count} table-format scholarships");
                    return true;
                }
            }
            
            // Pattern 4: Enhanced HTML table detection (for web-scraped content)
            if (text.Contains("Scholarship Name") || text.Contains("Application Period") || 
                text.Contains("View website") || text.Contains("Australia Awards") ||
                Regex.IsMatch(text, @"\d+\s+[A-Z][^\d\n]+.*?(January|February|March|April|May|June|July|August|September|October|November|December)", RegexOptions.IgnoreCase))
            {
                _logger.LogInformation("Detected HTML table with scholarship data - enhanced parsing");
                
                scholarshipChunks.Clear();
                
                // Enhanced pattern for table-like structure with numbered entries
                var tableEntryPattern = @"(?:^|\n)\s*(\d+)\s+([^\n\d]+?(?:\n(?!\s*\d+\s)[^\n]*)*?)(?=\n\s*\d+\s|\n\s*$|$)";
                var enhancedTableMatches = Regex.Matches(text, tableEntryPattern, RegexOptions.Multiline | RegexOptions.Singleline);
                
                if (enhancedTableMatches.Count >= 3)
                {
                    _logger.LogInformation($"Found {enhancedTableMatches.Count} table entries using enhanced pattern");
                    
                    foreach (Match match in enhancedTableMatches)
                    {
                        var entryNumber = match.Groups[1].Value;
                        var entryContent = match.Groups[2].Value.Trim();
                        
                        // Clean up the content and format for AI
                        var cleanContent = Regex.Replace(entryContent, @"\s+", " ").Trim();
                        
                        if (cleanContent.Length > 20)
                        {
                            var formattedEntry = $"Entry {entryNumber}: {cleanContent}";
                            scholarshipChunks.Add(formattedEntry);
                        }
                    }
                    
                    if (scholarshipChunks.Count >= 3)
                    {
                        _logger.LogInformation($"Successfully extracted {scholarshipChunks.Count} scholarships from enhanced table format");
                        return true;
                    }
                }
                
                // Fallback: Try to split by lines and look for scholarship entries
                var lines = text.Split('\n');
                var currentScholarship = new StringBuilder();
                var scholarshipCount = 0;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // Skip header lines and empty lines
                    if (string.IsNullOrWhiteSpace(line) ||
                        line.Contains("Scholarship Name") || 
                        line.Contains("Application Period") || 
                        line.Contains("View website") ||
                        line.Contains("Discipline") ||
                        line.Contains("Contact"))
                    {
                        continue;
                    }
                    
                    // Look for lines that start with a number (scholarship entry)
                    if (Regex.IsMatch(line, @"^\d+\s"))
                    {
                        // Save previous scholarship if we have one
                        if (currentScholarship.Length > 20)
                        {
                            scholarshipChunks.Add(CleanText(currentScholarship.ToString()));
                            scholarshipCount++;
                        }
                        
                        // Start new scholarship
                        currentScholarship.Clear();
                        currentScholarship.AppendLine(line);
                    }
                    else if (currentScholarship.Length > 0 && line.Length > 5)
                    {
                        // Add to current scholarship (continuation of table row data)
                        currentScholarship.AppendLine(line);
                    }
                }
                
                // Don't forget the last scholarship
                if (currentScholarship.Length > 20)
                {
                    scholarshipChunks.Add(CleanText(currentScholarship.ToString()));
                    scholarshipCount++;
                }
                
                if (scholarshipCount >= 3)
                {
                    _logger.LogInformation($"Successfully extracted {scholarshipCount} scholarships from HTML table fallback parsing");
                    return true;
                }
            }
            
            // Pattern 5: Enhanced line-by-line detection for continuous text
            var lines2 = text.Split('\n');
            var scholarshipEntries = new List<string>();
            
            for (int i = 0; i < lines2.Length; i++)
            {
                var line = lines2[i].Trim();
                
                // Look for lines that could be scholarship entries
                if (line.Length > 20 && 
                    (Regex.IsMatch(line, @"^\d+[\s\.]") || // Starts with number
                     line.Contains("Scholarship") || 
                     line.Contains("Grant") ||
                     line.Contains("Award") ||
                     line.Contains("Fellowship")))
                {
                    // Collect this entry and potential continuation
                    var entry = line;
                    
                    // Look for continuation lines (up to 3 lines)
                    for (int j = i + 1; j < Math.Min(i + 4, lines2.Length); j++)
                    {
                        var nextLine = lines2[j].Trim();
                        
                        if (string.IsNullOrWhiteSpace(nextLine) || 
                            Regex.IsMatch(nextLine, @"^\d+[\s\.]") ||
                            nextLine.Contains("Scholarship Name"))
                        {
                            break;
                        }
                        
                        entry += " " + nextLine;
                    }
                    
                    if (entry.Length > 30)
                    {
                        scholarshipEntries.Add(CleanText(entry));
                    }
                }
            }
            
            if (scholarshipEntries.Count >= 3)
            {
                _logger.LogInformation($"Successfully extracted {scholarshipEntries.Count} scholarships using line-by-line detection");
                scholarshipChunks = scholarshipEntries.Take(25).ToList(); // Limit to prevent too many
                return true;
            }
            
            _logger.LogInformation("No scholarship list pattern detected");
            scholarshipChunks.Clear();
            return false;
        }

        private List<string> SplitIntoScholarshipChunks(string text)
        {
            var chunks = new List<string>();
            
            _logger.LogInformation($"Splitting text of {text.Length} characters into scholarship chunks");
            
            // First, try to detect scholarship lists with numbered entries
            if (DetectScholarshipList(text, out var listChunks))
            {
                chunks.AddRange(listChunks);
                _logger.LogInformation($"Detected scholarship list format: {chunks.Count} scholarships found");
                return chunks;
            }
            
            // Enhanced separators for better scholarship detection
            var separators = new[] 
            { 
                "\n\n\n", // Triple newlines
                "\n\n", // Double newlines
                "Scholarship #", 
                "SCHOLARSHIP:", 
                "Title:", 
                "Program:",
                "Grant:",
                "Award:",
                "Financial Aid:",
                "Application:",
                "Deadline:",
                "Requirements:",
                "Eligibility:",
                "Benefits:",
                "Amount:",
                "Value:",
                "Duration:",
                "Contact:",
                "More information:",
                "For more details:",
                "Description:",
                "Overview:",
                "Summary:"
            };
            
            string bestSeparator = "";
            string[] bestParts = { text };
            
            // Find the separator that gives us the most meaningful chunks
            foreach (var separator in separators)
            {
                var parts = text.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > bestParts.Length && parts.Length <= 20) // Don't want too many tiny chunks
                {
                    bestParts = parts;
                    bestSeparator = separator;
                }
            }
            
            if (!string.IsNullOrEmpty(bestSeparator))
            {
                _logger.LogInformation($"Using separator '{bestSeparator}' which created {bestParts.Length} parts");
                
                // Filter and clean the parts
                var filteredChunks = bestParts
                    .Where(p => p.Length > 100 && p.Length < 5000) // Reasonable size chunks
                    .Where(p => IsHighQualityScholarshipContent(p)) // Must contain scholarship keywords
                    .Select(p => CleanText(p))
                    .Take(15) // Limit number of chunks
                    .ToList();
                    
                chunks.AddRange(filteredChunks);
                _logger.LogInformation($"Added {filteredChunks.Count} filtered chunks");
            }
            
            // If no good splits found or we need more content, try paragraph-based chunking
            if (chunks.Count < 3 && text.Length > 1000)
            {
                _logger.LogInformation("Trying paragraph-based chunking as backup");
                
                // Split by paragraphs (double newlines) and process each
                var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                var scholarshipParagraphs = paragraphs
                    .Where(p => p.Length > 200 && p.Length < 3000)
                    .Where(p => ContainsScholarshipKeywords(p))
                    .Select(p => CleanText(p))
                    .Take(10)
                    .ToList();
                    
                if (scholarshipParagraphs.Count > chunks.Count)
                {
                    chunks = scholarshipParagraphs;
                    _logger.LogInformation($"Used paragraph-based chunking: {chunks.Count} chunks");
                }
            }
            
            // Final fallback: create fixed-size chunks
            if (chunks.Count == 0 && text.Length > 500)
            {
                _logger.LogInformation("Using fixed-size chunking as final fallback");
                
                for (int i = 0; i < text.Length && chunks.Count < 8; i += 800)
                {
                    var chunkSize = Math.Min(1200, text.Length - i); // Overlapping chunks
                    var chunk = text.Substring(i, chunkSize);
                    
                    if (ContainsScholarshipKeywords(chunk))
                    {
                        chunks.Add(CleanText(chunk));
                    }
                }
                
                _logger.LogInformation($"Fixed-size chunking created {chunks.Count} chunks");
            }
            
            // If still no chunks, add the whole text if it contains scholarship keywords
            if (chunks.Count == 0 && ContainsScholarshipKeywords(text))
            {
                chunks.Add(CleanText(text));
                _logger.LogInformation("Added entire text as single chunk");
            }
            
            _logger.LogInformation($"Final chunking result: {chunks.Count} chunks");
            return chunks;
        }

        private string ExtractBasicTitle(string text)
        {
            // Extract first meaningful line as title
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Take(5))
            {
                var cleaned = line.Trim();
                if (cleaned.Length > 10 && cleaned.Length < 200)
                {
                    return cleaned;
                }
            }
            
            return "Untitled Scholarship";
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";
                
            // Escape quotes and wrap in quotes
            var escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        private bool IsTableRow(HtmlNode node)
        {
            return node.Name.ToLowerInvariant() == "tr" && 
                   node.SelectNodes(".//td")?.Count > 0;
        }

        private List<string> ExtractUrlsFromNode(HtmlNode node)
        {
            var urls = new List<string>();
            
            try
            {
                // Extract URLs from anchor tags
                var links = node.SelectNodes(".//a[@href]");
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        var href = link.GetAttributeValue("href", "");
                        var linkText = CleanText(link.InnerText).ToLowerInvariant();
                        
                        // Look for external links, especially those that indicate scholarship websites
                        if (!string.IsNullOrWhiteSpace(href) && IsExternalScholarshipUrl(href, linkText))
                        {
                            if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
                            {
                                urls.Add(absoluteUri.ToString());
                            }
                        }
                    }
                }
                
                // Also look for URLs in text content using regex
                var textContent = node.InnerText;
                var urlMatches = Regex.Matches(textContent, @"https?://[^\s\)]+", RegexOptions.IgnoreCase);
                foreach (Match match in urlMatches)
                {
                    if (Uri.TryCreate(match.Value, UriKind.Absolute, out var uri))
                    {
                        urls.Add(uri.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error extracting URLs from node: {ex.Message}");
            }
            
            return urls.Distinct().ToList();
        }

        private bool IsExternalScholarshipUrl(string href, string linkText)
        {
            // Skip internal/navigation links
            if (href.StartsWith("#") || href.StartsWith("javascript:") || href.StartsWith("mailto:"))
                return false;
                
            // Skip common non-scholarship links
            var skipPatterns = new[] { "facebook.com", "twitter.com", "linkedin.com", "youtube.com", "instagram.com" };
            if (skipPatterns.Any(pattern => href.ToLowerInvariant().Contains(pattern)))
                return false;
                
            // Look for scholarship-related link text
            var scholarshipIndicators = new[] 
            { 
                "view website", "apply", "more info", "details", "scholarship", "grant", 
                "application", "website", "visit", "learn more", "official", "portal"
            };
            
            // If it's an external URL (contains domain) and has scholarship-related text, include it
            if (href.Contains(".") && scholarshipIndicators.Any(indicator => linkText.Contains(indicator)))
                return true;
                
            // If it's a full URL to an external domain, consider it
            if (Uri.TryCreate(href, UriKind.Absolute, out var uri))
            {
                // Skip if it's the same domain as the source (internal links)
                // This could be enhanced to compare with the source URL domain
                return true;
            }
            
            return false;
        }

        private string ProcessTableRow(HtmlNode tableRow)
        {
            try
            {
                var cells = tableRow.SelectNodes(".//td");
                if (cells == null || cells.Count == 0)
                    return "";

                var cellTexts = new List<string>();
                var extractedUrls = new List<string>();
                
                foreach (var cell in cells)
                {
                    var cellText = CleanText(cell.InnerText);
                    if (!string.IsNullOrWhiteSpace(cellText))
                    {
                        cellTexts.Add(cellText);
                    }
                    
                    // Extract URLs from links within the cell
                    var links = cell.SelectNodes(".//a[@href]");
                    if (links != null)
                    {
                        foreach (var link in links)
                        {
                            var href = link.GetAttributeValue("href", "");
                            if (!string.IsNullOrWhiteSpace(href))
                            {
                                // Convert relative URLs to absolute URLs if needed
                                if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
                                {
                                    extractedUrls.Add(absoluteUri.ToString());
                                }
                                else if (href.StartsWith("/") || href.StartsWith("./"))
                                {
                                    // Handle relative URLs - you might want to make this configurable
                                    extractedUrls.Add(href);
                                }
                            }
                        }
                    }
                }

                // Format as structured text for AI parsing
                if (cellTexts.Count >= 2)
                {
                    var formattedRow = new StringBuilder();
                    
                    // Try to detect the structure: Number | Name | Discipline | Period | Website | Contact
                    if (cellTexts.Count >= 3)
                    {
                        formattedRow.AppendLine($"Scholarship Number: {cellTexts[0]}");
                        formattedRow.AppendLine($"Scholarship Name: {cellTexts[1]}");
                        
                        if (cellTexts.Count > 2)
                        {
                            formattedRow.AppendLine($"Discipline: {cellTexts[2]}");
                        }
                        if (cellTexts.Count > 3)
                        {
                            formattedRow.AppendLine($"Application Period: {cellTexts[3]}");
                        }
                        if (cellTexts.Count > 4)
                        {
                            formattedRow.AppendLine($"Website: {cellTexts[4]}");
                        }
                        if (cellTexts.Count > 5)
                        {
                            formattedRow.AppendLine($"Contact: {cellTexts[5]}");
                        }
                        
                        // Add extracted URLs
                        if (extractedUrls.Any())
                        {
                            formattedRow.AppendLine($"External URLs: {string.Join(", ", extractedUrls)}");
                        }
                    }
                    else
                    {
                        // Fallback: join all cells
                        formattedRow.AppendLine(string.Join(" | ", cellTexts));
                        
                        // Add extracted URLs for fallback format too
                        if (extractedUrls.Any())
                        {
                            formattedRow.AppendLine($"External URLs: {string.Join(", ", extractedUrls)}");
                        }
                    }
                    
                    return formattedRow.ToString().Trim();
                }
                
                var result = string.Join(" ", cellTexts);
                if (extractedUrls.Any())
                {
                    result += $" | External URLs: {string.Join(", ", extractedUrls)}";
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error processing table row: {ex.Message}");
                return CleanText(tableRow.InnerText);
            }
        }

        /// <summary>
        /// Scrapes detailed scholarship information from external application URLs
        /// </summary>
        public async Task<EnhancedScrapedScholarship?> ScrapeExternalUrlAsync(string externalUrl, EnhancedScrapedScholarship baseScholarship)
        {
            try
            {
                _logger.LogInformation($"Scraping external URL: {externalUrl}");
                
                // Validate URL
                if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out var validUri))
                {
                    _logger.LogWarning($"Invalid external URL: {externalUrl}");
                    return null;
                }

                // Rate limiting to avoid being blocked
                await Task.Delay(2000); // 2 second delay between requests
                
                // Create a separate HttpClient for external requests with different headers
                using var externalClient = new HttpClient();
                externalClient.Timeout = TimeSpan.FromSeconds(45); // Longer timeout for external sites
                
                // Enhanced headers to avoid detection as bot
                externalClient.DefaultRequestHeaders.Clear();
                externalClient.DefaultRequestHeaders.Add("User-Agent", 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                externalClient.DefaultRequestHeaders.Add("Accept", 
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                externalClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                externalClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                externalClient.DefaultRequestHeaders.Add("DNT", "1");
                externalClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                externalClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                externalClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                externalClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                externalClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                externalClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                externalClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                
                // Additional retry logic for external requests
                HttpResponseMessage? response = null;
                var maxRetries = 3;
                var retryDelay = 1000; // Start with 1 second
                
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        if (retry > 0)
                        {
                            _logger.LogInformation($"Retrying external URL request (attempt {retry + 1}/{maxRetries}) after {retryDelay}ms delay");
                            await Task.Delay(retryDelay);
                            retryDelay *= 2; // Exponential backoff
                        }
                        
                        response = await externalClient.GetAsync(externalUrl);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            break; // Success, exit retry loop
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogWarning($"Rate limited by {externalUrl}, waiting longer before retry");
                            await Task.Delay(5000); // Wait 5 seconds for rate limiting
                        }
                        else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            // Client errors (4xx) usually won't be resolved by retrying
                            _logger.LogWarning($"Client error {response.StatusCode} for {externalUrl}, skipping retries");
                            break;
                        }
                    }
                    catch (TaskCanceledException) when (retry < maxRetries - 1)
                    {
                        _logger.LogWarning($"Request timeout for {externalUrl}, retrying...");
                        continue;
                    }
                    catch (HttpRequestException) when (retry < maxRetries - 1)
                    {
                        _logger.LogWarning($"HTTP error for {externalUrl}, retrying...");
                        continue;
                    }
                }
                
                if (response == null || !response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch external URL {externalUrl}: {(response != null ? response.StatusCode.ToString() : "No response")}");
                    return null;
                }
                
                var html = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(html) || html.Length < 100)
                {
                    _logger.LogWarning($"External URL returned insufficient content: {html?.Length ?? 0} characters");
                    return null;
                }
                
                _logger.LogInformation($"Successfully fetched {html.Length} characters from external URL");
                
                // Parse the HTML to extract scholarship details
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // Extract main content from the external page
                var externalContent = ExtractMainContentFromExternalPage(doc);
                
                if (string.IsNullOrWhiteSpace(externalContent))
                {
                    _logger.LogWarning("Could not extract meaningful content from external page");
                    return null;
                }
                
                _logger.LogInformation($"Extracted {externalContent.Length} characters of content from external page");
                
                // Use AI to parse the external content with enhanced prompts
                var enhancedScholarship = await ParseExternalScholarshipWithAIAsync(externalContent, externalUrl, baseScholarship);
                
                if (enhancedScholarship != null)
                {
                    _logger.LogInformation($"Successfully enhanced scholarship '{enhancedScholarship.Title}' with external data");
                    enhancedScholarship.ParsingNotes.Add($"Enhanced with external URL: {externalUrl}");
                }
                
                return enhancedScholarship;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning($"External URL request timed out: {externalUrl}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"HTTP error fetching external URL {externalUrl}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scraping external URL: {externalUrl}");
                return null;
            }
        }

        /// <summary>
        /// Extracts main content from external scholarship provider pages
        /// </summary>
        private string ExtractMainContentFromExternalPage(HtmlDocument doc)
        {
            var contentSelectors = new[]
            {
                // Common content selectors for scholarship pages
                "//main",
                "//article",
                "//div[contains(@class, 'content')]",
                "//div[contains(@class, 'main')]",
                "//div[contains(@class, 'scholarship')]",
                "//div[contains(@class, 'program')]",
                "//div[contains(@class, 'opportunity')]",
                "//div[contains(@class, 'award')]",
                "//div[contains(@class, 'grant')]",
                "//div[contains(@class, 'details')]",
                "//div[contains(@class, 'description')]",
                "//div[contains(@class, 'information')]",
                "//div[contains(@class, 'about')]",
                "//div[contains(@id, 'content')]",
                "//div[contains(@id, 'main')]",
                "//section",
                "//div[@role='main']",
                "//body"
            };
            
            foreach (var selector in contentSelectors)
            {
                try
                {
                    var contentNode = doc.DocumentNode.SelectSingleNode(selector);
                    if (contentNode != null)
                    {
                        var text = CleanText(contentNode.InnerText);
                        if (!string.IsNullOrWhiteSpace(text) && text.Length > 200)
                        {
                            _logger.LogInformation($"Extracted content using selector: {selector} ({text.Length} characters)");
                            
                            // Remove navigation, footer, and other non-content elements
                            var cleanedText = CleanExternalPageContent(text);
                            
                            if (cleanedText.Length > 500)
                            {
                                return cleanedText;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"Selector {selector} failed: {ex.Message}");
                    continue;
                }
            }
            
            return "";
        }

        /// <summary>
        /// Cleans extracted content from external pages by removing navigation, footer, and irrelevant content
        /// </summary>
        private string CleanExternalPageContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "";
            
            // Common phrases to remove from external content
            var removePatterns = new[]
            {
                @"Skip to (?:main )?content",
                @"Navigation menu",
                @"Main menu",
                @"Footer",
                @"Copyright.*?\d{4}",
                @"All rights reserved",
                @"Privacy Policy",
                @"Terms (?:of Use|and Conditions)",
                @"Contact Us",
                @"Follow us on",
                @"Social media",
                @"Subscribe to",
                @"Newsletter",
                @"Cookie Policy",
                @"Search this site",
                @"Last updated",
                @"Print this page",
                @"Share this page",
                @"Home\s+About\s+",
                @"Menu\s+Home\s+",
                @"Login\s+Register",
                @"Sign in\s+Sign up"
            };
            
            var cleaned = content;
            
            foreach (var pattern in removePatterns)
            {
                cleaned = Regex.Replace(cleaned, pattern, "", RegexOptions.IgnoreCase);
            }
            
            // Remove excessive whitespace
            cleaned = Regex.Replace(cleaned, @"\s+", " ");
            cleaned = cleaned.Trim();
            
            // Focus on scholarship-related content
            var lines = cleaned.Split('\n');
            var scholarshipLines = new List<string>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Length > 20 && 
                    (ContainsScholarshipKeywords(trimmedLine) || 
                     scholarshipLines.Count > 0)) // Include context after finding scholarship content
                {
                    scholarshipLines.Add(trimmedLine);
                }
                
                // Stop if we have enough content
                if (scholarshipLines.Count > 50)
                    break;
            }
            
            return string.Join(" ", scholarshipLines);
        }

        /// <summary>
        /// Uses AI to parse detailed scholarship information from external website content
        /// </summary>
        private async Task<EnhancedScrapedScholarship?> ParseExternalScholarshipWithAIAsync(string externalContent, string externalUrl, EnhancedScrapedScholarship baseScholarship)
        {
            try
            {
                _logger.LogInformation($"Starting AI parsing for external content of {externalContent.Length} characters");
                
                var aiPrompt = CreateEnhancedScholarshipParsingPrompt(externalContent, baseScholarship);
                
                _logger.LogInformation("Sending enhanced prompt to AI service for external content...");
                var aiResponse = await _openAIService.GetChatCompletionAsync(aiPrompt);
                
                _logger.LogInformation($"AI Response received for external content ({aiResponse.Length} characters)");
                
                var result = ParseAIResponse(aiResponse, externalContent, externalUrl);
                
                if (result != null)
                {
                    // Merge with base scholarship data intelligently
                    var mergedScholarship = MergeScholarshipData(baseScholarship, result);
                    mergedScholarship.ExternalApplicationUrl = externalUrl;
                    mergedScholarship.ParsingNotes.Add("Enhanced with external website content");
                    mergedScholarship.ParsingConfidence = Math.Max(mergedScholarship.ParsingConfidence, 0.8); // Higher confidence for external data
                    
                    _logger.LogInformation($"Successfully merged scholarship data for: '{mergedScholarship.Title}'");
                    return mergedScholarship;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing external scholarship with AI");
                return null;
            }
        }

        /// <summary>
        /// Creates enhanced AI prompt for parsing external scholarship website content
        /// </summary>
        private string CreateEnhancedScholarshipParsingPrompt(string externalContent, EnhancedScrapedScholarship baseScholarship)
        {
            return $@"You are an AI assistant that extracts comprehensive scholarship information from official scholarship provider websites.

CONTEXT: This is detailed content from the official scholarship provider website for: '{baseScholarship.Title}'
The content below contains complete scholarship details including application requirements, benefits, deadlines, and eligibility criteria.

INSTRUCTIONS:
- Extract ALL available scholarship details from this official source
- Be thorough and comprehensive - this is the authoritative source
- Focus on key information that students need to apply
- Extract monetary values, deadlines, requirements, and benefits
- Look for application procedures, contact information, and specific criteria
- Convert any dates to YYYY-MM-DD format (assume current year 2025 if not specified)
- Extract GPA requirements, course requirements, year level requirements
- Look for scholarship value/amount information
- Extract any specific university or institution requirements

CRITICAL: Return ONLY a single JSON object with complete scholarship information.

Extract these fields comprehensively:
- Title (official scholarship name from the provider website)
- Description (comprehensive description including purpose and scope)
- Benefits (detailed benefits including financial and non-financial support)
- MonetaryValue (extract scholarship amount as decimal, e.g., 50000.00)
- ApplicationDeadline (application deadline in YYYY-MM-DD format)
- Requirements (comprehensive eligibility and application requirements)
- SlotsAvailable (number of scholarships available if mentioned)
- MinimumGPA (GPA requirement as decimal if specified)
- RequiredCourse (specific field of study or course requirements)
- RequiredYearLevel (target year level: 1-4 for undergraduate, 5-8 for graduate)
- RequiredUniversity (specific university requirements if any)
- ExternalApplicationUrl (application portal URL if different from this page)

Be extremely thorough - this is the official source so extract as much detail as possible.

Official scholarship website content:
{externalContent}

JSON Response:";
        }

        /// <summary>
        /// Intelligently merges scholarship data from table source with external website details
        /// </summary>
        private EnhancedScrapedScholarship MergeScholarshipData(EnhancedScrapedScholarship baseScholarship, EnhancedScrapedScholarship externalScholarship)
        {
            var merged = new EnhancedScrapedScholarship
            {
                // Use external title if it's more comprehensive, otherwise use base
                Title = !string.IsNullOrWhiteSpace(externalScholarship.Title) && externalScholarship.Title.Length > baseScholarship.Title?.Length 
                    ? externalScholarship.Title 
                    : baseScholarship.Title ?? externalScholarship.Title,
                
                // Combine descriptions intelligently
                Description = CombineText(baseScholarship.Description, externalScholarship.Description, " | "),
                
                // Use external benefits as they're more detailed
                Benefits = !string.IsNullOrWhiteSpace(externalScholarship.Benefits) 
                    ? externalScholarship.Benefits 
                    : baseScholarship.Benefits,
                
                // Prefer external monetary value as it's from official source
                MonetaryValue = externalScholarship.MonetaryValue ?? baseScholarship.MonetaryValue,
                
                // Use external deadline as it's more reliable
                ApplicationDeadline = externalScholarship.ApplicationDeadline ?? baseScholarship.ApplicationDeadline,
                
                // Combine requirements for comprehensive information
                Requirements = CombineText(baseScholarship.Requirements, externalScholarship.Requirements, " | "),
                
                // Use external data for detailed fields
                SlotsAvailable = externalScholarship.SlotsAvailable ?? baseScholarship.SlotsAvailable,
                MinimumGPA = externalScholarship.MinimumGPA ?? baseScholarship.MinimumGPA,
                RequiredCourse = CombineText(baseScholarship.RequiredCourse, externalScholarship.RequiredCourse, " | "),
                RequiredYearLevel = externalScholarship.RequiredYearLevel ?? baseScholarship.RequiredYearLevel,
                RequiredUniversity = CombineText(baseScholarship.RequiredUniversity, externalScholarship.RequiredUniversity, " | "),
                
                // Prefer external URL for applications
                ExternalApplicationUrl = externalScholarship.ExternalApplicationUrl ?? baseScholarship.ExternalApplicationUrl,
                
                // Keep original metadata
                SourceUrl = baseScholarship.SourceUrl,
                ScrapedAt = baseScholarship.ScrapedAt,
                IsActive = true,
                IsInternal = false, // External URLs indicate external scholarships
                
                // Combine raw text for reference
                RawText = $"TABLE DATA: {baseScholarship.RawText}\n\nEXTERNAL CONTENT: {externalScholarship.RawText}",
                
                // Combine parsing notes
                ParsingNotes = baseScholarship.ParsingNotes.Concat(externalScholarship.ParsingNotes).ToList(),
                
                // Use higher confidence from external parsing
                ParsingConfidence = Math.Max(baseScholarship.ParsingConfidence, externalScholarship.ParsingConfidence)
            };
            
            return merged;
        }

        /// <summary>
        /// Combines two text fields intelligently, avoiding duplication
        /// </summary>
        private string? CombineText(string? text1, string? text2, string separator = " | ")
        {
            if (string.IsNullOrWhiteSpace(text1) && string.IsNullOrWhiteSpace(text2))
                return null;
            
            if (string.IsNullOrWhiteSpace(text1))
                return text2;
            
            if (string.IsNullOrWhiteSpace(text2))
                return text1;
            
            // Avoid duplication if texts are very similar
            if (text1.Length > 10 && text2.Length > 10)
            {
                var similarity = CalculateTextSimilarity(text1, text2);
                if (similarity > 0.8) // 80% similar, just use the longer one
                {
                    return text1.Length > text2.Length ? text1 : text2;
                }
            }
            
            return $"{text1}{separator}{text2}";
        }

        /// <summary>
        /// Calculates simple text similarity to avoid duplicate information
        /// </summary>
        private double CalculateTextSimilarity(string text1, string text2)
        {
            var words1 = text1.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var words2 = text2.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            
            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();
            
            return union > 0 ? (double)intersection / union : 0;
        }
    }
}
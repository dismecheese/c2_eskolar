using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using c2_eskolar.Data;
using c2_eskolar.Models;

namespace c2_eskolar.Services.WebScraping
{
    public class ScrapingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScrapingBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Run daily

        public ScrapingBackgroundService(IServiceProvider serviceProvider, ILogger<ScrapingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_period);
            
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }

        private async Task DoWorkAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled web scraping tasks...");
                
                using var scope = _serviceProvider.CreateScope();
                var scrapingService = scope.ServiceProvider.GetRequiredService<IWebScrapingService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Define scholarship sources to scrape
                var scholarshipSources = new[]
                {
                    "https://www.scholarships.com/financial-aid/college-scholarships",
                    "https://www.fastweb.com/college-scholarships",
                    // Add more sources as needed
                };
                
                foreach (var source in scholarshipSources)
                {
                    try
                    {
                        var scrapedScholarships = await scrapingService.ScrapeScholarshipsAsync(source);
                        
                        foreach (var scrapedScholarship in scrapedScholarships)
                        {
                            // Check if scholarship already exists
                            var descriptionSample = !string.IsNullOrEmpty(scrapedScholarship.Description) 
                                ? scrapedScholarship.Description.Substring(0, Math.Min(50, scrapedScholarship.Description.Length))
                                : "";
                            
                            var existingScholarship = await dbContext.Scholarships
                                .FirstOrDefaultAsync(s => s.Title == scrapedScholarship.Title && 
                                                        (s.Description == null || s.Description.Contains(descriptionSample)));
                            
                            if (existingScholarship == null)
                            {
                                // Create new scholarship from scraped data
                                var scholarship = new Scholarship
                                {
                                    Title = scrapedScholarship.Title,
                                    Description = scrapedScholarship.Description,
                                    Benefits = scrapedScholarship.Amount ?? "Amount not specified",
                                    MonetaryValue = ParseAmount(scrapedScholarship.Amount ?? ""),
                                    ApplicationDeadline = ParseDeadline(scrapedScholarship.ApplicationDeadline) ?? DateTime.Now.AddMonths(6),
                                    Requirements = scrapedScholarship.Eligibility ?? "Requirements not specified",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now,
                                    IsActive = true
                                };
                                
                                dbContext.Scholarships.Add(scholarship);
                            }
                        }
                        
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"Processed {scrapedScholarships.Count} scholarships from {source}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing scholarships from {source}");
                    }
                }
                
                _logger.LogInformation("Completed scheduled web scraping tasks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled web scraping task");
            }
        }

        private decimal? ParseAmount(string amountText)
        {
            if (string.IsNullOrWhiteSpace(amountText))
                return null;
                
            var match = System.Text.RegularExpressions.Regex.Match(amountText, @"\$?([\d,]+(?:\.\d{2})?)");
            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var amount))
            {
                return amount;
            }
            
            return null;
        }

        private DateTime? ParseDeadline(string deadlineText)
        {
            if (string.IsNullOrWhiteSpace(deadlineText))
                return null;
                
            if (DateTime.TryParse(deadlineText, out var deadline))
            {
                return deadline;
            }
            
            return null;
        }
    }
}
# Web Scraping Implementation Guide for eSkolar System

## Overview

This guide explains how to implement and use web scraping in your eSkolar scholarship management system. Web scraping can help automate the discovery of new scholarships, verify institutions and organizations, and gather scholarship-related news.

## Architecture

### Components Created

1. **IWebScrapingService** - Interface defining scraping operations
2. **WebScrapingService** - Main implementation using HtmlAgilityPack
3. **ScrapingBackgroundService** - Scheduled background scraping
4. **ScrapingController** - API endpoints for manual scraping
5. **WebScrapingManagement.razor** - Admin interface for managing scraping
6. **ScrapingConfiguration** - Configuration models

### Dependencies Added

```xml
<PackageReference Include="HtmlAgilityPack" Version="1.11.54" />
<PackageReference Include="AngleSharp" Version="0.17.1" />
<PackageReference Include="Selenium.WebDriver" Version="4.15.0" />
<PackageReference Include="Selenium.WebDriver.ChromeDriver" Version="118.0.5993.7000" />
<PackageReference Include="Quartz" Version="3.8.0" />
```

## Configuration

### appsettings.json

```json
"WebScraping": {
  "ScholarshipSources": [
    {
      "Name": "Scholarships.com",
      "Url": "https://www.scholarships.com/financial-aid/college-scholarships",
      "ContainerSelector": ".scholarship-item",
      "TitleSelector": "h3",
      "DescriptionSelector": ".description",
      "AmountSelector": ".amount",
      "DeadlineSelector": ".deadline",
      "IsEnabled": false
    }
  ],
  "MaxConcurrentRequests": 3,
  "DelayBetweenRequests": 2000,
  "EnableScheduledScraping": false,
  "ScrapingIntervalHours": 24
}
```

## Use Cases

### 1. Scholarship Discovery

**Purpose**: Automatically find new scholarship opportunities from various websites.

**How it works**:
- Scrapes scholarship listing websites
- Extracts title, description, amount, deadline, and eligibility
- Saves new scholarships to your database
- Avoids duplicates by checking existing titles

**Usage**:
```csharp
var scholarships = await _scrapingService.ScrapeScholarshipsAsync("https://example.com/scholarships");
```

### 2. Institution Verification

**Purpose**: Verify the legitimacy of educational institutions.

**How it works**:
- Searches for official institution websites
- Extracts official name, location, accreditation status
- Verifies institution information against official sources

**Usage**:
```csharp
var result = await _scrapingService.VerifyInstitutionAsync("University Name", "https://university.edu");
```

### 3. Organization Verification

**Purpose**: Verify benefactor organizations for legitimacy.

**How it works**:
- Checks if organization name appears on their official website
- Basic verification to prevent fraudulent organizations

**Usage**:
```csharp
var isVerified = await _scrapingService.VerifyOrganizationAsync("Foundation Name", "https://foundation.org");
```

### 4. News Scraping

**Purpose**: Gather scholarship-related news and announcements.

**How it works**:
- Scrapes news websites for scholarship announcements
- Extracts title, content, and publication dates
- Can be displayed on dashboards for users

**Usage**:
```csharp
var news = await _scrapingService.ScrapeScholarshipNewsAsync();
```

## Implementation Steps

### 1. Enable Web Scraping

In `appsettings.json`, set:
```json
"EnableScheduledScraping": true
```

### 2. Configure Target Websites

Add scholarship sources to your configuration:

```json
"ScholarshipSources": [
  {
    "Name": "Target Website",
    "Url": "https://target-website.com/scholarships",
    "ContainerSelector": ".scholarship-card", // CSS selector for scholarship containers
    "TitleSelector": "h2.title",              // CSS selector for title
    "DescriptionSelector": ".description",     // CSS selector for description
    "AmountSelector": ".amount",              // CSS selector for amount
    "DeadlineSelector": ".deadline",          // CSS selector for deadline
    "IsEnabled": true
  }
]
```

### 3. Customize Selectors

For each target website, you need to:

1. **Inspect the HTML structure** of the scholarship listings
2. **Identify CSS selectors** for each data element
3. **Test selectors** using browser developer tools
4. **Update configuration** with correct selectors

**Example**: For a website with this structure:
```html
<div class="scholarship-item">
  <h3 class="scholarship-title">Full Tuition Scholarship</h3>
  <p class="scholarship-desc">Description here...</p>
  <span class="award-amount">$10,000</span>
  <div class="deadline">December 31, 2024</div>
</div>
```

Your configuration would be:
```json
{
  "ContainerSelector": ".scholarship-item",
  "TitleSelector": ".scholarship-title",
  "DescriptionSelector": ".scholarship-desc",
  "AmountSelector": ".award-amount",
  "DeadlineSelector": ".deadline"
}
```

### 4. Access the Admin Interface

Navigate to `/admin/webscraping` (requires Admin role) to:
- Manually trigger scraping operations
- Test new website configurations
- View scraping results
- Verify institutions and organizations

### 5. API Endpoints

Use these endpoints for integration:

- `POST /api/scraping/scholarships` - Scrape scholarships from URL
- `POST /api/scraping/verify-institution` - Verify institution
- `POST /api/scraping/verify-organization` - Verify organization
- `GET /api/scraping/news` - Scrape scholarship news

## Best Practices

### 1. Respect Website Policies

- **Check robots.txt** before scraping any website
- **Implement delays** between requests (configured via `DelayBetweenRequests`)
- **Limit concurrent requests** to avoid overloading servers
- **Use appropriate User-Agent** headers

### 2. Handle Errors Gracefully

```csharp
try
{
    var scholarships = await _scrapingService.ScrapeScholarshipsAsync(url);
}
catch (HttpRequestException ex)
{
    // Handle network errors
    _logger.LogError(ex, "Network error while scraping {Url}", url);
}
catch (ArgumentException ex)
{
    // Handle invalid URLs or selectors
    _logger.LogError(ex, "Invalid configuration for {Url}", url);
}
```

### 3. Monitor and Maintain

- **Regularly test** configurations as websites change their structure
- **Monitor logs** for scraping errors
- **Update selectors** when target websites are redesigned
- **Verify data quality** of scraped information

### 4. Legal Considerations

- **Review Terms of Service** of target websites
- **Implement rate limiting** to be respectful
- **Consider API alternatives** when available
- **Ensure compliance** with local data protection laws

## Troubleshooting

### Common Issues

1. **No data scraped**:
   - Check if selectors are correct
   - Verify website is accessible
   - Check for anti-bot protection

2. **403/429 HTTP errors**:
   - Increase delay between requests
   - Update User-Agent header
   - Consider using proxy rotation

3. **Incomplete data**:
   - Verify all CSS selectors
   - Check if website uses JavaScript loading
   - Consider using Selenium for dynamic content

### Testing Selectors

Use browser developer tools:

1. **Open target website** in browser
2. **Press F12** to open developer tools
3. **Use Console** to test selectors:
   ```javascript
   document.querySelectorAll('.scholarship-item'); // Test container selector
   document.querySelector('.scholarship-title').textContent; // Test title selector
   ```

## Advanced Features

### Using Selenium for JavaScript-Heavy Sites

For websites that load content dynamically with JavaScript:

```csharp
public async Task<List<ScrapedScholarship>> ScrapeWithSeleniumAsync(string url)
{
    var options = new ChromeOptions();
    options.AddArgument("--headless");
    
    using var driver = new ChromeDriver(options);
    driver.Navigate().GoToUrl(url);
    
    // Wait for content to load
    await Task.Delay(5000);
    
    var pageSource = driver.PageSource;
    // Process with HtmlAgilityPack as usual
}
```

### Scheduled Background Scraping

The background service automatically:
- Runs daily (configurable)
- Scrapes all enabled sources
- Saves new scholarships to database
- Logs all activities

### Data Validation

Scraped data goes through validation:
- Amount parsing and normalization
- Date parsing for deadlines
- Duplicate detection
- Required field validation

## Security Considerations

1. **Rate Limiting**: Implemented to prevent abuse
2. **Admin Only**: Scraping operations require Admin role
3. **Input Validation**: All URLs and parameters are validated
4. **Error Handling**: Sensitive information is not exposed in error messages
5. **Logging**: All operations are logged for monitoring

## Next Steps

1. **Test with real websites** by updating configurations
2. **Monitor performance** and adjust delays as needed
3. **Expand to more sources** as the system proves reliable
4. **Consider ML/AI** for better data extraction
5. **Implement email notifications** for new scholarships found

This implementation provides a solid foundation for web scraping in your scholarship system while maintaining security, reliability, and respect for target websites.
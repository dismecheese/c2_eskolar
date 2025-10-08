# AI-Enhanced Web Scraping for Scholarship Data

This enhanced web scraping system uses OpenAI GPT-4.1 Mini to intelligently parse scholarship information from websites and automatically populate database-ready CSV files.

## Features

### 🧠 AI-Powered Data Extraction
- Automatically extracts structured scholarship data from any website
- Uses OpenAI GPT-4.1 Mini for intelligent parsing
- Maps extracted data to your `Scholarship` model fields
- Provides confidence scores for data quality assessment

### 📊 Database-Ready CSV Export
- Generates CSV files that match your `Scholarship` table structure
- Includes all required fields: Title, Benefits, MonetaryValue, ApplicationDeadline, etc.
- Ready for direct database import
- Handles data validation and formatting

### 🎯 Intelligent Field Mapping
The AI automatically extracts and maps:
- **Title** - Scholarship program name
- **Description** - Detailed scholarship information
- **Benefits** - What the scholarship provides (monetary + non-monetary)
- **MonetaryValue** - Main financial amount (extracted as decimal)
- **ApplicationDeadline** - Deadline dates (converted to proper format)
- **Requirements** - Eligibility criteria and application requirements
- **MinimumGPA** - GPA requirements (if specified)
- **RequiredCourse** - Specific program/course requirements
- **RequiredYearLevel** - Year level restrictions
- **RequiredUniversity** - Institution-specific scholarships
- **ExternalApplicationUrl** - Application links

## How to Use

### 1. Access the AI Scraping Tab
1. Navigate to `/admin/webscraping` (SuperAdmin role required)
2. Click on "AI-Enhanced Scraping" tab
3. Enter the website URL containing scholarship information

### 2. Scrape with AI
1. Enter the scholarship website URL
2. Click "Scrape with AI" 
3. The system will:
   - Extract text content from the website
   - Send it to OpenAI for intelligent parsing
   - Return structured scholarship data
   - Display confidence scores for data quality

### 3. Review and Export
1. Review the parsed scholarships in the table
2. Check confidence scores (Green: >70%, Yellow: 40-70%, Red: <40%)
3. Click "Export CSV" to download the database-ready file

### 4. Import to Database
The generated CSV can be imported directly into your database:

```sql
-- Example SQL Server BULK INSERT
BULK INSERT Scholarships
FROM 'path/to/ai_scholarships_20251008_143022.csv'
WITH (
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIRSTROW = 2
);
```

## CSV Format

The exported CSV includes these columns matching your `Scholarship` model:

| Column | Type | Description |
|--------|------|-------------|
| Title | string | Scholarship name (required) |
| Description | string | Detailed description |
| Benefits | string | What scholarship provides |
| MonetaryValue | decimal | Main monetary amount |
| ApplicationDeadline | datetime | Application deadline |
| Requirements | string | Eligibility criteria |
| SlotsAvailable | int | Number of slots |
| MinimumGPA | decimal | Required GPA |
| RequiredCourse | string | Course requirements |
| RequiredYearLevel | int | Year level (1-8) |
| RequiredUniversity | string | University restrictions |
| IsActive | boolean | Always true for scraped |
| IsInternal | boolean | Always false for scraped |
| ExternalApplicationUrl | string | Application URL |
| SourceUrl | string | Original website |
| ScrapedAt | datetime | When data was scraped |
| ParsingConfidence | decimal | AI confidence score |

## Technical Implementation

### Services Added
- `IEnhancedWebScrapingService` - Enhanced scraping interface
- `EnhancedWebScrapingService` - AI-powered implementation
- Integration with existing `OpenAIService`

### AI Prompt Engineering
The system uses a carefully crafted prompt that:
- Instructs GPT to extract specific scholarship fields
- Returns structured JSON responses
- Handles missing or unclear data gracefully
- Provides conservative, accurate extractions

### Error Handling
- Fallback to basic text extraction if AI fails
- Validation of AI JSON responses
- Confidence scoring based on completeness
- Detailed logging for debugging

## Example Workflow

1. **Input**: `https://scholarships.example.com/programs`
2. **AI Processing**: Extracts and structures scholarship data
3. **Output**: CSV with 15 scholarships, avg confidence 85%
4. **Import**: Direct upload to database via CSV import

## Benefits

✅ **Accuracy**: AI understands context and extracts relevant data  
✅ **Speed**: Process multiple scholarships from a page in one operation  
✅ **Consistency**: Standardized output format ready for database  
✅ **Quality**: Confidence scores help identify data quality issues  
✅ **Scalability**: Can handle various website structures automatically  

## Configuration

Ensure your `appsettings.json` includes:

```json
{
  "AzureOpenAI": {
    "ApiKey": "your-api-key",
    "Endpoint": "your-endpoint",
    "DeploymentName": "gpt-4-mini"
  }
}
```

## Security

- SuperAdmin role required for access
- API key secured in configuration
- Input validation on URLs
- Rate limiting through Azure OpenAI

Ready to revolutionize your scholarship data collection! 🚀
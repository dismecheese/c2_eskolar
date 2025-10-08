namespace c2_eskolar.Services.WebScraping
{
    public class ScrapingConfiguration
    {
        public List<ScrapingSource> ScholarshipSources { get; set; } = new();
        public List<ScrapingSource> NewsSources { get; set; } = new();
        public int MaxConcurrentRequests { get; set; } = 3;
        public int DelayBetweenRequests { get; set; } = 1000; // milliseconds
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        public bool EnableScheduledScraping { get; set; } = true;
        public int ScrapingIntervalHours { get; set; } = 24;
    }

    public class ScrapingSource
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public string TitleSelector { get; set; } = "";
        public string DescriptionSelector { get; set; } = "";
        public string AmountSelector { get; set; } = "";
        public string DeadlineSelector { get; set; } = "";
        public string ContainerSelector { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
    }
}
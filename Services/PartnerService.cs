using c2_eskolar.Models;
using System.Globalization;
using CsvHelper;

namespace c2_eskolar.Services
{
    // Service for loading partner organizations (from CSV or default values)
    public class PartnerService
    {
        private readonly IWebHostEnvironment _environment;

        // Inject hosting environment to locate wwwroot (WebRootPath)
        public PartnerService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Loads partner data asynchronously (from CSV if available, else fallback)
        public async Task<List<Partner>> GetPartnersAsync()
        {
            var partners = new List<Partner>();

            // Path to the CSV file inside wwwroot/data/partners
            var csvPath = Path.Combine(_environment.WebRootPath, "data", "partners", "partners.csv");

            try
            {
                if (File.Exists(csvPath))
                {
                    using var reader = new StringReader(await File.ReadAllTextAsync(csvPath));
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                    partners = csv.GetRecords<Partner>().ToList();
                }
                else
                {
                    // Fallback to default partners if CSV doesn't exist
                    partners = GetDefaultPartners();
                }
            }
            catch (Exception ex)
            {
                // Log error and return default partners
                Console.WriteLine($"Error reading partners CSV: {ex.Message}");
                partners = GetDefaultPartners();
            }

            return partners;
        }

        // Provides hardcoded fallback partners (when CSV missing or invalid)
        private List<Partner> GetDefaultPartners()
        {
            return new List<Partner>
            {
                new Partner { OrganizationName = "Sample Organization 1", LogoPath = "/images/partners/placeholder-logo.svg" },
                new Partner { OrganizationName = "Sample Organization 2", LogoPath = "/images/partners/default-logo.png" },
                new Partner { OrganizationName = "Sample Organization 3", LogoPath = "/images/partners/default-logo.png" }
            };
        }
    }
}

using c2_eskolar.Models;
using System.Globalization;
using CsvHelper;

namespace c2_eskolar.Services
{
    public class PartnerService
    {
        private readonly IWebHostEnvironment _environment;

        public PartnerService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<List<Partner>> GetPartnersAsync()
        {
            var partners = new List<Partner>();
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

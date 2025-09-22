namespace c2_eskolar.Models
{
    // Represents a partner organization associated with the platform
    public class Partner
    {
        // Name of the partner organization
        public string OrganizationName { get; set; } = string.Empty;

        // Path or URL to the organization's logo image
        public string LogoPath { get; set; } = string.Empty;
    }
}

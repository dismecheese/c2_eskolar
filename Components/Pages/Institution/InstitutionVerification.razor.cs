using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;

namespace c2_eskolar.Components.Pages.Institution
{
    public partial class InstitutionVerification : ComponentBase
    {
        public class InstitutionVerificationModel
        {
            [Required(ErrorMessage = "Institution name is required.")]
            public string InstitutionName { get; set; } = string.Empty;

            public string? InstitutionType { get; set; } = string.Empty;

            [Required(ErrorMessage = "Address is required.")]
            public string Address { get; set; } = string.Empty;

            public string? Website { get; set; } = string.Empty;

            public string? Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contact number is required.")]
            public string ContactNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contact email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string ContactEmail { get; set; } = string.Empty;

            // Admin fields
            [Required(ErrorMessage = "Admin first name is required.")]
            public string AdminFirstName { get; set; } = string.Empty;

            public string? AdminMiddleName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Admin last name is required.")]
            public string AdminLastName { get; set; } = string.Empty;

            public string? AdminPosition { get; set; } = string.Empty;

            [Required(ErrorMessage = "Admin email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string AdminEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Admin contact number is required.")]
            public string AdminContactNumber { get; set; } = string.Empty;

            public string? Accreditation { get; set; } = string.Empty;

            // Added missing Dean fields
            public string? DeanName { get; set; } = string.Empty;
            public string? DeanEmail { get; set; } = string.Empty;
        }

        // File upload variables for Document Upload section
        private string? IdUploadStatus;
        private string? IdFileName;
        private string? AuthLetterUploadStatus;
        private string? AuthLetterFileName;

        // File upload URLs
        private string? AdminValidationUploadUrl;
        private string? LogoUploadUrl;

        // File upload valid check
        private bool IsFileUploadsValid => !string.IsNullOrEmpty(AdminValidationUploadUrl) && !string.IsNullOrEmpty(LogoUploadUrl);

        private InstitutionVerificationModel verificationModel = new InstitutionVerificationModel();
        private bool IsSubmitting = false;
        private string ProfileErrorMessage = "";

        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private Services.InstitutionProfileService InstitutionProfileService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        // File upload and removal methods for Document Upload section
        private async Task OnIdFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;
            IdFileName = file.Name;
            IdUploadStatus = "Uploading...";
            try
            {
                var content = new MultipartFormDataContent();
                var stream = file.OpenReadStream(5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "file", file.Name);
                content.Add(new StringContent("IdDocument"), "docType");
                var response = await Http.PostAsync("/api/document/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    AdminValidationUploadUrl = result?.url;
                    IdUploadStatus = "Uploaded!";
                }
                else
                {
                    IdUploadStatus = "Upload failed.";
                }
            }
            catch (Exception ex)
            {
                IdUploadStatus = $"Error: {ex.Message}";
            }
        }

        private async Task OnAuthLetterFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null) return;
            AuthLetterFileName = file.Name;
            AuthLetterUploadStatus = "Uploading...";
            try
            {
                var content = new MultipartFormDataContent();
                var stream = file.OpenReadStream(5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "file", file.Name);
                content.Add(new StringContent("AuthLetter"), "docType");
                var response = await Http.PostAsync("/api/document/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    LogoUploadUrl = result?.url;
                    AuthLetterUploadStatus = "Uploaded!";
                }
                else
                {
                    AuthLetterUploadStatus = "Upload failed.";
                }
            }
            catch (Exception ex)
            {
                AuthLetterUploadStatus = $"Error: {ex.Message}";
            }
        }

        private async Task RemoveIdDocument()
        {
            if (string.IsNullOrEmpty(AdminValidationUploadUrl)) return;
            try
            {
                var uri = new Uri(AdminValidationUploadUrl);
                var fileName = uri.Segments[^1];
                var response = await Http.DeleteAsync($"/api/document/{fileName}");
                if (response.IsSuccessStatusCode)
                {
                    IdFileName = null;
                    AdminValidationUploadUrl = null;
                    IdUploadStatus = null;
                }
                else
                {
                    IdUploadStatus = "Remove failed.";
                }
            }
            catch (Exception ex)
            {
                IdUploadStatus = $"Error: {ex.Message}";
            }
        }

        private async Task RemoveAuthLetterDocument()
        {
            if (string.IsNullOrEmpty(LogoUploadUrl)) return;
            try
            {
                var uri = new Uri(LogoUploadUrl);
                var fileName = uri.Segments[^1];
                var response = await Http.DeleteAsync($"/api/document/{fileName}");
                if (response.IsSuccessStatusCode)
                {
                    AuthLetterFileName = null;
                    LogoUploadUrl = null;
                    AuthLetterUploadStatus = null;
                }
                else
                {
                    AuthLetterUploadStatus = "Remove failed.";
                }
            }
            catch (Exception ex)
            {
                AuthLetterUploadStatus = $"Error: {ex.Message}";
            }
        }

        public class UploadResult { public string? url { get; set; } }

        // Existing logic for HandleValidSubmit and OnInitializedAsync
        private async Task HandleValidSubmit()
        {
            if (IsSubmitting) return;
            IsSubmitting = true;
            ProfileErrorMessage = "";
            try
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var userId = authState.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    ProfileErrorMessage = "User not found.";
                    IsSubmitting = false;
                    return;
                }

                var institutionProfile = await InstitutionProfileService.GetProfileByUserIdAsync(userId) ?? new c2_eskolar.Models.InstitutionProfile {
                    UserId = userId,
                    InstitutionName = verificationModel.InstitutionName,
                    AdminFirstName = verificationModel.AdminFirstName,
                    AdminLastName = verificationModel.AdminLastName
                };

                institutionProfile.InstitutionName = verificationModel.InstitutionName;
                institutionProfile.InstitutionType = verificationModel.InstitutionType;
                institutionProfile.Address = verificationModel.Address;
                institutionProfile.ContactNumber = verificationModel.ContactNumber;
                institutionProfile.ContactEmail = verificationModel.ContactEmail;
                institutionProfile.Website = verificationModel.Website;
                institutionProfile.Description = verificationModel.Description;
                institutionProfile.AdminFirstName = verificationModel.AdminFirstName;
                institutionProfile.AdminMiddleName = verificationModel.AdminMiddleName;
                institutionProfile.AdminLastName = verificationModel.AdminLastName;
                institutionProfile.AdminPosition = verificationModel.AdminPosition;
                institutionProfile.Accreditation = verificationModel.Accreditation;
                institutionProfile.Logo = LogoUploadUrl;
                institutionProfile.ProfilePicture = LogoUploadUrl;
                institutionProfile.AdminValidationDocument = AdminValidationUploadUrl;
                institutionProfile.VerificationStatus = "Pending";

                await InstitutionProfileService.SaveProfileAsync(institutionProfile);
                ProfileErrorMessage = "Verification submitted successfully!";
            }
            catch (Exception ex)
            {
                ProfileErrorMessage = $"Error submitting verification: {ex.Message}";
            }
            IsSubmitting = false;
        }

        protected override async Task OnInitializedAsync()
        {
            // Load existing institution profile data if available
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var profile = await InstitutionProfileService.GetProfileByUserIdAsync(userId);
                if (profile != null)
                {
                    // Pre-populate form with existing data
                    verificationModel.InstitutionName = profile.InstitutionName ?? string.Empty;
                    verificationModel.InstitutionType = profile.InstitutionType ?? string.Empty;
                    verificationModel.Address = profile.Address ?? string.Empty;
                    verificationModel.ContactNumber = profile.ContactNumber ?? string.Empty;
                    verificationModel.ContactEmail = profile.ContactEmail ?? string.Empty;
                    verificationModel.Website = profile.Website ?? string.Empty;
                    verificationModel.Description = profile.Description ?? string.Empty;
                    verificationModel.AdminFirstName = profile.AdminFirstName ?? string.Empty;
                    verificationModel.AdminMiddleName = profile.AdminMiddleName ?? string.Empty;
                    verificationModel.AdminLastName = profile.AdminLastName ?? string.Empty;
                    verificationModel.AdminPosition = profile.AdminPosition ?? string.Empty;
                    verificationModel.AdminEmail = profile.ContactEmail ?? string.Empty; // Use institution contact email as admin email default
                    verificationModel.Accreditation = profile.Accreditation ?? string.Empty;
                    
                    // Set upload URLs if documents exist
                    AdminValidationUploadUrl = profile.AdminValidationDocument;
                    LogoUploadUrl = profile.ProfilePicture;
                }
            }
        }
    }
}
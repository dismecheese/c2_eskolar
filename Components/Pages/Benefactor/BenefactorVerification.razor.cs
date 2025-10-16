using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;

namespace c2_eskolar.Components.Pages.Benefactor
{
    public partial class BenefactorVerification : ComponentBase
    {
        public class BenefactorVerificationModel
        {
            [Required(ErrorMessage = "Organization/Company name is required.")]
            public string InstitutionName { get; set; } = string.Empty;

            public string? InstitutionType { get; set; } = string.Empty;

            [Required(ErrorMessage = "Address is required.")]
            public string Address { get; set; } = string.Empty;

            public string? Website { get; set; } = string.Empty;

            public string? Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Organization contact number is required.")]
            public string ContactNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contact email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string ContactEmail { get; set; } = string.Empty;

            // Personal fields (for the benefactor representative)
            [Required(ErrorMessage = "First name is required.")]
            public string AdminFirstName { get; set; } = string.Empty;

            public string? AdminMiddleName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Last name is required.")]
            public string AdminLastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Sex is required.")]
            public string Sex { get; set; } = string.Empty;

            [Required(ErrorMessage = "Birthdate is required.")]
            public DateTime? BirthDate { get; set; }

            [Required(ErrorMessage = "Nationality is required.")]
            public string Nationality { get; set; } = string.Empty;

            public string? AdminPosition { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string AdminEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contact number is required.")]
            public string AdminContactNumber { get; set; } = string.Empty;

            public string? Accreditation { get; set; } = string.Empty;

            // Contact person fields - required for benefactors
            [Required(ErrorMessage = "Contact person name is required.")]
            public string DeanName { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Contact person email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string DeanEmail { get; set; } = string.Empty;
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

        private BenefactorVerificationModel verificationModel = new BenefactorVerificationModel();
        private bool IsSubmitting = false;
        private string ProfileErrorMessage = "";
        protected bool ShowAlreadySubmittedModal = false;
        protected bool HasAlreadySubmitted = false;
        protected string CurrentAccountStatus = "";

        // Sex Dropdown logic for custom select (component-level)
        private bool ShowSexDropdown = false;
        private string SelectedSex
        {
            get => verificationModel.Sex;
            set => verificationModel.Sex = value;
        }
        private string SelectedSexText => string.IsNullOrEmpty(SelectedSex) ? "Select sex" : SelectedSex;
        private void ToggleSexDropdown() => ShowSexDropdown = !ShowSexDropdown;
        private void CloseSexDropdown() => ShowSexDropdown = false;
        private void SelectSex(string value)
        {
            SelectedSex = value;
            ShowSexDropdown = false;
        }

        // Organization Type Dropdown logic for custom select (component-level)
        private bool ShowOrganizationTypeDropdown = false;
        private string SelectedOrganizationType
        {
            get => verificationModel.InstitutionType ?? "";
            set => verificationModel.InstitutionType = value;
        }
        private string SelectedOrganizationTypeText => string.IsNullOrEmpty(SelectedOrganizationType) ? "Select type" : SelectedOrganizationType;
        private void ToggleOrganizationTypeDropdown() => ShowOrganizationTypeDropdown = !ShowOrganizationTypeDropdown;
        private void CloseOrganizationTypeDropdown() => ShowOrganizationTypeDropdown = false;
        private void SelectOrganizationType(string value)
        {
            SelectedOrganizationType = value;
            ShowOrganizationTypeDropdown = false;
        }

        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private Services.BenefactorProfileService BenefactorProfileService { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private Services.DocumentIntelligenceService DocumentIntelligenceService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        // File upload and removal methods for Document Upload section
        private async Task OnIdFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null)
            {
                IdUploadStatus = "No file selected.";
                StateHasChanged();
                return;
            }
            // Validate file type and size before upload
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExt = System.IO.Path.GetExtension(file.Name).ToLowerInvariant();
            if (!validExtensions.Contains(fileExt))
            {
                IdUploadStatus = $"Invalid file type: {fileExt}. Allowed: {string.Join(", ", validExtensions)}";
                StateHasChanged();
                return;
            }
            if (file.Size > 5 * 1024 * 1024)
            {
                IdUploadStatus = "File too large. Max 5MB.";
                StateHasChanged();
                return;
            }
            IdFileName = file.Name;
            IdUploadStatus = "Uploading...";
            StateHasChanged();
            try
            {
                // Step 1: Upload the file
                var content = new MultipartFormDataContent();
                var stream = file.OpenReadStream(5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "file", file.Name);
                content.Add(new StringContent("IdDocument"), "docType");
                var response = await Http.PostAsync("/api/document/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    AdminValidationUploadUrl = result?.url;
                    IdUploadStatus = "Processing with AI...";
                    StateHasChanged();
                    // Step 2: Analyze the document with Document Intelligence
                    try
                    {
                        var extractedData = await DocumentIntelligenceService.AnalyzeBenefactorIdDocumentAsync(file);
                        if (extractedData != null)
                        {
                            PrepopulateFromIdDocument(extractedData);
                            IdUploadStatus = "Uploaded & Data Extracted!";
                        }
                        else
                        {
                            IdUploadStatus = "Uploaded! (AI extraction failed). Please check file clarity and type.";
                        }
                    }
                    catch (Exception ex)
                    {
                        IdUploadStatus = $"AI extraction error: {ex.Message}";
                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    IdUploadStatus = $"Upload failed: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                IdUploadStatus = $"Error: {ex.Message}";
            }
            StateHasChanged();
        }

        private async Task OnAuthLetterFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file == null)
            {
                AuthLetterUploadStatus = "No file selected.";
                StateHasChanged();
                return;
            }
            // Validate file type and size before upload
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExt = System.IO.Path.GetExtension(file.Name).ToLowerInvariant();
            if (!validExtensions.Contains(fileExt))
            {
                AuthLetterUploadStatus = $"Invalid file type: {fileExt}. Allowed: {string.Join(", ", validExtensions)}";
                StateHasChanged();
                return;
            }
            if (file.Size > 5 * 1024 * 1024)
            {
                AuthLetterUploadStatus = "File too large. Max 5MB.";
                StateHasChanged();
                return;
            }
            AuthLetterFileName = file.Name;
            AuthLetterUploadStatus = "Uploading...";
            StateHasChanged();
            try
            {
                // Step 1: Upload the file
                var content = new MultipartFormDataContent();
                var stream = file.OpenReadStream(5 * 1024 * 1024);
                content.Add(new StreamContent(stream), "file", file.Name);
                content.Add(new StringContent("OrgDocument"), "docType");
                var response = await Http.PostAsync("/api/document/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    LogoUploadUrl = result?.url;
                    AuthLetterUploadStatus = "Processing with AI...";
                    StateHasChanged();
                    // Step 2: Analyze the document with Document Intelligence
                    try
                    {
                        var extractedData = await DocumentIntelligenceService.AnalyzeBenefactorAuthLetterAsync(file);
                        if (extractedData != null)
                        {
                            PrepopulateFromBenefactorAuthLetter(extractedData);
                            AuthLetterUploadStatus = "Uploaded & Data Extracted!";
                        }
                        else
                        {
                            AuthLetterUploadStatus = "Uploaded! (AI extraction failed). Document may not contain clear organization information or may be in an unsupported format. Please verify file is an official authorization letter with clear text.";
                        }
                    }
                    catch (Exception ex)
                    {
                        AuthLetterUploadStatus = $"AI extraction error: {ex.Message}. Please check that the document is clear and contains organization information.";
                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    AuthLetterUploadStatus = $"Upload failed: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                AuthLetterUploadStatus = $"Error: {ex.Message}";
            }
            StateHasChanged();
        }

        private void PrepopulateFromIdDocument(Services.ExtractedBenefactorIdData extractedData)
        {
            // Prepopulate personal information fields
            if (!string.IsNullOrEmpty(extractedData.AdminFirstName))
                verificationModel.AdminFirstName = extractedData.AdminFirstName;
            if (!string.IsNullOrEmpty(extractedData.AdminMiddleName))
                verificationModel.AdminMiddleName = extractedData.AdminMiddleName;
            if (!string.IsNullOrEmpty(extractedData.AdminLastName))
                verificationModel.AdminLastName = extractedData.AdminLastName;
            if (!string.IsNullOrEmpty(extractedData.AdminEmail))
                verificationModel.AdminEmail = extractedData.AdminEmail;
            if (!string.IsNullOrEmpty(extractedData.AdminContactNumber))
                verificationModel.AdminContactNumber = extractedData.AdminContactNumber;
            if (!string.IsNullOrEmpty(extractedData.AdminPosition))
                verificationModel.AdminPosition = extractedData.AdminPosition;
            
            // Prepopulate additional benefactor-specific fields
            if (!string.IsNullOrEmpty(extractedData.Sex))
                verificationModel.Sex = extractedData.Sex;
            if (extractedData.DateOfBirth.HasValue)
                verificationModel.BirthDate = extractedData.DateOfBirth;
            if (!string.IsNullOrEmpty(extractedData.Nationality))
                verificationModel.Nationality = extractedData.Nationality;
                
            // Show success message
            ProfileErrorMessage = "Personal information has been automatically filled from your ID document. Please review and complete any missing fields.";
            StateHasChanged();
        }

        private void PrepopulateFromOrgDocument(Services.ExtractedInstitutionAuthLetterData extractedData)
        {
            // Prepopulate organization information fields
            if (!string.IsNullOrEmpty(extractedData.InstitutionName))
                verificationModel.InstitutionName = extractedData.InstitutionName;
            if (!string.IsNullOrEmpty(extractedData.InstitutionType))
                verificationModel.InstitutionType = extractedData.InstitutionType;
            if (!string.IsNullOrEmpty(extractedData.Address))
                verificationModel.Address = extractedData.Address;
            if (!string.IsNullOrEmpty(extractedData.ContactNumber))
                verificationModel.ContactNumber = extractedData.ContactNumber;
            if (!string.IsNullOrEmpty(extractedData.Website))
                verificationModel.Website = extractedData.Website;
            if (!string.IsNullOrEmpty(extractedData.Description))
                verificationModel.Description = extractedData.Description;
            if (!string.IsNullOrEmpty(extractedData.DeanName))
                verificationModel.DeanName = extractedData.DeanName;
            if (!string.IsNullOrEmpty(extractedData.DeanEmail))
                verificationModel.DeanEmail = extractedData.DeanEmail;
            // Show success message
            ProfileErrorMessage = "Organization information has been automatically filled from your document. Please review and complete any missing fields.";
            StateHasChanged();
        }

        private void PrepopulateFromBenefactorAuthLetter(Services.ExtractedBenefactorAuthLetterData extractedData)
        {
            // Prepopulate organization information fields for benefactor
            if (!string.IsNullOrEmpty(extractedData.OrganizationName))
                verificationModel.InstitutionName = extractedData.OrganizationName;
            if (!string.IsNullOrEmpty(extractedData.OrganizationType))
                verificationModel.InstitutionType = extractedData.OrganizationType;
            if (!string.IsNullOrEmpty(extractedData.Address))
                verificationModel.Address = extractedData.Address;
            if (!string.IsNullOrEmpty(extractedData.ContactNumber))
                verificationModel.ContactNumber = extractedData.ContactNumber;
            if (!string.IsNullOrEmpty(extractedData.Website))
                verificationModel.Website = extractedData.Website;
            if (!string.IsNullOrEmpty(extractedData.OfficialEmailDomain))
                verificationModel.Description = extractedData.OfficialEmailDomain; // Maps to "Official Email Domain" field
            if (!string.IsNullOrEmpty(extractedData.AuthorizedRepresentativeName))
                verificationModel.DeanName = extractedData.AuthorizedRepresentativeName;
            if (!string.IsNullOrEmpty(extractedData.AuthorizedRepresentativeEmail))
                verificationModel.DeanEmail = extractedData.AuthorizedRepresentativeEmail;
            
            // Also try to set the organization contact email if we have the representative email
            if (!string.IsNullOrEmpty(extractedData.AuthorizedRepresentativeEmail))
                verificationModel.ContactEmail = extractedData.AuthorizedRepresentativeEmail;
                
            // Show success message
            ProfileErrorMessage = "Organization information has been automatically filled from your authorization letter. Please review and complete any missing fields.";
            StateHasChanged();
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

        // Handle form submission for benefactor verification
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

                var benefactorProfile = await BenefactorProfileService.GetProfileByUserIdAsync(userId) ?? new c2_eskolar.Models.BenefactorProfile {
                    UserId = userId,
                    AdminFirstName = verificationModel.AdminFirstName,
                    AdminLastName = verificationModel.AdminLastName,
                    OrganizationName = verificationModel.InstitutionName
                };

                // Map verification model to benefactor profile
                benefactorProfile.OrganizationName = verificationModel.InstitutionName;
                benefactorProfile.OrganizationType = verificationModel.InstitutionType;
                benefactorProfile.Address = verificationModel.Address;
                benefactorProfile.ContactNumber = verificationModel.ContactNumber;
                benefactorProfile.ContactEmail = verificationModel.ContactEmail;
                benefactorProfile.Website = verificationModel.Website;
                benefactorProfile.AdminFirstName = verificationModel.AdminFirstName;
                benefactorProfile.AdminMiddleName = verificationModel.AdminMiddleName;
                benefactorProfile.AdminLastName = verificationModel.AdminLastName;
                benefactorProfile.AdminPosition = verificationModel.AdminPosition;
                benefactorProfile.Sex = verificationModel.Sex;
                benefactorProfile.Nationality = verificationModel.Nationality;
                benefactorProfile.BirthDate = verificationModel.BirthDate;
                benefactorProfile.Description = verificationModel.Description;
                benefactorProfile.Logo = LogoUploadUrl;
                benefactorProfile.AccountStatus = "Pending";
                benefactorProfile.AccountStatus = "Pending";

                await BenefactorProfileService.SaveProfileAsync(benefactorProfile);
                ShowSuccessModal = true;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ProfileErrorMessage = $"Error submitting verification: {ex.Message}";
            }
            IsSubmitting = false;
        }

        protected override async Task OnInitializedAsync()
        {
            // Load existing benefactor profile data if available
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var profile = await BenefactorProfileService.GetProfileByUserIdAsync(userId);
                if (profile != null)
                {
                    // Check account status to determine if user has already submitted
                    CurrentAccountStatus = profile.AccountStatus ?? "";
                    
                    // Check if user has already submitted (any status except null/empty)
                    if (!string.IsNullOrEmpty(profile.AccountStatus))
                    {
                        HasAlreadySubmitted = true;
                        ShowAlreadySubmittedModal = true;
                    }
                    
                    // Pre-populate form with existing data
                    verificationModel.InstitutionName = profile.OrganizationName ?? string.Empty;
                    verificationModel.InstitutionType = profile.OrganizationType ?? string.Empty;
                    verificationModel.Address = profile.Address ?? string.Empty;
                    verificationModel.ContactNumber = profile.ContactNumber ?? string.Empty;
                    verificationModel.ContactEmail = profile.ContactEmail ?? string.Empty;
                    verificationModel.Website = profile.Website ?? string.Empty;
                    verificationModel.AdminFirstName = profile.AdminFirstName ?? string.Empty;
                    verificationModel.AdminMiddleName = profile.AdminMiddleName ?? string.Empty;
                    verificationModel.AdminLastName = profile.AdminLastName ?? string.Empty;
                    verificationModel.AdminPosition = profile.AdminPosition ?? string.Empty;
                    verificationModel.Sex = profile.Sex ?? string.Empty;
                    verificationModel.Nationality = profile.Nationality ?? string.Empty;
                    verificationModel.BirthDate = profile.BirthDate;
                    verificationModel.Description = profile.Description ?? string.Empty;
                    
                    // Set upload URLs if documents exist
                    LogoUploadUrl = profile.Logo;
                }
            }
        }

        // Modal logic for success modal
        public bool ShowSuccessModal = false;
        
        public void CloseSuccessModal()
        {
            ShowSuccessModal = false;
            InvokeAsync(StateHasChanged);
        }
        
        public void ViewProfile()
        {
            ShowSuccessModal = false;
            Navigation.NavigateTo("/dashboard/benefactor/profile/unverified");
        }

        // Modal logic for already submitted modal
        public void CloseAlreadySubmittedModal()
        {
            ShowAlreadySubmittedModal = false;
            InvokeAsync(StateHasChanged);
        }
        
        public void GoToDashboard()
        {
            ShowAlreadySubmittedModal = false;
            Navigation.NavigateTo("/dashboard/benefactor");
        }
        
        public void ViewBenefactorProfile()
        {
            ShowAlreadySubmittedModal = false;
            Navigation.NavigateTo("/benefactor/profile");
        }
    }
}
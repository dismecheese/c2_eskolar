---
applyTo: '**'
---

# User Memory

## User Preferences
- Programming languages: C#, ASP.NET Core, Blazor
- Code style preferences: Professional, service-based, clear naming
- Development environment: Windows, VS Code, PowerShell
- Communication style: Concise, professional, clear explanations

## Project Context
- Current project type: Blazor web app
- Tech stack: .NET 9.0, Azure SQL, Azure Blob Storage
- Architecture patterns: Service-based, DI, clean separation
- Key requirements: Scalable file storage, clear config naming, separate containers for documents and photos
- Institution verification modal now only requires AdminValidationDocument (ID/proof of employment); business permit and accreditation certificate uploads removed; validation enforced for admin document and logo/profile picture.

## Coding Patterns
- Preferred patterns and practices: Service-based, DI, clear config
- Code organization preferences: Services in Services/, config in appsettings.json
- Testing approaches: Not specified
- Documentation style: XML comments, clear summaries

## Context7 Research History
- Azure Blob Storage best practices: Use separate containers for different file types when needed
- Implementation patterns: Use 'documents' and 'photos' containers for clarity and access control
- Version-specific findings: N/A

## Conversation History
- Container was originally 'documents', then 'files', now split into 'documents' and 'photos' containers
- BlobStorageService refactored to support both containers with clear methods
- Institution verification modal updated: only AdminValidationDocument required, business permit/accreditation certificate removed, model and Razor mapping updated, validation enforced.
- Institution verification modal successfully moved to InstitutionDashLayout.razor for global enforcement, following StudentDashboard.razor pattern.
- Fixed RZ9999 EditForm ChildContent context ambiguity by adding Context="modalFormContext" to nested EditForm in InstitutionVerification.razor.
- Modal now automatically appears for non-verified institutions on all dashboard pages with proper verification status checking.

## Notes
- Use UploadDocumentAsync/UploadPhotoAsync and DownloadDocumentAsync/DownloadPhotoAsync for correct container
- If more file types are added, extend config and service accordingly
- InstitutionProfile now has AdminValidationDocument property for admin document URL
- Institution verification modal is globally enforced through layout, similar to student verification pattern
- Modal includes "Skip for Now" and "Verify Institution" buttons with proper navigation to /institution/verification

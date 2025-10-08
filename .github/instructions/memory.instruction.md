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
- Current project type: Blazor web app with enhanced scholarship management
- Tech stack: .NET 9.0, Azure SQL, Azure Blob Storage, Azure OpenAI Service (v2.3.0-beta.2)
- Architecture patterns: Service-based, DI, DbContextFactory for Blazor Server, clean separation
- Key requirements: Scalable file storage, external URL scraping, admin dashboard, approval workflows, Azure AI token tracking
- Azure AI Integration: GPT-3.5-turbo and GPT-4 variants with comprehensive usage monitoring and cost calculation
- Region: eastus2 for Azure OpenAI deployment
- Institution verification modal now only requires AdminValidationDocument (ID/proof of employment); business permit and accreditation certificate uploads removed; validation enforced for admin document and logo/profile picture.

## Recent Major Accomplishments
### Azure AI Token Tracking System ✅ NEW
- Successfully implemented comprehensive Azure OpenAI token usage tracking with real cost calculation
- Enhanced AITokenUsage model with Azure-specific fields (DeploymentName, Region, RequestDurationMs)
- Created AITokenTrackingService with complete Azure pricing matrix and reflection-based SDK compatibility
- Modified OpenAIService to automatically track all API calls with detailed timing and usage data
- Applied database migration "AddAzureFieldsToAITokenUsage" successfully
- Updated SuperAdminDashboard to display real token usage data instead of estimates
- Resolved Azure OpenAI SDK compatibility issues using reflection-based property discovery
- System provides accurate cost monitoring for business intelligence and budget tracking

### External URL Enhancement System ✅
- Successfully implemented comprehensive external URL scraping with AI-powered content analysis
- Enhanced data merging with similarity detection algorithms
- Tested successfully with 26 scholarships at 81% confidence from PUP website

### Super Admin Dashboard Implementation ✅
- Created comprehensive ScholarshipManagement.razor dashboard with EskoBot Intelligence branding
- Implemented professional UI with advanced filtering, sorting, pagination, and responsive design
- Applied database migrations for scraped scholarship tables with proper relationships
- Fixed DbContextFactory pattern implementation for Blazor Server thread safety
- Service successfully registered and application running without errors

## Current Dashboard Features
- External URL scraping with AI enhancement and intelligent content extraction
- Professional admin interface with comprehensive filtering and search capabilities
- Approval workflow system with status tracking and bulk operations
- Statistics cards with real-time data visualization
- Publishing pipeline to main scholarship system with EskoBot Intelligence attribution
- Responsive Bootstrap design with custom gradient styling

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
- ScrapedScholarshipService implements DbContextFactory pattern using "using var context = await _contextFactory.CreateDbContextAsync()" for thread safety
- Super Admin Dashboard accessible at /admin/scholarship-management with EskoBot Intelligence branding
- External URL scraping system fully operational with AI-enhanced content analysis
- ScholarshipManagement.razor now uses SuperAdminDashLayout instead of default layout for consistent admin interface
- Navigation between SuperAdmin pages working seamlessly with active state highlighting

## Current Architectural Restructuring (October 9, 2025) ✅ COMPLETED
- **User Request**: Separate scraped scholarship approval workflow from published scholarship management
- **WebScrapingManagement.razor**: Added new "Scholarship Approval" tab (tab 3) with complete approval workflow transferred from ScholarshipManagement
  - Comprehensive approval workflow with statistics, filtering, pagination, and approval actions
  - Uses ScrapedScholarshipService for approval operations
  - Maintains existing scraping functionality in other tabs
- **ScholarshipManagement.razor**: Completely restructured to manage approved/posted scholarships visible to students
  - Changed from ScrapedScholarship entity to actual Scholarship entity with Institution/Benefactor relationships
  - Updated service injection from IScrapedScholarshipService to IDbContextFactory<ApplicationDbContext>
  - Transformed statistics, filtering, and table display for published scholarship context
  - Fixed BenefactorProfile property reference (OrganizationName instead of BenefactorName)
  - Fixed CSS @media query in Razor (required @@media instead of @media)
- **Design Preservation**: Successfully preserved excellent UI design while changing underlying data models and functionality
- **Service Separation**: Clear architectural separation achieved - WebScrapingManagement for scraped data approval, ScholarshipManagement for published scholarships
- **Build Status**: Project builds successfully with only minor warnings from other unrelated files
- **Data Flow**: Web scraping → approval workflow (WebScrapingManagement tab 3) → published scholarships (ScholarshipManagement) → student visibility

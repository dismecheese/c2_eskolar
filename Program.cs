using System;
using c2_eskolar.Components;
using c2_eskolar.Data;
using c2_eskolar.Services; // Add this import
using c2_eskolar.Services.AI; // Add this import for AI services
using c2_eskolar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // Add this for [FromForm]
using Microsoft.EntityFrameworkCore;
using BlazorBootstrap;
using Blazored.LocalStorage;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddBlazorBootstrap();
    builder.Services.AddBlazoredLocalStorage();

    // Add Controllers for API endpoints
    builder.Services.AddControllers();

// Register IDbContextFactory for ApplicationDbContext in DI container for Blazor Server concurrency safety
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    });
});

    // Add ASP.NET Core Identity
    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            // Password requirements
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Email confirmation (disable for development)
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole>() // Add role support
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

    // Add authorization services
    builder.Services.AddAuthorization();


    // Register AuthService
    builder.Services.AddScoped<AuthService>();

    // Register HttpClient for DI in Blazor Server and API Controllers
    builder.Services.AddHttpClient();
    
    // Register a named HttpClient for Document Intelligence without NavigationManager dependency
    builder.Services.AddHttpClient("DocumentIntelligence");
    
    // Register a default HttpClient with NavigationManager.BaseUri for Blazor components
    builder.Services.AddScoped<HttpClient>(sp =>
    {
        try 
        {
            var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
            return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
        }
        catch
        {
            // Fallback for API controllers where NavigationManager is not available
            return new HttpClient();
        }
    });


// Register custom services
builder.Services.AddScoped<PartnerService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<AnnouncementSeedService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<StudentProfileService>();
builder.Services.AddScoped<BenefactorProfileService>();
builder.Services.AddScoped<InstitutionProfileService>();
builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<BookmarkService>();


// Register web scraping services
builder.Services.AddScoped<c2_eskolar.Services.WebScraping.IWebScrapingService, c2_eskolar.Services.WebScraping.WebScrapingService>();
builder.Services.AddScoped<c2_eskolar.Services.WebScraping.IEnhancedWebScrapingService, c2_eskolar.Services.WebScraping.EnhancedWebScrapingService>();
builder.Services.AddHostedService<c2_eskolar.Services.WebScraping.ScrapingBackgroundService>();
builder.Services.Configure<c2_eskolar.Services.WebScraping.ScrapingConfiguration>(
    builder.Configuration.GetSection("WebScraping"));

// DEPRECATED: Old scraped scholarship service - now using IEnhancedWebScrapingService
// Register ScrapedScholarshipService - Enhanced AI-powered scholarship management
builder.Services.AddScoped<IScrapedScholarshipService, ScrapedScholarshipService>();

// Register DocumentIntelligenceService with named HttpClient
builder.Services.AddScoped<DocumentIntelligenceService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("DocumentIntelligence");
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<DocumentIntelligenceService>>();
    var openAIService = sp.GetRequiredService<OpenAIService>();
    return new DocumentIntelligenceService(httpClient, config, logger, openAIService);
});
builder.Services.AddScoped<VerificationDocumentService>();

// Register AI Services
builder.Services.AddScoped<ProfileSummaryService>();
builder.Services.AddScoped<ScholarshipRecommendationService>();
builder.Services.AddScoped<AnnouncementRecommendationService>();
builder.Services.AddScoped<QueryClassificationService>();
builder.Services.AddScoped<StudentContextService>();
builder.Services.AddScoped<BenefactorContextService>();
builder.Services.AddScoped<InstitutionContextService>();
builder.Services.AddScoped<ContextGenerationService>();
builder.Services.AddScoped<DisplayContextAwarenessService>();
builder.Services.AddScoped<ChatbotMessageFormattingService>();
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<AITokenTrackingService>();
builder.Services.AddScoped<SuperAdminAnalyticsService>();
builder.Services.AddScoped<MonthlyStatisticsService>();

// Register background services
builder.Services.AddHostedService<c2_eskolar.BackgroundServices.MonthlyAggregationBackgroundService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
        app.UseHttpsRedirection(); // Only use HTTPS redirection in production
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
    }
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapControllers();
    app.MapRazorComponents<c2_eskolar.Components.App>()
        .AddInteractiveServerRenderMode();

    // Move seeding logic to async method and await before app.Run
    async Task SeedDataAsync(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roles = { "Student", "Benefactor", "Institution", "SuperAdmin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Created role: {role}");
                }
            }
            // Create SuperAdmin user if not exists
            var superAdminEmail = "super@gmail.com";
            var existingSuperAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (existingSuperAdmin == null)
            {
                var superAdminUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(superAdminUser, "@Super123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                    Console.WriteLine($"✅ Created SuperAdmin: {superAdminEmail} / @Super123");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            var testEmail = "student@test.com";
            var existingUser = await userManager.FindByEmailAsync(testEmail);
            if (existingUser == null)
            {
                var testUser = new IdentityUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(testUser, "Student123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "Student");
                    Console.WriteLine($"✅ Created test user: {testEmail} / Student123!");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            var benefactorEmail = "benefactor@test.com";
            var existingBenefactor = await userManager.FindByEmailAsync(benefactorEmail);
            if (existingBenefactor == null)
            {
                var benefactorUser = new IdentityUser
                {
                    UserName = benefactorEmail,
                    Email = benefactorEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(benefactorUser, "Benefactor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(benefactorUser, "Benefactor");
                    Console.WriteLine($"✅ Created test benefactor: {benefactorEmail} / Benefactor123!");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create test benefactor: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            var institutionEmail = "institution@test.com";
            var existingInstitution = await userManager.FindByEmailAsync(institutionEmail);
            if (existingInstitution == null)
            {
                var institutionUser = new IdentityUser
                {
                    UserName = institutionEmail,
                    Email = institutionEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(institutionUser, "Institution123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(institutionUser, "Institution");
                    Console.WriteLine($"✅ Created test institution: {institutionEmail} / Institution123!");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create test institution: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            var seedService = scope.ServiceProvider.GetRequiredService<AnnouncementSeedService>();
            await seedService.SeedSampleAnnouncementsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Startup error: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    await SeedDataAsync(app);
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unhandled exception in Main: {ex.Message}\n{ex.StackTrace}");
    throw;
}
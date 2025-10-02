using System;
using c2_eskolar.Components;
using c2_eskolar.Data;
using c2_eskolar.Services; // Add this import
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

// Add Entity Framework with SQL Server
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IDbContextFactory for ApplicationDbContext in DI container for Blazor Server concurrency safety
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

    // Register HttpClient for DI in Blazor Server
    builder.Services.AddHttpClient();
    // Register a default HttpClient with NavigationManager.BaseUri for direct injection
    builder.Services.AddScoped<HttpClient>(sp =>
    {
        var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
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
builder.Services.AddScoped<VerificationDocumentService>();
builder.Services.AddScoped<ProfileSummaryService>();
builder.Services.AddScoped<OpenAIService>();

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
            string[] roles = { "Student", "Benefactor", "Institution" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Created role: {role}");
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
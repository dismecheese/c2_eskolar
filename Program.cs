using c2_eskolar.Components;
using c2_eskolar.Data;
using c2_eskolar.Services; // Add this import
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // Add this for [FromForm]
using Microsoft.EntityFrameworkCore;
using BlazorBootstrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

// Add Controllers for API endpoints
builder.Services.AddControllers();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
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

// ADD THIS LINE: Register AuthService
builder.Services.AddScoped<AuthService>();

// Register custom services
builder.Services.AddScoped<PartnerService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<AnnouncementSeedService>();

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

// ADD THESE IN THE CORRECT ORDER:
app.UseRouting();
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers(); // Add this for API controllers
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// UPDATED SECTION: Seed roles and test users
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create roles
    string[] roles = { "Student", "Benefactor", "Institution" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Created role: {role}");
        }
    }

    // Create a test student user
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

    // Create a test benefactor user
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

    // Create a test institution user
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
    
    // Seed sample announcements
    var seedService = scope.ServiceProvider.GetRequiredService<AnnouncementSeedService>();
    await seedService.SeedSampleAnnouncementsAsync();
}

app.Run();
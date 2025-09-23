using c2_eskolar.Components;
using c2_eskolar.Data;
using c2_eskolar.Models;
using c2_eskolar.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBootstrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity (use ApplicationUser)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// Custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PartnerService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<AnnouncementSeedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ✅ Seed roles and test users
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Student", "Benefactor", "Institution" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Created role: {role}");
        }
    }

    // Create Student
    var studentEmail = "student@test.com";
    if (await userManager.FindByEmailAsync(studentEmail) == null)
    {
        var student = new ApplicationUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(student, "Student123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(student, "Student");
            Console.WriteLine($"✅ Created test student: {studentEmail} / Student123!");
        }
    }

    // Create Benefactor
    var benefactorEmail = "benefactor@test.com";
    if (await userManager.FindByEmailAsync(benefactorEmail) == null)
    {
        var benefactor = new ApplicationUser
        {
            UserName = benefactorEmail,
            Email = benefactorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(benefactor, "Benefactor123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(benefactor, "Benefactor");
            Console.WriteLine($"✅ Created test benefactor: {benefactorEmail} / Benefactor123!");
        }
    }

    // Create Institution
    var institutionEmail = "institution@test.com";
    if (await userManager.FindByEmailAsync(institutionEmail) == null)
    {
        var institution = new ApplicationUser
        {
            UserName = institutionEmail,
            Email = institutionEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(institution, "Institution123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(institution, "Institution");
            Console.WriteLine($"✅ Created test institution: {institutionEmail} / Institution123!");
        }
    }

    // Seed announcements
    var seedService = scope.ServiceProvider.GetRequiredService<AnnouncementSeedService>();
    await seedService.SeedSampleAnnouncementsAsync();
}

app.Run();

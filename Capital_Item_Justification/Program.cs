
using Capital_Item_Justification.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Repository;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.Services;
using PdfSharp.Fonts;
using Capital_Item_Justification.PDF;
using Serilog;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERILOG CONFIGURATION
// =====================================================

//var logDirectory = @"E:\CIJ_Software\CIJ_Log";
var logDirectory = builder.Configuration["Serilog:LogDirectory"]
    ?? @"E:\CIJ_Software\CIJ_Log";

Directory.CreateDirectory(logDirectory);

var retainedFileCountLimit =
    builder.Configuration.GetValue<int?>("Serilog:RetainedFileCountLimit") ?? 14;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Information()
    .MinimumLevel.Override(
        "Microsoft",
        Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override(
        "Microsoft.AspNetCore",
        Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override(
        "Microsoft.EntityFrameworkCore",
        Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        Path.Combine(logDirectory, "cij-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedFileCountLimit,
        shared: true,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// =====================================================
// PDFSHARP FONT RESOLVER
// =====================================================

GlobalFontSettings.FontResolver = new PdfSharpFontResolver();

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(
//        new DirectoryInfo(
//            @"E:\CIJ_Software\CIJ_DataProtectionKeys"))
//    .SetApplicationName("CIJ");

// =====================================================
// DATABASE CONFIGURATION
// =====================================================

builder.Services.AddDbContext<CIJDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CIJConnection")));

// =====================================================
// ASP.NET CORE IDENTITY CONFIGURATION
// =====================================================

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Password settings
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CIJDbContext>()
.AddDefaultTokenProviders();

// =====================================================
// AUTHENTICATION COOKIE CONFIGURATION
// =====================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.Cookie.Name = "CIJ.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Production should use HTTPS.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// =====================================================
// SESSION CONFIGURATION
// =====================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.Name = ".CIJ.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// =====================================================
// MVC CONFIGURATION
// =====================================================

builder.Services.AddControllersWithViews();

// =====================================================
// DEPENDENCY INJECTION - SERVICES AND REPOSITORIES
// =====================================================

builder.Services.AddScoped<ICIJRequestService, CIJRequestService>();
builder.Services.AddScoped<ICIJMainRepository, CIJMainRepository>();

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IDeptService, DeptService>();
builder.Services.AddScoped<IDeptRepository, DeptRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

builder.Services.AddScoped<ICIJPdfService, CIJPdfService>();

builder.Services.AddScoped<
    IFunctionHeadDepartmentService,
    FunctionHeadDepartmentService>();

builder.Services.AddScoped<
    IFunctionHeadDepartmentRepository,
    FunctionHeadDepartmentRepository>();

// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();

// =====================================================
// STARTUP LOGGING
// =====================================================

app.Logger.LogInformation(
    "CIJ application started. Environment={Environment}",
    app.Environment.EnvironmentName);

// Log HTTP request information, including status codes and elapsed time.
app.UseSerilogRequestLogging();

// =====================================================
// HTTP REQUEST PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// =====================================================
// ROUTE CONFIGURATION
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CIJ}/{action=Dashboard}/{id?}");

// =====================================================
// DATABASE ROLE SEEDING
// =====================================================

try
{
    using var scope = app.Services.CreateScope();

    var roleManager =
        scope.ServiceProvider.GetRequiredService<
            RoleManager<ApplicationRole>>();

    await DbSeeder.SeedRoles(roleManager);
}
catch (Exception ex)
{
    app.Logger.LogError(
        ex,
        "An error occurred while seeding CIJ roles.");

    throw;
}

// =====================================================
// RUN APPLICATION
// =====================================================

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CIJ application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

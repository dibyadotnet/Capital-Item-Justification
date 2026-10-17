using Capital_Item_Justification.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;
using Capital_Item_Justification.Models;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Repository;
using Capital_Item_Justification.Services.Interfaces;
using Capital_Item_Justification.Services;
using PdfSharp.Fonts;
using Capital_Item_Justification.PDF;

var builder = WebApplication.CreateBuilder(args);

// PDFsharp font resolver
GlobalFontSettings.FontResolver = new PdfSharpFontResolver();

builder.Services.AddDbContext<CIJDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CIJConnection")));

//builder.Services.AddDefaultIdentity<IdentityUser>(options =>
//{
//    options.SignIn.RequireConfirmedAccount = false;

//    options.Password.RequireDigit = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireLowercase = false;
//    options.Password.RequireNonAlphanumeric = false;
//}).AddRoles<IdentityRole>().AddEntityFrameworkStores<CIJDbContext>();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Password
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;

        // Lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User
        options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<CIJDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;

    options.Cookie.Name = "CIJ.Auth";
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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
builder.Services.AddScoped<IFunctionHeadDepartmentService, FunctionHeadDepartmentService>();
builder.Services.AddScoped<IFunctionHeadDepartmentRepository, FunctionHeadDepartmentRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CIJ}/{action=Dashboard}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();


    await DbSeeder.SeedRoles(roleManager);
    //await DbSeeder.SeedAdmin(userManager);
}

app.Run();

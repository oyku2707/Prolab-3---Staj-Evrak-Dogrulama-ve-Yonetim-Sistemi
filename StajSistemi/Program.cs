using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Services;
using StajSistemi.Services.Dataset;
using StajSistemi.Domain.Interfaces;
using StajSistemi.Validation.Rules;
using StajSistemi.Services.Extractors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IDatasetReader, CsvDatasetReader>();
builder.Services.AddScoped<DatasetImportService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(4);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

 });   
    // Doğrulama kuralları 
    builder.Services.AddScoped<IValidationRule, TcKimlikRule>();
    builder.Services.AddScoped<IValidationRule, PhoneRule>();
    builder.Services.AddScoped<IValidationRule, EmailRule>();
    builder.Services.AddScoped<IValidationRule, EngineerCountRule>();
    builder.Services.AddScoped<IValidationRule, FormVsDocumentRule>();
    builder.Services.AddScoped<IDocumentFieldExtractor, DocxFieldExtractor>();
    builder.Services.AddScoped<IValidationService, ValidationService>(); 


var app = builder.Build();

await DbSeeder.SeedAdminAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
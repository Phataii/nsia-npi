using nsia.Data;
using nsia.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// MySQL / Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        o => o.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    )
);

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Email — typed HttpClient, do NOT also add AddScoped for IEmailService
builder.Services.AddHttpClient<IEmailService, EmailService>();

// File service
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();         // must be before MapControllerRoute
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed default admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.AdminUsers.Any())
    {
        db.AdminUsers.Add(new nsia.Models.AdminUser
        {
            FullName = "NSIA Admin",
            Email = "admin@nsia.com.ng",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            Role = "SuperAdmin",
        });
        await db.SaveChangesAsync();
    }
}

app.Run();
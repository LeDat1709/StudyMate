using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Implementations;
using StudyMate.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database + EF Core ─────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// ── 2. ASP.NET Identity ───────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policy
    options.Password.RequiredLength          = 8;
    options.Password.RequireUppercase        = true;
    options.Password.RequireDigit            = true;
    options.Password.RequireNonAlphanumeric  = false;

    // Lockout
    options.Lockout.MaxFailedAccessAttempts  = 5;
    options.Lockout.DefaultLockoutTimeSpan   = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers       = true;

    // User
    options.User.RequireUniqueEmail          = true;

    // Sign-in: bắt buộc xác thực email trước khi đăng nhập
    options.SignIn.RequireConfirmedEmail     = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── 3. Cookie / Session ───────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath           = "/Account/Login";
    options.LogoutPath          = "/Account/Logout";
    options.AccessDeniedPath    = "/Account/AccessDenied";
    options.SlidingExpiration   = true;
    options.ExpireTimeSpan      = TimeSpan.FromDays(1);  // mặc định 1 ngày
});

// ── 4. Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// ── 5. MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── 6. Seed Roles + Subjects khi khởi động ────────────────────────────────────
await SeedRolesAsync(app);
await SeedSubjectsAsync(app);

// ── 7. Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // phải trước UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// ── Seed Roles helper ─────────────────────────────────────────────────────────
static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = ["Admin", "Tutor", "Student", "Guest"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// ── Seed Subjects (M2-T1) ─────────────────────────────────────────────────────
static async Task SeedSubjectsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedData.SeedSubjectsAsync(db);
}

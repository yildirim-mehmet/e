
using EgitimKayit.Data; // ApplicationDbContext için
using EgitimKayit.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ↓↓↓ BUNLARI EKLEYİN ↓↓↓
// DbContext Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Custom Services
builder.Services.AddScoped<IAuthService, AuthService>(); // ← Bu satırı ekleyin

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContext Accessor
builder.Services.AddHttpContextAccessor();
// ↑↑↑ BUNLARI EKLEYİN ↑↑↑

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ↓↓↓ BUNU EKLEYİN ↓↓↓
app.UseSession();
// ↑↑↑ BUNU EKLEYİN ↑↑↑

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
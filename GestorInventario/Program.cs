using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var cultureInfo = new System.Globalization.CultureInfo("en-US");
cultureInfo.NumberFormat.CurrencySymbol = "€";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// ── Pipeline ─────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Productos}/{action=Index}/{id?}");

app.Run();
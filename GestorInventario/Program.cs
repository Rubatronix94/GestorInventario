using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using Microsoft.AspNetCore.Identity;

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

// ---Identity — autenticación
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Requisitos de contraseña (puedes ajustarlos)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Redirige al login si no está autenticado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Productos}/{action=Index}/{id?}");

//app.Run();

// Crear usuario administrador inicial — BORRAR después de crearlo
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    if (await userManager.FindByEmailAsync("admin@gestor.com") == null)
    {
        var user = new IdentityUser
        {
            UserName = "admin@gestor.com",
            Email = "admin@gestor.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Admin123!");
    }
}

app.Run();
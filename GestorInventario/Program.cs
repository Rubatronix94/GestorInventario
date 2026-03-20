using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using Microsoft.AspNetCore.Identity;
using GestorInventario.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Base de datos (Supabase PostgreSQL) ──────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── Cultura / moneda ─────────────────────────────────────────
var cultureInfo = new System.Globalization.CultureInfo("en-US");
cultureInfo.NumberFormat.CurrencySymbol = "€";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// ── Identity con ApplicationUser (tiene EmpresaId) ───────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Cookie / login ───────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantService>();

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

// ── Crear usuario administrador inicial ──────────────────────
// ⚠️ BORRAR este bloque después de arrancar la app una vez
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1. Crear empresa demo si no existe
    var empresa = context.Empresas.FirstOrDefault(e => e.Id == 1);
    if (empresa == null)
    {
        empresa = new Empresa { Nombre = "Empresa Demo", Email = "admin@gestor.com" };
        context.Empresas.Add(empresa);
        context.SaveChanges();
    }

    // 2. Crear usuario admin vinculado a esa empresa
    if (await userManager.FindByEmailAsync("admin@gestor.com") == null)
    {
        var user = new ApplicationUser
        {
            UserName = "admin@gestor.com",
            Email = "admin@gestor.com",
            EmailConfirmed = true,
            EmpresaId = empresa.Id
        };
        await userManager.CreateAsync(user, "Admin123!");
    }
}
// ── FIN bloque inicialización ─────────────────────────────────

app.Run();
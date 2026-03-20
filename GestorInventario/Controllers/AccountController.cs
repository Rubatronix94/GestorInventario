using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestorInventario.Models;
using GestorInventario.Data;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Productos");

            ModelState.AddModelError("", "Email o contraseña incorrectos.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        // Crea una nueva Empresa y un usuario Admin vinculado a ella
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Crear la empresa
            var empresa = new Empresa
            {
                Nombre = model.NombreEmpresa,
                Email = model.Email,
                FechaRegistro = DateTime.UtcNow,
                Activa = true
            };
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            // 2. Crear el usuario vinculado a esa empresa
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                EmpresaId = empresa.Id  // ← vínculo clave
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Productos");
            }

            // Si hubo errores, borramos la empresa creada y mostramos los errores
            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using GestorInventario.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class ProveedoresController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TenantService _tenant;

        public ProveedoresController(AppDbContext context, TenantService tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // GET: /Proveedores
        public async Task<IActionResult> Index()
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            var proveedores = await _context.Proveedores
                .Where(p => p.EmpresaId == empresaId)
                .Include(p => p.Productos)
                .ToListAsync();

            return View(proveedores);
        }

        // GET: /Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var proveedor = await _context.Proveedores
                .Where(p => p.EmpresaId == empresaId)
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // GET: /Proveedores/Create
        public IActionResult Create() => View();

        // POST: /Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Contacto,Telefono,Email")] Proveedor proveedor)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            if (ModelState.IsValid)
            {
                proveedor.EmpresaId = empresaId; // ← asignamos la empresa automáticamente
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        // GET: /Proveedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // POST: /Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Contacto,Telefono,Email")] Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var existe = await _context.Proveedores
                .AnyAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (!existe) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    proveedor.EmpresaId = empresaId; // forzamos siempre el EmpresaId correcto
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExiste(proveedor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        // GET: /Proveedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var proveedor = await _context.Proveedores
                .Where(p => p.EmpresaId == empresaId)
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // POST: /Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExiste(int id) =>
            _context.Proveedores.Any(p => p.Id == id);
    }
}

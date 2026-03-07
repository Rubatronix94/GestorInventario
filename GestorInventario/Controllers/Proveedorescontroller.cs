using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class ProveedoresController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Proveedores
        public async Task<IActionResult> Index()
        {
            // var proveedores = await _context.Proveedores.ToListAsync();
            var proveedores = await _context.Proveedores
            .Include(p => p.Productos)
            .ToListAsync();
            return View(proveedores);
        }

        // GET: /Proveedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Cargamos el proveedor junto con sus productos relacionados
            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // GET: /Proveedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Contacto,Telefono,Email")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
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

            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        // POST: /Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Contacto,Telefono,Email")] Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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

            var proveedor = await _context.Proveedores
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
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExiste(int id)
        {
            return _context.Proveedores.Any(p => p.Id == id);
        }
    }
}
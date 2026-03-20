using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using GestorInventario.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TenantService _tenant;

        public CategoriasController(AppDbContext context, TenantService tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // GET: /Categorias
        public async Task<IActionResult> Index()
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            var categorias = await _context.Categorias
                .Where(c => c.EmpresaId == empresaId)
                .Include(c => c.Productos)
                .ToListAsync();

            return View(categorias);
        }

        // GET: /Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var categoria = await _context.Categorias
                .Where(c => c.EmpresaId == empresaId)
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // GET: /Categorias/Create
        public IActionResult Create() => View();

        // POST: /Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            if (ModelState.IsValid)
            {
                categoria.EmpresaId = empresaId; // ← asignamos la empresa automáticamente
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // GET: /Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: /Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var existe = await _context.Categorias
                .AnyAsync(c => c.Id == id && c.EmpresaId == empresaId);

            if (!existe) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    categoria.EmpresaId = empresaId; // forzamos siempre el EmpresaId correcto
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExiste(categoria.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // GET: /Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var categoria = await _context.Categorias
                .Where(c => c.EmpresaId == empresaId)
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: /Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExiste(int id) =>
            _context.Categorias.Any(c => c.Id == id);
    }
}

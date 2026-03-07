using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────
        // INDEX — Lista de productos con sus relaciones
        // Aquí usamos Include() para cargar Categoria y Proveedor
        // en la misma consulta (evita el problema N+1 de consultas)
        // GET: /Productos
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)   // Carga la categoría relacionada
                .Include(p => p.Proveedor)   // Carga el proveedor relacionado
                .OrderBy(p => p.Nombre)      // Ordena alfabéticamente
                .ToListAsync();

            return View(productos);
        }

        // ─────────────────────────────────────────────────────────
        // DETAILS — Detalle completo del producto con historial
        // GET: /Productos/Details/5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Include(p => p.Movimientos.OrderByDescending(m => m.Fecha)) // Historial reciente primero
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // ─────────────────────────────────────────────────────────
        // CREATE (GET) — Formulario con dropdowns de Categoria y Proveedor
        // GET: /Productos/Create
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Create()
        {
            // SelectList genera las opciones para los <select> del formulario
            // Parámetros: (colección, campo valor, campo texto mostrado)
            await CargarSelectLists();
            return View();
        }

        // POST: /Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,Stock,StockMinimo,CategoriaId,ProveedorId,Activo")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                producto.FechaAlta = DateTime.Now;
                _context.Add(producto);
                await _context.SaveChangesAsync();

                // TempData guarda un mensaje que se muestra solo una vez
                // en la siguiente página (como una notificación flash)
                TempData["Exito"] = $"Producto '{producto.Nombre}' creado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, recargamos los SelectList antes de devolver la vista
            await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
            return View(producto);
        }

        // GET: /Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null) return NotFound();

            await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
            return View(producto);
        }

        // POST: /Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,StockMinimo,CategoriaId,ProveedorId,Activo,FechaAlta")] Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = $"Producto '{producto.Nombre}' actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExiste(producto.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CargarSelectLists(producto.CategoriaId, producto.ProveedorId);
            return View(producto);
        }

        // GET: /Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: /Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Producto eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────
        // Método privado: carga los SelectList en ViewData
        // para que las vistas puedan generar los <select>
        // Los parámetros opcionales preseleccionan el valor actual
        // (importante en Edit para que salga el valor correcto)
        // ─────────────────────────────────────────────────────────
        private async Task CargarSelectLists(int? categoriaId = null, int? proveedorId = null)
        {
            var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            var proveedores = await _context.Proveedores.OrderBy(p => p.Nombre).ToListAsync();

            ViewData["CategoriaId"] = new SelectList(categorias, "Id", "Nombre", categoriaId);
            ViewData["ProveedorId"] = new SelectList(proveedores, "Id", "Nombre", proveedorId);
        }

        private bool ProductoExiste(int id)
        {
            return _context.Productos.Any(p => p.Id == id);
        }
    }
}
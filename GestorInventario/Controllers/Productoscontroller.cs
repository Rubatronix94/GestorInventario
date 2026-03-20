using GestorInventario.Data;
using GestorInventario.Models;
using GestorInventario.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Controllers
{
    [Authorize] // Solo usuarios logueados
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TenantService _tenant;

        public ProductosController(AppDbContext context, TenantService tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            // ✅ Solo productos de la empresa del usuario logueado
            var productos = await _context.Productos
                .Where(p => p.EmpresaId == empresaId)
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            // Solo categorías y proveedores de esta empresa
            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.EmpresaId == empresaId), "Id", "Nombre");
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedores.Where(p => p.EmpresaId == empresaId), "Id", "Nombre");

            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            if (ModelState.IsValid)
            {
                // ✅ Asignamos el EmpresaId automáticamente — el usuario no lo ve ni lo toca
                producto.EmpresaId = empresaId;
                producto.FechaAlta = DateTime.UtcNow;

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.EmpresaId == empresaId), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedores.Where(p => p.EmpresaId == empresaId), "Id", "Nombre", producto.ProveedorId);

            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            // ✅ Verificamos que el producto pertenece a esta empresa
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (producto == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.EmpresaId == empresaId), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedores.Where(p => p.EmpresaId == empresaId), "Id", "Nombre", producto.ProveedorId);

            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            if (ModelState.IsValid)
            {
                // Cargamos el original para no perder EmpresaId ni FechaAlta
                var productoOriginal = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

                if (productoOriginal == null) return NotFound();

                // Actualizamos solo los campos editables del formulario
                productoOriginal.Nombre = producto.Nombre;
                productoOriginal.Descripcion = producto.Descripcion;
                productoOriginal.Precio = producto.Precio;
                productoOriginal.Stock = producto.Stock;
                productoOriginal.StockMinimo = producto.StockMinimo;
                productoOriginal.Activo = producto.Activo;
                productoOriginal.CategoriaId = producto.CategoriaId;
                productoOriginal.ProveedorId = producto.ProveedorId;
                // EmpresaId y FechaAlta se quedan como estaban ✅

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.EmpresaId == empresaId), "Id", "Nombre", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedores.Where(p => p.EmpresaId == empresaId), "Id", "Nombre", producto.ProveedorId);

            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
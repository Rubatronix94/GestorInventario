using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using GestorInventario.Services;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class MovimientosStockController : Controller
    {
        private readonly AppDbContext _context;
        private readonly TenantService _tenant;

        public MovimientosStockController(AppDbContext context, TenantService tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        // GET: /MovimientosStock
        public async Task<IActionResult> Index()
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            // Solo movimientos de productos que pertenecen a esta empresa
            var movimientos = await _context.MovimientosStock
                .Include(m => m.Producto)
                .Where(m => m.Producto!.EmpresaId == empresaId)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            return View(movimientos);
        }

        // GET: /MovimientosStock/Create?productoId=5
        public async Task<IActionResult> Create(int? productoId)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();
            await CargarSelectProductos(empresaId, productoId);
            return View();
        }

        // POST: /MovimientosStock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductoId,Tipo,Cantidad,Notas")] MovimientoStock movimiento)
        {
            var empresaId = await _tenant.GetEmpresaIdAsync();

            if (ModelState.IsValid)
            {
                // Verificamos que el producto pertenece a esta empresa
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == movimiento.ProductoId && p.EmpresaId == empresaId);

                if (producto == null)
                {
                    ModelState.AddModelError("", "El producto seleccionado no existe.");
                    await CargarSelectProductos(empresaId, movimiento.ProductoId);
                    return View(movimiento);
                }

                int stockAnterior = producto.Stock;

                if (movimiento.Tipo == TipoMovimiento.Entrada)
                {
                    producto.Stock += movimiento.Cantidad;
                }
                else if (movimiento.Tipo == TipoMovimiento.Salida)
                {
                    if (producto.Stock < movimiento.Cantidad)
                    {
                        ModelState.AddModelError("Cantidad",
                            $"Stock insuficiente. Stock actual: {producto.Stock} unidades.");
                        await CargarSelectProductos(empresaId, movimiento.ProductoId);
                        return View(movimiento);
                    }
                    producto.Stock -= movimiento.Cantidad;
                }
                else if (movimiento.Tipo == TipoMovimiento.Ajuste)
                {
                    producto.Stock = movimiento.Cantidad;
                }

                movimiento.StockResultante = producto.Stock;
                movimiento.Fecha = DateTime.UtcNow;

                _context.Update(producto);
                _context.Add(movimiento);
                await _context.SaveChangesAsync();

                TempData["Exito"] = $"Movimiento registrado. Stock de '{producto.Nombre}': {stockAnterior} → {producto.Stock}";
                return RedirectToAction(nameof(Index));
            }

            await CargarSelectProductos(empresaId, movimiento.ProductoId);
            return View(movimiento);
        }

        // GET: /MovimientosStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empresaId = await _tenant.GetEmpresaIdAsync();

            var movimiento = await _context.MovimientosStock
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id && m.Producto!.EmpresaId == empresaId);

            if (movimiento == null) return NotFound();

            return View(movimiento);
        }

        // Solo muestra productos activos de esta empresa
        private async Task CargarSelectProductos(int empresaId, int? productoId = null)
        {
            var productos = await _context.Productos
                .Where(p => p.Activo && p.EmpresaId == empresaId)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewData["ProductoId"] = new SelectList(productos, "Id", "Nombre", productoId);
        }
    }
}

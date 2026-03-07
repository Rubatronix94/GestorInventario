using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class MovimientosStockController : Controller
    {
        private readonly AppDbContext _context;

        public MovimientosStockController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────
        // INDEX — Historial de todos los movimientos
        // GET: /MovimientosStock
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var movimientos = await _context.MovimientosStock
                .Include(m => m.Producto)              // Necesitamos el nombre del producto
                .OrderByDescending(m => m.Fecha)       // Más recientes primero
                .ToListAsync();

            return View(movimientos);
        }

        // ─────────────────────────────────────────────────────────
        // CREATE (GET) — Formulario para registrar un movimiento
        // Opcionalmente recibe productoId para preseleccionar
        // GET: /MovimientosStock/Create?productoId=5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Create(int? productoId)
        {
            await CargarSelectProductos(productoId);
            return View();
        }

        // ─────────────────────────────────────────────────────────
        // CREATE (POST) — Aquí está la lógica de negocio importante:
        // 1. Validamos que no quede stock negativo
        // 2. Actualizamos el stock del producto
        // 3. Guardamos el movimiento con el stock resultante
        // Todo en una transacción para que sea atómico
        // POST: /MovimientosStock/Create
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductoId,Tipo,Cantidad,Notas")] MovimientoStock movimiento)
        {
            if (ModelState.IsValid)
            {
                // Buscamos el producto para verificar y actualizar su stock
                var producto = await _context.Productos.FindAsync(movimiento.ProductoId);

                if (producto == null)
                {
                    ModelState.AddModelError("", "El producto seleccionado no existe.");
                    await CargarSelectProductos(movimiento.ProductoId);
                    return View(movimiento);
                }

                // ── Lógica de actualización de stock ──────────────
                int stockAnterior = producto.Stock;

                if (movimiento.Tipo == TipoMovimiento.Entrada)
                {
                    // Entrada: sumamos al stock
                    producto.Stock += movimiento.Cantidad;
                }
                else if (movimiento.Tipo == TipoMovimiento.Salida)
                {
                    // Salida: verificamos que hay suficiente stock
                    if (producto.Stock < movimiento.Cantidad)
                    {
                        ModelState.AddModelError("Cantidad",
                            $"Stock insuficiente. Stock actual: {producto.Stock} unidades.");
                        await CargarSelectProductos(movimiento.ProductoId);
                        return View(movimiento);
                    }
                    producto.Stock -= movimiento.Cantidad;
                }
                else if (movimiento.Tipo == TipoMovimiento.Ajuste)
                {
                    // Ajuste: la cantidad es el nuevo stock total
                    producto.Stock = movimiento.Cantidad;
                }

                // Guardamos el stock resultante en el movimiento
                // (sirve como auditoría histórica)
                movimiento.StockResultante = producto.Stock;
                movimiento.Fecha = DateTime.Now;

                // Guardamos ambos cambios (producto + movimiento) a la vez
                _context.Update(producto);
                _context.Add(movimiento);
                await _context.SaveChangesAsync();

                TempData["Exito"] = $"Movimiento registrado. Stock de '{producto.Nombre}': {stockAnterior} → {producto.Stock}";
                return RedirectToAction(nameof(Index));
            }

            await CargarSelectProductos(movimiento.ProductoId);
            return View(movimiento);
        }

        // ─────────────────────────────────────────────────────────
        // DETAILS — Detalle de un movimiento concreto
        // GET: /MovimientosStock/Details/5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movimiento = await _context.MovimientosStock
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimiento == null) return NotFound();

            return View(movimiento);
        }

        // ─────────────────────────────────────────────────────────
        // Nota: en MovimientosStock NO hay Edit ni Delete
        // Los movimientos son registros de auditoría — una vez
        // creados no se deben modificar ni borrar.
        // Esto es una decisión de diseño importante en sistemas reales.
        // ─────────────────────────────────────────────────────────

        private async Task CargarSelectProductos(int? productoId = null)
        {
            var productos = await _context.Productos
                .Where(p => p.Activo)              // Solo productos activos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewData["ProductoId"] = new SelectList(productos, "Id", "Nombre", productoId);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Data;
using GestorInventario.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestorInventario.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        // ─────────────────────────────────────────────────────────
        // El DbContext es la "puerta de entrada" a la base de datos.
        // Lo inyectamos en el constructor (Dependency Injection).
        // ASP.NET Core se encarga de crearlo y pasárnoslo solo.
        // ─────────────────────────────────────────────────────────
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────
        // INDEX — Muestra la lista de todas las categorías
        // GET: /Categorias
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // ToListAsync() trae todos los registros de la tabla Categorias
           // var categorias = await _context.Categorias.ToListAsync();
            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .ToListAsync();

            return View(categorias); // Pasa la lista a la vista Index.cshtml
        }

        // ─────────────────────────────────────────────────────────
        // DETAILS — Muestra el detalle de una categoría por su Id
        // GET: /Categorias/Details/5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            // Si no pasan id, devolvemos error 404
            if (id == null) return NotFound();

            // Include() carga también los Productos relacionados (navegación)
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // ─────────────────────────────────────────────────────────
        // CREATE (GET) — Muestra el formulario vacío para crear
        // GET: /Categorias/Create
        // ─────────────────────────────────────────────────────────
        public IActionResult Create()
        {
            // No necesita ir a la BD, solo muestra el formulario vacío
            return View();
        }

        // ─────────────────────────────────────────────────────────
        // CREATE (POST) — Recibe el formulario y guarda en la BD
        // POST: /Categorias/Create
        // [ValidateAntiForgeryToken] protege contra ataques CSRF
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            // ModelState.IsValid comprueba las validaciones del modelo
            // (Required, StringLength, etc. que definimos con DataAnnotations)
            if (ModelState.IsValid)
            {
                _context.Add(categoria);          // Prepara el INSERT
                await _context.SaveChangesAsync(); // Ejecuta el INSERT en la BD
                return RedirectToAction(nameof(Index)); // Redirige al listado
            }

            // Si hay errores de validación, volvemos a mostrar el formulario
            // con los datos que el usuario ya había escrito
            return View(categoria);
        }

        // ─────────────────────────────────────────────────────────
        // EDIT (GET) — Muestra el formulario con los datos actuales
        // GET: /Categorias/Edit/5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // FindAsync() es más eficiente que FirstOrDefaultAsync cuando
            // buscamos por clave primaria
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // ─────────────────────────────────────────────────────────
        // EDIT (POST) — Recibe el formulario y actualiza en la BD
        // POST: /Categorias/Edit/5
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Categoria categoria)
        {
            // Verificamos que el id de la URL coincide con el del modelo
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);        // Prepara el UPDATE
                    await _context.SaveChangesAsync(); // Ejecuta el UPDATE
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Este error ocurre si dos usuarios editan a la vez
                    if (!CategoriaExiste(categoria.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE (GET) — Muestra pantalla de confirmación
        // GET: /Categorias/Delete/5
        // ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // ─────────────────────────────────────────────────────────
        // DELETE (POST) — Confirma y borra de la BD
        // POST: /Categorias/Delete/5
        // ─────────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria != null)
            {
                _context.Categorias.Remove(categoria); // Prepara el DELETE
                await _context.SaveChangesAsync();     // Ejecuta el DELETE
            }

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────
        // Método privado de utilidad: comprueba si existe una
        // categoría con ese Id (lo usamos en Edit para manejar
        // errores de concurrencia)
        // ─────────────────────────────────────────────────────────
        private bool CategoriaExiste(int id)
        {
            return _context.Categorias.Any(c => c.Id == id);
        }
    }
}
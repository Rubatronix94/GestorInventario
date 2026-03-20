using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Nombre de la empresa")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "CIF / NIF")]
        public string? Cif { get; set; }

        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Navegación
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}

using GestorInventario.Models;
using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Contacto")]
        public string? Contacto { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        // Navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
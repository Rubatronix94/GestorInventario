using GestorInventario.Models;
using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        // Navegación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
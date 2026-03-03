using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestorInventario.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio (€)")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        [Display(Name = "Stock actual")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Stock mínimo (alerta)")]
        public int StockMinimo { get; set; } = 5;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de alta")]
        public DateTime FechaAlta { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [Display(Name = "Proveedor")]
        public int? ProveedorId { get; set; }

        // Navegación
        public Categoria? Categoria { get; set; }
        public Proveedor? Proveedor { get; set; }
        public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();

        // Propiedad calculada (no se guarda en BD)
        [NotMapped]
        public bool StockBajo => Stock <= StockMinimo;
    }
}
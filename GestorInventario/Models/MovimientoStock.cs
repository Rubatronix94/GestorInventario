using GestorInventario.Models;
using System.ComponentModel.DataAnnotations;

namespace GestorInventario.Models
{
    public enum TipoMovimiento
    {
        [Display(Name = "Entrada")]
        Entrada,

        [Display(Name = "Salida")]
        Salida,

        [Display(Name = "Ajuste")]
        Ajuste
    }

    public class MovimientoStock
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public TipoMovimiento Tipo { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Stock resultante")]
        public int StockResultante { get; set; }

        [StringLength(300)]
        [Display(Name = "Motivo / Notas")]
        public string? Notas { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Foreign Key
        [Required]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        // Navegación
        public Producto? Producto { get; set; }
    }
}
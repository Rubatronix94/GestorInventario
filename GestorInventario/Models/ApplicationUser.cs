using Microsoft.AspNetCore.Identity;

namespace GestorInventario.Models
{
    // Extendemos IdentityUser para añadir EmpresaId
    public class ApplicationUser : IdentityUser
    {
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }
}

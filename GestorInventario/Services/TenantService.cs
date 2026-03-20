using GestorInventario.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GestorInventario.Services
{
    /// <summary>
    /// Obtiene el EmpresaId del usuario autenticado.
    /// Inyéctalos en los controladores para filtrar datos por empresa.
    /// </summary>
    public class TenantService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(UserManager<ApplicationUser> userManager,
                             IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Devuelve el EmpresaId del usuario actual. Lanza excepción si no está autenticado.
        /// </summary>
        public async Task<int> GetEmpresaIdAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User
                            .FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new UnauthorizedAccessException("Usuario no autenticado.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user?.EmpresaId == null)
                throw new InvalidOperationException("El usuario no tiene empresa asignada.");

            return user.EmpresaId.Value;
        }
    }
}

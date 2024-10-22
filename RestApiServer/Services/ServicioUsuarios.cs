using Microsoft.AspNetCore.Identity;
using RestApiServer.Entities;

namespace RestApiServer.Services
{
    public interface IServicioUsuarios
    {
        Task<ApplicationUser?> ObtenerUsuarioAsync();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> ObtenerUsuarioAsync()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity == null || !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var cuentaClaim = _httpContextAccessor.HttpContext.User.FindFirst("cuenta");

            if (cuentaClaim is null)
            {
                return null;
            }

            var cuenta = cuentaClaim.Value;

            // Busca el usuario por el nombre de cuenta
            return await _userManager.FindByNameAsync(cuenta);
        }

    }
}

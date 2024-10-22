using Microsoft.AspNetCore.Identity;

namespace RestApiServer.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int PersonaId { get; set; }
    }
}

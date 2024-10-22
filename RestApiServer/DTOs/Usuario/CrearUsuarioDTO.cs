namespace RestApiServer.DTOs.Usuario
{
    public class CrearUsuarioDTO
    {
        public int PersonaId { get; set; }
        public string Cuenta { get; set; } = null!;
        public string Password { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
    }
}

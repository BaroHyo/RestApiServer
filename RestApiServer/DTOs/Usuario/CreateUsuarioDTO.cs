namespace RestApiServer.DTOs.Usuario
{
    public class CreateUsuarioDTO
    {
        public int PersonaId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public int SucursalId { get; set; }
    }
}

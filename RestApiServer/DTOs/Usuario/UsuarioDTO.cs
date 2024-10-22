namespace RestApiServer.DTOs.Usuario
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string Inicales { get; set; } = null!;
        public string rol { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}

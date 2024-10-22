namespace RestApiServer.DTOs.Usuario
{
    public class RespuestaAutenticacionDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiracion { get; set; }
        public UsuarioDTO Usuario { get; set;} 
    }
}

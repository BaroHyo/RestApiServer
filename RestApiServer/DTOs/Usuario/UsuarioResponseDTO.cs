namespace RestApiServer.DTOs.Usuario
{
    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public int RolId { get; set; }
        public int SucursalId { get; set; }
        public string UserName { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public string Sucursal { get; set; } = null!;
    }
}

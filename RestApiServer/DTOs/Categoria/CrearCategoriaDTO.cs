using System.ComponentModel.DataAnnotations;

namespace RestApiServer.DTOs.Categoria
{
    public class CrearCategoriaDTO
    {
        [Required]
        public string Codigo { get; set; } = null!;
        [Required]
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
    }
}

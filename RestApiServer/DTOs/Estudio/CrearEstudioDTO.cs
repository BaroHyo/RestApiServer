namespace RestApiServer.DTOs.Estudio
{
    public class CrearEstudioDTO
    {
        public int TipoEstudioId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
    }
}

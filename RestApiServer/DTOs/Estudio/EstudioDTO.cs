namespace RestApiServer.DTOs.Estudio
{
    public class EstudioDTO
    {
        public int Id { get; set; }
        public int TipoEstudioId { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
    }
}

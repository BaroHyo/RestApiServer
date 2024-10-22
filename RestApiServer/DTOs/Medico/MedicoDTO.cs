namespace RestApiServer.DTOs.Medico
{
    public class MedicoDTO
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string Codigo { get; set; } = null!;
        public DateTime FechaAsignacion { get; set; }
        public DateTime FechaFinalizacion { get; set; }
    }
}

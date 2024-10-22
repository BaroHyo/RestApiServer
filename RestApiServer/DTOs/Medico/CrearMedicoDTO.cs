namespace RestApiServer.DTOs.Medico
{
    public class CrearMedicoDTO
    {
        public int PersonaId { get; set; }
        public string Codigo { get; set; } = null!;
        public DateTime FechaAsignacion { get; set; }
        public DateTime FechaFinalizacion { get; set; }
    }
}

namespace RestApiServer.Entities
{
    public class Medico
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public Persona Persona { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public DateTime FechaAsignacion { get; set; }
        public DateTime FechaFinalizacion { get; set; }
    }
}

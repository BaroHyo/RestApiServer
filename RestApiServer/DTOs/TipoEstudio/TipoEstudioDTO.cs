﻿namespace RestApiServer.DTOs.TipoEstudio
{
    public class TipoEstudioDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
    }
}
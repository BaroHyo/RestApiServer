﻿namespace RestApiServer.Entities
{
    public class CategoriaDet
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; } 
        public string Nombre { get; set; } = null!;
        public string Valor { get; set; } = null!;
        public float Orden { get; set; } 
    }
}

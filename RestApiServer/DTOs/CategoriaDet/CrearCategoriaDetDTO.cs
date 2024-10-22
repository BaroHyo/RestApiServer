namespace RestApiServer.DTOs.CategoriaDet
{
    public class CrearCategoriaDetDTO
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Valor { get; set; } = null!;
        public float Orden { get; set; }
    }
}

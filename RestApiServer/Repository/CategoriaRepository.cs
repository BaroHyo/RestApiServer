using Dapper;
using Newtonsoft.Json;
using RestApiServer.Entities;
using System.Data;

namespace RestApiServer.Repository
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> ObtenerTodos(string? filtro = null);
        Task<Categoria?> ObtenerPorId(int id);
        Task<(string Message, int ReturnCode)> Crear(Categoria categoria);
        Task<(string Message, int ReturnCode)> Actualizar(Categoria categoria);
        Task<(string Message, int ReturnCode)> Borrar(int id);
        Task<bool> Existe(int id);
    }

    public class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
    {
        private const string StoredProcedureSelect = "sp_categoria_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_categoria_ime";

        public CategoriaRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<List<Categoria>> ObtenerTodos(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }

        public async Task<Categoria?> ObtenerPorId(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);


            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<(string Message, int ReturnCode)> Crear(Categoria categoria)
        {
            var categoriaJson = JsonConvert.SerializeObject(categoria);
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", categoriaJson, DbType.String);
            parameters.Add("@Action", "INSERT", DbType.String);

            return await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);
        }

        public async Task<(string Message, int ReturnCode)> Actualizar(Categoria categoria)
        {
            var categoriaJson = JsonConvert.SerializeObject(categoria);
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", categoriaJson, DbType.String);
            parameters.Add("@Action", "UPDATE", DbType.String);

            return await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);
        }

        public async Task<(string Message, int ReturnCode)> Borrar(int id)
        {
            var categoriaJson = JsonConvert.SerializeObject(new { Id = id });
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", categoriaJson, DbType.String);
            parameters.Add("@Action", "DELETE", DbType.String);

            return await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);
        }

        public async Task<bool> Existe(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "EXISTE");
            parameters.Add("@Id", id);
            return await ExecuteQuerySingleAsync<bool>(StoredProcedureSelect, parameters);
        }
    }
}

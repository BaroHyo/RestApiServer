using Dapper;
using Newtonsoft.Json;
using RestApiServer.DTOs.Usuario;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;

namespace RestApiServer.Repository
{
    public interface IUsuarioRepository
    {
        Task<(string Message, int ReturnCode)> Actualizar(ApplicationUser usuario);
        Task<(string Message, int ReturnCode)> Borrar(int id);
        Task<int> Crear(ApplicationUser usuario);
        Task<bool> Existe(int id);
        Task<UsuarioResponseDTO?> ObtenerPorId(int id);
        Task<List<UsuarioResponseDTO>> ObtenerTodos(string? filtro = null);
    }

    public class UsuarioRepository : BaseRepository<UsuarioResponseDTO>, IUsuarioRepository
    {
        private const string StoredProcedureSelect = "sp_usuarios_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_usuarios_ime";

        public UsuarioRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<List<UsuarioResponseDTO>> ObtenerTodos(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }

        public async Task<UsuarioResponseDTO?> ObtenerPorId(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);


            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<int> Crear(ApplicationUser usuario)
        {
            var dataJson = JsonConvert.SerializeObject(usuario);
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", dataJson, DbType.String);
            parameters.Add("@Action", "INSERT", DbType.String);
            try
            {
                var messageJson = await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);

                var response = JsonConvert.DeserializeObject<ResponseInsert>(messageJson.Message);

                if (response?.Id == null)
                {
                    throw new InvalidOperationException(response?.Message);
                }


                return response?.Id != null && int.TryParse(response.Id, out int id) ? id : 0;
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse(true, ex.Message);

                throw new Exception(JsonConvert.SerializeObject(errorResponse), ex);
            }
             
        }

        public async Task<(string Message, int ReturnCode)> Actualizar(ApplicationUser usuario)
        {
            var dataJson = JsonConvert.SerializeObject(usuario);
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", dataJson, DbType.String);
            parameters.Add("@Action", "UPDATE", DbType.String);

            return await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);
        }

        public async Task<(string Message, int ReturnCode)> Borrar(int id)
        {
            var dataJson = JsonConvert.SerializeObject(new { Id = id });
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", dataJson, DbType.String);
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

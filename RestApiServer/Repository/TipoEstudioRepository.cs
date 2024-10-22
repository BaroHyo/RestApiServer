using Dapper;
using Newtonsoft.Json;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;

namespace RestApiServer.Repository
{
    public interface ITipoEstudioRepository
    {
        Task<List<TipoEstudio>> FindAll(string? filtro = null);
        Task<TipoEstudio?> FindById(int id);
        Task<int> Create(TipoEstudio tipoEstudio);
        Task<(string Message, int ReturnCode)> Update(TipoEstudio tipoEstudio);
        Task<(string Message, int ReturnCode)> Delete(int id);
        Task<bool> Existe(int id);
    }
    public class TipoEstudioRepository: BaseRepository<TipoEstudio>, ITipoEstudioRepository
    {
        private const string StoredProcedureSelect = "sp_tipo_estudio_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_tipo_estudio_ime";
        public TipoEstudioRepository(IConfiguration configuration): base(configuration) { }

        public async Task<List<TipoEstudio>> FindAll(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }
        public async Task<TipoEstudio?> FindById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<int> Create(TipoEstudio tipoEstudio)
        {
            var parametersJson = JsonConvert.SerializeObject(tipoEstudio);

            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", parametersJson, DbType.String);
            parameters.Add("@Action", "INSERT", DbType.String);

            var messageJson = await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);

            var response = JsonConvert.DeserializeObject<ResponseInsert>(messageJson.Message);

            string? idString = response?.Id;
            int id = int.TryParse(idString, out int result) ? result : 0;

            return id;
        }

        public async Task<(string Message, int ReturnCode)> Update(TipoEstudio tipoEstudio)
        {
            var parametersJson = JsonConvert.SerializeObject(tipoEstudio);
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", parametersJson, DbType.String);
            parameters.Add("@Action", "UPDATE", DbType.String);

            return await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);
        }

        public async Task<(string Message, int ReturnCode)> Delete(int id)
        {
            var parametersJson = JsonConvert.SerializeObject(new { Id = id });
            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", parametersJson, DbType.String);
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

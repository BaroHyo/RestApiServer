using Dapper;
using Newtonsoft.Json;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;

namespace RestApiServer.Repository
{
    public interface IEstudioRepository
    {
        Task<List<Estudio>> FindAll(string? filtro = null);
        Task<Estudio?> FindById(int id);
        Task<int> Create(Estudio estudio);
        Task<(string Message, int ReturnCode)> Update(Estudio estudio);
        Task<(string Message, int ReturnCode)> Delete(int id);
        Task<bool> Existe(int id);
    }
    public class EstudioRepository: BaseRepository<Estudio>, IEstudioRepository
    {
        private const string StoredProcedureSelect = "sp_estudio_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_estudio_ime";
        public EstudioRepository(IConfiguration configuration):base(configuration) { }

        public async Task<List<Estudio>> FindAll(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }
        public async Task<Estudio?> FindById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<int> Create(Estudio estudio)
        {
            var parametersJson = JsonConvert.SerializeObject(estudio);

            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", parametersJson, DbType.String);
            parameters.Add("@Action", "INSERT", DbType.String);

            var messageJson = await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);

            var response = JsonConvert.DeserializeObject<ResponseInsert>(messageJson.Message);

            string? idString = response?.Id;
            int id = int.TryParse(idString, out int result) ? result : 0;

            return id;
        }

        public async Task<(string Message, int ReturnCode)> Update(Estudio estudio)
        {
            var parametersJson = JsonConvert.SerializeObject(estudio);
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

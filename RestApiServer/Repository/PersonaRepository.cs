using Dapper;
using Newtonsoft.Json;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;

namespace RestApiServer.Repository
{
    public interface IPersonaRepository
    {
        Task<List<Persona>> FindAll(string? filtro = null);
        Task<Persona?> FindById(int id);
        Task<int> Create(Persona persona);
        Task<(string Message, int ReturnCode)> Update(Persona persona);
        Task<(string Message, int ReturnCode)> Delete(int id);
        Task<bool> Existe(int id);
    }
    public class PersonaRepository: BaseRepository<Persona>, IPersonaRepository
    {
        private const string StoredProcedureSelect = "sp_persona_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_persona_ime";
        public PersonaRepository(IConfiguration configuration):base(configuration){}

        public async Task<List<Persona>> FindAll(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }
        public async Task<Persona?> FindById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<int> Create(Persona persona)
        {
            var parametersJson = JsonConvert.SerializeObject(persona);

            var parameters = new DynamicParameters();
            parameters.Add("@Parametros", parametersJson, DbType.String);
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


        public async Task<(string Message, int ReturnCode)> Update(Persona persona)
        {
            var parametersJson = JsonConvert.SerializeObject(persona);
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

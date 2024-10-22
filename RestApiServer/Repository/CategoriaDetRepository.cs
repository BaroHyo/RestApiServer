using Dapper;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestApiServer.DTOs;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;

namespace RestApiServer.Repository
{
    public  interface ICategoriaDetRepository
    {
        Task<List<CategoriaDet>> FindAll(string? filtro = null);
        Task<CategoriaDet?> FindById(int id);
        Task<int> Create(CategoriaDet categoriaDet);
        Task<(string Message, int ReturnCode)> Update(CategoriaDet categoriaDet);
        Task<(string Message, int ReturnCode)> Delete(int id);
        Task<bool> Existe(int id);
        Task<List<CategoriaDet>> FindAllCombox(string codigo);

    }
    public class CategoriaDetRepository : BaseRepository<CategoriaDet>, ICategoriaDetRepository
    {
        private const string StoredProcedureSelect = "sp_categoria_det_sel";
        private const string StoredProcedureInsertUpdateDelete = "sp_categoria_det_ime";
        public CategoriaDetRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<List<CategoriaDet>> FindAll(string? filtro = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT", DbType.String);
            parameters.Add("@Filtro", filtro, DbType.String);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }
        public async Task<CategoriaDet?> FindById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_BY_ID", DbType.String);
            parameters.Add("@Id", id, DbType.Int32);

            return await ExecuteQueryAsync(StoredProcedureSelect, parameters).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<int> Create(CategoriaDet categoriaDet)
        {
             var parametersJson = JsonConvert.SerializeObject(categoriaDet);

             var parameters = new DynamicParameters();
             parameters.Add("@Parametros", parametersJson, DbType.String); 
             parameters.Add("@Action", "INSERT", DbType.String);

             var messageJson = await ExecuteStoredProcedureAsync(StoredProcedureInsertUpdateDelete, parameters);

             var response = JsonConvert.DeserializeObject<ResponseInsert>(messageJson.Message);

            string? idString = response?.Id;
            int id = int.TryParse(idString, out int result) ? result : 0;
            
            return id;
        }

        public async Task<(string Message, int ReturnCode)> Update(CategoriaDet categoriaDet)
        {
            var parametersJson = JsonConvert.SerializeObject(categoriaDet);
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

        public async Task<List<CategoriaDet>> FindAllCombox(string codigo)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Action", "SELECT_COMBOX", DbType.String);
            var filtro = $"WHERE c.Codigo = '{codigo}'"; // Asegúrate de que el valor de codigo sea seguro
            parameters.Add("@Filtro", filtro, DbType.String);
            return await ExecuteQueryAsync(StoredProcedureSelect, parameters);
        }

    }
}

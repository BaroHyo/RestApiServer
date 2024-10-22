using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RestApiServer.Repository
{
    public abstract class BaseRepository<T>
    {
        protected readonly string? connectionString;

        protected BaseRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Cambia el nombre del parámetro de tipo T en los métodos genéricos
        protected async Task<TItem> ExecuteQuerySingleAsync<TItem>(string storedProcedure, object parameters)
        {
            await using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleAsync<TItem>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
        }

        protected async Task<List<T>> ExecuteQueryAsync(string storedProcedure, DynamicParameters parameters)
        {
            await using var conexion = new SqlConnection(connectionString);
            return (await conexion.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure)).AsList();
        }

        protected async Task<List<TItem>> ExecuteQueryDimAsync<TItem>(string storedProcedure, DynamicParameters parameters)
        {
            await using var conexion = new SqlConnection(connectionString);
            return (await conexion.QueryAsync<TItem>(storedProcedure, parameters, commandType: CommandType.StoredProcedure)).AsList();
        }

        protected async Task<(string Message, int ReturnCode)> ExecuteStoredProcedureAsync(string storedProcedure, DynamicParameters parameters)
        {
            await using var conexion = new SqlConnection(connectionString);

            parameters.Add("@Message", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await conexion.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);

            var message = parameters.Get<string>("@Message");
            var returnCode = parameters.Get<int>("@ReturnCode");

            return (message, returnCode);
        }
    }
}

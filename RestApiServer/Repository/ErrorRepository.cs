using Dapper;
using Microsoft.Data.SqlClient;
using RestApiServer.Entities;

namespace RestApiServer.Repository
{
    public interface IErrorRepository
    {
        Task Crear(Error error);
    }
    public class ErrorRepository: IErrorRepository
    {
        private readonly string connectionString;

        public ErrorRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task Crear(Error error)
        {
            using (var conexion = new SqlConnection(connectionString))
            {
                await conexion.ExecuteAsync("sp_errores_crear",
                    new { error.MensajeDeError, error.StackTrace, error.Fecha },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}

using Dapper;
using RestApiServer.DTOs.Usuario;
using RestApiServer.Entities;
using RestApiServer.Utilities;
using System.Data;
using System.Security.Claims;

namespace RestApiServer.Repository
{
    public interface IUsuariosRepository
    {
        Task AsignarClaims(ApplicationUser user, IEnumerable<Claim> claims);
        Task<int> Crear(ApplicationUser usuario);
        Task<ApplicationUser?> FindByIdAsync(string userId);
        Task<ApplicationUser?> FindByNameAsync(string normalizedUserName);
        Task<IList<Claim>> GetClaimsAsync(ApplicationUser user);
        Task RemoverClaims(ApplicationUser user, IEnumerable<Claim> claims);
        Task<UsuarioDTO?> FindUsuarioAsync(int userId);
    }

    public class UsuariosRepository : IUsuariosRepository
    {
        private readonly IDbConnection _connection;

        public UsuariosRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NormalizedUserName", normalizedUserName);

            // Ejecutar una consulta para obtener el usuario por nombre de usuario
            var user = await _connection.QuerySingleOrDefaultAsync<ApplicationUser>("SELECT * FROM Usuarios WHERE NormalizedUserName = @NormalizedUserName", parameters);

            return user;
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);

            // Ejecutar una consulta para obtener el usuario por nombre de usuario
            var user = await _connection.QuerySingleOrDefaultAsync<ApplicationUser>("SELECT * FROM Usuarios WHERE Id  = @userId", userId);

            return user;
        }

        public async Task<int> Crear(ApplicationUser usuario)
        {
            var userId = await _connection.QuerySingleAsync<int>("sp_crear_usuario", new
            {
                usuario.UserName,
                usuario.NormalizedUserName,
                usuario.PasswordHash,
                usuario.PersonaId
            }, commandType: CommandType.StoredProcedure);

            return userId;

        }

        public async Task<UsuarioDTO?> FindUsuarioAsync(int userId)
        {
            var query = " select u.Id, p.PrimerNombre +' '+p.SegundoNombre +' '+p.ApellidoPaterno + ' '+p.ApellidoMaterno as NombreCompleto, upper(substring(p.PrimerNombre, 1, 1) +''+substring(p.ApellidoPaterno, 1, 1)) as Inicales, r.Name as rol, u.Email  from Usuarios u join Persona p on p.Id = u.PersonaId  join UsuariosRoles s on s.UserId = u.Id  join Roles r on r.Id = s.RoleId where u.Id = @UserId";
            var parameters = new { UserId = userId };

            var usuario = await _connection.QueryFirstAsync<UsuarioDTO>(query, parameters);

            return usuario;

        }

        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var query = "SELECT ClaimType, ClaimValue FROM UsuariosClaims WHERE UserId = @UserId";
            var parameters = new { UserId = user.Id };

            var claims = await _connection.QueryAsync<ClaimData>(query, parameters);

            var result = new List<Claim>();
            foreach (var claimData in claims)
            {
                result.Add(new Claim(claimData.ClaimType, claimData.ClaimValue));
            }

            return result;
        }
        public async Task AsignarClaims(ApplicationUser user, IEnumerable<Claim> claims)
        {
            var sql = @"INSERT INTO UsuariosClaims (UserId, ClaimType, ClaimValue)
                        VALUES (@Id, @Type, @Value)";

            var parametros = claims.Select(x => new { user.Id, x.Type, x.Value });

            await _connection.ExecuteAsync(sql, parametros);
        }
        public async Task RemoverClaims(ApplicationUser user, IEnumerable<Claim> claims)
        {
            var sql = @"DELETE UsuariosClaims WHERE UserId = @Id AND ClaimType = @Type";
            var parametros = claims.Select(x => new { user.Id, x.Type });

            await _connection.ExecuteAsync(sql, parametros);
        }
    }
}

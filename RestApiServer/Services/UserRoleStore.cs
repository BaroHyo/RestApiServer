using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace RestApiServer.Services
{
    public class UserRoleStore : IRoleStore<IdentityRole<int>>
    {
        private readonly IDbConnection _dbConnection;

        public UserRoleStore(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }
        #region IRoleStore<IdentityRole<int>> Implementation
        public async Task<IdentityResult> CreateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            try
            {
                var sql = @"INSERT INTO Roles (Name, NormalizedName, ConcurrencyStamp) 
                    VALUES (@Name, @NormalizedName, @ConcurrencyStamp); 
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await _dbConnection.ExecuteScalarAsync<int>(sql, new
                {
                    role.Name,
                    role.NormalizedName,
                    role.ConcurrencyStamp
                });

                // Asignar el Id al rol
                role.Id = id;

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                // Registrar el error si es necesario y devolver un resultado fallido
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Error creating role: {ex.Message}"
                });
            }
        }


        public async Task<IdentityResult> DeleteAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            var sql = "DELETE FROM Roles WHERE Id = @Id";
            var result = await _dbConnection.ExecuteAsync(sql, new { role.Id });

            return result > 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Error deleting role." });
        }

        public void Dispose()
        {
        }

        public async Task<IdentityRole<int>?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var sql = "SELECT * FROM Roles WHERE Id = @Id";
            var role = await _dbConnection.QuerySingleOrDefaultAsync<IdentityRole<int>>(sql, new { Id = roleId });
            return role;
        }

        public async Task<IdentityRole<int>?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var sql = "SELECT * FROM Roles WHERE NormalizedName = @NormalizedName";
            var role = await _dbConnection.QuerySingleOrDefaultAsync<IdentityRole<int>>(sql, new { NormalizedName = normalizedRoleName });
            return role;
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<int> GetRoleIdAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole<int> role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole<int> role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async  Task<IdentityResult> UpdateAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            var sql = "UPDATE Roles SET Name = @Name, NormalizedName = @NormalizedName, ConcurrencyStamp = @ConcurrencyStamp WHERE Id = @Id";
            var result = await _dbConnection.ExecuteAsync(sql, new
            {
                role.Id,
                role.Name,
                role.NormalizedName,
                role.ConcurrencyStamp
            });

            return result > 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Error updating role." });
        }

        Task<string> IRoleStore<IdentityRole<int>>.GetRoleIdAsync(IdentityRole<int> role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            // Devuelve el Id del usuario como cadena
            return Task.FromResult(role.Id.ToString());
        }

        #endregion

    }
}

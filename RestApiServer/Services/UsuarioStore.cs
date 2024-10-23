using Dapper;
using Microsoft.AspNetCore.Identity;
using RestApiServer.Entities;
using RestApiServer.Repository;
using System.Data;
using System.Security.Claims;

namespace RestApiServer.Services
{
    public class UsuarioStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserClaimStore<ApplicationUser> , IUserRoleStore<ApplicationUser>
    {
        private readonly IAuthRepository  usuariosRepository;

        private readonly IDbConnection _dbConnection;

        public UsuarioStore(IAuthRepository  usuariosRepository, IDbConnection dbConnection)
        {
            this.usuariosRepository = usuariosRepository;
            _dbConnection = dbConnection;
        }

        #region IUserStore<ApplicationUser> Implementation
        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            user.Id = await usuariosRepository.Crear(user);
            return IdentityResult.Success;
        }
        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await usuariosRepository.FindByNameAsync(normalizedUserName);
        }
        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                var query = @"UPDATE [dbo].[Usuarios]
                      SET UserName = @UserName,
                          NormalizedUserName = @NormalizedUserName,
                          Email = @Email,
                          NormalizedEmail = @NormalizedEmail,
                          EmailConfirmed = @EmailConfirmed,
                          PasswordHash = @PasswordHash,
                          SecurityStamp = @SecurityStamp,
                          ConcurrencyStamp = @ConcurrencyStamp,
                          PhoneNumber = @PhoneNumber,
                          PhoneNumberConfirmed = @PhoneNumberConfirmed,
                          TwoFactorEnabled = @TwoFactorEnabled,
                          LockoutEnd = @LockoutEnd,
                          LockoutEnabled = @LockoutEnabled,
                          AccessFailedCount = @AccessFailedCount
                      WHERE Id = @Id";

                var parameters = new
                {
                    user.UserName,
                    user.NormalizedUserName,
                    user.Email,
                    user.NormalizedEmail,
                    user.EmailConfirmed,
                    user.PasswordHash,
                    user.SecurityStamp,
                    user.ConcurrencyStamp,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed,
                    user.TwoFactorEnabled,
                    user.LockoutEnd,
                    user.LockoutEnabled,
                    user.AccessFailedCount,
                    user.Id // Condición para actualizar el usuario correcto
                };

                await _dbConnection.ExecuteAsync(query, parameters);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return IdentityResult.Failed(new IdentityError { Description = $"Error al actualizar el usuario: {ex.Message}" });
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            try
            {
                var query = @"DELETE FROM [dbo].[Usuarios]
                      WHERE Id = @Id";

                var parameters = new
                {
                    user.Id // Condición para eliminar el usuario correcto
                };

                await _dbConnection.ExecuteAsync(query, parameters);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return IdentityResult.Failed(new IdentityError { Description = $"Error al eliminar el usuario: {ex.Message}" });
            }
        }

        public void Dispose()
        {
            
        }
        public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await usuariosRepository.FindByIdAsync(userId);
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<int> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserPasswordStore<ApplicationUser> Implementation
        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }
        #endregion

        #region IUserClaimStore<ApplicationUser> Implementation
        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return await usuariosRepository.GetClaimsAsync(user);
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            await usuariosRepository.AsignarClaims(user, claims);
        }

        public Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            await usuariosRepository.RemoverClaims(user, claims);
        }

        public Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IUserStore<ApplicationUser>.GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Devuelve el Id del usuario como cadena
            return Task.FromResult(user.Id.ToString());
        }

        #endregion

        #region IUserRoleStore Implementation
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var queryRole = "SELECT Id FROM Roles WHERE NormalizedName = @NormalizedName";
            var roleId = await _dbConnection.ExecuteScalarAsync<int>(queryRole, new { NormalizedName = roleName.ToUpper() });

            if (roleId == 0)
            {
                throw new KeyNotFoundException($"Role '{roleName}' not found.");
            }

            var queryUserRole = "INSERT INTO UsuariosRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
            await _dbConnection.ExecuteAsync(queryUserRole, new { UserId = user.Id, RoleId = roleId });
  
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var query = @"
            SELECT r.Name 
            FROM Roles r
            INNER JOIN UsuariosRoles ur ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId";

            var roles = await _dbConnection.QueryAsync<string>(query, new { UserId = user.Id });
            return roles.AsList();
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var query = @"
            SELECT COUNT(*)
            FROM Roles r
            INNER JOIN UsuariosRoles ur ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedName";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserId = user.Id, NormalizedName = roleName.ToUpper() });
            return count > 0;
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var queryRole = "SELECT Id FROM Roles WHERE NormalizedName = @NormalizedName";
            var roleId = await _dbConnection.ExecuteScalarAsync<int>(queryRole, new { NormalizedName = roleName.ToUpper() });

            if (roleId == 0)
            {
                throw new KeyNotFoundException($"Role '{roleName}' not found.");
            }

            var query = "DELETE FROM UsuariosRoles WHERE UserId = @UserId AND RoleId = @RoleId";
            await _dbConnection.ExecuteAsync(query, new { UserId = user.Id, RoleId = roleId });
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var query = @"
            SELECT u.* 
            FROM Usuarios u
            INNER JOIN UsuariosRoles ur ON u.Id = ur.UserId
            INNER JOIN Roles r ON ur.RoleId = r.Id
            WHERE r.NormalizedName = @NormalizedName";

            var users = await _dbConnection.QueryAsync<ApplicationUser>(query, new { NormalizedName = roleName.ToUpper() });
            return users.AsList();
        }

   

        #endregion

    }
}

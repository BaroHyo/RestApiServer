using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestApiServer.DTOs.Usuario;
using RestApiServer.Entities;
using RestApiServer.Filters;
using RestApiServer.Repository;
using RestApiServer.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestApiServer.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuth(this RouteGroupBuilder group)
        {
            group.MapPost("/registrar", Registrar)
                .AddEndpointFilter<FiltroValidaciones<CrearUsuarioDTO>>();
            group.MapPost("/login", Login)
             .AddEndpointFilter<FiltroValidaciones<CredencialesUsuarioDTO>>();

            return group;
        }


        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<IEnumerable<IdentityError>>>> Registrar(
            CrearUsuarioDTO crearUsuarioDTO,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            IMapper mapper,
            IUsuariosRepository usuariosRepositor)
        {
            var usuario = new ApplicationUser
            {
                UserName = crearUsuarioDTO.Cuenta,
                NormalizedUserName = crearUsuarioDTO.Cuenta.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                PersonaId = crearUsuarioDTO.PersonaId
            };

            var resultado = await userManager.CreateAsync(usuario, crearUsuarioDTO.Password);

            if (resultado.Succeeded)
            {
                if (crearUsuarioDTO.Roles != null && crearUsuarioDTO.Roles.Any())
                {
                    var rolesNoExistentes = new List<string>();

                    // Verificar si los roles existen
                    foreach (var role in crearUsuarioDTO.Roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            rolesNoExistentes.Add(role);
                        }
                    }

                    if (rolesNoExistentes.Any())
                    {
                        return TypedResults.BadRequest(new[] {
                         new IdentityError { Description = $"Los siguientes roles no existen: {string.Join(", ", rolesNoExistentes)}." } }.AsEnumerable()); // Asegurando que se devuelve IEnumerable<IdentityError>
                    }

                    // Asignar roles uno por uno usando AddToRoleAsync
                    foreach (var role in crearUsuarioDTO.Roles)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(usuario, role);
                        if (!addRoleResult.Succeeded)
                        {
                            return TypedResults.BadRequest(addRoleResult.Errors.AsEnumerable());
                        }
                    }
                }

                // Crear token de autenticación
                var credecial = mapper.Map<CredencialesUsuarioDTO>(crearUsuarioDTO);

                var credencialesRespuesta = await ConstruirToken(credecial, configuration, userManager, usuariosRepositor);
                return TypedResults.Ok(credencialesRespuesta);
            }
            else
            {
                return TypedResults.BadRequest(resultado.Errors.AsEnumerable());
            }
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<string>>> Login(
            CredencialesUsuarioDTO credencialesUsuarioDTO,
            [FromServices] SignInManager<ApplicationUser> signInManager,
            [FromServices] UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUsuariosRepository usuariosRepositor)
        {
            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.Cuenta);

            if (usuario is null)
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuarioDTO.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO, configuration, userManager, usuariosRepositor);
                return TypedResults.Ok(respuestaAutenticacion);
            }
            else
            {
                return TypedResults.BadRequest("Login incorrecto");
            }
        }

        /*public async static Task<Results<Ok<RespuestaAutenticacionDTO>, NotFound>> RenovarToken(
            IServicioUsuarios servicioUsuarios,
            IConfiguration configuration,
            [FromServices] UserManager<ApplicationUser> userManager)
        {
            var usuario = await servicioUsuarios.ObtenerUsuarioAsync();

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO { Cuenta = usuario.Email! };

            var respuestaAutenticacionDTO = await ConstruirToken(credencialesUsuarioDTO, configuration, userManager);

            return TypedResults.Ok(respuestaAutenticacionDTO);
        }*/

        private static async Task<RespuestaAutenticacionDTO> ConstruirToken(
               CredencialesUsuarioDTO credencialesUsuarioDTO,
               IConfiguration configuration,
               UserManager<ApplicationUser> userManager,
               IUsuariosRepository usuariosRepository)
        {
            // Buscar el usuario una sola vez
            var usuario = await userManager.FindByNameAsync(credencialesUsuarioDTO.Cuenta);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            // Obtener claims base
            var claims = new List<Claim>
            {
                new Claim("cuenta", credencialesUsuarioDTO.Cuenta)
            };

            // Añadir claims de la base de datos
            var claimsDB = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsDB);

            // Obtener datos adicionales del usuario
            var user = await usuariosRepository.FindUsuarioAsync(usuario.Id);

            // Generar las credenciales de firma
            var llave = Llaves.ObtenerLlave(configuration);
            var creds = new SigningCredentials(llave.First(), SecurityAlgorithms.HmacSha256);

            // Definir expiración
            var expiracion = DateTime.UtcNow.AddYears(1);

            // Crear el token de seguridad
            var tokenDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            // Generar el token en string
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            // Retornar la respuesta de autenticación
            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion,
                Usuario = user
            };
        }

    }
}

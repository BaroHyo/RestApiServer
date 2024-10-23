
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.Usuario;
using RestApiServer.Entities;
using RestApiServer.Repository;
using static Dapper.SqlMapper;

namespace RestApiServer.Endpoints
{
    public static class UsuariosEndpoints
    {
        public static RouteGroupBuilder MapUsuario(this RouteGroupBuilder group)
        {

            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("usuario-get"));
            group.MapGet("/{id:int}", ObtenerPorId);


            return group;
        }

        static async Task<Ok<List<UsuarioResponseDTO>>> ObtenerTodos(IUsuarioRepository repositorio)
        {
            var categorias = await repositorio.ObtenerTodos();
            return TypedResults.Ok(categorias);
        }


        static async Task<Results<Ok<UsuarioResponseDTO>, NotFound>> ObtenerPorId(int id, IUsuarioRepository repositorio)
        {
            var categoria = await repositorio.ObtenerPorId(id);

            if (categoria is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(categoria);
        }
        static async Task<Results<Created<UsuarioResponseDTO>, ValidationProblem, BadRequest<IEnumerable<IdentityError>>>> Crear(
                          CreateUsuarioDTO  createUsuarioDTO,
                          [FromServices] UserManager<ApplicationUser> userManager,
                            [FromServices] RoleManager<IdentityRole<int>> roleManager,
                          IOutputCacheStore outputCacheStore,
                          IMapper mapper)
        {

            var usuario = new ApplicationUser
            {
                UserName = createUsuarioDTO.UserName,
                NormalizedUserName = createUsuarioDTO.UserName.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                PersonaId = createUsuarioDTO.PersonaId
            };
            var resultado = await userManager.CreateAsync(usuario, createUsuarioDTO.Password);

            if (resultado.Succeeded)
            {
                if (createUsuarioDTO.Roles != null && createUsuarioDTO.Roles.Any())
                {
                    var rolesNoExistentes = new List<string>();

                    foreach (var role in createUsuarioDTO.Roles)
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
                    foreach (var role in createUsuarioDTO.Roles)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(usuario, role);
                        if (!addRoleResult.Succeeded)
                        {
                            return TypedResults.BadRequest(addRoleResult.Errors.AsEnumerable());
                        }
                    }
                }
                await outputCacheStore.EvictByTagAsync("usuario-get", default);

                var entityDTO = mapper.Map<UsuarioResponseDTO>(usuario);

                return TypedResults.Created($"/api/usuario/{usuario.Id}", entityDTO); 

 
                }
            else
            {
                return TypedResults.BadRequest(resultado.Errors.AsEnumerable());
            }

           



        }

    }
}

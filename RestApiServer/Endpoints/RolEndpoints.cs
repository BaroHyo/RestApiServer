using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using RestApiServer.DTOs.Roles;


namespace RestApiServer.Endpoints
{
    public static class RolEndpoints
    {
        public static RouteGroupBuilder MapRol(this RouteGroupBuilder group)
        {
            group.MapPost("/", Crear);
            group.MapGet("/{roleId:int}", FindById);
            group.MapPut("/{roleId:int}", Update);
            group.MapDelete("/{roleId:int}", Delete);
         
            return group;
        }

        static async Task<IResult> FindById(int roleId, RoleManager<IdentityRole<int>> roleManager, IMapper mapper)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role is null)
            {
                return Results.NotFound();
            }

            var roleDTO = mapper.Map<RoleDTO>(role);
            return Results.Ok(roleDTO);
        }

        static async Task<Results<Created<RoleDTO>, BadRequest<IEnumerable<IdentityError>>>> Crear(CrearRoleDTO crearRole, RoleManager<IdentityRole<int>> roleManager, IMapper mapper)
        {
            var role = new IdentityRole<int>
            {
                Name = crearRole.Name,
                NormalizedName = crearRole.Name.ToUpper()
            };

            var result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                var roleDTO = mapper.Map<RoleDTO>(role);

                return TypedResults.Created($"/api/rol/{role.Id}", roleDTO);
            }
            else
            {
                return TypedResults.BadRequest(result.Errors);
            }
        }

        static async Task<Results<NotFound, NoContent, BadRequest<IEnumerable<IdentityError>>>> Update(
         RoleManager<IdentityRole<int>> roleManager,
         int roleId,
         CrearRoleDTO updatedRole)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role is null)
            {
                return TypedResults.NotFound();
            }

            role.Name = updatedRole.Name;
            role.NormalizedName = updatedRole.Name.ToUpper();

            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return TypedResults.NoContent();
            }

            return TypedResults.BadRequest(result.Errors);
        }

        static async Task<Results<NotFound, NoContent, BadRequest<IEnumerable<IdentityError>>>> Delete(RoleManager<IdentityRole<int>> roleManager, int roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role is null)
            {
                return TypedResults.NotFound();
            }

            var result = await roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return TypedResults.NoContent();
            }

            return TypedResults.BadRequest(result.Errors);
        }


    }
}

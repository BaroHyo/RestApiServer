using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.CategoriaDet;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class CategoriaDetEndpoints
    {
        public static RouteGroupBuilder MapCategoriasDet(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("categoria-det-get"));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPost("/", Crear);
            group.MapPut("/{id:int}", Actualizar);
            group.MapDelete("/{id:int}", Borrar);
            group.MapGet("/combox/{codigo}", ObtenerCombox).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("categoria-combox-get"));

            return group;
        }

        static async Task<Ok<List<CategoriaDetDTO>>> ObtenerTodos(ICategoriaDetRepository repositorio, IMapper mapper)
        {
            var categoriaDet = await repositorio.FindAll();
            var categoriasDetDTO = mapper.Map<List<CategoriaDetDTO>>(categoriaDet);
            return TypedResults.Ok(categoriasDetDTO);
        }

        static async Task<Results<Ok<CategoriaDetDTO>, NotFound>> ObtenerPorId(int id, ICategoriaDetRepository repositorio, IMapper mapper)
        {
            var detalle = await repositorio.FindById(id);

            if (detalle is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<CategoriaDetDTO>(detalle));
        }

        static async Task<Results<Created<CategoriaDetDTO>, ValidationProblem>> Crear(
                                CrearCategoriaDetDTO crearCategoriaDetDTO,
                                ICategoriaDetRepository repositorio,
                                IOutputCacheStore outputCacheStore,
                                IMapper mapper)
        {

            var detalle = mapper.Map<CategoriaDet>(crearCategoriaDetDTO);

            var id = await repositorio.Create(detalle);
            detalle.Id = id;

            await outputCacheStore.EvictByTagAsync("categoria-det-get", default);

            var detalleDTO = mapper.Map<CategoriaDetDTO>(detalle);

            return TypedResults.Created($"/api/categoria-detalle/{detalle.Id}", detalleDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> Actualizar(int id,
                         CrearCategoriaDetDTO  crearCategoriaDetDTO,
                         ICategoriaDetRepository repositorio,
                         IOutputCacheStore outputCacheStore,
                         IMapper mapper)
        {

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var entity = mapper.Map<CategoriaDet>(crearCategoriaDetDTO);
            entity.Id = id;

            await repositorio.Update(entity);
            await outputCacheStore.EvictByTagAsync("categoria-det-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Borrar(int id, ICategoriaDetRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Delete(id);
            await outputCacheStore.EvictByTagAsync("categoria-det-get", default);
            return TypedResults.NoContent();
        }
        static async Task<Ok<List<CategoriaDetDTO>>> ObtenerCombox(string codigo, ICategoriaDetRepository repositorio, IMapper mapper)
        {
            var categoriaDet = await repositorio.FindAllCombox(codigo);
            var categoriasDetDTO = mapper.Map<List<CategoriaDetDTO>>(categoriaDet);
            return TypedResults.Ok(categoriasDetDTO);
        }

    }
}

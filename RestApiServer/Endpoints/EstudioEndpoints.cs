using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.Estudio;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class EstudioEndpoints
    {
        public static RouteGroupBuilder MapEstudio(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("estudio-get"));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPost("/", Crear);
            group.MapPut("/{id:int}", Actualizar);
            group.MapDelete("/{id:int}", Borrar);
            return group;
        }

        static async Task<Ok<List<EstudioDTO>>> ObtenerTodos(IEstudioRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindAll();
            var entityDTO = mapper.Map<List<EstudioDTO>>(entity);
            return TypedResults.Ok(entityDTO);
        }

        static async Task<Results<Ok<EstudioDTO>, NotFound>> ObtenerPorId(int id, IEstudioRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindById(id);

            if (entity is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<EstudioDTO>(entity));
        }

        static async Task<Results<Created<EstudioDTO>, ValidationProblem>> Crear(
                                CrearEstudioDTO crearEstudioDTO,
                                IEstudioRepository repositorio,
                                IOutputCacheStore outputCacheStore,
                                IMapper mapper)
        {

            var entity = mapper.Map<Estudio>(crearEstudioDTO);

            var id = await repositorio.Create(entity);
            entity.Id = id;

            await outputCacheStore.EvictByTagAsync("estudio-get", default);

            var entityDTO = mapper.Map<EstudioDTO>(entity);

            return TypedResults.Created($"/api/estudio/{entity.Id}", entityDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> Actualizar(int id,
                         CrearEstudioDTO crearEstudioDTO,
                         IEstudioRepository repositorio,
                         IOutputCacheStore outputCacheStore,
                         IMapper mapper)
        {

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var entity = mapper.Map<Estudio>(crearEstudioDTO);
            entity.Id = id;

            await repositorio.Update(entity);
            await outputCacheStore.EvictByTagAsync("estudio-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Borrar(int id, IEstudioRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Delete(id);
            await outputCacheStore.EvictByTagAsync("estudio-get", default);
            return TypedResults.NoContent();
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.TipoEstudio;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class TipoEstudioEndpoints
    {
        public static RouteGroupBuilder MapTipoEstudio(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("tipo-estudio-get"));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPost("/", Crear);
            group.MapPut("/{id:int}", Actualizar);
            group.MapDelete("/{id:int}", Borrar);
            return group;
        }
        static async Task<Ok<List<TipoEstudioDTO>>> ObtenerTodos(ITipoEstudioRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindAll();
            var entityDTO = mapper.Map<List<TipoEstudioDTO>>(entity);
            return TypedResults.Ok(entityDTO);
        }

        static async Task<Results<Ok<TipoEstudioDTO>, NotFound>> ObtenerPorId(int id, ITipoEstudioRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindById(id);

            if (entity is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<TipoEstudioDTO>(entity));
        }

        static async Task<Results<Created<TipoEstudioDTO>, ValidationProblem>> Crear(
                                CrearTipoEstudioDTO crearTipoEstudioDTO,
                                ITipoEstudioRepository repositorio,
                                IOutputCacheStore outputCacheStore,
                                IMapper mapper)
        {

            var entity = mapper.Map<TipoEstudio>(crearTipoEstudioDTO);

            var id = await repositorio.Create(entity);
            entity.Id = id;

            await outputCacheStore.EvictByTagAsync("tipo-estudio-get", default);

            var entityDTO = mapper.Map<TipoEstudioDTO>(entity);

            return TypedResults.Created($"/api/tipo-estudio/{entity.Id}", entityDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> Actualizar(int id,
                         CrearTipoEstudioDTO crearTipoEstudioDTO,
                         ITipoEstudioRepository repositorio,
                         IOutputCacheStore outputCacheStore,
                         IMapper mapper)
        {

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var entity = mapper.Map<TipoEstudio>(crearTipoEstudioDTO);
            entity.Id = id;

            await repositorio.Update(entity);       
            await outputCacheStore.EvictByTagAsync("tipo-estudio-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Borrar(int id, ITipoEstudioRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Delete(id);
            await outputCacheStore.EvictByTagAsync("tipo-estudio-get", default);
            return TypedResults.NoContent();
        }
    }
}

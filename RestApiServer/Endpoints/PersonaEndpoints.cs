using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.Persona;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class PersonaEndpoints
    {
        public static RouteGroupBuilder MapPersona(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("persona-get"));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPost("/", Crear);
            group.MapPut("/{id:int}", Actualizar);
            group.MapDelete("/{id:int}", Borrar);
            return group;
        }
        static async Task<Ok<List<PersonaDTO>>> ObtenerTodos(IPersonaRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindAll();
            var entityDTO = mapper.Map<List<PersonaDTO>>(entity);
            return TypedResults.Ok(entityDTO);
        }

        static async Task<Results<Ok<PersonaDTO>, NotFound>> ObtenerPorId(int id, IPersonaRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindById(id);

            if (entity is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<PersonaDTO>(entity));
        }

        static async Task<Results<Created<PersonaDTO>, ValidationProblem>> Crear(
                                CrearPersonaDTO crearPersonaDTO,
                                IPersonaRepository repositorio,
                                IOutputCacheStore outputCacheStore,
                                IMapper mapper)
        {

            var entity = mapper.Map<Persona>(crearPersonaDTO);

            var id = await repositorio.Create(entity);
            entity.Id = id;

            await outputCacheStore.EvictByTagAsync("persona-get", default);

            var entityDTO = mapper.Map<PersonaDTO>(entity);

            return TypedResults.Created($"/api/persona/{entity.Id}", entityDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> Actualizar(int id,
                         CrearPersonaDTO crearPersonaDTO,
                         IPersonaRepository repositorio,
                         IOutputCacheStore outputCacheStore,
                         IMapper mapper)
        {

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var entity = mapper.Map<Persona>(crearPersonaDTO);
            entity.Id = id;

            await repositorio.Update(entity);
            await outputCacheStore.EvictByTagAsync("persona-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Borrar(int id, IPersonaRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Delete(id);
            await outputCacheStore.EvictByTagAsync("persona-get", default);
            return TypedResults.NoContent();
        }
    }
}

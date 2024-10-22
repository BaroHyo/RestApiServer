using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.Medico;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class MedicoEndpoints
    {
        public static RouteGroupBuilder MapMedico(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerTodos).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("medico-get"));
            group.MapGet("/combox/", ObtenerMedico).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("med-cmb-get"));
            group.MapGet("/{id:int}", ObtenerPorId);
            group.MapPost("/", Crear);
            group.MapPut("/{id:int}", Actualizar);
            group.MapDelete("/{id:int}", Borrar);
            return group;
        }
        static async Task<Ok<List<MedicoDTO>>> ObtenerTodos(IMedicoRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindAll();
            var entityDTO = mapper.Map<List<MedicoDTO>>(entity);
            return TypedResults.Ok(entityDTO);
        }
        static async Task<Ok<List<MedicoComboxDTO>>> ObtenerMedico(IMedicoRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindByMedico();
           
            return TypedResults.Ok(entity);
        }
        static async Task<Results<Ok<MedicoDTO>, NotFound>> ObtenerPorId(int id, IMedicoRepository repositorio, IMapper mapper)
        {
            var entity = await repositorio.FindById(id);

            if (entity is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(mapper.Map<MedicoDTO>(entity));
        }

        static async Task<Results<Created<MedicoDTO>, ValidationProblem>> Crear(
                                CrearMedicoDTO crearMedicoDTO,
                                IMedicoRepository repositorio,
                                IOutputCacheStore outputCacheStore,
                                IMapper mapper)
        {

            var entity = mapper.Map<Medico>(crearMedicoDTO);

            var id = await repositorio.Create(entity);
            entity.Id = id;

            await outputCacheStore.EvictByTagAsync("medico-get", default);

            var entityDTO = mapper.Map<MedicoDTO>(entity);

            return TypedResults.Created($"/api/medico/{entity.Id}", entityDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> Actualizar(int id,
                         CrearMedicoDTO crearMedicoDTO,
                         IMedicoRepository repositorio,
                         IOutputCacheStore outputCacheStore,
                         IMapper mapper)
        {

            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var entity = mapper.Map<Medico>(crearMedicoDTO);
            entity.Id = id;

            await repositorio.Update(entity);
            await outputCacheStore.EvictByTagAsync("medico-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Borrar(int id, IMedicoRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Delete(id);
            await outputCacheStore.EvictByTagAsync("medico-get", default);
            return TypedResults.NoContent();
        }
    }
}

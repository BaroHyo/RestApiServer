using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using RestApiServer.DTOs.Categoria;
using RestApiServer.Entities;
using RestApiServer.Repository;

namespace RestApiServer.Endpoints
{
    public static class CategoriaEndpoints
    {

        public static RouteGroupBuilder MapCategorias(this RouteGroupBuilder group)
        {
            group.MapGet("/", ObtenerCategoria).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("categoria-get"));
            group.MapGet("/{id:int}", ObtenerCategoriaPorId);
            group.MapPost("/", CrearCategoria);  
            group.MapPut("/{id:int}", ActualizarCategoria);
            group.MapDelete("/{id:int}", BorrarCategoria);
            return group;
        }

        static async Task<Ok<List<CategoriaDTO>>> ObtenerCategoria(ICategoriaRepository repositorio, IMapper mapper)
        {
            var categorias = await repositorio.ObtenerTodos();
            var categoriasDTO = mapper.Map<List<CategoriaDTO>>(categorias);
            return TypedResults.Ok(categoriasDTO);
        }
        static async Task<Results<Ok<CategoriaDTO>, NotFound>> ObtenerCategoriaPorId(int id, ICategoriaRepository repositorio, IMapper mapper)
        {
            var categoria = await repositorio.ObtenerPorId(id);

            if (categoria is null)
            {
                return TypedResults.NotFound();
            }

            var categoriaDTO = mapper.Map<CategoriaDTO>(categoria);

            return TypedResults.Ok(categoriaDTO);
        }

        static async Task<Results<Created<CategoriaDTO>, ValidationProblem>> CrearCategoria(
                                 CrearCategoriaDTO crearCategoriaDTO,
                                 ICategoriaRepository repositorio,
                                 IOutputCacheStore outputCacheStore,
                                 IMapper mapper)
        {

            var categoria = mapper.Map<Categoria>(crearCategoriaDTO);

            var response = await repositorio.Crear(categoria);
            await outputCacheStore.EvictByTagAsync("categoria-get", default);
            var categoriaDTO = mapper.Map<CategoriaDTO>(categoria);

            return TypedResults.Created($"/api/categorias/{categoria.Id}", categoriaDTO);
        }

        static async Task<Results<NotFound, NoContent, ValidationProblem>> ActualizarCategoria(int id,
                            CrearCategoriaDTO crearCategoriaDTO, 
                            ICategoriaRepository repositorio, 
                            IOutputCacheStore outputCacheStore,
                            IMapper mapper)
        {
           
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            var categoria = mapper.Map<Categoria>(crearCategoriaDTO);
            categoria.Id = id;

            await repositorio.Actualizar(categoria);
            await outputCacheStore.EvictByTagAsync("categoria-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> BorrarCategoria(int id, ICategoriaRepository repositorio, IOutputCacheStore outputCacheStore)
        {
            var existe = await repositorio.Existe(id);

            if (!existe)
            {
                return TypedResults.NotFound();
            }

            await repositorio.Borrar(id);
            await outputCacheStore.EvictByTagAsync("categoria-get", default);
            return TypedResults.NoContent();
        }
    }
}

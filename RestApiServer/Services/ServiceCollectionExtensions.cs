using RestApiServer.Repository;

namespace RestApiServer.Services
{
     public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ICategoriaDetRepository, CategoriaDetRepository>();
            services.AddScoped<IEstudioRepository, EstudioRepository>();
            services.AddScoped<IMedicoRepository, MedicoRepository>();
            services.AddScoped<IPersonaRepository, PersonaRepository>();
            services.AddScoped<ITipoEstudioRepository, TipoEstudioRepository>();
            services.AddScoped<IErrorRepository, ErrorRepository>();
            services.AddScoped<IUsuariosRepository, UsuariosRepository>();


            return services;
        }
    }

}

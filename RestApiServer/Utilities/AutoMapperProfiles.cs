using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RestApiServer.DTOs.Categoria;
using RestApiServer.DTOs.CategoriaDet;
using RestApiServer.DTOs.Estudio;
using RestApiServer.DTOs.Medico;
using RestApiServer.DTOs.Persona;
using RestApiServer.DTOs.Roles;
using RestApiServer.DTOs.TipoEstudio;
using RestApiServer.DTOs.Usuario;
using RestApiServer.Entities;

namespace RestApiServer.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Categoria Mappings
            CreateMap<CategoriaDTO, Categoria>().ReverseMap();
            CreateMap<CrearCategoriaDTO, Categoria>();

            // CategoriaDet Mappings
            CreateMap<CategoriaDetDTO, CategoriaDet>().ReverseMap();
            CreateMap<CrearCategoriaDetDTO, CategoriaDet>();

            // Estudio Mappings
            CreateMap<EstudioDTO, Estudio>().ReverseMap();
            CreateMap<CrearEstudioDTO, Estudio>();

            // TipoEstudio Mappings
            CreateMap<TipoEstudioDTO, TipoEstudio>().ReverseMap();
            CreateMap<CrearTipoEstudioDTO, TipoEstudio>();

            // Persona Mappings
            CreateMap<PersonaDTO, Persona>().ReverseMap();
            CreateMap<CrearPersonaDTO, Persona>();

            // Medico Mappings
            CreateMap<MedicoDTO, Medico>().ReverseMap();
            CreateMap<CrearMedicoDTO, Medico>();
           

            // Medico role
            CreateMap<RoleDTO, IdentityRole<int>>().ReverseMap();


            CreateMap<CredencialesUsuarioDTO, CrearUsuarioDTO>().ReverseMap();

            CreateMap<CreateUsuarioDTO, ApplicationUser>()
                        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                        .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Ignorar el PasswordHash si necesitas manejarlo por separado
                        .ForMember(dest => dest.Persona, opt => opt.Ignore()) // Ignorar si no quieres crear una instancia de Persona
                        .ForMember(dest => dest.PersonaId, opt => opt.Ignore()); // Ignorar si no quieres asignar PersonaId


        }
    }
}

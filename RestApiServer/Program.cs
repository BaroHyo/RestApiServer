using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RestApiServer.Endpoints;
using RestApiServer.Entities;
using RestApiServer.Repository;
using RestApiServer.Services;
using RestApiServer.Utilities;
using System.Data;

var builder = WebApplication.CreateBuilder(args);


var keysPath = "/var/lib/paciente/dp-keys";
if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

// Configurar la protección de datos
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath)) // Ruta a las claves
    .SetApplicationName("SistemaPaciente"); // Nombre de la aplicación

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(config =>
    {
        config.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddOutputCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories
builder.Services.AddRepositories();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // FluentValidation
builder.Services.AddProblemDetails();


builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;

    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKeys = Llaves.ObtenerTodasLasLlave(builder.Configuration),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddTransient<IUserStore<ApplicationUser>, UsuarioStore>();
builder.Services.AddTransient<IRoleStore<IdentityRole<int>>, UserRoleStore>();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
  .AddRoles<IdentityRole<int>>()
  .AddUserStore<UsuarioStore>()
  .AddRoleStore<UserRoleStore>();
builder.Services.AddTransient<SignInManager<ApplicationUser>>();


var app = builder.Build();

// Swagger
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseSwagger();
app.UseSwaggerUI();


// Global exception handler
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error;

    if (exception is not null)
    {
        var error = new Error
        {
            Fecha = DateTime.UtcNow,
            MensajeDeError = exception.Message,
            StackTrace = exception.StackTrace
        };

        var errorRepository = context.RequestServices.GetRequiredService<IErrorRepository>();
        await errorRepository.Crear(error);

        await Results.BadRequest(new { tipo = "error", mensaje = "Ha ocurrido un mensaje de error inesperado", estatus = 500 })
                      .ExecuteAsync(context);
    }
}));

app.UseHttpsRedirection();

app.UseCors();
app.UseOutputCache();
app.UseAuthorization();

app.MapGet("/", () => "¡Hola, mundo!");
// Define endpoints
app.MapGroup("/api/categorias").MapCategorias(); //.RequireAuthorization();
app.MapGroup("/api/categoria-detalle").MapCategoriasDet();
app.MapGroup("/api/estudio").MapEstudio();
app.MapGroup("/api/tipo-estudio").MapTipoEstudio();
app.MapGroup("/api/persona").MapPersona();
app.MapGroup("/api/medico").MapMedico();
app.MapGroup("/api/auth").MapAuth();
app.MapGroup("/api/rol").MapRol();

// Run the app
app.Run();

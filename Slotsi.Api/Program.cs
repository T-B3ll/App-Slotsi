using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Scalar.AspNetCore;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Repositories;
using Slotsi.Api.Service;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración y validación del Patrón Options (Mongo y JWT)
builder.Services.AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<JwtSetting>()
    .Bind(builder.Configuration.GetSection(JwtSetting.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// 2. Cliente de MongoDB como Singleton seguro entre hilos
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
    clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
    return new MongoClient(clientSettings);
});

// 3. Registro de Repositorios y Servicios propios de Slotsi
builder.Services.AddSingleton<IPasswordHasher<UsuarioEntity>, PasswordHasher<UsuarioEntity>>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<ITrabajadorRepository, TrabajadoreRepository>();
builder.Services.AddSingleton<IServicioRepository, ServiciosRepository>();
builder.Services.AddSingleton<ICalificacionRepository, CalificacionRepository>();
builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<INegocioRepository, NegocioRepository>();
builder.Services.AddSingleton<ICitaRepository, CitaRepository>();

// 4. Configuración de Autenticación JWT y validación de tokens revocados
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSetting>>((opciones, jwt) =>
    {
        var settings = jwt.Value;
        // Conserva los nombres originales de los claims ("sub", "jti", "email"...)
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            ValidateLifetime = true,
            // Sin tolerancia: un token pasado de su hora de expiración se rechaza de inmediato
            ClockSkew = TimeSpan.Zero,
        };
        opciones.Events = new JwtBearerEvents
        {
            // Rechaza tokens cerrados o revocados (Logout)
            OnTokenValidated = async contexto =>
            {
                var jti = contexto.Principal?.FindFirst("jti")?.Value;
                var usuarios = contexto.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
                if (jti is null || await usuarios.TokenRevocadoAsync(jti, contexto.HttpContext.RequestAborted))
                    contexto.Fail("Token revocado.");
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddRouting(opciones => opciones.LowercaseUrls = true);

// 5. Documentación OpenAPI / Scalar / Swagger
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6. Manejo de excepciones globales
builder.Services.AddExceptionHandler<MongoExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// 7. Asegurar índices en MongoDB al arrancar la API
await app.Services.GetRequiredService<IUsuarioRepository>().AsegurarIndicesAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI y Scalar
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Slotsi API - Documentaci n";
        options.Theme = ScalarTheme.BluePlanet;
        options.OpenApiRoutePattern = "/openapi/v1.json";
    });

    // Swagger UI clásico como alternativa
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Slotsi API v1");
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}
else
{
    app.UseHttpsRedirection();
}

// 8. Pipeline de autenticación y autorización (¡Orden crucial!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
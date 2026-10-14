using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Scalar.AspNetCore;
using Slotsi.Api.Configuration;
using Slotsi.Api.Infrastructure;
using Slotsi.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
    clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
    return new MongoClient(clientSettings);
});


builder.Services.AddSingleton<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<INegocioRepository, NegocioRepository>();

builder.Services.AddControllers();
builder.Services.AddRouting(opciones => opciones.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<MongoExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

   
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

  
        app.MapScalarApiReference(options =>
        {
            options.Title = "Slotsi API - Documentación";
            options.Theme = ScalarTheme.BluePlanet;
            options.OpenApiRoutePattern = "/openapi/v1.json";
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
    }

    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<UsuarioEntity> _usuarios;

    public UsuarioRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        _usuarios = cliente
            .GetDatabase(settings.DatabaseName)
            .GetCollection<UsuarioEntity>(settings.UsuariosCollection);
    }

  
    public Task<List<UsuarioEntity>> ObtenerTodosAsync(CancellationToken ct = default) =>
        _usuarios.Find(_ => true).ToListAsync(ct);

    public async Task<UsuarioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default) =>
        await _usuarios.Find(u => u.Id == id).FirstOrDefaultAsync(ct);

    public Task CrearAsync(UsuarioEntity usuario, CancellationToken ct = default) =>
        _usuarios.InsertOneAsync(usuario, cancellationToken: ct);

    public async Task<bool> ActualizarAsync(UsuarioEntity usuario, CancellationToken ct = default)
    {
        var resultado = await _usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        var resultado = await _usuarios.DeleteOneAsync(u => u.Id == id, ct);
        return resultado.DeletedCount > 0;
    }


    public async Task<UsuarioEntity?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default)
    {
       
        var filtro = Builders<UsuarioEntity>.Filter.Eq(u => u.Correo, correo.ToLower().Trim());
        return await _usuarios.Find(filtro).FirstOrDefaultAsync(ct);
    }

    public async Task<UsuarioEntity?> BuscarPorTextoAsync(string texto, CancellationToken ct = default)
    {
        var patron = $@"^\s*{System.Text.RegularExpressions.Regex.Escape(texto.Trim())}\s*$";
        var regex = new BsonRegularExpression(patron, "i");

        var filtro = Builders<UsuarioEntity>.Filter.Or(
            Builders<UsuarioEntity>.Filter.Regex(u => u.Correo, regex),
            Builders<UsuarioEntity>.Filter.Regex(u => u.NombreCompleto, regex)
        );

        return await _usuarios.Find(filtro).FirstOrDefaultAsync(ct);
    }
}
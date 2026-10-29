using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IMongoCollection<TokenRevocadoEntity> _revocados;
    private readonly IMongoCollection<UsuarioEntity> _usuarios;

    public UsuarioRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        var db = cliente.GetDatabase(settings.DatabaseName);
        _usuarios = db.GetCollection<UsuarioEntity>(settings.UsuariosCollection);
        _revocados = db.GetCollection<TokenRevocadoEntity>(settings.TokensRevocadosCollection);
    }

    public async Task AsegurarIndicesAsync(CancellationToken ct = default)
    {
        await _usuarios.Indexes.CreateOneAsync(new CreateIndexModel<UsuarioEntity>(
        Builders<UsuarioEntity>.IndexKeys.Ascending(u => u.Correo),
        new CreateIndexOptions { Unique = true }), cancellationToken: ct);
        await _revocados.Indexes.CreateOneAsync(new CreateIndexModel<TokenRevocadoEntity>(
        Builders<TokenRevocadoEntity>.IndexKeys.Ascending(t => t.ExpiraEn),
        new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }), cancellationToken: ct);
    }

    public Task<List<UsuarioEntity>> ObtenerTodosAsync(CancellationToken ct = default) =>
        _usuarios.Find(_ => true).ToListAsync(ct);

    public async Task<UsuarioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var oid)) return null;
        var filtro = Builders<UsuarioEntity>.Filter.Eq("_id", oid);
        return await _usuarios.Find(filtro).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CrearAsync(UsuarioEntity usuario, CancellationToken ct = default)
    {
        try
        {
            await _usuarios.InsertOneAsync(usuario, cancellationToken: ct);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<bool> ActualizarAsync(UsuarioEntity usuario, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(usuario.Id, out var oid)) return false;

        var filtro = Builders<UsuarioEntity>.Filter.Eq("_id", oid);

        var update = Builders<UsuarioEntity>.Update
            .Set(u => u.NombreCompleto, usuario.NombreCompleto)
            .Set(u => u.Correo, usuario.Correo)
            .Set(u => u.Telefono, usuario.Telefono)
            .Set(u => u.Cedula, usuario.Cedula)
            .Set(u => u.Contrasena, usuario.Contrasena)
            .Set(u => u.TipoUsuario, usuario.TipoUsuario)
            .Set(u => u.EstaActivo, usuario.EstaActivo)
            .Set(u => u.ResetTokenHash, usuario.ResetTokenHash)
            .Set(u => u.ResetTokenExpira, usuario.ResetTokenExpira);

        var resultado = await _usuarios.UpdateOneAsync(filtro, update, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstadoAsync(string id, bool activo, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var oid)) return false;

        var filtro = Builders<UsuarioEntity>.Filter.Eq("_id", oid);
        var update = Builders<UsuarioEntity>.Update.Set(u => u.EstaActivo, activo);
        var result = await _usuarios.UpdateOneAsync(filtro, update, cancellationToken: ct);

        return result.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var oid)) return false;

        var filtro = Builders<UsuarioEntity>.Filter.Eq("_id", oid);
        var resultado = await _usuarios.DeleteOneAsync(filtro, ct);
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

    public Task RevocarTokenAsync(string jti, DateTime expiraEn, CancellationToken ct = default) =>
        _revocados.ReplaceOneAsync(t => t.Jti == jti,
        new TokenRevocadoEntity { Jti = jti, ExpiraEn = expiraEn },
        new ReplaceOptions { IsUpsert = true }, ct);

    public async Task<bool> TokenRevocadoAsync(string jti, CancellationToken ct = default) =>
        await _revocados.Find(t => t.Jti == jti).AnyAsync(ct);
}
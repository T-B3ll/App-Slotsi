using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class NegocioRepository : INegocioRepository
{
    private readonly IMongoCollection<NegocioEntity> _negocios;

    public NegocioRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        _negocios = cliente
            .GetDatabase(settings.DatabaseName)
            .GetCollection<NegocioEntity>(settings.NegociosCollection);
    }

    public Task<List<NegocioEntity>> ObtenerTodosAsync(CancellationToken ct = default) =>
        _negocios.Find(_ => true).ToListAsync(ct);

    public async Task<NegocioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return null;
        return await _negocios.Find(n => n.Id.Equals(objectId)).FirstOrDefaultAsync(ct);
    }

    public async Task<NegocioEntity?> ObtenerPorUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(usuarioId, out var objectId)) return null;
        return await _negocios.Find(n => n.UsuarioId.Equals(objectId)).FirstOrDefaultAsync(ct);
    }

    public Task CrearAsync(NegocioEntity negocio, CancellationToken ct = default) =>
        _negocios.InsertOneAsync(negocio, cancellationToken: ct);

    public async Task<bool> ActualizarAsync(NegocioEntity negocio, CancellationToken ct = default)
    {
        var resultado = await _negocios.ReplaceOneAsync(n => n.Id.Equals(negocio.Id), negocio, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return false;
        var resultado = await _negocios.DeleteOneAsync(n => n.Id.Equals(objectId), ct);
        return resultado.DeletedCount > 0;
    }
}
using Microsoft.Extensions.Options;
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

    public async Task<NegocioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default) =>
        await _negocios.Find(n => n.Id == id).FirstOrDefaultAsync(ct);

    public async Task<NegocioEntity?> ObtenerPorUsuarioIdAsync(string usuarioId, CancellationToken ct = default) =>
        await _negocios.Find(n => n.UsuarioId == usuarioId).FirstOrDefaultAsync(ct);

    public Task CrearAsync(NegocioEntity negocio, CancellationToken ct = default) =>
        _negocios.InsertOneAsync(negocio, cancellationToken: ct);

    public async Task<bool> ActualizarAsync(NegocioEntity negocio, CancellationToken ct = default)
    {
        var resultado = await _negocios.ReplaceOneAsync(n => n.Id == negocio.Id, negocio, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        var resultado = await _negocios.DeleteOneAsync(n => n.Id == id, ct);
        return resultado.DeletedCount > 0;
    }
}
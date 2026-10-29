using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class CitaRepository : ICitaRepository
{
    private readonly IMongoCollection<CitaEntity> _citas;

    public CitaRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        _citas = cliente.GetDatabase(settings.DatabaseName).GetCollection<CitaEntity>("Citas");
    }
    public Task<List<CitaEntity>> ObtenerPorRangoAsync(
    string negocioId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(negocioId, out var negocioObjId))
            return Task.FromResult(new List<CitaEntity>());

        var inicio = desde.Date;
        var fin = hasta.Date.AddDays(1); // límite superior exclusivo

        return _citas.Find(c =>
                c.NegocioId == negocioObjId &&
                c.FechaHoraInicio >= inicio &&
                c.FechaHoraInicio < fin &&
                c.Estado != "Cancelada")
            .SortBy(c => c.FechaHoraInicio)
            .ToListAsync(ct);
    }
    public Task<List<CitaEntity>> ObtenerTodasAsync(CancellationToken ct = default) =>
        _citas.Find(_ => true).ToListAsync(ct);

    public async Task<CitaEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return null;
        return await _citas.Find(c => c.Id == objectId).FirstOrDefaultAsync(ct);
    }

    public Task<List<CitaEntity>> ObtenerPorRangoConCanceladasAsync(
    string negocioId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(negocioId, out var negocioObjId))
            return Task.FromResult(new List<CitaEntity>());

        var inicio = desde;
        var fin = hasta;

        return _citas.Find(c =>
                c.NegocioId == negocioObjId &&
                c.FechaHoraInicio >= inicio &&
                c.FechaHoraInicio <= fin)
            .SortByDescending(c => c.FechaHoraInicio)
            .ToListAsync(ct);
    }

    public Task<List<CitaEntity>> ObtenerPorFechaAsync(DateTime fecha, CancellationToken ct = default)
    {
        var inicioDia = fecha.Date;
        var finDia = inicioDia.AddDays(1).AddTicks(-1);
        return _citas.Find(c => c.FechaHoraInicio >= inicioDia && c.FechaHoraInicio <= finDia).ToListAsync(ct);
    }

    public Task<List<CitaEntity>> ObtenerPorUsuarioAsync(string clienteId, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(clienteId, out var clienteObjId)) return Task.FromResult(new List<CitaEntity>());
        return _citas.Find(c => c.ClienteId == clienteObjId).ToListAsync(ct);
    }

    public Task CrearAsync(CitaEntity cita, CancellationToken ct = default) =>
        _citas.InsertOneAsync(cita, cancellationToken: ct);

    public async Task<bool> ActualizarAsync(CitaEntity cita, CancellationToken ct = default)
    {
        var resultado = await _citas.ReplaceOneAsync(c => c.Id == cita.Id, cita, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return false;
        var resultado = await _citas.DeleteOneAsync(c => c.Id == objectId, ct);
        return resultado.DeletedCount > 0;
    }
}
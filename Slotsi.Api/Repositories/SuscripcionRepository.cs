using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class SuscripcionRepository : ISuscripcionRepository
{
    private readonly IMongoCollection<SuscripcionActualEntity> _suscripciones;
    private readonly IMongoCollection<RegistroPagoEntity> _pagos;

    public SuscripcionRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        var db = cliente.GetDatabase(settings.DatabaseName);
        _suscripciones = db.GetCollection<SuscripcionActualEntity>("PagoSub");
        _pagos = db.GetCollection<RegistroPagoEntity>("HistorialPagos");
    }

    public async Task<bool> EsActivaAsync(string usuarioId, CancellationToken ct = default)
    {
        var sub = await _suscripciones.Find(s => s.UsuarioId == usuarioId && s.Estado == "Activa").FirstOrDefaultAsync(ct);
        return sub != null && sub.ProximoVencimiento > DateTime.UtcNow;
    }

    public async Task ProcesarPagoAsync(string usuarioId, decimal monto, string metodoPago, CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;
        var proximoPago = ahora.AddMonths(1);

        var registro = new RegistroPagoEntity
        {
            UsuarioId = usuarioId,
            FechaPago = ahora,
            Monto = monto,
            MetodoPago = metodoPago,
            EstadoTransaccion = "Completado"
        };
        await _pagos.InsertOneAsync(registro, cancellationToken: ct);

        var filtro = Builders<SuscripcionActualEntity>.Filter.Eq(s => s.UsuarioId, usuarioId);
        var actualizacion = Builders<SuscripcionActualEntity>.Update
            .Set(s => s.Estado, "Activa")
            .Set(s => s.FechaUltimoPago, ahora)
            .Set(s => s.ProximoVencimiento, proximoPago)
            .Set(s => s.MontoPagado, monto);

        await _suscripciones.UpdateOneAsync(filtro, actualizacion, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task<SuscripcionActualEntity?> ObtenerSuscripcionActivaAsync(string usuarioId, CancellationToken ct = default)
    {
        return await _suscripciones.Find(s => s.UsuarioId == usuarioId && s.Estado == "Activa").FirstOrDefaultAsync(ct);
    }
}
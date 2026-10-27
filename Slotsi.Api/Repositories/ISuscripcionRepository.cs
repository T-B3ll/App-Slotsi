using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public interface ISuscripcionRepository
{
    Task<bool> EsActivaAsync(string usuarioId, CancellationToken ct = default);
    Task ProcesarPagoAsync(string usuarioId, decimal monto, string metodoPago, CancellationToken ct = default);
    Task<SuscripcionActualEntity?> ObtenerSuscripcionActivaAsync(string usuarioId, CancellationToken ct = default);
}
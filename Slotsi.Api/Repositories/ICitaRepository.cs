using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public interface ICitaRepository
{
    Task<List<CitaEntity>> ObtenerPorRangoAsync(string negocioId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<List<CitaEntity>> ObtenerTodasAsync(CancellationToken ct = default);
    Task<CitaEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
    Task<List<CitaEntity>> ObtenerPorFechaAsync(DateTime fecha, CancellationToken ct = default);
    Task<List<CitaEntity>> ObtenerPorUsuarioAsync(string clienteId, CancellationToken ct = default);
    Task CrearAsync(CitaEntity cita, CancellationToken ct = default);
    Task<bool> ActualizarAsync(CitaEntity cita, CancellationToken ct = default);
    Task<bool> EliminarAsync(string id, CancellationToken ct = default);
}
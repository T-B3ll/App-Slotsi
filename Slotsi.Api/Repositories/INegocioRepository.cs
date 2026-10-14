using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public interface INegocioRepository
{
    Task<List<NegocioEntity>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<NegocioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
    Task<NegocioEntity?> ObtenerPorUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
    Task CrearAsync(NegocioEntity negocio, CancellationToken ct = default);
    Task<bool> ActualizarAsync(NegocioEntity negocio, CancellationToken ct = default);
    Task<bool> EliminarAsync(string id, CancellationToken ct = default);
}
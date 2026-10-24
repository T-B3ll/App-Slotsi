using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories
{
    public interface IServicioRepository
    {
        Task<List<ServiciosEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default);
        Task<ServiciosEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
        Task CrearAsync(ServiciosEntity servicio, CancellationToken ct = default);
        Task<bool> ActualizarAsync(ServiciosEntity servicio, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(string id, bool activo, CancellationToken ct = default);
        Task<bool> EliminarAsync(string id, CancellationToken ct = default);
    }
}

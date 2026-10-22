using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories
{
    public interface ITrabajadorRepository
    {
        Task<List<TrabajadorEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default);
        Task<TrabajadorEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
        Task CrearAsync(TrabajadorEntity trabajador, CancellationToken ct = default);
        Task<bool> ActualizarAsync(TrabajadorEntity trabajador, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(string id, bool activo, CancellationToken ct = default);
        Task<bool> EliminarAsync(string id, CancellationToken ct = default);
    }
}

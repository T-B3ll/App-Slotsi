using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories
{
    public interface ICalificacionRepository
    {
        Task<List<CalificacionEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default);
        Task<List<CalificacionEntity>> ObtenerPorUsuarioAsync(string usuarioId, CancellationToken ct = default);
        Task<(double Promedio, int Total)> ObtenerPromedioNegocioAsync(string negocioId, CancellationToken ct = default);
        Task CrearAsync(CalificacionEntity calificacion, CancellationToken ct = default);
        Task<bool> EliminarAsync(string id, CancellationToken ct = default);
    }
}

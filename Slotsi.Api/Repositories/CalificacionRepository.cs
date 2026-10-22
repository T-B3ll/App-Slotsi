using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories
{
    public class CalificacionRepository : ICalificacionRepository
    {
        private readonly IMongoCollection<CalificacionEntity> _calificaciones;

        public CalificacionRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
        {
            var settings = opciones.Value;
            _calificaciones = cliente
                .GetDatabase(settings.DatabaseName)
                .GetCollection<CalificacionEntity>(settings.CalificacionesCollection);
        }

        public async Task<List<CalificacionEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(negocioId, out var oid)) return new();
            return await _calificaciones.Find(c => c.IdNegocio == oid).ToListAsync(ct);
        }

        public async Task<List<CalificacionEntity>> ObtenerPorUsuarioAsync(string usuarioId, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(usuarioId, out var oid)) return new();
            return await _calificaciones.Find(c => c.IdUsuarioCliente == oid).ToListAsync(ct);
        }

        
        public async Task<(double Promedio, int Total)> ObtenerPromedioNegocioAsync(string negocioId, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(negocioId, out var oid)) return (0, 0);

            var rangos = await _calificaciones
                .Find(c => c.IdNegocio == oid && c.NegocioCalificado)
                .Project(c => c.RangoCalificacion)
                .ToListAsync(ct);

            return rangos.Count == 0 ? (0, 0) : (rangos.Average(), rangos.Count);
        }

        public Task CrearAsync(CalificacionEntity calificacion, CancellationToken ct = default) =>
            _calificaciones.InsertOneAsync(calificacion, cancellationToken: ct);

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(id, out var oid)) return false;
            var r = await _calificaciones.DeleteOneAsync(c => c.Id == oid, ct);
            return r.DeletedCount > 0;
        }
    }
}

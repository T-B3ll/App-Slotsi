using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories
{
    public class TrabajadoreRepository: ITrabajadorRepository
    {
        private readonly IMongoCollection<TrabajadorEntity> _trabajadores;

        public TrabajadoreRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
        {
            var settings = opciones.Value;
            _trabajadores = cliente
                .GetDatabase(settings.DatabaseName)
                .GetCollection<TrabajadorEntity>(settings.TrabajadoresCollection);
        }

        public async Task<List<TrabajadorEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(negocioId, out var oid)) return new();
            return await _trabajadores.Find(t => t.NegocioId == oid).ToListAsync(ct);
        }

        public async Task<TrabajadorEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(id, out var oid)) return null;
            return await _trabajadores.Find(t => t.Id == oid).FirstOrDefaultAsync(ct);
        }

        public Task CrearAsync(TrabajadorEntity trabajador, CancellationToken ct = default) =>
            _trabajadores.InsertOneAsync(trabajador, cancellationToken: ct);

        public async Task<bool> ActualizarAsync(TrabajadorEntity trabajador, CancellationToken ct = default)
        {
            var r = await _trabajadores.ReplaceOneAsync(t => t.Id == trabajador.Id, trabajador, cancellationToken: ct);
            return r.MatchedCount > 0;
        }

        public async Task<bool> CambiarEstadoAsync(string id, bool activo, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(id, out var oid)) return false;
            var r = await _trabajadores.UpdateOneAsync(
                t => t.Id == oid,
                Builders<TrabajadorEntity>.Update.Set(t => t.Activo, activo),
                cancellationToken: ct);
            return r.MatchedCount > 0;
        }

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            if (!ObjectId.TryParse(id, out var oid)) return false;
            var r = await _trabajadores.DeleteOneAsync(t => t.Id == oid, ct);
            return r.DeletedCount > 0;
        }
    }
}

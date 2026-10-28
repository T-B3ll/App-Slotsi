using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Slotsi.Api.Repositories
{
    public class ServiciosRepository : IServicioRepository
    {
        private readonly IMongoCollection<ServiciosEntity> _servicios;

        public ServiciosRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
        {
            var settings = opciones.Value;
            _servicios = cliente
                .GetDatabase(settings.DatabaseName)
                .GetCollection<ServiciosEntity>(settings.ServiciosCollection);
        }

        public async Task<List<ServiciosEntity>> ObtenerPorNegocioAsync(string negocioId, CancellationToken ct = default)
        {
            try
            {
                if (!ObjectId.TryParse(negocioId, out var oid)) return new();
                return await _servicios.Find(s => s.NegocioId == oid).ToListAsync(ct);
            }
            catch (TaskCanceledException)
            {
                // Si la conexión con Atlas se cancela (timeout o red), devolvemos lista vacía
                System.Diagnostics.Debug.WriteLine("[ERROR API]: Timeout al conectar con MongoDB Atlas en ObtenerPorNegocioAsync");
                return new List<ServiciosEntity>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR API]: {ex.Message}");
                return new List<ServiciosEntity>();
            }
        }

        public async Task<ServiciosEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
        {
            try
            {
                if (!ObjectId.TryParse(id, out var oid)) return null;
                return await _servicios.Find(s => s.Id == oid).FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR API]: {ex.Message}");
                return null;
            }
        }

        public Task CrearAsync(ServiciosEntity servicio, CancellationToken ct = default) =>
            _servicios.InsertOneAsync(servicio, cancellationToken: ct);

        public async Task<bool> ActualizarAsync(ServiciosEntity servicio, CancellationToken ct = default)
        {
            var r = await _servicios.ReplaceOneAsync(s => s.Id == servicio.Id, servicio, cancellationToken: ct);
            return r.MatchedCount > 0;
        }

        public async Task<bool> CambiarEstadoAsync(string id, bool activo, CancellationToken ct = default)
        {
            try
            {
                if (!ObjectId.TryParse(id, out var oid)) return false;
                var r = await _servicios.UpdateOneAsync(
                    s => s.Id == oid,
                    Builders<ServiciosEntity>.Update.Set(s => s.Activo, activo),
                    cancellationToken: ct);
                return r.MatchedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR API]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
        {
            try
            {
                if (!ObjectId.TryParse(id, out var oid)) return false;
                var r = await _servicios.DeleteOneAsync(s => s.Id == oid, ct);
                return r.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR API]: {ex.Message}");
                return false;
            }
        }
    }
}
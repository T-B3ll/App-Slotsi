using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class SuscripcionService
    {

        private readonly IMongoCollection<SuscripcionActual> _suscripcionesActivas;
        private readonly IMongoCollection<RegistroPago> _historialPagos;



        public SuscripcionService()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);

            // Conexión a las dos colecciones nuevas
            _suscripcionesActivas = database.GetCollection<SuscripcionActual>("PagoSub");
            _historialPagos = database.GetCollection<RegistroPago>("HistorialPagos");


        }

        public async Task<bool> EsActivaAsync(string usuarioId)
        {
            var sub = await _suscripcionesActivas
                 .Find(s => s.UsuarioId == usuarioId && s.Estado == "Activa")
                .FirstOrDefaultAsync();

            return sub != null && sub.ProximoVencimiento > DateTime.UtcNow;
        }

        public async Task ProcesarPagoExitosoAsync(string usuarioId, decimal monto, string metodoPago)
        {
            var ahora = DateTime.UtcNow;
            var proximoPago = ahora.AddMonths(1);

            var nuevoRegistro = new RegistroPago
            {
                UsuarioId = usuarioId,
                FechaPago = ahora,
                Monto = monto,
                MetodoPago = metodoPago,
                EstadoTransaccion = "Completado"
            };

            await _historialPagos.InsertOneAsync(nuevoRegistro);

            var filtro = Builders<SuscripcionActual>.Filter.Eq(s => s.UsuarioId, usuarioId);


            var actualizacion = Builders<SuscripcionActual>.Update
                .Set(s => s.Estado, "Activa")
                .Set(s => s.FechaUltimoPago, ahora)
                .Set(s => s.ProximoVencimiento, proximoPago)
                .Set(s => s.MontoPagado, monto);


            await _suscripcionesActivas.UpdateOneAsync(filtro, actualizacion, new UpdateOptions { IsUpsert = true });

        }
        public async Task<List<RegistroPago>> ObtenerHistorialAsync(string usuarioId)
        {
            return await _historialPagos
                .Find(h => h.UsuarioId == usuarioId)
                 .SortByDescending(h => h.FechaPago)
                 .ToListAsync();
        }

    }
}

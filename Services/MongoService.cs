using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            _database = client.GetDatabase(MongoDbSettings.DatabaseName);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _database.RunCommandAsync<MongoDB.Bson.BsonDocument>(
                        new MongoDB.Bson.BsonDocument("ping", 1));
                    System.Diagnostics.Debug.WriteLine(">>> [EXITO] Conexion con MongoDB Atlas establecida correctamente.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($">>> [ERROR CONEXION ATLAS]: {ex.Message}");
                }
            });
        }


        public async Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            var collection = _database.GetCollection<Usuario>("Usuarios");
            return await collection.Find(_ => true).ToListAsync();
        }

        public async Task<bool> ActualizarEstadoUsuarioAsync(string id, bool nuevoEstado)
        {
            if (string.IsNullOrEmpty(id)) return false;

            var collection = _database.GetCollection<Usuario>("Usuarios");
            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, id);
            var update = Builders<Usuario>.Update.Set(u => u.EstaActivo, nuevoEstado);

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Servicios>> ObtenerServiciosAsync()
        {
            try
            {
                var collection = _database.GetCollection<Servicios>("Servicios");
                var lista = await collection.Find(_ => true).ToListAsync();

                if (lista == null || lista.Count == 0)
                {
                    var collectionMin = _database.GetCollection<Servicios>("servicios");
                    lista = await collectionMin.Find(_ => true).ToListAsync();
                }

                return lista ?? new List<Servicios>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB SERVICIOS]: {ex.Message}");
                return new List<Servicios>();
            }
        }

    
        public async Task<bool> GuardarCitaAsync(Citas cita)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MONGO] Intentando guardar cita: Fecha={cita.FechaHoraInicio:yyyy-MM-dd}, Hora={cita.FechaHoraInicio:HH:mm}, Cliente={cita.Cliente?.Nombre}");

                var collection = _database.GetCollection<Citas>("Citas");
                await collection.InsertOneAsync(cita);

                System.Diagnostics.Debug.WriteLine($"[MONGO] ✅ Cita guardada con ID: {cita.Id}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MONGO ERROR]: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[MONGO STACK]: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<Citas>> ObtenerCitasAsync()
        {
            try
            {
                var collection = _database.GetCollection<Citas>("Citas");
                return await collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB CITAS]: {ex.Message}");
                return new List<Citas>();
            }
        }

        public async Task<List<Citas>> ObtenerCitasPorFechaAsync(DateTime fecha)
        {
            try
            {
                var collection = _database.GetCollection<Citas>("Citas");
                var inicioDia = fecha.Date;
                var finDia = fecha.Date.AddDays(1).AddTicks(-1);

                var filter = Builders<Citas>.Filter.Gte(c => c.FechaHoraInicio, inicioDia) &
                             Builders<Citas>.Filter.Lte(c => c.FechaHoraInicio, finDia);

                return await collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB CITAS POR FECHA]: {ex.Message}");
                return new List<Citas>();
            }
        }
    }
}
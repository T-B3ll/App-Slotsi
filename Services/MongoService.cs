using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Collections.Generic;
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

            // --- PRUEBA DE DIAGNÓSTICO DE CONEXIÓN A ATLAS ---
            try
            {
                _database.RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                System.Diagnostics.Debug.WriteLine(">>> [EXITO] Conexion con MongoDB Atlas establecida correctamente.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($">>> [ERROR FATAL DE CONEXION ATLAS]: {ex.Message}");
            }
        }

        // --- MÉTODOS DE USUARIOS ---
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

        // --- MÉTODOS DE SERVICIOS (UN SOLO MÉTODO) ---
        public async Task<List<Servicios>> ObtenerServiciosAsync()
        {
            try
            {
                // Busca primero en "Servicios" y si está vacía busca en "servicios"
                var collection = _database.GetCollection<Servicios>("Servicios");
                var lista = await collection.Find(_ => true).ToListAsync();

                if (lista == null || !lista.Any())
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

        // --- MÉTODOS DE CITAS ---
        public async Task<bool> GuardarCitaAsync(Cita_cliente cita)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MONGO] Intentando guardar cita: Fecha={cita.Fecha}, Hora={cita.Hora}, Cliente={cita.NombreCliente}");

                var collection = _database.GetCollection<Cita_cliente>("Citas");
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

        public async Task<List<Cita_cliente>> ObtenerCitasAsync()
        {
            try
            {
                var collection = _database.GetCollection<Cita_cliente>("Citas");
                return await collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB CITAS]: {ex.Message}");
                return new List<Cita_cliente>();
            }
        }
    }
}
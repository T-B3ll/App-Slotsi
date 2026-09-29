using MongoDB.Driver;
using slotsi_citas.Models;
using App.Models;
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
        public async Task<List<Servicio>> ObtenerServiciosAsync()
        {
            try
            {
                // Busca primero en "Servicios" y si está vacía busca en "servicios"
                var collection = _database.GetCollection<Servicio>("Servicios");
                var lista = await collection.Find(_ => true).ToListAsync();

                if (lista == null || !lista.Any())
                {
                    var collectionMin = _database.GetCollection<Servicio>("servicios");
                    lista = await collectionMin.Find(_ => true).ToListAsync();
                }

                return lista ?? new List<Servicio>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB SERVICIOS]: {ex.Message}");
                return new List<Servicio>();
            }
        }

        // --- MÉTODOS DE CITAS ---
        public async Task<bool> GuardarCitaAsync(Cita_cliente cita)
        {
            try
            {
                var collection = _database.GetCollection<Cita_cliente>("Citas");
                await collection.InsertOneAsync(cita);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB CITAS]: {ex.Message}");
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
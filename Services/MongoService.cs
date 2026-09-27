using MongoDB.Driver;
using slotsi_citas.Models;
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

        public async Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            var collection = _database.GetCollection<Usuario>("Usuarios");
            return await collection.Find(_ => true).ToListAsync();
        }
    }
}
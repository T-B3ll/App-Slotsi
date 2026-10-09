using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using System.Threading.Tasks;
namespace slotsi_citas.Services
{
    public class NegocioService
    {

        private readonly IMongoCollection<Negocio> _negocios;



        public NegocioService()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);

            var database = client.GetDatabase(MongoDbSettings.DatabaseName);
            _negocios = database.GetCollection<Negocio>("Negocios");
        }

        public async Task CrearAsync(Negocio negocio)
        {
            await _negocios.InsertOneAsync(negocio);
        }

        public async Task<List<Negocio>> ObtenerTodosAsync()
        {
            
            return await _negocios.Find(_ => true).ToListAsync();
        }

        public async Task ActualizarAsync(Negocio negocio)
        {
            if (string.IsNullOrEmpty(negocio.Id))
                throw new Exception("El negocio no tiene un ID válido.");

            var filter = Builders<Negocio>.Filter.Eq("_id", ObjectId.Parse(negocio.Id));
            await _negocios.ReplaceOneAsync(filter, negocio);
        }
    }
}

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
    }
}

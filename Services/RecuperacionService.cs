using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Threading.Tasks;
namespace slotsi_citas.Services
{
    public class RecuperacionService
    {
        private readonly IMongoCollection<RecuperacionModel> _recuperaciones;

        public RecuperacionService()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);
            _recuperaciones = database.GetCollection<RecuperacionModel>("RecuperarContraseña");
        }

      
        public async Task<string> GenerarSolicitudAsync(string correo)
        {
      
            var random = new Random();
            string codigo = random.Next(100000, 999999).ToString();

            var solicitud = new RecuperacionModel
            {
                Correo = correo.ToLower().Trim(),
                Codigo = codigo,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(10), 
                Usado = false
            };

            await _recuperaciones.InsertOneAsync(solicitud);
            return codigo;
        }

        public async Task<RecuperacionModel?> ValidarCodigoAsync(string correo, string codigo)
        {
            var filtro = Builders<RecuperacionModel>.Filter.And(
                Builders<RecuperacionModel>.Filter.Eq(r => r.Correo, correo.ToLower().Trim()),
                Builders<RecuperacionModel>.Filter.Eq(r => r.Codigo, codigo),
                Builders<RecuperacionModel>.Filter.Eq(r => r.Usado, false),
                Builders<RecuperacionModel>.Filter.Gt(r => r.FechaExpiracion, DateTime.UtcNow)
            );

            return await _recuperaciones.Find(filtro).FirstOrDefaultAsync();
        }

       
        public async Task MarcarComoUsadoAsync(string id)
        {
            var filtro = Builders<RecuperacionModel>.Filter.Eq(r => r.Id, id);
            var update = Builders<RecuperacionModel>.Update.Set(r => r.Usado, true);
            await _recuperaciones.UpdateOneAsync(filtro, update);
        }
    }

}

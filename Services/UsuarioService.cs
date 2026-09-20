using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    class UsuarioService
    {

 
        private readonly IMongoCollection<Usuario> _usuarios;

   
        public UsuarioService()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);

            
            _usuarios = database.GetCollection<Usuario>("Usuarios");
        }

      
        public async Task CrearAsync(Usuario usuario) =>
            await _usuarios.InsertOneAsync(usuario);


        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
            await _usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();

        public async Task<List<Usuario>> ObtenerTodosAsync() =>
            await _usuarios.Find(_ => true).ToListAsync();

     
        public async Task ActualizarAsync(Usuario usuario) =>
            await _usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);

        public async Task CambiarEstadoAsync(string id, bool estaActivo)
        {
            var filtro = Builders<Usuario>.Filter.Eq(u => u.Id, id);
            var actualizacion = Builders<Usuario>.Update.Set(u => u.EstaActivo, estaActivo);
            await _usuarios.UpdateOneAsync(filtro, actualizacion);
        }

    }
}

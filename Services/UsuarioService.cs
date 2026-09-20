using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class UsuarioService
    {

 
        private readonly IMongoCollection<Usuario> _usuarios;



   
      public UsuarioService()
        {
            var cadena = MongoDbSettings.ConnectionString;

            System.Diagnostics.Debug.WriteLine($"=== DEBUG INICIO ===");
            System.Diagnostics.Debug.WriteLine($"Cadena leída: '{cadena}'");
            System.Diagnostics.Debug.WriteLine($"Es nula o vacía: {string.IsNullOrEmpty(cadena)}");
            System.Diagnostics.Debug.WriteLine($"Longitud: {cadena?.Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"=== DEBUG FIN ===");


            if (string.IsNullOrWhiteSpace(cadena))
            {
                throw new InvalidOperationException(
                    "ERROR CRÍTICO: La cadena de conexión está VACÍA. " +
                    "Revisa MongoDbSettings.cs y asegúrate de que ConnectionString tenga valor.");
            }


            try
            {
                var client = new MongoClient(cadena);
                var database = client.GetDatabase(MongoDbSettings.DatabaseName);
                _usuarios = database.GetCollection<Usuario>("Usuarios");

                System.Diagnostics.Debug.WriteLine("✅ Cliente MongoDB creado exitosamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Error al crear cliente: {ex.Message}");
                throw;
            }
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

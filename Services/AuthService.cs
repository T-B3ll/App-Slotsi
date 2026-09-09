using slotsi_citas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;

namespace slotsi_citas.Services
{
    public class AuthService
    {
        private readonly Client _supabaseClient;

        // Constructor: Recibe el cliente de Supabase que configuramos antes
        public AuthService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

  
        /// Intenta iniciar sesión con email y contraseña
      
        public async Task<Usuario> LoginAsync(string email, string password)
        {
            try
            {
                // 1. Le pedimos a Supabase que valide las credenciales
                var session = await _supabaseClient.Auth.SignIn(email, password);

                if (session != null && session.User != null)
                {
                    // 2. Si es válido, creamos nuestro objeto Usuario local
                    //    con los datos que vienen de Supabase
                    return new Usuario
                    {
                        Id = session.User.Id,
                        Email = session.User.Email,
                        Nombre = session.User.UserMetadata?["name"]?.ToString() ?? "Usuario"
                    };
                }

                return null; // Si no hubo sesión, falló el login
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear el error
                Console.WriteLine($"Error al login: {ex.Message}");
                return null;
            }
        }

 
        public async Task LogoutAsync()
        {
            await _supabaseClient.Auth.SignOut();
        }

      
        public bool IsLoggedIn()
        {
            return _supabaseClient.Auth.CurrentSession != null;
        }
    }
}


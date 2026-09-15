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

       
        public AuthService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

  
      
      
        public async Task<Usuario> LoginAsync(string email, string password)
        {
            try
            {
               
                var session = await _supabaseClient.Auth.SignIn(email, password);

                if (session != null && session.User != null)
                {
                  
                    return new Usuario
                    {
                        Id = session.User.Id,
                        Email = session.User.Email,
                        Nombre = session.User.UserMetadata?["name"]?.ToString() ?? "Usuario"
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                
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


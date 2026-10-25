using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slotsi_citas.Services.Auth;
using System.Threading.Tasks;
using slotsi_citas.Services.Api;
using Slotsi.Shared.Dtos;


namespace slotsi_citas.Services.Api
{
    public class AuthApiService : ApiServiceBase, IAuthApiService
    {
        private readonly ISesionService _sesion;

        public AuthApiService(HttpClient http, ISesionService sesion) : base(http) => _sesion = sesion;

        public Task RegistrarAsync(RegistrarUsuarioDto dto) => PostAsync("api/auth/registro", dto);

        public async Task IniciarSesionAsync(string email, string contrasena)
        {
            var token = await PostAsync<TokenDto>("api/auth/login", new LoginDto { Email = email, Contrasena = contrasena });
            await _sesion.IniciarAsync(token);
        }

        public async Task CerrarSesionAsync()
        {
            try
            {
                await PostAsync("api/auth/logout", new { });
            }
            catch (ApiException)
            {
                //sin conexion o token ya invalido: igualmente se cierra la sesion actual
            }
            finally
            {
                _sesion.Cerrar();
            }
        }

        public Task<SolicitudRestablecimientoRespuestaDto> SolicitarRestablecimientoAsync(string email) =>
            PostAsync<SolicitudRestablecimientoRespuestaDto>("api/auth/olvide-contrasena",
                new SolicitarRestablecimientoDto { Email = email });

        public Task RestablecerContrasenaAsync(RestablecerContrasenaDto dto) =>
            PostAsync("api/auth/restablecer-contrasena", dto);
    }
}

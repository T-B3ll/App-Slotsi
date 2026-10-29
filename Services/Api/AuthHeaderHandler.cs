using slotsi_citas.Services.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services.Api
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ISesionService _sesion;

        public AuthHeaderHandler(ISesionService sesion) => _sesion = sesion;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _sesion.Token;
            if (token is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var respuesta = await base.SendAsync(request, cancellationToken);
            // Sin token adjunto, un 401 es "credenciales incorrectas" del login: no se toca la sesión.
            if (token is not null && respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                _sesion.Cerrar();
                NavigationAuth.IrALogin();
            }

            return respuesta;
        }
    }
}
using slotsi_citas.Services.Auth;
using System.Net;
using System.Net.Http.Headers;

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

            if (token is not null
                && respuesta.StatusCode == HttpStatusCode.Unauthorized
                && request.RequestUri?.AbsolutePath.EndsWith("/logout") != true)
            {
                _sesion.Cerrar();
                NavigationAuth.IrALogin();
            }

            return respuesta;
        }
    }
}
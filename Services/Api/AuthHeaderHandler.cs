using slotsi_citas.Services.Auth;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
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

            bool esRutaDeAutenticacion = request.RequestUri?.AbsolutePath.Contains("/api/auth/") == true;
            bool esRutaDeRegistro = request.RequestUri?.AbsolutePath.Contains("/api/usuarios") == true
                                    && request.Method == HttpMethod.Post;

            if (token is not null && respuesta.StatusCode == HttpStatusCode.Unauthorized
                && !esRutaDeAutenticacion && !esRutaDeRegistro)
            {
                _sesion.Cerrar();
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Shell.Current is not null)
                        await Shell.Current.GoToAsync("//login");
                });
            }
            return respuesta;
        }
    }
}
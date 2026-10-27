using slotsi_citas.Services.Api;
using Slotsi.Shared.Dtos;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class SuscripcionService : ApiServiceBase
    {
        public SuscripcionService(HttpClient http) : base(http) { }

        public async Task<bool> EsActivaAsync(string usuarioId)
        {
            try
            {
                var sub = await GetOrNullAsync<SuscripcionActivaDto>($"api/suscripcion/activa/{usuarioId}");
                return sub != null && sub.ProximoVencimiento > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public async Task ProcesarPagoExitosoAsync(string usuarioId, decimal monto, string metodoPago)
        {
            var dto = new PagoDto
            {
                UsuarioId = usuarioId,
                Monto = monto,
                MetodoPago = metodoPago
            };
            await PostAsync<object>("api/suscripcion/pagar", dto);
        }

        public async Task<SuscripcionActivaDto?> ObtenerSuscripcionActivaAsync(string usuarioId)
        {
            try
            {
                return await GetOrNullAsync<SuscripcionActivaDto>($"api/suscripcion/activa/{usuarioId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> YaPagoEsteMesAsync(string usuarioId)
        {
            return await EsActivaAsync(usuarioId);
        }
    }
}
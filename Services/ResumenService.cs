using Slotsi.Shared.Dtos;
using slotsi_citas.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class ResumenService
    {
        private readonly HttpClient _http;

        public ResumenService(HttpClient http) => _http = http;

        public async Task<ResumenDiaDto?> ObtenerResumenAsync(
            string negocioId, DateTime desde, DateTime hasta, CancellationToken ct = default)
        {
            var d = desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var h = hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var url = $"api/citas/resumen?negocioId={negocioId}&desde={d}&hasta={h}";
            return await _http.GetFromJsonAsync<ResumenDiaDto>(url, ct);
        }

        public async Task<List<CitaDto>> ObtenerCitasClienteAsync(string clienteId, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"api/citas/usuario/{clienteId}", ct);

            if (!resp.IsSuccessStatusCode)
            {
                var cuerpo = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"{(int)resp.StatusCode}: {cuerpo}");
            }

            return await resp.Content.ReadFromJsonAsync<List<CitaDto>>(cancellationToken: ct)
                   ?? new List<CitaDto>();
        }
    }
}

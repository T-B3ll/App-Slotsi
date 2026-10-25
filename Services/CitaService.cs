using slotsi_citas.Services.Api;
using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class CitaService : ApiServiceBase
    {
        public CitaService(HttpClient http) : base(http) { }

        public async Task<List<CitaDto>> ObtenerPorFechaAsync(DateTime fecha)
        {
            return await GetAsync<List<CitaDto>>($"api/citas?fecha={fecha:yyyy-MM-dd}");
        }

        public async Task<List<CitaDto>> ObtenerPorUsuarioAsync(string clienteId)
        {
            return await GetAsync<List<CitaDto>>($"api/citas/usuario/{clienteId}");
        }

        public async Task CrearAsync(CitaDto cita)
        {
            await PostAsync<CitaDto>("api/citas", cita);
        }
    }
}
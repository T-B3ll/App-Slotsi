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


        public async Task<List<TopServicioDto>> ObtenerTopServiciosAsync(string negocioId, string periodo = "semana")
        {
            return await GetAsync<List<TopServicioDto>>($"api/citas/reportes/top-servicios?negocioId={negocioId}&periodo={periodo}");
        }

        public async Task<List<DemandaSemanalDto>> ObtenerDemandaSemanalAsync(string negocioId, string periodo = "semana")
        {
            return await GetAsync<List<DemandaSemanalDto>>($"api/citas/reportes/demanda-semanal?negocioId={negocioId}&periodo={periodo}");
        }

        public async Task<bool> EliminarAsync(string id)
        {
            try
            {
                await DeleteAsync($"api/citas/{id}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarEstadoAsync(string id, CitaDto cita)
        {
            try
            {
                await PutAsync($"api/citas/{id}", cita);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
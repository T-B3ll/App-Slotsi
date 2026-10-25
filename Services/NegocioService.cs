using slotsi_citas.Models;
using slotsi_citas.Services.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public class NegocioService : ApiServiceBase
    {
        public NegocioService(HttpClient http) : base(http)
        {
        }

        public async Task CrearAsync(Negocio negocio)
        {
            await PostAsync<Negocio>("api/negocios", negocio);
        }

        public async Task<List<Negocio>> ObtenerTodosAsync()
        {
            return await GetAsync<List<Negocio>>("api/negocios");
        }

        public async Task<Negocio?> ObtenerPorUsuarioIdAsync(string usuarioId)
        {
            return await GetOrNullAsync<Negocio>($"api/negocios/usuario/{usuarioId}");
        }

        
        public async Task<Negocio?> ObtenerPorIdAsync(string id)
        {
            try
            {
                return await GetOrNullAsync<Negocio>($"api/negocios/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task ActualizarAsync(Negocio negocio)
        {
            if (string.IsNullOrEmpty(negocio.Id))
                throw new Exception("El negocio no tiene un ID válido.");

            await PutAsync($"api/negocios/{negocio.Id}", negocio);
        }
    }
}
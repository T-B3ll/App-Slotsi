using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slotsi.Shared.Dtos;
using slotsi_citas.Services.Api;

namespace slotsi_citas.Services
{
    public class TrabajadorService : ApiServiceBase
    {
        public TrabajadorService(HttpClient http) : base(http) { }

        public Task<List<TrabajadorDto>> ObtenerPorNegocioAsync(string negocioId) =>
            GetAsync<List<TrabajadorDto>>($"/api/trabajadores/negocio/{Uri.EscapeDataString(negocioId)}");

        public Task<TrabajadorDto> CrearAsync(TrabajadorDto dto) =>
            PostAsync<TrabajadorDto>("/api/trabajadores", dto);

        public Task ActualizarAsync(TrabajadorDto dto) =>
            PutAsync($"/api/trabajadores/{dto.Id}", dto);

        public Task CambiarEstadoAsync(string id, bool activo) =>
            PatchAsync($"/api/trabajadores/{id}/estado", new { Activo = activo });

        public Task EliminarAsync(string id) =>
            DeleteAsync($"/api/trabajadores/{id}");
    }
}

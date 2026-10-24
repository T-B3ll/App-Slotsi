using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slotsi.Shared.Dtos;
using slotsi_citas.Services.Api;

namespace slotsi_citas.Services
{
    public class CalificacionService : ApiServiceBase
    {
        public CalificacionService(HttpClient http) : base(http) { }

        public Task<List<CalificacionesDtos>> ObtenerPorNegocioAsync(string negocioId) =>
            GetAsync<List<CalificacionesDtos>>($"/api/calificacion/negocio/{Uri.EscapeDataString(negocioId)}");

        public Task<PromedioCalificacionDto?> ObtenerPromedioNegocioAsync(string negocioId) =>
            GetAsync<PromedioCalificacionDto?>($"/api/calificacion/negocio/{Uri.EscapeDataString(negocioId)}/promedio");

        public Task<List<CalificacionesDtos>> ObtenerPorUsuarioAsync(string usuarioId) =>
            GetAsync<List<CalificacionesDtos>>($"/api/calificacion/usuario/{Uri.EscapeDataString(usuarioId)}");

        public Task<CalificacionesDtos> CrearAsync(CalificacionesDtos dto) =>
            PostAsync<CalificacionesDtos>("/api/calificacion", dto);

        public Task EliminarAsync(string id) =>
            DeleteAsync($"/api/calificacion/{id}");
    }
}
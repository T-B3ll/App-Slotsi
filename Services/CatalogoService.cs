using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using slotsi_citas.Services.Api;

namespace slotsi_citas.Services
{
    public class CatalogoService : ApiServiceBase
    {
        public CatalogoService(HttpClient http) : base(http) { }

        public async Task<List<Servicios>> ObtenerServiciosPorNegocioAsync(string negocioId)
        {
            var dtos = await GetAsync<List<ServiciosDto>>($"/api/servicios/negocio/{Uri.EscapeDataString(negocioId)}") ?? new();

            var lista = new List<Servicios>();
            foreach (var d in dtos)
            {
                lista.Add(new Servicios
                {
                    Id = d.Id,
                    NegocioId = d.NegocioId,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion,
                    DuracionMinutos = d.DuracionMinutos,
                    Precio = d.Precio,
                    Categoria = d.Categoria,
                    ColaboradoresHabilitados = d.ColaboradoresHabilitados ?? new(),
                    DisponibleDomicilio = d.DisponibleDomicilio,
                    Activo = d.Activo,
                    Foto = d.Foto
                });
            }
            return lista;
        }

        public async Task<Negocio?> ObtenerNegocioPorIdAsync(string negocioId)
        {
            try
            {
                return await GetAsync<Negocio>($"/api/negocios/{Uri.EscapeDataString(negocioId)}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<Negocio?> ObtenerNegocioPorUsuarioAsync(string usuarioId)
        {
            try
            {
                return await GetAsync<Negocio>($"/api/negocios/usuario/{Uri.EscapeDataString(usuarioId)}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CrearServicioAsync(Servicios servicio)
        {
            try
            {
                var dto = new ServiciosDto
                {
                    NegocioId = servicio.NegocioId,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    DuracionMinutos = servicio.DuracionMinutos,
                    Precio = servicio.Precio,
                    Categoria = servicio.Categoria,
                    ColaboradoresHabilitados = servicio.ColaboradoresHabilitados ?? new(),
                    DisponibleDomicilio = servicio.DisponibleDomicilio,
                    Activo = servicio.Activo,
                    Foto = servicio.Foto
                };

                var resultado = await PostAsync<ServiciosDto>("/api/servicios", dto);
                return resultado != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarServicioAsync(string id, Servicios servicio)
        {
            try
            {
                var dto = new ServiciosDto
                {
                    Id = servicio.Id,
                    NegocioId = servicio.NegocioId,
                    Nombre = servicio.Nombre,
                    Descripcion = servicio.Descripcion,
                    DuracionMinutos = servicio.DuracionMinutos,
                    Precio = servicio.Precio,
                    Categoria = servicio.Categoria,
                    ColaboradoresHabilitados = servicio.ColaboradoresHabilitados ?? new(),
                    DisponibleDomicilio = servicio.DisponibleDomicilio,
                    Activo = servicio.Activo,
                    Foto = servicio.Foto
                };

                await PutAsync($"/api/servicios/{Uri.EscapeDataString(id)}", dto);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarEstadoServicioAsync(string id, bool activo)
        {
            try
            {
                await PatchAsync($"/api/servicios/{Uri.EscapeDataString(id)}/estado", new { Activo = activo });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
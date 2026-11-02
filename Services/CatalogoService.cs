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
        private readonly TrabajadorService _trabajadorService;

        public CatalogoService(HttpClient http, TrabajadorService trabajadorService) : base(http)
        {
            _trabajadorService = trabajadorService;
        }

        public async Task<List<Servicios>> ObtenerServiciosPorNegocioAsync(string negocioId)
        {
            var dtos = await GetAsync<List<ServiciosDto>>($"/api/servicios/negocio/{Uri.EscapeDataString(negocioId)}") ?? new();

            var trabajadoresDto = await _trabajadorService.ObtenerPorNegocioAsync(negocioId);
            var trabajadoresReales = new List<Trabajador>();

            if (trabajadoresDto != null)
            {
                foreach (var t in trabajadoresDto)
                {
                    trabajadoresReales.Add(new Trabajador
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Especialidad = t.Especialidad,
                        Foto = t.Foto,
                        Activo = t.Activo
                    });
                }
            }

            var lista = new List<Servicios>();
            foreach (var d in dtos)
            {
                var servicio = new Servicios
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
                };

                if (servicio.ColaboradoresHabilitados != null)
                {
                    foreach (var idTrabajador in servicio.ColaboradoresHabilitados)
                    {
                        var tOriginal = trabajadoresReales.FirstOrDefault(t => t.Id == idTrabajador);
                        if (tOriginal != null)
                        {
                            var clon = new Trabajador
                            {
                                Id = tOriginal.Id,
                                Nombre = tOriginal.Nombre,
                                Especialidad = tOriginal.Especialidad,
                                Foto = tOriginal.Foto,
                                Activo = tOriginal.Activo,
                                ServicioPadre = servicio
                            };
                            servicio.ColaboradoresDelServicio.Add(clon);
                        }
                    }
                }

                lista.Add(servicio);
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
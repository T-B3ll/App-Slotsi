using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;
using MongoDB.Bson;
using Slotsi.Api.Infrastructure;

namespace Slotsi.Api.Mapping
{
    public static class ServicioMapping
    {
        public static ServiciosDto ToDto(this ServiciosEntity e) => new()
        {
            Id = e.Id.ToString(),
            NegocioId = e.NegocioId.ToString(),
            Nombre = e.Nombre ?? "",
            Descripcion = e.Descripcion ?? "",
            DuracionMinutos = e.DuracionMinutos,
            Precio = e.Precio,
            Categoria = e.Categoria ?? "",
            ColaboradoresHabilitados = e.ColaboradoresHabilitados?.Select(c => c.ToString()).ToList() ?? new(),
            DisponibleDomicilio = e.DisponibleDomicilio,
            Activo = e.Activo,
            Foto = e.Foto ?? ""
        };

        public static ServiciosEntity ToEntity(this ServiciosDto d, string? id = null) => new()
        {
            Id = !string.IsNullOrEmpty(id) ? ObjectId.Parse(id) : ObjectId.GenerateNewId(),
            NegocioId = ObjectId.Parse(d.NegocioId),
            Nombre = (d.Nombre ?? "").Trim(),
            Descripcion = (d.Descripcion ?? "").Trim(),
            DuracionMinutos = d.DuracionMinutos,
            Precio = d.Precio,
            Categoria = (d.Categoria ?? "").Trim(),
            ColaboradoresHabilitados = (d.ColaboradoresHabilitados ?? new())
                .Where(IdValidator.EsValido).Select(ObjectId.Parse).ToList(),
            DisponibleDomicilio = d.DisponibleDomicilio,
            Activo = d.Activo,
            Foto = d.Foto
        };
    }
}

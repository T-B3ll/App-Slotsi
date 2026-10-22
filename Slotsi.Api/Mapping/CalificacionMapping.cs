using MongoDB.Bson;
using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping
{
    public static class CalificacionMapping
    {
        public static CalificacionesDtos ToDto(this CalificacionEntity e) => new()
        {
            Id = e.Id.ToString(),
            IdNegocio = e.IdNegocio.ToString(),
            IdColaborador = e.IdColaborador?.ToString(),
            IdUsuarioCliente = e.IdUsuarioCliente.ToString(),
            RangoCalificacion = e.RangoCalificacion,
            NegocioCalificado = e.NegocioCalificado,
            NombreNegocio = e.NombreNegocio ?? "",
            NombreUsuario = e.NombreUsuario ?? "",
            Comentario = e.Comentario ?? ""
        };

        public static CalificacionEntity ToEntity(this CalificacionesDtos d, string? id = null) => new()
        {
            Id = !string.IsNullOrEmpty(id) ? ObjectId.Parse(id) : ObjectId.GenerateNewId(),
            IdNegocio = ObjectId.Parse(d.IdNegocio),
            IdColaborador = ObjectId.TryParse(d.IdColaborador, out var col) ? col : null,
            IdUsuarioCliente = ObjectId.Parse(d.IdUsuarioCliente),
            RangoCalificacion = d.RangoCalificacion,
            NegocioCalificado = d.NegocioCalificado,
            NombreNegocio = (d.NombreNegocio ?? "").Trim(),
            NombreUsuario = (d.NombreUsuario ?? "").Trim(),
            Comentario = (d.Comentario ?? "").Trim()
        };
    }
}

using MongoDB.Bson;
using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping
{
    public static class TrabajadorMappingcs
    {
        public static TrabajadorDto ToDto(this TrabajadorEntity e) => new()
        {
            Id = e.Id.ToString(),
            NegocioId = e.NegocioId.ToString(),
            Nombre = e.Nombre ?? "",
            Especialidad = e.Especialidad ?? "",
            Correo = e.Correo ?? "",
            Telefono = e.Telefono ?? "",
            Cedula = e.Cedula ?? "",
            Foto = e.Foto ?? "",
            ArchivoProfesional = e.ArchivoProfesional ?? "",
            Activo = e.Activo,
            HorarioTrabajo = e.HorarioTrabajo?.Select(h => new HorarioTrabajoDto
            {
                Dia = h.Dia ?? "",
                Inicio = h.Inicio ?? "",
                Fin = h.Fin ?? ""
            }).ToList() ?? new()
        };

        public static TrabajadorEntity ToEntity(this TrabajadorDto d, string? id = null) => new()
        {
            Id = !string.IsNullOrEmpty(id) ? ObjectId.Parse(id) : ObjectId.GenerateNewId(),
            NegocioId = ObjectId.Parse(d.NegocioId),
            Nombre = (d.Nombre ?? "").Trim(),
            Especialidad = (d.Especialidad ?? "").Trim(),
            Correo = (d.Correo ?? "").Trim().ToLower(),
            Telefono = (d.Telefono ?? "").Trim(),
            Cedula = (d.Cedula ?? "").Trim(),
            Foto = d.Foto,
            ArchivoProfesional = d.ArchivoProfesional,
            Activo = d.Activo,
            HorarioTrabajo = (d.HorarioTrabajo ?? new()).Select(h => new HorarioTrabajoEntity
            {
                Dia = h.Dia,
                Inicio = h.Inicio,
                Fin = h.Fin
            }).ToList()
        };
    }
}

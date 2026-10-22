using Slotsi.Shared.Dtos;

namespace slotsi_citas.Models;

public static class TrabajadorDtoExtensions
{
    public static Trabajador ToModel(this TrabajadorDto d) => new()
    {
        Id = d.Id ?? string.Empty,
        NegocioId = d.NegocioId,
        Nombre = d.Nombre,
        Especialidad = d.Especialidad,
        Correo = d.Correo,
        Telefono = d.Telefono,
        Cedula = d.Cedula,
        Foto = d.Foto,
        ArchivoProfesional = d.ArchivoProfesional,
        Activo = d.Activo,
        HorarioTrabajo = d.HorarioTrabajo
            .Select(h => new HorarioTrabajo { Dia = h.Dia, Inicio = h.Inicio, Fin = h.Fin })
            .ToList()
    };

    public static TrabajadorDto ToDto(this Trabajador t) => new()
    {
        Id = string.IsNullOrEmpty(t.Id) ? null : t.Id,
        NegocioId = t.NegocioId ?? string.Empty,
        Nombre = t.Nombre ?? string.Empty,
        Especialidad = t.Especialidad ?? string.Empty,
        Correo = t.Correo ?? string.Empty,
        Telefono = t.Telefono ?? string.Empty,
        Cedula = t.Cedula ?? string.Empty,
        Foto = t.Foto ?? string.Empty,
        ArchivoProfesional = t.ArchivoProfesional ?? string.Empty,
        Activo = t.Activo,
        HorarioTrabajo = (t.HorarioTrabajo ?? new())
            .Select(h => new HorarioTrabajoDto
            {
                Dia = h.Dia ?? string.Empty,
                Inicio = h.Inicio ?? string.Empty,
                Fin = h.Fin ?? string.Empty
            })
            .ToList()
    };
}
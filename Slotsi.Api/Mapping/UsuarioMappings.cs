using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping;

public static class UsuarioMappings
{
    public static UsuarioDto ToDto(this UsuarioEntity entity) => new()
    {
        Id = entity.Id,
        NombreCompleto = entity.NombreCompleto,
        Correo = entity.Correo,
        Telefono = entity.Telefono,
        Cedula = entity.Cedula,
        Contrasena = entity.Contrasena,
        TipoUsuario = entity.TipoUsuario,
        EstaActivo = entity.EstaActivo
    };

    public static UsuarioEntity ToEntity(this UsuarioDto dto, string? id = null) => new()
    {
        Id = id,
        NombreCompleto = dto.NombreCompleto.Trim(),
        Correo = dto.Correo.Trim().ToLower(),
        Telefono = dto.Telefono.Trim(),
        Cedula = dto.Cedula.Trim(),
        Contrasena = dto.Contrasena,
        TipoUsuario = dto.TipoUsuario,
        EstaActivo = dto.EstaActivo
    };
}
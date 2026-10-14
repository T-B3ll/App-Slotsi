using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping;

public static class NegocioMappings
{
    public static NegocioDto ToDto(this NegocioEntity entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        NombreNegocio = entity.NombreNegocio,
        UsuarioId = entity.UsuarioId,
        NumeroRuc = entity.NumeroRuc,
        DireccionNegocio = entity.DireccionNegocio,
        TipoServicio = entity.TipoServicio,
        UrlFotoPerfil = entity.UrlFotoPerfil,
        Categoria = entity.Categoria,
        FechaRegistro = entity.FechaRegistro,
        Horarios = entity.Horarios.ToDictionary(
            h => h.Key,
            h => new HorarioDiaDto { Apertura = h.Value.Apertura, Cierre = h.Value.Cierre, Pausas = h.Value.Pausas }
        ),
        DiasCerrados = entity.DiasCerrados
    };

    public static NegocioEntity ToEntity(this NegocioDto dto, string? id = null) => new()
    {
        Id = id,
        Nombre = dto.Nombre.Trim(),
        NombreNegocio = dto.NombreNegocio.Trim(),
        UsuarioId = dto.UsuarioId,
        NumeroRuc = dto.NumeroRuc.Trim(),
        DireccionNegocio = dto.DireccionNegocio.Trim(),
        TipoServicio = dto.TipoServicio,
        UrlFotoPerfil = dto.UrlFotoPerfil,
        Categoria = dto.Categoria.Trim(),
        FechaRegistro = dto.FechaRegistro,
        Horarios = dto.Horarios.ToDictionary(
            h => h.Key,
            h => new NegocioEntity.HorarioDiaEntity { Apertura = h.Value.Apertura, Cierre = h.Value.Cierre, Pausas = h.Value.Pausas }
        ),
        DiasCerrados = dto.DiasCerrados
    };
}
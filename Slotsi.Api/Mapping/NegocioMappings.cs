using MongoDB.Bson;
using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping;

public static class NegocioMappings
{
    public static NegocioDto ToDto(this NegocioEntity entity) => new()
    {
        Id = entity.Id.ToString(),
        Nombre = entity.Nombre,
        NombreNegocio = entity.NombreNegocio,
        UsuarioId = entity.UsuarioId.ToString(),
        NumeroRuc = entity.NumeroRuc,
        DireccionNegocio = entity.DireccionNegocio,
        TipoServicio = entity.TipoServicio,
        UrlFotoPerfil = entity.UrlFotoPerfil,
        UrlDocumentoTitulo = entity.UrlDocumentoTitulo,
        Categoria = entity.Categoria,
        FechaRegistro = entity.FechaRegistro,
        Horarios = entity.Horarios?.ToDictionary(
              h => h.Key,
              h => new HorarioDiaDto
              {
                  Apertura = h.Value.Apertura,
                  Cierre = h.Value.Cierre,
                  Pausas = h.Value.Pausas
              }
          ) ?? new Dictionary<string, HorarioDiaDto>(),
        DiasCerrados = entity.DiasCerrados ?? new List<string>()
    };

    public static NegocioEntity ToEntity(this NegocioDto dto, string? id = null) => new()
    {
        Id = !string.IsNullOrEmpty(id) ? ObjectId.Parse(id) : ObjectId.GenerateNewId(),
        Nombre = dto.Nombre.Trim(),
        NombreNegocio = dto.NombreNegocio.Trim(),


        UsuarioId = !string.IsNullOrEmpty(dto.UsuarioId) ? ObjectId.Parse(dto.UsuarioId).ToString() : string.Empty,

        NumeroRuc = dto.NumeroRuc.Trim(),
        DireccionNegocio = dto.DireccionNegocio.Trim(),
        TipoServicio = dto.TipoServicio,
        UrlFotoPerfil = dto.UrlFotoPerfil,
        UrlDocumentoTitulo = dto.UrlDocumentoTitulo,
        Categoria = dto.Categoria.Trim(),
        FechaRegistro = dto.FechaRegistro,
        Horarios = dto.Horarios?.ToDictionary(
             h => h.Key,
             h => new NegocioEntity.HorarioDiaEntity { Apertura = h.Value.Apertura, Cierre = h.Value.Cierre, Pausas = h.Value.Pausas }
         ) ?? new Dictionary<string, NegocioEntity.HorarioDiaEntity>(),
        DiasCerrados = dto.DiasCerrados ?? new List<string>()
    };

}
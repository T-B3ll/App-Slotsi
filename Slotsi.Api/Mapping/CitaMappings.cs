using MongoDB.Bson;
using Slotsi.Api.Entities;
using Slotsi.Shared.Dtos;

namespace Slotsi.Api.Mapping;

public static class CitaMappings
{
    public static CitaDto ToDto(this CitaEntity entity) => new()
    {
        Id = entity.Id.ToString(),
        NegocioId = entity.NegocioId.ToString(),
        ClienteId = entity.ClienteId.ToString(),
        TrabajadorId = entity.TrabajadorId.ToString(),
        ServicioId = entity.ServicioId.ToString(),
        ServicioNombre = entity.ServicioNombre,
        Precio = entity.Precio,
        Moneda = entity.Moneda,
        FechaHoraInicio = entity.FechaHoraInicio,
        FechaHoraFin = entity.FechaHoraFin,
        DuracionMinutos = entity.DuracionMinutos,
        Tipo = entity.Tipo,
        Estado = entity.Estado,
        MetodoPago = entity.MetodoPago,
        Notas = entity.Notas,
        ClienteNombre = entity.ClienteNombre,
        ClienteTelefono = entity.ClienteTelefono
    };

    public static CitaEntity ToEntity(this CitaDto dto, string? id = null)
    {
        return new CitaEntity
        {
            Id = !string.IsNullOrEmpty(id) ? ObjectId.Parse(id) : ObjectId.GenerateNewId(),
            NegocioId = ObjectId.Parse(dto.NegocioId),
            ClienteId = ObjectId.Parse(dto.ClienteId),
            TrabajadorId = string.IsNullOrEmpty(dto.TrabajadorId) ? ObjectId.Empty : ObjectId.Parse(dto.TrabajadorId),
            ServicioId = string.IsNullOrEmpty(dto.ServicioId) ? ObjectId.Empty : ObjectId.Parse(dto.ServicioId),
            ServicioNombre = dto.ServicioNombre,
            Precio = dto.Precio,
            Moneda = dto.Moneda,
            FechaHoraInicio = dto.FechaHoraInicio,
            FechaHoraFin = dto.FechaHoraFin,
            DuracionMinutos = dto.DuracionMinutos,
            Tipo = dto.Tipo,
            Estado = dto.Estado,
            MetodoPago = dto.MetodoPago,
            Notas = dto.Notas,
            ClienteNombre = dto.ClienteNombre,
            ClienteTelefono = dto.ClienteTelefono
        };
    }
}
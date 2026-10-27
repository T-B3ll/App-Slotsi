using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Slotsi.Api.Entities;

[BsonIgnoreExtraElements]
public class SuscripcionActualEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public string Estado { get; set; } = "Inactiva";
    public DateTime FechaUltimoPago { get; set; }
    public DateTime ProximoVencimiento { get; set; }
    public decimal MontoPagado { get; set; }
}

[BsonIgnoreExtraElements]
public class RegistroPagoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoTransaccion { get; set; } = "Completado";
}
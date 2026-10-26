using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Slotsi.Api.Entities;

[BsonIgnoreExtraElements]
public class ClienteInfo
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
[BsonIgnoreExtraElements]public class CitaEntity
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId NegocioId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId TrabajadorId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ServicioId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId ClienteId { get; set; }

    public string ServicioNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "NIO";
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public int DuracionMinutos { get; set; }
    public string Tipo { get; set; } = "EnLocal";
    public string Estado { get; set; } = "Confirmada";
    public string MetodoPago { get; set; } = "Efectivo";
    public string Notas { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ClienteInfo? Cliente { get; set; }

    [BsonIgnoreIfNull] public string? ClienteNombre { get; set; }
    [BsonIgnoreIfNull] public string? ClienteTelefono { get; set; }
}
 
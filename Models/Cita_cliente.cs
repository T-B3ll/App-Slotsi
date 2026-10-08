using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Cita_cliente
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Fecha { get; set; }

        // Guardamos la hora como string para facilitar la comparación en la UI ("4:00 PM")
        public string Hora { get; set; } = string.Empty;

        public string NombreCliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Estado { get; set; } = "PENDIENTE";
        public string? TelefonoCliente { get; set; }
        public string? Notas { get; set; }
        public double? Precio { get; set; }

        // Campo nuevo para vincular con el negocio específico
        public string? IdNegocio { get; set; }

        [BsonIgnore]
        public string ColorEstado => Estado?.ToUpper() switch
        {
            "CONFIRMADA" => "#10B981",
            "PENDIENTE" => "#F59E0B",
            "CANCELADA" => "#EF4444",
            "OCUPADO" => "#6B7280",
            _ => "#6B7280"
        };

        [BsonIgnore]
        public string ColorFondoEstado => Estado?.ToUpper() switch
        {
            "CONFIRMADA" => "#ECFDF5",
            "PENDIENTE" => "#FFFBEB",
            "CANCELADA" => "#FEF2F2",
            "OCUPADO" => "#F3F4F6",
            _ => "#F3F4F6"
        };
    }
}
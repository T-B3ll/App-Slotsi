using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements] // Evita errores si MongoDB tiene campos adicionales en el documento
    public class Cita_cliente
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Cambiado de int a string ObjectId para MongoDB

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Fecha { get; set; }

        public string Hora { get; set; } = string.Empty; // Guardar la hora como string "4:00 PM" evita errores de serialización BSON

        [BsonIgnore] // Propiedad calculada solo para la UI (MongoDB la ignora al guardar)
        public string HoraDisplay => Hora;

        public string NombreCliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Estado { get; set; } = "PENDIENTE"; // "CONFIRMADA", "PENDIENTE", "CANCELADA"
        public string? TelefonoCliente { get; set; }
        public string? Notas { get; set; }
        public double? Precio { get; set; }

        // Colores para la interfaz (Borde y Fondo)
        [BsonIgnore]
        public string ColorEstado => Estado?.ToUpper() switch
        {
            "CONFIRMADA" => "#10B981", // Verde
            "PENDIENTE" => "#F59E0B",  // Naranja
            "CANCELADA" => "#EF4444",  // Rojo
            "OCUPADO" => "#6B7280",  // Gris
            _ => "#6B7280"
        };

        [BsonIgnore]
        public string ColorFondoEstado => Estado?.ToUpper() switch
        {
            "CONFIRMADA" => "#ECFDF5", // Verde claro
            "PENDIENTE" => "#FFFBEB",  // Naranja claro
            "CANCELADA" => "#FEF2F2",  // Rojo claro
            "OCUPADO" => "#F3F4F6",  // Gris claro
            _ => "#F3F4F6"
        };
    }
}
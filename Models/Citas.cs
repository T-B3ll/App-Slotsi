using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class Citas
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string NegocioId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string TrabajadorId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ServicioId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ClienteId { get; set; }

        public ClienteSnapshot Cliente { get; set; }
        public string ServicioNombre { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Precio { get; set; }

        public string Moneda { get; set; } = "NIO";
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int DuracionMinutos { get; set; }
        public string Tipo { get; set; } = "EnLocal";
        public string Estado { get; set; } = "Confirmada";
        public string MetodoPago { get; set; } = "Efectivo";
        public string Notas { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Propiedad calculada para coincidir con el Binding {Binding Cita.NombreCliente} del XAML
        [BsonIgnore]
        public string NombreCliente => Cliente?.Nombre ?? "Cliente";
    }

    public class ClienteSnapshot
    {
        public string Nombre { get; set; }
        public string Telefono { get; set; }
    }
}


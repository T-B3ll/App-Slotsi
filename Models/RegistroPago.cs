
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class RegistroPago
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        public decimal Monto { get; set; }


        public string MetodoPago { get; set; } = string.Empty;

        public string EstadoTransaccion { get; set; } = "Completado";
    }
}

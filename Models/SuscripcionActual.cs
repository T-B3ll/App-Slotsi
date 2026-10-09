using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class SuscripcionActual
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

    
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaUltimoPago { get; set; }

        public DateTime ProximoVencimiento { get; set; }

        public decimal MontoPagado { get; set; } = 5.00m;

  
    }
}

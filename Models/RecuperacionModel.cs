using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class RecuperacionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Correo { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;

        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; } = false;
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Servicios
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("negocio_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string NegocioId { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [BsonElement("precio")]
        public decimal Precio { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; }

        [BsonElement("colaboradores_habilitados")]
        public List<ObjectId> ColaboradoresHabilitados { get; set; }

        [BsonElement("disponible_domicilio")]
        public bool DisponibleDomicilio { get; set; }

        [BsonElement("activo")]
        public bool Activo { get; set; }

        [BsonElement("foto")]
        public string Foto { get; set; }

        public bool TieneFoto => !string.IsNullOrEmpty(Foto);
        public bool NoTieneFoto => string.IsNullOrEmpty(Foto);
    }

}

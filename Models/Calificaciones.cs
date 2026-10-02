using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    class Calificaciones
    { 
        public class CalificacionModel
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }

            [BsonElement("id_negocio")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string IdNegocio { get; set; }

            [BsonElement("id_usuario")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string IdUsuario { get; set; }

            [BsonElement("id_colaborador")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string IdColaborador { get; set; }

            [BsonElement("rango_calificacion")]
            public int RangoCalificacion { get; set; }

            [BsonElement("negocio_calificado")]
            public string NegocioCalificado { get; set; }

            [BsonElement("usuario_calificador")]
            public string UsuarioCalificador { get; set; }

            [BsonElement("comentario")]
            public string Comentario { get; set; }

            // Propiedad calculada para convertir el entero (ej. 3) en estrellas visuales ("★★★☆☆")
            [BsonIgnore]
            public string EstrellasVisuales => new string('★', Math.Clamp(RangoCalificacion, 0, 5)) +
                                               new string('☆', Math.Max(0, 5 - RangoCalificacion));
        }
    }
}

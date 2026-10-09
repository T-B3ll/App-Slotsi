using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    public class Calificaciones
    {
        [BsonIgnoreExtraElements]
        public class CalificacionModel
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? Id { get; set; }

            [BsonElement("id_negocio")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string IdNegocio { get; set; } = string.Empty;

            [BsonElement("id_colaborador")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? IdColaborador { get; set; }

            [BsonElement("id_usuario_cliente")]
            [BsonRepresentation(BsonType.ObjectId)]
            public string IdUsuarioCliente { get; set; } = string.Empty;

            [BsonElement("rango_calificacion")]
            public int RangoCalificacion { get; set; }

            [BsonElement("negocio_calificado")]
            public bool NegocioCalificado { get; set; }

            [BsonElement("nombre_negocio")]
            public string NombreNegocio { get; set; } = string.Empty;

            [BsonElement("nombre_usuario")]
            public string NombreUsuario { get; set; } = string.Empty;

            [BsonElement("comentario")]
            public string Comentario { get; set; } = string.Empty;

            [BsonIgnore]
            public string EstrellasVisuales => new string('★', Math.Clamp(RangoCalificacion, 0, 5)) +
                                               new string('☆', Math.Max(0, 5 - RangoCalificacion));

            [BsonIgnore]
            public string TituloMostrado { get; set; } = string.Empty;

            [BsonIgnore]
            public string SubtituloMostrado { get; set; } = string.Empty;
        }
    }
}
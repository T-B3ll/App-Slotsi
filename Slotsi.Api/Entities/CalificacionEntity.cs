using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Slotsi.Api.Entities
{
    [BsonIgnoreExtraElements]
    public class CalificacionEntity
    {
        [BsonId] public ObjectId Id { get; set; }
        [BsonElement("id_negocio")] public ObjectId IdNegocio { get; set; }
        [BsonElement("id_colaborador")] public ObjectId? IdColaborador { get; set; }
        [BsonElement("id_usuario_cliente")] public ObjectId IdUsuarioCliente { get; set; }
        [BsonElement("rango_calificacion")] public int RangoCalificacion { get; set; }
        [BsonElement("negocio_calificado")] public bool NegocioCalificado { get; set; }
        [BsonElement("nombre_negocio")] public string? NombreNegocio { get; set; }
        [BsonElement("nombre_usuario")] public string? NombreUsuario { get; set; }
        [BsonElement("comentario")] public string? Comentario { get; set; }
    }
}

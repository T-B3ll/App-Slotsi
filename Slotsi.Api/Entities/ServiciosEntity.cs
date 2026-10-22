using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Slotsi.Api.Entities
{
    [BsonIgnoreExtraElements]
    public class ServiciosEntity
    {
        [BsonId] public ObjectId Id { get; set; }
        [BsonElement("negocio_id")] public ObjectId NegocioId { get; set; }
        [BsonElement("nombre")] public string? Nombre { get; set; }
        [BsonElement("descripcion")] public string? Descripcion { get; set; }
        [BsonElement("duracion_minutos")] public int DuracionMinutos { get; set; }
        [BsonElement("precio")][BsonRepresentation(BsonType.Decimal128)]
        public decimal Precio { get; set; }
        [BsonElement("categoria")] public string? Categoria { get; set; }
        [BsonElement("colaboradores_habilitados")] public List<ObjectId>? ColaboradoresHabilitados { get; set; }
        [BsonElement("disponible_domicilio")] public bool DisponibleDomicilio { get; set; }
        [BsonElement("activo")] public bool Activo { get; set; }
        [BsonElement("foto")] public string? Foto { get; set; }
    }
}

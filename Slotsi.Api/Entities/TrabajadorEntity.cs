using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Slotsi.Api.Entities
{
    [BsonIgnoreExtraElements]
    public class TrabajadorEntity
    {
        [BsonId] public ObjectId Id { get; set; }
        [BsonElement("negocio_id")] public ObjectId NegocioId { get; set; }
        [BsonElement("nombre")] public string? Nombre { get; set; }
        [BsonElement("especialidad")] public string? Especialidad { get; set; }
        [BsonElement("correo")] public string? Correo { get; set; }
        [BsonElement("telefono")] public string? Telefono { get; set; }
        [BsonElement("cedula")] public string? Cedula { get; set; }
        [BsonElement("foto")] public string? Foto { get; set; }
        [BsonElement("archivoprofesional")] public string? ArchivoProfesional { get; set; }
        [BsonElement("activo")] public bool Activo { get; set; }
        [BsonElement("horario_trabajo")] public List<HorarioTrabajoEntity>? HorarioTrabajo { get; set; }
    }

    public class HorarioTrabajoEntity
    {
        [BsonElement("dia")] public string? Dia { get; set; }
        [BsonElement("inicio")] public string? Inicio { get; set; }
        [BsonElement("fin")] public string? Fin { get; set; }
    }
}


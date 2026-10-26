using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Slotsi.Api.Entities;

[BsonIgnoreExtraElements]
public class UsuarioEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("NombreCompleto")]
    public string NombreCompleto { get; set; } = string.Empty;

    [BsonElement("Correo")]
    public string Correo { get; set; } = string.Empty;

    [BsonElement("Telefono")]
    public string Telefono { get; set; } = string.Empty;

    [BsonElement("Cedula")]
    public string Cedula { get; set; } = string.Empty;

    [BsonElement("Contrasena")]
    public string Contrasena { get; set; } = string.Empty;

    [BsonElement("TipoUsuario")]
    public bool TipoUsuario { get; set; } = false;

    [BsonElement("EstaActivo")]
    public bool EstaActivo { get; set; } = true;

    [BsonElement("resetTokenHash")]
    public string? ResetTokenHash { get; set; }

    [BsonElement("resetTokenExpira")]
    public DateTime? ResetTokenExpira { get; set; }
}
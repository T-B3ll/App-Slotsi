using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    [BsonIgnoreExtraElements]
    public class Negocio
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("NombreNegocio")]
        public string NombreNegocio { get; set; } = string.Empty;

        [BsonElement("UsuarioId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;

        [BsonElement("NumeroRuc")]
        public string NumeroRuc { get; set; } = string.Empty;

        [BsonElement("DireccionNegocio")]
        public string DireccionNegocio { get; set; } = string.Empty;

        [BsonElement("TipoServicio")]
        public string TipoServicio { get; set; } = string.Empty;

        [BsonElement("UrlFotoPerfil")]
        public string UrlFotoPerfil { get; set; } = string.Empty;

        [BsonElement("UrlDocumentoTitulo")]
        public string UrlDocumentoTitulo { get; set; } = string.Empty;

        [BsonElement("Categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [BsonElement("Horarios")]
        public Dictionary<string, HorarioDia> Horarios { get; set; } = new();

        [BsonElement("DiasCerrados")]
        public List<string> DiasCerrados { get; set; } = new();

        public class HorarioDia
        {
            [BsonElement("a")] public string Apertura { get; set; } = "";
            [BsonElement("c")] public string Cierre { get; set; } = "";
            [BsonElement("p")] public List<string> Pausas { get; set; } = new();
        }

    }

}



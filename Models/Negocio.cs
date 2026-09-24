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

       


        [BsonElement("NombreNegocio")]
        public string NombreNegocio { get; set; } = string.Empty;




        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;

     
        public string NumeroRuc { get; set; } = string.Empty;
        public string DireccionNegocio { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;


        public string UrlFotoPerfil { get; set; } = string.Empty;
        public string UrlDocumentoTitulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }



}





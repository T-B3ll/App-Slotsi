using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    public class Negocio
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

    
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;

     
        public string NumeroRuc { get; set; } = string.Empty;
        public string DireccionNegocio { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;


        public string UrlFotoPerfil { get; set; } = string.Empty;
        public string UrlDocumentoTitulo { get; set; } = string.Empty; 

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }



}





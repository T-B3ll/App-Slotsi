using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    public class Negocio
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string CedulaJuridica { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;


        public string UrlFotoPerfil { get; set; }

        public bool TipoUsuario { get; set; } = false;
        public bool EstaActivo { get; set; } = true;




        public string NumeroRuc { get; set; } = string.Empty;
        public string DireccionNegocio { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;

        public string UrlDocumentoRuc { get; set; } = string.Empty; 
        public string UrlDocumentoTitulo { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;



    }



}

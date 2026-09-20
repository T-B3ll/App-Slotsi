using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class Usuario
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;

     
        public string Contrasena { get; set; } = string.Empty;


        public bool TipoUsuario { get; set; } = false;

       
        public bool EstaActivo { get; set; } = true;
    }
}

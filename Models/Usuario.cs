using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace slotsi_citas.Models
{
    public class Usuario
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

        // Propiedad calculada para extraer las iniciales del Nombre y Apellido (ej: "José José" -> "JJ")
        [BsonIgnore]
        public string InicialesNombre
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreCompleto))
                    return "?";

                var partes = NombreCompleto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 1)
                {
                    return partes[0].Substring(0, 1).ToUpper();
                }

                return (partes[0].Substring(0, 1) + partes[1].Substring(0, 1)).ToUpper();
            }
        }

        // Propiedad calculada para asignar un color dinámico según la letra inicial
        [BsonIgnore]
        public string ColorAvatar
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreCompleto))
                    return "#9CA3AF";

                char primeraLetra = char.ToUpper(NombreCompleto.Trim()[0]);

                // Paleta de colores atractivos asignados según la letra inicial
                return primeraLetra switch
                {
                    'A' => "#38BDF8", // Celeste / Sky
                    'B' => "#10B981", // Verde / Emerald
                    'C' => "#F59E0B", // Ámbar / Naranja
                    'D' => "#8B5CF6", // Violeta
                    'E' => "#EC4899", // Rosa
                    'F' => "#06B6D4", // Cian
                    'G' => "#84CC16", // Lime
                    'H' => "#F97316", // Naranja brillante
                    'I' => "#6366F1", // Índigo
                    'J' => "#2563EB", // Azul corporativo
                    'K' => "#14B8A6", // Teal
                    'L' => "#A855F7", // Púrpura
                    'M' => "#E11D48", // Rojo rosa
                    'N' => "#0284C7", // Azul claro
                    'O' => "#D97706", // Ámbar oscuro
                    'P' => "#7C3AED", // Violeta oscuro
                    'Q' => "#DB2777", // Fucsia
                    'R' => "#059669", // Verde esmeralda
                    'S' => "#0EA5E9", // Azul turquesa
                    'T' => "#4F46E5", // Índigo oscuro
                    'U' => "#C026D3", // Magenta
                    'V' => "#16A34A", // Verde
                    'W' => "#D946EF", // Fucsia claro
                    'X' => "#CA8A04", // Amarillo oscuro
                    'Y' => "#2563EB", // Azul
                    'Z' => "#0D9488", // Verde azulado
                    _ => "#6B7280"  // Gris por defecto
                };
            }
        }
    }
}
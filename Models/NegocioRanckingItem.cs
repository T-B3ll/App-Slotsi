using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class NegocioRanckingItem
    {
        public Negocio Negocio { get; init; } = null!;

        public string Titulo => string.IsNullOrWhiteSpace(Negocio.Nombre) ? Negocio.NombreNegocio : Negocio.Nombre;
        public string NombreNegocio => Negocio.NombreNegocio;
        public string Categoria => Negocio.Categoria;
        public string UrlFotoPerfil => Negocio.UrlFotoPerfil;

        public double Promedio => Negocio.PromedioCalificacion;
        public string PromedioTexto => Promedio.ToString("F1");
        public string ResenasTexto => $"({Negocio.TotalResenas} reseñas)";

        public string Iniciales
        {
            get
            {
                var partes = Titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 0) return "?";
                if (partes.Length == 1) return partes[0][..1].ToUpper();
                return $"{partes[0][0]}{partes[1][0]}".ToUpper();
            }
        }

        public string Estrellas
        {
            get
            {
                int llenas = Math.Clamp((int)Math.Round(Promedio), 0, 5);
                return new string('★', llenas) + new string('☆', 5 - llenas);
            }
        }
    }
}

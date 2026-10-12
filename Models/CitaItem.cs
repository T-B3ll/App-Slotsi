using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
    public class CitaItem
    {
        public class CitaCalendarioItem
        {
            public string ClienteNombre { get; set; } = string.Empty;
            public string ServicioYHora { get; set; } = string.Empty;
            public string HoraInicio { get; set; } = string.Empty;
            public string ColorBarra { get; set; } = "#34C759";
            public string ColorFondo { get; set; } = "#E8F8F0";
        }

        public class DiaSemanaItem
        {
            public DateTime Fecha { get; set; }
            public string LetraDia => Fecha.ToString("ddd");
            public string NumeroDia => Fecha.ToString("dd");
            public bool EsSeleccionado { get; set; }
            public string ColorTextoDia => EsSeleccionado ? "#2F6BFF" : "#8E8E93";
            public string ColorTextoNumero => EsSeleccionado ? "White" : "#1C1C1E";
            public string ColorFondoNumero => EsSeleccionado ? "#2F6BFF" : "Transparent";
        }

        public class ServicioItem
        {
            public string Nombre { get; set; } = string.Empty;
            public bool EsSeleccionado { get; set; }
            public string ColorFondo => EsSeleccionado ? "#2563EB" : "#F3F4F6";
            public string ColorTexto => EsSeleccionado ? "White" : "#374151";
        }

        public class MetodoPagoItem
        {
            public string Titulo { get; set; } = string.Empty;
            public string Icono { get; set; } = string.Empty;
            public bool EsSeleccionado { get; set; }
            public string ColorBorde => EsSeleccionado ? "#2563EB" : "#E5E7EB";
            public string ColorFondoIcono => EsSeleccionado ? "#2563EB" : "#F3F4F6";
            public string ColorIcono => EsSeleccionado ? "White" : "#4B5563";
        }
    }
}

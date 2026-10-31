using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Models
{
        public class CancelacionesReporteData
        {
            public string NombreNegocio { get; set; } = string.Empty;
            public string DireccionNegocio { get; set; } = string.Empty;
            public string Periodo { get; set; } = string.Empty;
            public DateTime Desde { get; set; }
            public DateTime Hasta { get; set; }
            public int Total { get; set; }
            public int Canceladas { get; set; }
            public int NoAsistio { get; set; }
            public double PorcentajePerdida { get; set; }
            public double PorcentajeCancelacion { get; set; }
            public double PorcentajeNoAsistio { get; set; }
            public List<CancelacionFila> Citas { get; set; } = new();
        }

    public class CancelacionFila
    {
        public string Fecha { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}

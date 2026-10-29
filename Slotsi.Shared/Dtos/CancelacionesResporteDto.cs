using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class CancelacionesResporteDto
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int Total { get; set; }
        public int Canceladas { get; set; }
        public int NoAsistio { get; set; }

        public double PorcentajeCancelacion { get; set; } 
        public double PorcentajeNoAsistio { get; set; }
        public double PorcentajePerdida { get; set; }  
        public List<CitaDto> Citas { get; set; } = new();
    }
}


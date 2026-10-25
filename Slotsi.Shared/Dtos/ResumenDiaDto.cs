using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class ResumenDiaDto
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int CitasConfirmadas { get; set; }
        public int CitasPendientes { get; set; }
        public int OcupacionPorcentaje { get; set; }
        public int ServiciosActivosHoy { get; set; }
        public List<CitaDto> Citas { get; set; } = new();
    }
}

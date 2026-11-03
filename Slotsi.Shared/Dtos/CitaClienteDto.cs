using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class CitaClienteDto
    {
        public string Id { get; set; } = string.Empty;
        public string NegocioNombre { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
        public DateTime FechaHoraInicio { get; set; }
        public string? Estado { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class ServiciosDto
    {
        public string? Id { get; set; }
        public string NegocioId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int DuracionMinutos { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = "";
        public List<string> ColaboradoresHabilitados { get; set; } = new();
        public bool DisponibleDomicilio { get; set; }
        public bool Activo { get; set; }
        public string Foto { get; set; } = "";
    }
}

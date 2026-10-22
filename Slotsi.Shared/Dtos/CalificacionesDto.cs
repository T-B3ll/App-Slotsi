using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slotsi.Shared.Dtos
{
    public class CalificacionesDtos
    {
        public string? Id { get; set; }
        public string IdNegocio { get; set; } = "";
        public string? IdColaborador { get; set; }
        public string IdUsuarioCliente { get; set; } = "";
        [Range(1, 5)] public int RangoCalificacion { get; set; } = 5;
        public bool NegocioCalificado { get; set; }
        public string NombreNegocio { get; set; } = "";
        public string NombreUsuario { get; set; } = "";
        public string Comentario { get; set; } = "";
    }

    public class PromedioCalificacionDto
    {
        public double Promedio { get; set; }
        public int Total { get; set; }
    }
}
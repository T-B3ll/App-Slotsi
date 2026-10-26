using System;

namespace slotsi_citas.Models
{
    public class Calificaciones
    {  
        public class CalificacionModel
        {
            public string? Id { get; set; }
            public string IdNegocio { get; set; } = string.Empty;
            public string? IdColaborador { get; set; }
            public string IdUsuarioCliente { get; set; } = string.Empty;
            public int RangoCalificacion { get; set; } = 5;
            public bool NegocioCalificado { get; set; } = false;
            public string NombreNegocio { get; set; } = string.Empty;
            public string NombreUsuario { get; set; } = string.Empty;
            public string Comentario { get; set; } = string.Empty;
            public string TituloMostrado { get; set; } = string.Empty;
            public string SubtituloMostrado { get; set; } = string.Empty;
        }
    }
}
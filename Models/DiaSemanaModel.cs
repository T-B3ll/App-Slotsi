using System;

namespace slotsi_citas.Models
{
    public class DiaSemanaModel
    {
        public DateTime Fecha { get; set; }
        public string NombreDia => Fecha.ToString("ddd");
        public string NumeroDia => Fecha.ToString("dd");
        public bool EsSeleccionado { get; set; }
    }
}
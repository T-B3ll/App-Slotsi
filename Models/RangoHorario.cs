using System;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Models
{
    public class RangoHorario : BindableObject
    {
        public string HoraDisplay { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }

        public bool EsDisponible { get; set; }
        public bool EsOcupado { get; set; }
        public bool EsNoDisponible { get; set; }

        public Citas Cita { get; set; }
    }
}
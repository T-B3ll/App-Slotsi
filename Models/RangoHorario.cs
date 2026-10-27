using System;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Models
{
    public class RangoHorario : BindableObject
    {
        public string HoraDisplay { get; set; } = string.Empty;
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }

        public bool EsDisponible { get; set; }
        public bool EsOcupado { get; set; }
        public bool EsNoDisponible { get; set; }

        // --- NUEVA PROPIEDAD ---
        public bool EsMiCita { get; set; }

        public string EstadoTexto { get; set; } = "Agendar cita";
        public string DetallePausa { get; set; } = string.Empty;

        public Citas? Cita { get; set; }

        public string RangoDisplay => $"{HoraDisplay} - {DateTime.Today.Add(HoraFin.TimeOfDay):h:mm tt}";
    }
}
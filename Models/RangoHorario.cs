using System;

namespace slotsi_citas.Models
{
    public class RangoHorario
    {
        public TimeSpan Hora { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        private string? _horaDisplay;
        public string HoraDisplay
        {
            get => _horaDisplay ?? DateTime.Today.Add(Hora != default ? Hora : HoraInicio).ToString("h:mm tt");
            set => _horaDisplay = value;
        }

        // Permite asignarle tanto Cita_cliente como CitaUsuario sin error de conversión
        public dynamic? Cita { get; set; }

        // Propiedades de estado para controlar la visibilidad y la navegación en la ViewModel
        public bool EsDisponible => Cita == null;

        public bool EsOcupado => Cita != null && GetEstadoCita() == "Ocupado";

        public bool EsNoDisponible => Cita != null && GetEstadoCita() == "NoDisponible";

        // Muestra el detalle del horario en tarjetas de ocupado/no disponible si existe
        public string HorarioDetalle => Cita?.NombreCliente ?? $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";

        private string GetEstadoCita()
        {
            if (Cita == null) return string.Empty;

            try
            {
                // Evalúa la propiedad Estado independientemente del tipo asignado en dynamic
                return Cita.Estado?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
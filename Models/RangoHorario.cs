using System;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Models
{
    public class RangoHorario : BindableObject
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

        private dynamic? _cita;
        // Permite asignar tanto en Cita_cliente como CitaUsuario sin error de conversión
        public dynamic? Cita
        {
            get => _cita;
            set
            {
                if (_cita != value)
                {
                    _cita = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EsDisponible));
                    OnPropertyChanged(nameof(EsOcupado));
                    OnPropertyChanged(nameof(EsNoDisponible));
                    OnPropertyChanged(nameof(EstadoTexto));
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorTexto));
                    OnPropertyChanged(nameof(HorarioDetalle));
                }
            }
        }

        private bool _esOcupadoManual;
        public bool EsOcupadoManual
        {
            get => _esOcupadoManual;
            set
            {
                if (_esOcupadoManual != value)
                {
                    _esOcupadoManual = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EsDisponible));
                    OnPropertyChanged(nameof(EsOcupado));
                    OnPropertyChanged(nameof(EstadoTexto));
                    OnPropertyChanged(nameof(ColorFondo));
                    OnPropertyChanged(nameof(ColorTexto));
                }
            }
        }

        // Propiedades de estado para controlar la visibilidad y la navegación en la ViewModel
        public bool EsDisponible => Cita == null && !EsOcupadoManual;

        public bool EsOcupado => EsOcupadoManual || (Cita != null && GetEstadoCita() == "Ocupado");

        public bool EsNoDisponible => Cita != null && GetEstadoCita() == "NoDisponible";

        // Texto que se muestra en la tarjeta interactiva
        public string EstadoTexto => EsOcupado ? "No disponible" : "Agendar cita";

        // Garantiza colores sólidos para evitar tarjetas transparentes en XAML
        public string ColorFondo => EsOcupado ? "#E5E5EA" : "#FFFFFF";
        public string ColorTexto => EsOcupado ? "#8E8E93" : "#2F6BFF";

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
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

       
        public dynamic? Cita { get; set; }
    }
}
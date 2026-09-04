namespace slotsi_citas.Models
{
    public class Cita_Usuario
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string TelefonoCliente { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Estado { get; set; } = "Confirmado"; // Confirmado, Ocupado, NoDisponible, Pendiente
        public string Notas { get; set; } = string.Empty;

        public string DetalleServicio => $"{Servicio} ({DateTime.Today.Add(HoraInicio):h:mm tt} - {DateTime.Today.Add(HoraFin):h:mm tt})";
    }
}
using App.Models;
using System.Collections.Generic;

namespace slotsi_citas.Services
{
    public static class CitaRepository
    {
        // Almacena las citas por hora (ej. "4:00 PM")
        public static Dictionary<string, Cita_cliente> CitasRegistradas { get; } = new Dictionary<string, Cita_cliente>();
    }
}
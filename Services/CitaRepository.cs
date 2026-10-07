using slotsi_citas.Models;
using System.Collections.Generic;

namespace slotsi_citas.Services
{
    public static class CitaRepository
    {
        public static Dictionary<string, Cita_cliente> CitasRegistradas { get; } = new Dictionary<string, Cita_cliente>();

        // Método para vaciar el caché local
        public static void LimpiarCache()
        {
            CitasRegistradas.Clear();
        }
    }
}
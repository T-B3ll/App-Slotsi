using slotsi_citas.Models;
using System.Collections.Generic;

namespace slotsi_citas.Services
{
    public static class CitaRepository
    {
        public static Dictionary<string, Citas> CitasRegistradas { get; } = new Dictionary<string, Citas>();

        // Método para vaciar el caché local
        public static void LimpiarCache()
        {
            CitasRegistradas.Clear();
        }
    }
}
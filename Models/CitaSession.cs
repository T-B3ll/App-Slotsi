using slotsi_citas.Models;

namespace slotsi_citas.Services
{
    public static class CitaSession
    {
        public static RangoHorario? HorarioSeleccionado { get; set; }
        public static DateTime FechaSeleccionada { get; set; } = DateTime.Today;
        public static string DireccionCliente { get; set; } = "Matagalpa, Nicaragua";

        // NUEVO: bandera para saber si el último guardado fue exitoso
        public static bool? UltimoGuardadoExitoso { get; set; }
    }
}
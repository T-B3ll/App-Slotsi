using System;
using System.Collections.Generic;
using slotsi_citas.Models;

namespace slotsi_citas.Services
{
    public static class CitaSession
    {
        public static RangoHorario? HorarioSeleccionado { get; set; }
        public static DateTime FechaSeleccionada { get; set; } = DateTime.Today;
        public static string DireccionCliente { get; set; } = "Matagalpa, Nicaragua";
        public static bool? UltimoGuardadoExitoso { get; set; }

        public static Negocio? NegocioActual { get; set; }
        public static string NegocioId { get; set; } = string.Empty;
        public static List<Servicios> ServiciosSeleccionados { get; set; } = new();
    }
}
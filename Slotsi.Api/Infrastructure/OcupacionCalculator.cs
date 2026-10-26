using Slotsi.Api.Entities;

namespace Slotsi.Api.Infrastructure
{
    public static class OcupacionCalculator
    {
        private static readonly string[] Dias = { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };

        private const int MinutosPorDefecto = 8 * 60; // solo si el negocio no tiene horarios configurados

        /// <summary>Minutos que el negocio está abierto en el rango, para UN trabajador.</summary>
        public static int MinutosDisponiblesPorTrabajador(NegocioEntity negocio, DateTime desde, DateTime hasta)
        {
            var dias = (hasta.Date - desde.Date).Days + 1;

            // Negocio sin horarios cargados: usar la jornada por defecto.
            if (negocio.Horarios is null || negocio.Horarios.Count == 0)
                return MinutosPorDefecto * dias;

            var total = 0;
            for (var dia = desde.Date; dia <= hasta.Date; dia = dia.AddDays(1))
            {
                var clave = Dias[(int)dia.DayOfWeek];

                if (negocio.DiasCerrados.Contains(clave, StringComparer.OrdinalIgnoreCase)) continue;
                if (!negocio.Horarios.TryGetValue(clave, out var horario) || horario is null) continue;

                total += MinutosDelDia(horario);
            }
            return total;
        }

        private static int MinutosDelDia(NegocioEntity.HorarioDiaEntity h)
        {
            if (!TimeSpan.TryParse(h.Apertura, out var apertura) ||
                !TimeSpan.TryParse(h.Cierre, out var cierre)) return 0;

            if (cierre <= apertura) return 0; // dato inválido (ej. cierre "04:00"): no suma

            var minutos = (int)(cierre - apertura).TotalMinutes;

            // Pausas con formato "HH:mm-HH:mm" (ej. "12:00-13:00"). Si no se entiende, se ignora.
            foreach (var pausa in h.Pausas ?? new List<string>())
            {
                var partes = pausa.Split('-', StringSplitOptions.TrimEntries);
                if (partes.Length == 2 &&
                    TimeSpan.TryParse(partes[0], out var inicio) &&
                    TimeSpan.TryParse(partes[1], out var fin) &&
                    fin > inicio)
                {
                    minutos -= (int)(fin - inicio).TotalMinutes;
                }
            }

            return Math.Max(0, minutos);
        }
    }
}

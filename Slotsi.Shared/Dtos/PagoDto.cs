using System;

namespace Slotsi.Shared.Dtos;

public class PagoDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public string EstadoTransaccion { get; set; } = "Completado";
}

public class SuscripcionActivaDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string Estado { get; set; } = "Inactiva";
    public DateTime FechaUltimoPago { get; set; }
    public DateTime ProximoVencimiento { get; set; }
    public decimal MontoPagado { get; set; }
}
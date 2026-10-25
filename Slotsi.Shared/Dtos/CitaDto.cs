using System;
using System.ComponentModel.DataAnnotations;

namespace Slotsi.Shared.Dtos;

public class CitaDto
{
    public string? Id { get; set; }

    [Required]
    public string NegocioId { get; set; } = string.Empty;

    [Required]
    public string ClienteId { get; set; } = string.Empty;

    public string? TrabajadorId { get; set; }
    public string? ServicioId { get; set; }

    public string ServicioNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "NIO";

    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public int DuracionMinutos { get; set; }

    public string Tipo { get; set; } = "EnLocal";
    public string Estado { get; set; } = "Confirmada";
    public string MetodoPago { get; set; } = "Efectivo";
    public string Notas { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
}
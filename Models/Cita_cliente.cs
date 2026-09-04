namespace App.Models;

public class Cita_cliente
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }

    public string HoraDisplay => DateTime.Today.Add(Hora).ToString("h:mm tt");

    public string NombreCliente { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string Estado { get; set; } = "PENDIENTE"; // "CONFIRMADA", "PENDIENTE", "CANCELADA"
    public string? TelefonoCliente { get; set; }
    public string? Notas { get; set; }
    public double? Precio { get; set; }

    // Borde de la tarjeta
    public string ColorEstado => Estado?.ToUpper() switch
    {
        "CONFIRMADA" => "#10B981", // Verde
        "PENDIENTE" => "#F59E0B", // Naranja
        "CANCELADA" => "#EF4444", // Rojo
        _ => "#6B7280"  // Gris
    };

    public string ColorFondoEstado => Estado?.ToUpper() switch
    {
        "CONFIRMADA" => "#ECFDF5", // Verde claro
        "PENDIENTE" => "#FFFBEB", // Naranja claro
        "CANCELADA" => "#FEF2F2", // Rojo claro
        _ => "#F3F4F6"
    };
}
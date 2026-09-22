namespace slotsi_citas.Models;

public class CitaCalendarioItem
{
    public string ClienteNombre { get; set; } = string.Empty;
    public string ServicioYHora { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string ColorBarra { get; set; } = "#34C759"; 
    public string ColorFondo { get; set; } = "#E8F8F0";
}

public class DiaSemanaItem
{
    public string LetraDia { get; set; } = string.Empty;
    public string NumeroDia { get; set; } = string.Empty;
    public bool EsSeleccionado { get; set; }
    public string ColorTextoDia => EsSeleccionado ? "#2F6BFF" : "#8E8E93";
    public string ColorTextoNumero => EsSeleccionado ? "White" : "#1C1C1E";
    public string ColorFondoNumero => EsSeleccionado ? "#2F6BFF" : "Transparent";
}
namespace slotsi_citas.Models;

public class ServicioItem
{
    public string Nombre { get; set; } = string.Empty;
    public bool EsSeleccionado { get; set; }
    public string ColorFondo => EsSeleccionado ? "#2563EB" : "#F3F4F6";
    public string ColorTexto => EsSeleccionado ? "White" : "#374151";
}

public class DetallePedidoItem
{
    public string Tag { get; set; } = string.Empty;
    public bool TieneTag => !string.IsNullOrEmpty(Tag);
    public string Titulo { get; set; } = string.Empty;
    public string Duracion { get; set; } = string.Empty;
    public string PlaceholderImagen { get; set; } = string.Empty;
}

public class MetodoPagoItem
{
    public string Titulo { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public bool EsSeleccionado { get; set; }
    public string ColorBorde => EsSeleccionado ? "#2563EB" : "#E5E7EB";
    public string ColorFondoIcono => EsSeleccionado ? "#2563EB" : "#F3F4F6";
    public string ColorIcono => EsSeleccionado ? "White" : "#4B5563";
}
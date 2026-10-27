namespace Slotsi.Shared.Dtos;

public class TopServicioDto
{
    public string NombreServicio { get; set; } = string.Empty;
    public int TotalCitas { get; set; }
    public decimal IngresosTotales { get; set; }
    public int DuracionMinutos { get; set; }
}

public class DemandaSemanalDto
{
    public string DiaSemana { get; set; } = string.Empty;
    public int Hora { get; set; }
    public int TotalCitas { get; set; }


}
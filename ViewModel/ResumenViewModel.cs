using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using slotsi_citas.Services;
using Slotsi.Shared.Dtos;

namespace slotsi_citas.ViewModel;

public partial class ResumenViewModel : ObservableObject
{
    private readonly ResumenService _service;

   
    private string NegocioId => Preferences.Get("negocio_id", string.Empty);

    [ObservableProperty] private DateTime desde = DateTime.Today;
    [ObservableProperty] private DateTime hasta = DateTime.Today;

    [ObservableProperty] private int citasConfirmadas;
    [ObservableProperty] private int citasPendientes;
    [ObservableProperty] private int ocupacionPorcentaje;
    [ObservableProperty] private int serviciosActivosHoy;

    [ObservableProperty] private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? error;

    public bool HasError => !string.IsNullOrEmpty(Error);

    public ObservableCollection<CitaItemViewModel> Citas { get; } = new();

    public ResumenViewModel(ResumenService service) => _service = service;

    [RelayCommand]
    private async Task CargarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Error = null;

            if (string.IsNullOrEmpty(NegocioId))
            {
                Error = "No se encontró el negocio de tu sesión.";
                return;
            }

            var resumen = await _service.ObtenerResumenAsync(NegocioId, Desde, Hasta);
            if (resumen is null) return;

            CitasConfirmadas = resumen.CitasConfirmadas;
            CitasPendientes = resumen.CitasPendientes;
            OcupacionPorcentaje = resumen.OcupacionPorcentaje;
            ServiciosActivosHoy = resumen.ServiciosActivosHoy;

            Citas.Clear();
            foreach (var cita in resumen.Citas)
                Citas.Add(new CitaItemViewModel(cita));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = $"No se pudo cargar el resumen: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

/// <summary>Prepara cada cita para pintarla en la tarjeta (textos y colores).</summary>
public class CitaItemViewModel
{
    private static readonly Color Verde = Color.FromArgb("#10B981");
    private static readonly Color VerdeClaro = Color.FromArgb("#DCFCE7");
    private static readonly Color VerdeOscuro = Color.FromArgb("#15803D");
    private static readonly Color Naranja = Color.FromArgb("#F59E0B");
    private static readonly Color NaranjaClaro = Color.FromArgb("#FEF3C7");
    private static readonly Color NaranjaOscuro = Color.FromArgb("#B45309");

    public CitaItemViewModel(CitaDto cita)
    {
        Hora = cita.FechaHoraInicio.ToString("h:mm tt", CultureInfo.InvariantCulture);

        Cliente = string.IsNullOrWhiteSpace(cita.ClienteNombre)
            ? "Cliente sin nombre"
            : cita.ClienteNombre;

        Servicio = cita.ServicioNombre;
        EstadoTexto = (cita.Estado ?? string.Empty).ToUpperInvariant();
        EsConfirmada = cita.Estado == "Confirmada";

        ColorPrincipal = EsConfirmada ? Verde : Naranja;
        ColorChipFondo = EsConfirmada ? VerdeClaro : NaranjaClaro;
        ColorChipTexto = EsConfirmada ? VerdeOscuro : NaranjaOscuro;
    }

    public string Hora { get; }
    public string Cliente { get; }
    public string Servicio { get; }
    public string EstadoTexto { get; }
    public bool EsConfirmada { get; }
    public Color ColorPrincipal { get; }
    public Color ColorChipFondo { get; }
    public Color ColorChipTexto { get; }
}

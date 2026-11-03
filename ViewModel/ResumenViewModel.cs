using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace slotsi_citas.ViewModel;

public partial class ResumenViewModel : ObservableObject
{
    private readonly ResumenService _service;
    private readonly NegocioService _negocioService; 
    private bool EsCliente => Preferences.Get("EsCliente", false);
    private string UsuarioId => Preferences.Get("UsuarioId", string.Empty);
    private string NegocioId => Preferences.Get("negocio_id", string.Empty);
    [ObservableProperty] private DateTime desde = DateTime.Today;
    [ObservableProperty] private DateTime hasta = DateTime.Today;
    [ObservableProperty] private string nombreNegocio = string.Empty;
    [ObservableProperty] private string direccionNegocio = string.Empty;
    [ObservableProperty] private string? urlFotoPerfil;
    [ObservableProperty] private string iniciales = string.Empty;
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
    public ResumenViewModel(ResumenService service, NegocioService negocioService)
    {
        _service = service;
        _negocioService = negocioService;
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Error = null;
            Citas.Clear();

            if (EsCliente)
            {           
                await CargarDatosClienteAsync();
                await CargarCitasClienteAsync();
            }
            else
            {
                await CargarDatosNegocioAsync();

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

                foreach (var cita in resumen.Citas)
                {
                    Citas.Add(new CitaItemViewModel(cita));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = $"No se pudo cargar la información: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task CargarDatosClienteAsync()
    {
        string nombreUsuario = Preferences.Get("NombreUsuario", "Cliente");
        NombreNegocio = $"¡Hola, {nombreUsuario}!";
        DireccionNegocio = "Bienvenido a Slotsi Citas";

        Iniciales = string.Concat(
            nombreUsuario.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                         .Take(2)
                         .Select(p => char.ToUpper(p[0])));

        return Task.CompletedTask;
    }

    private async Task CargarCitasClienteAsync()
    {
        try
        {
            CitasConfirmadas = Citas.Count(c => c.EsConfirmada);
            CitasPendientes = Citas.Count(c => !c.EsConfirmada);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Error = "No se pudieron cargar tus citas.";
        }
    }

    private async Task CargarDatosNegocioAsync()
    {
        try
        {
            Negocio? negocio = null;

            if (!string.IsNullOrEmpty(NegocioId))
                negocio = await _negocioService.ObtenerPorIdAsync(NegocioId);

            if (negocio == null && !string.IsNullOrEmpty(UsuarioId))
            {
                negocio = await _negocioService.ObtenerPorUsuarioIdAsync(UsuarioId);

                if (negocio != null && !string.IsNullOrEmpty(negocio.Id))
                {
                    Preferences.Set("negocio_id", negocio.Id);
                    Preferences.Set("NegocioSeleccionadoId", negocio.Id);
                }
            }

            if (negocio == null) return;

            NombreNegocio = negocio.NombreNegocio ?? string.Empty;
            DireccionNegocio = negocio.DireccionNegocio ?? string.Empty;
            UrlFotoPerfil = negocio.UrlFotoPerfil;

            Iniciales = string.Concat(
                NombreNegocio.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Take(2)
                             .Select(p => char.ToUpper(p[0])));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}

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
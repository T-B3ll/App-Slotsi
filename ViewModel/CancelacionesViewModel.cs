using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Slotsi.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Globalization;
using slotsi_citas.Models;
using System.Diagnostics;

namespace slotsi_citas.ViewModel;

public partial class CancelacionesViewModel : ObservableObject
{
    private CancelacionesResporteDto? _ultimoResultado;
    private DateTime _desde;
    private DateTime _hasta;
    [ObservableProperty] private string nombreNegocio = string.Empty;
    [ObservableProperty] private string direccionNegocio = string.Empty;
    private readonly Services.NegocioService _negocios;
    private readonly Services.CitaService _citas;
    private string NegocioId => Preferences.Get("negocio_id", string.Empty);

    public CancelacionesViewModel(Services.CitaService citas, Services.NegocioService negocios)
    {
        _citas = citas;
        _negocios = negocios;
    }
    private async Task CargarDatosNegocioAsync()
    {
        if (!string.IsNullOrEmpty(NombreNegocio)) return;

        try
        {
            var negocio = await _negocios.ObtenerPorIdAsync(NegocioId);
            if (negocio == null) return;

            NombreNegocio = negocio.NombreNegocio ?? string.Empty;
            DireccionNegocio = negocio.DireccionNegocio ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public CancelacionesReporteData? ConstruirDatosReporte()
    {
        if (_ultimoResultado is null) return null;
        var r = _ultimoResultado;

        return new CancelacionesReporteData
        {
            NombreNegocio = NombreNegocio,
            DireccionNegocio = DireccionNegocio,
            Periodo = IndicePeriodo >= 0 && IndicePeriodo < Opciones.Count ? Opciones[IndicePeriodo] : Opciones[2],
            Desde = _desde,
            Hasta = _hasta,
            Total = r.Total,
            Canceladas = r.Canceladas,
            NoAsistio = r.NoAsistio,
            PorcentajePerdida = r.PorcentajePerdida,
            PorcentajeCancelacion = r.PorcentajeCancelacion,
            PorcentajeNoAsistio = r.PorcentajeNoAsistio,
            Citas = Items.Select(i => new CancelacionFila
            {
                Fecha = i.Fecha,
                Hora = i.Hora,
                Cliente = i.Cliente,
                Servicio = i.Servicio,
                Estado = i.EstadoTexto
            }).ToList()
        };
    }
    public List<string> Opciones { get; } = new()
    {
        "Últimas 24 horas",
        "Últimos 7 días",
        "Últimos 30 días"
    };

    [ObservableProperty] private int indicePeriodo = 2;
    [ObservableProperty] private bool cargando;
    [ObservableProperty] private string etiquetaPerdida = "Pérdida · últimos 30 días";
    [ObservableProperty] private string porcentajePerdidaTexto = "0%";
    [ObservableProperty] private string detalleTexto = "0 citas canceladas / 0 total";
    [ObservableProperty] private string noAsistioTexto = "0 no asistieron";

    public ObservableCollection<CitaCancelacionItem> Items { get; } = new();


    partial void OnIndicePeriodoChanged(int value)
    {
        _ = CargarAsync();
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (string.IsNullOrEmpty(NegocioId))
        {
            await Shell.Current.DisplayAlert("Aviso", "No se encontró el negocio de tu sesión.", "OK");
            return;
        }

        try
        {
            Cargando = true;
            await CargarDatosNegocioAsync();

            var hasta = DateTime.Now;
            var desde = indicePeriodo switch
            {
                0 => hasta.AddHours(-24),
                1 => hasta.AddDays(-7),
                _ => hasta.AddDays(-30)
            };

            var r = await _citas.ObtenerCancelacionesAsync(NegocioId, desde, hasta);

            _ultimoResultado = r;           
            _desde = desde;                                       
            _hasta = hasta;

            EtiquetaPerdida = indicePeriodo switch
            {
                0 => "Pérdida · últimas 24 horas",
                1 => "Pérdida · últimos 7 días",
                _ => "Pérdida · últimos 30 días"
            };
            PorcentajePerdidaTexto = $"{r.PorcentajePerdida.ToString("0.#", CultureInfo.InvariantCulture)}%";
            DetalleTexto = $"{r.Canceladas} citas canceladas / {r.Total} total";
            NoAsistioTexto = $"{r.NoAsistio} no asistieron ({r.PorcentajeNoAsistio.ToString("0.#", CultureInfo.InvariantCulture)}%)";

            Items.Clear();
            foreach (var c in r.Citas)
                Items.Add(new CitaCancelacionItem(c));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            Cargando = false;
        }
    }
}

public class CitaCancelacionItem
{
    private static readonly CultureInfo Es = new("es-NI");

    public string Servicio { get; }
    public string Cliente { get; }
    public string Fecha { get; }
    public string Hora { get; }
    public string EstadoTexto { get; }
    public bool EsNoAsistio { get; }

    public CitaCancelacionItem(CitaDto c)
    {
        var f = c.FechaHoraInicio.ToLocalTime();
        var hoy = DateTime.Today;

        Servicio = c.ServicioNombre;
        Cliente = c.ClienteNombre;
        Hora = f.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        Fecha = f.Date == hoy ? $"Hoy, {f.ToString("dd MMM", Es)}"
              : f.Date == hoy.AddDays(-1) ? $"Ayer, {f.ToString("dd MMM", Es)}"
              : f.ToString("dd MMM yyyy", Es);
        EsNoAsistio = c.Estado?.Replace(" ", "").StartsWith("noasist", StringComparison.OrdinalIgnoreCase) == true;
        EstadoTexto = EsNoAsistio ? "No asistio" : "Cancelada";
    }
}
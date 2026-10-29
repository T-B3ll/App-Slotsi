using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Slotsi.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Globalization;

namespace slotsi_citas.ViewModel;

public partial class CancelacionesViewModel : ObservableObject
{
    private readonly Services.CitaService _citas;
    private string NegocioId => Preferences.Get("negocio_id", string.Empty);

    public CancelacionesViewModel(Services.CitaService citas) => _citas = citas;

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

            var hasta = DateTime.Now;
            var desde = indicePeriodo switch
            {
                0 => hasta.AddHours(-24),
                1 => hasta.AddDays(-7),
                _ => hasta.AddDays(-30)
            };

            var r = await _citas.ObtenerCancelacionesAsync(NegocioId, desde, hasta);

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
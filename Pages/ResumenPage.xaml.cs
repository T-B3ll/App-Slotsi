using CommunityToolkit.Maui.Storage;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using static slotsi_citas.Services.ReporteService;

namespace slotsi_citas.Pages;

public partial class ResumenPage : ContentPage
{
    private readonly NegocioService _negocioService;
    private readonly ResumenViewModel _vm;

    public ResumenPage(ResumenViewModel vm, NegocioService negocioService)
    {
        InitializeComponent();
        _vm = vm;
        _negocioService = negocioService;
        BindingContext = _vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarCommand.ExecuteAsync(null);
    }

    private async void OnGenerarPdfClicked(object sender, EventArgs e)
    {
        await ExportarAsync("PDF", "pdf",
            datos => new ReporteService().GenerarResumenPdf(datos));
    }

    private async void OnGenerarExcelClicked(object sender, EventArgs e)
    {
        await ExportarAsync("Excel", "xlsx",
            datos => new ReporteService().GenerarResumenExcel(datos));
    }

    private async Task ExportarAsync(string tipo, string extension, Func<ResumenReporteData, byte[]> generar)
    {
        try
        {
            if (_vm.Citas.Count == 0)
            {
                await DisplayAlert("Aviso", "No hay citas en este rango para exportar.", "OK");
                return;
            }
            var datos = new ResumenReporteData
            {
                NombreNegocio = _vm.NombreNegocio,
                DireccionNegocio = _vm.DireccionNegocio,
                Desde = _vm.Desde,
                Hasta = _vm.Hasta,
                CitasConfirmadas = _vm.CitasConfirmadas,
                CitasPendientes = _vm.CitasPendientes,
                OcupacionPorcentaje = _vm.OcupacionPorcentaje,
                ServiciosActivosHoy = _vm.ServiciosActivosHoy,
                Citas = _vm.Citas.Select(c => new ResumenCitaFila
                {
                    Hora = c.Hora,
                    Cliente = c.Cliente,
                    Servicio = c.Servicio,
                    Estado = c.EstadoTexto
                }).ToList()
            };

            byte[] bytes;
            MostrarCarga(true, $"Generando {tipo}...");
            try
            {
                bytes = await Task.Run(() => generar(datos));
            }
            finally
            {
                MostrarCarga(false);
            }

            string fileName = $"Reporte_Resumen_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

            using var stream = new MemoryStream(bytes);
            var resultado = await FileSaver.Default.SaveAsync(fileName, stream);

            if (resultado.IsSuccessful)
                await DisplayAlert("Éxito", $"Reporte guardado en: {resultado.FilePath}", "OK");
            else
                await DisplayAlert("Aviso", "Se canceló el guardado.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el {tipo}: {ex.Message}", "OK");
        }
    }
    private void MostrarCarga(bool visible, string mensaje = "Generando reporte...")
    {
        Lbl.Text = mensaje;
        OverlayGenerando.IsVisible = visible;
        BtnPdf.IsEnabled = !visible;
        BtnExcel.IsEnabled = !visible;
    }

    private async void OnFechaSeleccionada(object? sender, DateChangedEventArgs e)
    {
        if (_vm.Hasta < _vm.Desde)
        {
            _vm.Hasta = _vm.Desde;
            return;
        }

        await _vm.CargarCommand.ExecuteAsync(null);

    }
}
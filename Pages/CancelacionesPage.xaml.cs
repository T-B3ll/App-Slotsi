using CommunityToolkit.Maui.Storage;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CancelacionesPage : ContentPage
{
    private readonly CancelacionesViewModel _viewModel;
    public CancelacionesPage(CancelacionesViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;   
        BindingContext = _viewModel;
    }
    private async void OnVolverClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarCommand.ExecuteAsync(null);
    }

    private async void OnGenerarPdfClicked(object sender, EventArgs e)
    {
        await ExportarAsync("PDF", "pdf",
            datos => new ReporteService().GenerarCancelacionesPdf(datos));
    }

    private async void OnGenerarExcelClicked(object sender, EventArgs e)
    {
        await ExportarAsync("Excel", "xlsx",
            datos => new ReporteService().GenerarCancelacionesExcel(datos));
    }

    private async Task ExportarAsync(string tipo, string extension, Func<CancelacionesReporteData, byte[]> generar)
    {
        try
        {
            if (_viewModel.Cargando)
            {
                await DisplayAlert("Aviso", "Espera a que termine de cargar el reporte.", "OK");
                return;
            }

            var datos = _viewModel.ConstruirDatosReporte();
            if (datos is null || datos.Citas.Count == 0)
            {
                await DisplayAlert("Aviso", "No hay cancelaciones en este período para exportar.", "OK");
                return;
            }

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

            string fileName = $"Reporte_Cancelaciones_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

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
        LblGenerando.Text = mensaje;
        OverlayGenerando.IsVisible = visible;
        BtnPdf.IsEnabled = !visible;
        BtnExcel.IsEnabled = !visible;
    }
}

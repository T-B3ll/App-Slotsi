using slotsi_citas.ViewModel;
using slotsi_citas.Models;
using CommunityToolkit.Maui.Storage;
using slotsi_citas.Services;

namespace slotsi_citas.Pages;

public partial class MejoresNegociosPage : ContentPage
{
    private readonly MejoresNegociosViewModel _viewModel;
    public MejoresNegociosPage(MejoresNegociosViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }
    

    private async void OnGenerarPdfClicked(object sender, EventArgs e)
    {
        await ExportarAsync(
            "PDF",
            "pdf",
            (ranking, subtitulo) => new ReporteService().GenerarRankingNegociosPdf(ranking, subtitulo));
    }

    private async void OnGenerarExcelClicked(object sender, EventArgs e)
    {
        await ExportarAsync(
            "Excel",
            "xlsx",
            (ranking, subtitulo) => new ReporteService().GenerarRankingNegociosExcel(ranking, subtitulo));
    }

    private async Task ExportarAsync(
        string tipo,
        string extension,
        Func<List<NegocioRanckingItem>, string, byte[]> generar)
    {
        try
        {
            var ranking = _viewModel.Ranking.ToList();
            if (ranking.Count == 0)
            {
                await DisplayAlert("Aviso", "No hay negocios en el ranking para exportar.", "OK");
                return;
            }

            string subtitulo = _viewModel.TextoResultados;
            byte[] bytes;

            // Mostrar carga solo mientras se genera el archivo
            MostrarCarga(true, $"Generando {tipo}...");
            try
            {
                bytes = await Task.Run(() => generar(ranking, subtitulo));
            }
            finally
            {
                MostrarCarga(false);
            }

            string fileName = $"Reporte_MejoresNegocios_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";

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
    private void OnCategoriaClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        MarcarActivo(btn, BtnTodos, BtnSalones, BtnBarberia);
        _viewModel.CambiarCategoria(btn.Text);
    }

    private void OnTopClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || !int.TryParse(btn.ClassId, out int top)) return;
        MarcarActivo(btn, BtnTop10, BtnTop20, BtnTop50, BtnTop100);
        _viewModel.CambiarTop(top);
    }

    private static void MarcarActivo(Button activo, params Button[] grupo)
    {
        foreach (var b in grupo)
        {
            b.BackgroundColor = Colors.White;
            b.TextColor = Color.FromArgb("#4B5563");
            b.FontAttributes = FontAttributes.None;
        }
        activo.BackgroundColor = Color.FromArgb("#2563EB");
        activo.TextColor = Colors.White;
        activo.FontAttributes = FontAttributes.Bold;
    }

    private async void OnNegocioTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not NegocioRanckingItem item) return;

        string? id = item.Negocio.Id;
        if (string.IsNullOrEmpty(id))
        {
            await DisplayAlert("Aviso", "Este negocio no tiene un ID registrado.", "OK");
            return;
        }

        Preferences.Set("NegocioSeleccionadoId", id);
        await Shell.Current.GoToAsync(nameof(CatalogoCliente));
    }
}
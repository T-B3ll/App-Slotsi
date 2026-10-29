using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using slotsi_citas.Services;
using Slotsi.Shared.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.Pages;

public partial class ReporteDemandaPage : ContentPage
{
    private readonly CitaService _citaService;
    private readonly NegocioService _negocioService;

    public ReporteDemandaPage(CitaService citaService, NegocioService negocioService)
    {
        InitializeComponent();
        _citaService = citaService;
        _negocioService = negocioService;
        _ = CargarReporteAsync();
    }

    private async Task CargarReporteAsync()
    {
        try
        {
            string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);

            if (string.IsNullOrEmpty(negocioId))
            {
                string usuarioId = Preferences.Get("UsuarioId", string.Empty);
                if (!string.IsNullOrEmpty(usuarioId))
                {
                    var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(usuarioId);
                    if (negocio != null && !string.IsNullOrEmpty(negocio.Id))
                    {
                        negocioId = negocio.Id;
                        Preferences.Set("NegocioSeleccionadoId", negocioId);
                        Preferences.Set("negocio_id", negocioId);
                    }
                }
            }

            if (string.IsNullOrEmpty(negocioId))
            {
                await DisplayAlert("Aviso", "No se encontró un negocio asociado a tu cuenta.", "OK");
                return;
            }

            var demanda = await _citaService.ObtenerDemandaSemanalAsync(negocioId, "semana");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MatrizContainer.Children.Clear();

                var headerGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(new GridLength(40)) } };
                for (int i = 0; i < 7; i++)
                {
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                    var lbl = new Label { Text = new[] { "L", "M", "M", "J", "V", "S", "D" }[i], FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkGray, HorizontalOptions = LayoutOptions.Center };
                    Grid.SetColumn(lbl, i + 1);
                    headerGrid.Children.Add(lbl);
                }
                MatrizContainer.Children.Add(headerGrid);

                var horas = new[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

                foreach (var hora in horas)
                {
                    var rowGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(new GridLength(40)) } };
                    var horaLabel = new Label { Text = $"{hora}:00", FontSize = 10, TextColor = Colors.DarkGray, VerticalOptions = LayoutOptions.Center };
                    rowGrid.Children.Add(horaLabel);

                    for (int i = 0; i < 7; i++)
                    {
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                        var diaSemana = (DayOfWeek)((i + 1) % 7);

                        var item = demanda?.FirstOrDefault(d => d.DiaSemana == diaSemana.ToString() && d.Hora == hora);
                        int total = item?.TotalCitas ?? 0;

                        Color color;
                        if (total == 0) color = Color.FromArgb("#F1F5F9");
                        else if (total == 1) color = Color.FromArgb("#DBEAFE");
                        else if (total == 2) color = Color.FromArgb("#93C5FD");
                        else if (total == 3) color = Color.FromArgb("#34D399");
                        else color = Color.FromArgb("#10B981");

                        var box = new BoxView { Color = color, WidthRequest = 22, HeightRequest = 22, CornerRadius = 5, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                        Grid.SetColumn(box, i + 1);
                        rowGrid.Children.Add(box);
                    }
                    MatrizContainer.Children.Add(rowGrid);
                }

                if (demanda != null && demanda.Any())
                {
                    var bloqueMaximo = demanda.OrderByDescending(d => d.TotalCitas).First();

                    string diaEspanol = bloqueMaximo.DiaSemana switch
                    {
                        "Monday" => "Lunes",
                        "Tuesday" => "Martes",
                        "Wednesday" => "Miércoles",
                        "Thursday" => "Jueves",
                        "Friday" => "Viernes",
                        "Saturday" => "Sábado",
                        "Sunday" => "Domingo",
                        _ => bloqueMaximo.DiaSemana
                    };

                    int horaFin = bloqueMaximo.Hora + 1;

                    lblMensajeDemanda.Text = $"La mayor demanda se concentra el {diaEspanol} entre {bloqueMaximo.Hora}:00 y {horaFin}:00.";
                }
                else
                {
                    lblMensajeDemanda.Text = "Aún no hay suficiente demanda registrada para generar una recomendación.";
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el reporte: {ex.Message}", "OK");
        }
    }

    private async void OnGenerarPdfClicked(object sender, EventArgs e)
    {
        try
        {
            string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);
            var demanda = await _citaService.ObtenerDemandaSemanalAsync(negocioId, "semana");

            var pdfService = new ReporteService();
            byte[] pdfBytes = pdfService.GenerarDemandaSemanalPdf(demanda, "semana");

            string fileName = $"Reporte_Demanda_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            using var stream = new MemoryStream(pdfBytes);
            var resultado = await FileSaver.Default.SaveAsync(fileName, stream);

            if (resultado.IsSuccessful)
                await DisplayAlert("Éxito", $"Reporte guardado en: {resultado.FilePath}", "OK");
            else
                await DisplayAlert("Aviso", "Se canceló el guardado.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el PDF: {ex.Message}", "OK");
        }
    }

    private async void OnGenerarExcelClicked(object sender, EventArgs e)
    {
        try
        {
            string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);
            var demanda = await _citaService.ObtenerDemandaSemanalAsync(negocioId, "semana");

            var excelService = new ReporteService();
            byte[] excelBytes = excelService.GenerarDemandaSemanalExcel(demanda, "semana");

            string fileName = $"Reporte_Demanda_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            using var stream = new MemoryStream(excelBytes);
            var resultado = await FileSaver.Default.SaveAsync(fileName, stream);

            if (resultado.IsSuccessful)
                await DisplayAlert("Éxito", $"Reporte guardado en: {resultado.FilePath}", "OK");
            else
                await DisplayAlert("Aviso", "Se canceló el guardado.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el Excel: {ex.Message}", "OK");
        }
    }

    private async void OnVolverClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
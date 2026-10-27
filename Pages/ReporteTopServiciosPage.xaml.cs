using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using slotsi_citas.Services;
using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.Pages;

public partial class ReporteTopServiciosPage : ContentPage
{
    private readonly CitaService _citaService;
    private string _periodoActual = "semana";

    public ReporteTopServiciosPage(CitaService citaService)
    {
        InitializeComponent();
        _citaService = citaService;
        _ = CargarReporteAsync();
    }

    private async Task CargarReporteAsync()
    {
        try
        {
            string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);

            if (string.IsNullOrEmpty(negocioId))
            {
                await DisplayAlert("Aviso", "No se encontró un negocio asociado a tu cuenta.", "OK");
                return;
            }

            var topServicios = await _citaService.ObtenerTopServiciosAsync(negocioId, _periodoActual);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TopServiciosContainer.Children.Clear();
                TablaServiciosContainer.Children.Clear();

                if (topServicios == null || !topServicios.Any())
                {
                    TopServiciosContainer.Children.Add(new Label { Text = "No hay datos disponibles para este negocio.", TextColor = Colors.Gray });
                    return;
                }

                int totalCitas = topServicios.Sum(x => x.TotalCitas);
                int index = 1;
                foreach (var item in topServicios)
                {
                    double porcentaje = totalCitas > 0 ? (double)item.TotalCitas / totalCitas * 100 : 0;

                    var grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition(new GridLength(30)),
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(new GridLength(50))
                        },
                        Margin = new Thickness(0, 5)
                    };

                    var numeroLabel = new Label { Text = index.ToString(), FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkGray, VerticalOptions = LayoutOptions.Center };
                    Grid.SetColumn(numeroLabel, 0);
                    grid.Children.Add(numeroLabel);

                    var stack = new VerticalStackLayout { Spacing = 4 };
                    var nombreLabel = new Label { Text = item.NombreServicio, FontSize = 13, TextColor = Colors.Black, LineBreakMode = LineBreakMode.TailTruncation };
                    var barraBackground = new BoxView { Color = Color.FromArgb("#E0E7FF"), HeightRequest = 8, CornerRadius = 4 };
                    var barraProgreso = new BoxView { Color = Color.FromArgb("#2563EB"), HeightRequest = 8, CornerRadius = 4, HorizontalOptions = LayoutOptions.Start };

                    var progressGrid = new Grid { HeightRequest = 8, VerticalOptions = LayoutOptions.Center };
                    progressGrid.Children.Add(barraBackground);
                    progressGrid.SizeChanged += (s, e) =>
                    {
                        barraProgreso.WidthRequest = progressGrid.Width * (porcentaje / 100.0);
                    };
                    progressGrid.Children.Add(barraProgreso);

                    stack.Children.Add(nombreLabel);
                    stack.Children.Add(progressGrid);
                    Grid.SetColumn(stack, 1);
                    grid.Children.Add(stack);

                    var porcentajeLabel = new Label { Text = $"{porcentaje:0}%", FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkGray, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End };
                    Grid.SetColumn(porcentajeLabel, 2);
                    grid.Children.Add(porcentajeLabel);

                    TopServiciosContainer.Children.Add(grid);
                    index++;
                }

                foreach (var item in topServicios)
                {
                    var rowGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto),
                            new ColumnDefinition(new GridLength(70))
                        },
                        Padding = new Thickness(16, 12),
                        BackgroundColor = Colors.White
                    };

                    rowGrid.Children.Add(new Label { Text = item.NombreServicio, FontSize = 13, TextColor = Colors.Black, LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Center });

                    var citasLabel = new Label { Text = item.TotalCitas.ToString(), FontSize = 13, TextColor = Colors.DarkGray, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 10, 0) };
                    Grid.SetColumn(citasLabel, 1);
                    rowGrid.Children.Add(citasLabel);

                    var duracionLabel = new Label { Text = $"{item.DuracionMinutos} min", FontSize = 13, TextColor = Colors.DarkGray, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center }; Grid.SetColumn(duracionLabel, 2);
                    rowGrid.Children.Add(duracionLabel);

                    var separator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E5E7EB") };
                    var finalStack = new VerticalStackLayout { Spacing = 0 };
                    finalStack.Children.Add(rowGrid);
                    finalStack.Children.Add(separator);

                    TablaServiciosContainer.Children.Add(finalStack);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar el reporte: {ex.Message}", "OK");
        }
    }

    private async void OnVolverClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnSemanaClicked(object sender, EventArgs e)
    {
        _periodoActual = "semana";

        BtnSemana.BackgroundColor = Color.FromArgb("#2563EB");
        BtnSemana.TextColor = Colors.White;
        BtnMes.BackgroundColor = Colors.Transparent;
        BtnMes.TextColor = Color.FromArgb("#4B5563");

        await CargarReporteAsync();
    }

    private async void OnMesClicked(object sender, EventArgs e)
    {
        _periodoActual = "mes";

        BtnMes.BackgroundColor = Color.FromArgb("#2563EB");
        BtnMes.TextColor = Colors.White;
        BtnSemana.BackgroundColor = Colors.Transparent;
        BtnSemana.TextColor = Color.FromArgb("#4B5563");

        await CargarReporteAsync();
    }
}
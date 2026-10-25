using Microsoft.Maui.Controls;

namespace slotsi_citas.Pages;

public partial class ReportesMenuPage : ContentPage
{
    public ReportesMenuPage()
    {
        InitializeComponent();
    }

    private async void OnTopServiciosTapped(object sender, TappedEventArgs e)
    {

        await Shell.Current.GoToAsync(nameof(ReporteTopServiciosPage));
    }

    private async void OnDemandaSemanalTapped(object sender, TappedEventArgs e)
    {

        
    }
}
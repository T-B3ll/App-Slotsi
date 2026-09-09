namespace slotsi_citas.Pages;

public partial class OpcionRegistrosU : ContentPage
{
    public OpcionRegistrosU()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Regresa a la pantalla anterior (Login)
        await Navigation.PopAsync();
    }

    private async void OnBasicUserSelected(object sender, EventArgs e)
    {
        
        await DisplayAlert("Selección", "Ir a registro de Usuario Básico", "OK");
    }

    private async void OnBusinessOwnerSelected(object sender, EventArgs e)
    {
       
        await DisplayAlert("Selección", "Ir a registro de Dueño de Negocio", "OK");
    }
}
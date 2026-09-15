namespace slotsi_citas.Pages;

public partial class OpcionRegistrosU : ContentPage
{
    public OpcionRegistrosU()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
     
        await Navigation.PopAsync();
    }

    private async void OnBasicUserSelected(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistroUsuarioBasico());
    }

    private async void OnBusinessOwnerSelected(object sender, EventArgs e)
    {
       
        await Navigation.PushAsync(new RegistroUsuarioNegocio());
    }
}
namespace slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;

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
        
        var servicio = new UsuarioService();

     
        var viewModel = new UsuarioBasicoViewModel(servicio);

        var pagina = new RegistroUsuarioBasico(viewModel);

     
        await Navigation.PushAsync(pagina);
    }

    private async void OnBusinessOwnerSelected(object sender, EventArgs e)
    {
       
        await Navigation.PushAsync(new RegistroUsuarioNegocio());
    }
}
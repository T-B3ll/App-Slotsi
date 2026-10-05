namespace slotsi_citas.Pages;
using slotsi_citas.ViewModel;

public partial class EditarPerfilPage : ContentPage
{
    public EditarPerfilPage(EditarPerfilViewModel viewModel) 
    {
        InitializeComponent();
        BindingContext = viewModel; 
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
       
        await Navigation.PopAsync();
    }
}
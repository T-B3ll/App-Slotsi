using slotsi_citas.ViewModel;
using slotsi_citas.Pages;

namespace slotsi_citas.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent(); 
        BindingContext = viewModel;
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RecuperarContra());

    }

    private async void OnCreateAccountTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OpcionRegistrosU());
    }
    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        
      
    }

}
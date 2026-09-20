using slotsi_citas.ViewModel; 

namespace slotsi_citas.Pages;

public partial class RegistroUsuarioBasico : ContentPage
{
    private readonly UsuarioBasicoViewModel _viewModel;

    public RegistroUsuarioBasico(UsuarioBasicoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        var btn = (ImageButton)sender;
        btn.Source = PasswordEntry.IsPassword ? "eye_open.png" : "eye_closed.png";
    }
}
namespace slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using slotsi_citas.Services;

public partial class RegistroUsuariosPage : ContentPage
{
    public RegistroUsuariosPage()
    {
        InitializeComponent();

        BindingContext = new UsuarioBasicoViewModel(
          Application.Current.Handler.MauiContext.Services.GetService<UsuarioService>()
          );
    }
}
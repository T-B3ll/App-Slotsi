using slotsi_citas.ViewModel;
using slotsi_citas.Models;

namespace slotsi_citas.Pages;

public partial class Cita_UsuarioPage : ContentPage
{
    private Cita_UsuarioViewModel? _viewModel;

    public Cita_UsuarioPage()
    {
        InitializeComponent();
    }

    public Cita_UsuarioPage(Cita_UsuarioViewModel viewModel) : this()
    {
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel ??= BindingContext as Cita_UsuarioViewModel;
        _viewModel?.CargarCitasUsuario();
    }
}
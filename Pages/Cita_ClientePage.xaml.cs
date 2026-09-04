using slotsi_citas.ViewModels;

namespace slotsi_citas.Pages;

public partial class Cita_ClientePage : ContentPage
{
    private Cita_clienteViewModel? _viewModel;

    public Cita_ClientePage()
    {
        InitializeComponent();
    }

    public Cita_ClientePage(Cita_clienteViewModel viewModel) : this()
    {
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel ??= BindingContext as Cita_clienteViewModel;
        _viewModel?.RefrescarCitasCliente();
    }
}
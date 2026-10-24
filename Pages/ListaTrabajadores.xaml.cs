using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class ListaTrabajadores : ContentPage
{
    private readonly ListaTrabjadoresViewModel _viewModel;

    public ListaTrabajadores(ListaTrabjadoresViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.CargarTrabajadoresAsync();
        }
    }
    private async void OnEstadotrabajadorToggled(object sender, ToggledEventArgs e)
    {
        if (sender is Switch sw && sw.BindingContext is Models.Trabajador trab)
        {
            if (e.Value == trab.Activo) return;

            trab.Activo = e.Value;
            await _viewModel.CambiarEstadoActivoAsync(trab);
        }
    }
}
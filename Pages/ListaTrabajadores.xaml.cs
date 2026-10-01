using slotsi_citas.ViewModel;
using MongoDB.Driver;

namespace slotsi_citas.Pages;

public partial class ListaTrabajadores : ContentPage
{
    private readonly ListaTrabjadoresViewModel _viewModel;

    public ListaTrabajadores()
    {
        InitializeComponent();
        _viewModel = new ListaTrabjadoresViewModel();
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
        if (sender is Switch switchControl && switchControl.BindingContext is Models.Trabajador trab)
        {
            if (BindingContext is ViewModel.ListaTrabjadoresViewModel _viewModel)
            {
                await _viewModel.CambiarEstadoActivoAsync(trab);
            }
        }
    }
}
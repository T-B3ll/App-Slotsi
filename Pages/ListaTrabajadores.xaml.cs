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
}
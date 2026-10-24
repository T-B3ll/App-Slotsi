using slotsi_citas.Services;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CatalogoCliente : ContentPage
{
    private readonly CatalogoServiciosViewModel _vm;
    private bool _isLoading = false;
    public CatalogoCliente(CatalogoService catalogoService, TrabajadorService trabajadorService)
	{
		InitializeComponent();
        _vm = new CatalogoServiciosViewModel(catalogoService,trabajadorService);

        BindingContext = _vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarservicioClienteAsync();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
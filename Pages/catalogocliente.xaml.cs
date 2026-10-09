using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class CatalogoCliente : ContentPage
{
    private readonly CatalogoServiciosViewModel _vm;
    public CatalogoCliente()
	{
		InitializeComponent();
        _vm = new CatalogoServiciosViewModel();
        BindingContext = _vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarservicioClienteAsync();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//dueñosnegocios");
    }
}
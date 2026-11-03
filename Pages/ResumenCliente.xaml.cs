using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class ResumenCliente : ContentPage
{
	private readonly ResumenClienteViewModel _vm;
    public ResumenCliente(ResumenClienteViewModel vm)
	{
		InitializeComponent();
		_vm = vm;
        BindingContext = _vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CargarCommand.ExecuteAsync(null);
    }
}
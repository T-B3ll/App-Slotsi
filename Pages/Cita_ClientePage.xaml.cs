using slotsi_citas.ViewModels;

namespace slotsi_citas.Pages;

public partial class Cita_ClientePage : ContentPage
{
    public Cita_ClientePage()
    {
        InitializeComponent();
        BindingContext = new Cita_clienteViewModel();
    }

    public Cita_ClientePage(Cita_clienteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Obtener la instancia activa del ViewModel
        if (BindingContext is Cita_clienteViewModel vm)
        {
            vm.RefrescarCitasCliente();
        }
        else
        {
            var nuevoVm = new Cita_clienteViewModel();
            BindingContext = nuevoVm;
            nuevoVm.RefrescarCitasCliente();
        }
    }
}
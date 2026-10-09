using slotsi_citas.ViewModels;

namespace slotsi_citas.Pages;

public partial class Cita_ClientePage : ContentPage
{
    public Cita_ClientePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is Cita_clienteViewModel vm)
        {
            vm.RefrescarCitasCliente();
        }
    }
}
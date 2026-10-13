using slotsi_citas.ViewModel;
using slotsi_citas.ViewModels;
using slotsi_citas.ViewModel; // Asegúrate de mantener el namespace que coincida con tu ViewModel

namespace slotsi_citas.Pages;
public partial class Cita_ClientePage : ContentPage
{
    public Cita_ClientePage()
    {
        InitializeComponent();
    }

    // Constructor que recibe la ViewModel mediante Inyección de Dependencias
    public Cita_ClientePage(CitasClienteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Carga/refresca las citas de MongoDB de forma asíncrona al abrir la pantalla
        if (BindingContext is CitasClienteViewModel vm)
        {
            await vm.CargarRangosHorariosAsync();
        }
    }
}
using slotsi_citas.ViewModel; 

namespace slotsi_citas.Pages;

public partial class SeleccionarCitaPage : ContentPage
{
    public SeleccionarCitaPage()
    {
        InitializeComponent();

        BindingContext = new SeleccionarCitaViewModel();
    }
}
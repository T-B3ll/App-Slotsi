using slotsi_citas.ViewModel;
namespace slotsi_citas.Pages;

public partial class DueñosNegociosPage : ContentPage
{
	public DueñosNegociosPage()
	{
		InitializeComponent();
        BindingContext = new DueñosNegociosViewModel();

    }
}
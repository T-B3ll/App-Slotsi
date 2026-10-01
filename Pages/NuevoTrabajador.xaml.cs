using slotsi_citas.ViewModels;

namespace slotsi_citas.Pages;

public partial class NuevoTrabajador : ContentPage
{
	public NuevoTrabajador(NuevoTrabajadorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
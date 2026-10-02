using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages
{

	public partial class catalogoservicios : ContentPage
	{
		private readonly CatalogoServiciosViewModel _vm;
		public catalogoservicios()
		{
			InitializeComponent();
			_vm = new CatalogoServiciosViewModel();
			BindingContext = _vm;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _vm.CargarServiciosAsync();
		}

		private async void OnEstadoServicioToggled(object sender, ToggledEventArgs e)
		{
			if (sender is Switch switchControl && switchControl.BindingContext is Models.Servicios servicio)
			{
				if (BindingContext is ViewModel.CatalogoServiciosViewModel vm)
				{
					await vm.CambiarEstadoActivoAsync(servicio);
				}
			}
		}
	}
}
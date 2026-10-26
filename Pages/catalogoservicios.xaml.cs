using slotsi_citas.Services;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages
{
    public partial class catalogoservicios : ContentPage
    {
        private readonly CatalogoServiciosViewModel _vm;

        public catalogoservicios(CatalogoService catalogoService, TrabajadorService trabajadorService)
        {
            InitializeComponent();
            _vm = new CatalogoServiciosViewModel(catalogoService, trabajadorService);
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // se ejecuta la carga en segundo plano para que no bloquee la interfaz
            if (BindingContext is CatalogoServiciosViewModel vm)
            {
                await Task.Run(async () => await _vm.CargarServiciosAsync());
            }
        }

        private async void OnEstadoServicioToggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch switchControl && switchControl.BindingContext is Models.Servicios servicio)
            {
                if (BindingContext is ViewModel.CatalogoServiciosViewModel vm)
                {
                    // Usamos Task.Run también aquí para no bloquear al cambiar el switch
                    await Task.Run(async () => await vm.CambiarEstadoActivoAsync(servicio));
                }
            }
        }
    }
}
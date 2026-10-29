using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

    public partial class ResumenPage : ContentPage
    {
        private readonly ResumenViewModel _vm;

        public ResumenPage(ResumenViewModel vm)
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

        private async void OnFechaSeleccionada(object? sender, DateChangedEventArgs e)
        {
            if (_vm.Hasta < _vm.Desde)
            {
                _vm.Hasta = _vm.Desde;
                return;
            }

            await _vm.CargarCommand.ExecuteAsync(null);
        }
    }
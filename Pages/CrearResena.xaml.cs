using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages
{
    public partial class CrearResena : ContentPage
    {
        private readonly CrearResenaViewModel _vm;
        public CrearResena(CrearResenaViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm != null)
            {
                await _vm.CargarCitasCompletadasAsync();
            }
        }
    }
}
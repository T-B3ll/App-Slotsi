using System.Collections.Generic;
using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using System.Threading.Tasks;

namespace slotsi_citas.Pages
{
    public partial class CalendarioPage : ContentPage, IQueryAttributable
    {
        private readonly CitasClienteViewModel _viewModel;

        public CalendarioPage(CitasClienteViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel?.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                // se ejecuta la carga en un hilo secundario para no congelar la UI
                await Task.Run(async () => await _viewModel.InicializarAsync());
            }
        }
    }
}
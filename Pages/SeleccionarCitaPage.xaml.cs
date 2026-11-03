using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace slotsi_citas.Pages
{
    public partial class SeleccionarCitaPage : ContentPage
    {
        private readonly SeleccionarCitaViewModel _viewModel;

        public SeleccionarCitaPage(SeleccionarCitaViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                if (CitaSession.HorarioSeleccionado != null)
                {
                    _viewModel.HorarioSeleccionado = CitaSession.HorarioSeleccionado;
                }

                if (CitaSession.FechaSeleccionada != default)
                {
                    _viewModel.FechaSeleccionada = CitaSession.FechaSeleccionada;
                }

                // 👇 Recuperar los servicios si se perdieron al volver del calendario 👇
                if (_viewModel.ServiciosTarjetas.Count == 0 && CitaSession.ServiciosSeleccionados != null && CitaSession.ServiciosSeleccionados.Any())
                {
                    var query = new Dictionary<string, object>
                    {
                        { "ServiciosSeleccionados", CitaSession.ServiciosSeleccionados }
                    };
                    _viewModel.ApplyQueryAttributes(query);
                }

                await _viewModel.CargarDatosSesionClienteAsync();
            }
        }

        private void OnMenuClicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }
}
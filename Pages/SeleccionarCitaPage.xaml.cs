using Microsoft.Maui.Controls;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System;

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

                if (_viewModel.NegocioActualObj == null && CitaSession.NegocioActual != null)
                {
                    _viewModel.NegocioActualObj = CitaSession.NegocioActual;
                    _viewModel.NegocioNombre = !string.IsNullOrEmpty(CitaSession.NegocioActual.NombreNegocio)
                        ? CitaSession.NegocioActual.NombreNegocio
                        : CitaSession.NegocioActual.Nombre;
                    _viewModel.NegocioFoto = CitaSession.NegocioActual.UrlFotoPerfil;
                    if (!string.IsNullOrEmpty(CitaSession.NegocioActual.DireccionNegocio))
                    {
                        _viewModel.DireccionCliente = CitaSession.NegocioActual.DireccionNegocio;
                    }
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
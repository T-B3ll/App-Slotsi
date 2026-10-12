using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using System;

namespace slotsi_citas.Pages
{
    public partial class SeleccionarCitaPage : ContentPage
    {
        private readonly SeleccionarCitaViewModel _viewModel;
        public SeleccionarCitaPage()
        {
            InitializeComponent();
            _viewModel = new SeleccionarCitaViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel != null)
            {
                await _viewModel.CargarDatosSesionClienteAsync();
                await _viewModel.CargarColaboradoresParaServiciosAsync();
            }

        }
    }
}
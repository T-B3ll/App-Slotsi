using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using slotsi_citas.ViewModels;
using System;

namespace slotsi_citas.Pages
{
    public partial class SeleccionarCitaPage : ContentPage
    {
        private SeleccionarCitaViewModel? ViewModel => BindingContext as SeleccionarCitaViewModel;

        public SeleccionarCitaPage()
        {
            InitializeComponent();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SeleccionarCitaViewModel vm)
            {
                await vm.CargarServiciosBDAsync();
            }
        }
    }
}
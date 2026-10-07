using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using System;

namespace slotsi_citas.Pages
{
    public partial class SeleccionarCitaPage : ContentPage
    {
        public SeleccionarCitaPage()
        {
            InitializeComponent();
            BindingContext = new SeleccionarCitaViewModel();
        }

        public SeleccionarCitaPage(SeleccionarCitaViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
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
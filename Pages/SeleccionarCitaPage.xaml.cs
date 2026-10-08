using Microsoft.Maui.Controls;
using slotsi_citas.ViewModels;
using slotsi_citas.Models;
using System;

namespace slotsi_citas.Pages
{
    [QueryProperty(nameof(HorarioParam), "HorarioSeleccionado")]
    [QueryProperty(nameof(FechaParam), "FechaSeleccionada")]
    public partial class SeleccionarCitaPage : ContentPage
    {
        private RangoHorario? _horarioParam;
        private DateTime? _fechaParam;

        public RangoHorario? HorarioParam
        {
            get => _horarioParam;
            set
            {
                _horarioParam = value;
                ActualizarViewModel();
            }
        }

        public DateTime? FechaParam
        {
            get => _fechaParam;
            set
            {
                _fechaParam = value;
                ActualizarViewModel();
            }
        }

        public SeleccionarCitaPage()
        {
            InitializeComponent();

            // ✅ LÍNEA SAGRADA: Sin esto, nada funciona
            BindingContext = new SeleccionarCitaViewModel();

            System.Diagnostics.Debug.WriteLine(">>> PÁGINA INICIALIZADA CORRECTAMENTE <<<");
        }

        // ✅ ESTE ES EL MÉTODO QUE FALTABA PARA LA PRUEBA NUCLEAR
        private async void BotonConfirmar_Clicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(">>> BOTÓN ROJO CLICKED DISPARADO <<<");
            await DisplayAlert("FUNCIONA", "El botón SÍ responde al tacto.\nEl problema era el Command/Binding.", "OK");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SeleccionarCitaViewModel vm)
            {
                if (vm.FechaSeleccionada == default || vm.FechaSeleccionada == DateTime.MinValue)
                    vm.FechaSeleccionada = DateTime.Today;

                await vm.CargarServiciosBDAsync();
            }
        }

        private void ActualizarViewModel()
        {
            if (BindingContext is SeleccionarCitaViewModel vm)
            {
                if (_horarioParam != null)
                {
                    vm.HorarioSeleccionado = _horarioParam;
                    System.Diagnostics.Debug.WriteLine($"[NAV] Horario: {_horarioParam.HoraDisplay}");
                }

                if (_fechaParam.HasValue)
                {
                    vm.FechaSeleccionada = _fechaParam.Value;
                    System.Diagnostics.Debug.WriteLine($"[NAV] Fecha: {_fechaParam.Value}");
                }
            }
        }
    }
}
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace slotsi_citas.ViewModels
{
    public class Cita_clienteViewModel : BindableObject
    {
        private readonly IServiceProvider? _serviceProvider;
        private string _rangoSemanal = "Ago 18 - 24, 2026";

        public string RangoSemanal
        {
            get => _rangoSemanal;
            set { _rangoSemanal = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RangoHorario> RangosHorarios { get; set; }

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand AgendarCitaClienteCommand { get; }

        public Cita_clienteViewModel() : this(null) { }

        public Cita_clienteViewModel(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RangosHorarios = new ObservableCollection<RangoHorario>();

            SemanaAnteriorCommand = new Command(() => { });
            SemanaSiguienteCommand = new Command(() => { });
            AgendarCitaClienteCommand = new Command<RangoHorario>(AgendarCita);

            CargarHorarios();
        }

        public void CargarHorarios()
        {
            RangosHorarios.Clear();

            List<string> horas = new List<string>
            {
                "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM",
                "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM"
            };

            foreach (var hora in horas)
            {
                string claveLocalOcupada = $"Cita_Ocupada_{hora}";
                bool estaGuardadaLocalmente = Preferences.Default.Get(claveLocalOcupada, false);

                if (estaGuardadaLocalmente && !CitaRepository.CitasRegistradas.ContainsKey(hora))
                {
                    string clienteLocal = Preferences.Default.Get($"Cliente_{hora}", "Cliente");
                    CitaRepository.CitasRegistradas[hora] = new Cita_cliente
                    {
                        Hora = hora,
                        Estado = "Ocupado",
                        NombreCliente = clienteLocal
                    };
                }

                if (CitaRepository.CitasRegistradas.TryGetValue(hora, out var citaGuardada))
                {
                    Preferences.Default.Set(claveLocalOcupada, true);
                    Preferences.Default.Set($"Cliente_{hora}", citaGuardada.NombreCliente ?? "Cliente");

                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = citaGuardada
                    });
                }
                else if (hora == "1:00 PM")
                {
                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = new Cita_cliente { Estado = "NoDisponible" }
                    });
                }
                else
                {
                    Preferences.Default.Remove(claveLocalOcupada);
                    Preferences.Default.Remove($"Cliente_{hora}");

                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = null
                    });
                }
            }
        }

        private async void AgendarCita(RangoHorario? rango)
        {
            if (rango == null)
                return;

            bool estaOcupado = rango.Cita != null &&
                              (rango.Cita.Estado == "Ocupado" || rango.Cita.Estado == "NoDisponible");

            if (estaOcupado)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Horario No Disponible",
                        "Este horario ya se encuentra reservado. Elige otro horario libre.",
                        "Aceptar");
                }
                return;
            }

            string direccionGuardada = Preferences.Default.Get("direccion_guardada", "Matagalpa, Nicaragua");

            var parametros = new Dictionary<string, object>
            {
                { "HorarioSeleccionado", rango },
                { "DireccionCliente", direccionGuardada }
            };

            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(nameof(SeleccionarCitaPage), parametros);
                }
                else
                {
                    NavegarFallback(rango, direccionGuardada);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR NAVEGACION]: {ex.Message}");
                NavegarFallback(rango, direccionGuardada);
            }
        }

        private async void NavegarFallback(RangoHorario rango, string direccionGuardada)
        {
            var paginaDestino = _serviceProvider?.GetService<SeleccionarCitaPage>() ?? new SeleccionarCitaPage();

            if (paginaDestino.BindingContext is SeleccionarCitaViewModel vm)
            {
                vm.HorarioSeleccionado = rango;
                vm.DireccionCliente = direccionGuardada;
            }

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page?.Navigation != null)
            {
                await window.Page.Navigation.PushAsync(paginaDestino);
            }
        }

        public void RefrescarCitasCliente()
        {
            CargarHorarios();
        }
    }
}
using App.Models;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

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
                // 1. Si existe en memoria/BD como Ocupado
                if (CitaRepository.CitasRegistradas.TryGetValue(hora, out var citaGuardada))
                {
                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = citaGuardada
                    });
                }
                // 2. Horario de descanso
                else if (hora == "1:00 PM")
                {
                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = new Cita_cliente { Estado = "NoDisponible" }
                    });
                }
                // 3. Horario disponible para agendar
                else
                {
                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = hora,
                        Cita = null // Cita nula indica que el slot está libre
                    });
                }
            }
        }

        private async void AgendarCita(RangoHorario? rango)
        {
            if (rango == null)
                return;

            // Solo bloqueamos SI la cita existe Y su estado es "Ocupado" o "NoDisponible"
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

            // Si está libre, navegamos a SeleccionarCitaPage
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
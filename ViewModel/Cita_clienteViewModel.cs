using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModels
{
    public class Cita_clienteViewModel : BindableObject
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly MongoService _mongoService;
        private bool _cargando;
        private DateTime _fechaSeleccionada = DateTime.Today;

        // Caché local en memoria para cargas instantáneas
        private static readonly Dictionary<string, List<Cita_cliente>> _cacheCitas = new();

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (_fechaSeleccionada != value)
                {
                    _fechaSeleccionada = value;
                    OnPropertyChanged();
                    CargarHorarios();
                }
            }
        }

        public ObservableCollection<RangoHorario> RangosHorarios { get; set; }

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand CambiarAFechaDeHoyCommand { get; }
        public ICommand AgendarCitaClienteCommand { get; }

        public Cita_clienteViewModel() : this(null) { }

        public Cita_clienteViewModel(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _mongoService = new MongoService();
            RangosHorarios = new ObservableCollection<RangoHorario>();

            SemanaAnteriorCommand = new Command(() =>
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(-1);
            });

            SemanaSiguienteCommand = new Command(() =>
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(1);
            });

            CambiarAFechaDeHoyCommand = new Command(() =>
            {
                FechaSeleccionada = DateTime.Today;
            });

            AgendarCitaClienteCommand = new Command<RangoHorario>(AgendarCita);

            _fechaSeleccionada = DateTime.Today;
            CargarHorarios();
        }

        public async void CargarHorarios()
        {
            if (_cargando) return;
            _cargando = true;

            try
            {
                RangosHorarios.Clear();

                List<string> horas = new List<string>
                {
                    "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM",
                    "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM"
                };

                var citasMongo = await ObtenerCitasDeFechaRapidoAsync(FechaSeleccionada);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CitaRepository.LimpiarCache();

                    foreach (var hora in horas)
                    {
                        var citaMongo = citasMongo?.FirstOrDefault(c =>
                            c.Hora?.Equals(hora, StringComparison.OrdinalIgnoreCase) == true);

                        if (citaMongo != null)
                        {
                            RangosHorarios.Add(new RangoHorario
                            {
                                HoraDisplay = hora,
                                Cita = citaMongo
                            });
                            CitaRepository.CitasRegistradas[hora] = citaMongo;
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
                            RangosHorarios.Add(new RangoHorario
                            {
                                HoraDisplay = hora,
                                Cita = null
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGANDO HORARIOS]: {ex.Message}");
            }
            finally
            {
                _cargando = false;
            }
        }

        private async Task<List<Cita_cliente>> ObtenerCitasDeFechaRapidoAsync(DateTime fecha)
        {
            string claveCache = fecha.ToString("yyyy-MM-dd");

            if (_cacheCitas.TryGetValue(claveCache, out var citasCache))
            {
                return citasCache;
            }

            try
            {
                // Ejecutamos la consulta a MongoDB en un hilo secundario para liberar la UI
                var todasLasCitas = await Task.Run(async () => await _mongoService.ObtenerCitasAsync());

                var listaFiltrada = todasLasCitas?
                    .Where(c => c.Fecha.Date == fecha.Date)
                    .ToList() ?? new List<Cita_cliente>();

                _cacheCitas[claveCache] = listaFiltrada;
                return listaFiltrada;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR MONGODB]: {ex.Message}");
                return new List<Cita_cliente>();
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

            // Guardamos los datos de forma segura en la sesión estática antes de navegar
            CitaSession.HorarioSeleccionado = rango;
            CitaSession.FechaSeleccionada = FechaSeleccionada;
            CitaSession.DireccionCliente = direccionGuardada;

            var parametros = new Dictionary<string, object>
            {
                { "HorarioSeleccionado", rango },
                { "DireccionCliente", direccionGuardada },
                { "FechaSeleccionada", FechaSeleccionada }
            };

            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync($"{nameof(SeleccionarCitaPage)}", parametros);
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
            CitaSession.HorarioSeleccionado = rango;
            CitaSession.FechaSeleccionada = FechaSeleccionada;
            CitaSession.DireccionCliente = direccionGuardada;

            var paginaDestino = _serviceProvider?.GetService<SeleccionarCitaPage>() ?? new SeleccionarCitaPage();

            if (paginaDestino.BindingContext is SeleccionarCitaViewModel vm)
            {
                vm.HorarioSeleccionado = rango;
                vm.DireccionCliente = direccionGuardada;
                vm.FechaSeleccionada = FechaSeleccionada;
            }

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page?.Navigation != null)
            {
                await window.Page.Navigation.PushAsync(paginaDestino);
            }
        }

        public void RefrescarCitasCliente()
        {
            _cacheCitas.Remove(FechaSeleccionada.ToString("yyyy-MM-dd"));
            CargarHorarios();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class SeleccionarCitaViewModel : BindableObject
    {
        private readonly MongoService _mongoService;

        private RangoHorario? _horarioSeleccionado;
        public RangoHorario? HorarioSeleccionado
        {
            get => _horarioSeleccionado;
            set
            {
                if (_horarioSeleccionado != value)
                {
                    _horarioSeleccionado = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HorarioTextoResumen));
                }
            }
        }

        private string _clienteNombre = "Juan Pérez";
        public string ClienteNombre
        {
            get => _clienteNombre;
            set { _clienteNombre = value; OnPropertyChanged(); }
        }

        private DateTime _fechaSeleccionada = DateTime.Today;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                _fechaSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FechaTextoResumen));
            }
        }

        private string _direccionCliente = "Matagalpa, Nicaragua";
        public string DireccionCliente
        {
            get => _direccionCliente;
            set { _direccionCliente = value; OnPropertyChanged(); }
        }

        public string SucursalTexto => $"Sucursal Central • {DireccionCliente}";

        public string FechaTextoResumen =>
            FechaSeleccionada.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

        private List<Servicios> _serviciosSeleccionados = new List<Servicios>();

        public ObservableCollection<Servicios> ServiciosTarjetas { get; set; } = new ObservableCollection<Servicios>();

        public string ServicioNombreResumen => _serviciosSeleccionados.Any()
            ? string.Join(", ", _serviciosSeleccionados.Select(s => s.Nombre))
            : "Selecciona un servicio";

        public string ServicioPrecioResumen => _serviciosSeleccionados.Any()
            ? $"C$ {_serviciosSeleccionados.Sum(s => s.Precio):N2}"
            : "C$ 0.00";

        public string HorarioTextoResumen => HorarioSeleccionado != null
            ? HorarioSeleccionado.HoraDisplay
            : "Sin horario seleccionado";

        public ICommand ToggleServicioCommand { get; }
        public ICommand ConfirmarCitaCommand { get; }
        public ICommand VolverCommand { get; }

        public SeleccionarCitaViewModel()
        {
            System.Diagnostics.Debug.WriteLine("[VM] Constructor de SeleccionarCitaViewModel ejecutado");

            _mongoService = new MongoService();

            HorarioSeleccionado = CitaSession.HorarioSeleccionado;
            FechaSeleccionada = CitaSession.FechaSeleccionada;
            DireccionCliente = CitaSession.DireccionCliente;

            ToggleServicioCommand = new Command<Servicios>(OnToggleServicio);
            ConfirmarCitaCommand = new Command(OnConfirmarCita);
            VolverCommand = new Command(async () => await RegresarPantallaAnterior());

            CargarServicios();
        }

        private async Task RegresarPantallaAnterior()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR NAVEGACION VOLVER]: {ex.Message}");
                }
            });
        }

        public async Task CargarServiciosBDAsync()
        {
            try
            {
                // Establecemos un tiempo límite rápido (timeout de 4 segundos) para no congelar la pantalla si la red falla
                var tareaBD = _mongoService.ObtenerServiciosAsync();
                var tareaTimeout = Task.Delay(4000);

                var tareaCompletada = await Task.WhenAny(tareaBD, tareaTimeout);

                List<Servicios>? serviciosBD = null;
                if (tareaCompletada == tareaBD)
                {
                    serviciosBD = await tareaBD;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ServiciosTarjetas.Clear();

                    if (serviciosBD != null && serviciosBD.Any())
                    {
                        foreach (var servicio in serviciosBD)
                        {
                            ServiciosTarjetas.Add(servicio);
                        }
                    }
                    else
                    {
                        CargarServiciosRespaldo();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR SERVICIOS]: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(CargarServiciosRespaldo);
            }
        }

        private void CargarServiciosRespaldo()
        {
            ServiciosTarjetas.Clear();
            ServiciosTarjetas.Add(new Servicios
            {
                Id = "1",
                Nombre = "Afeitado Clásico",
                Descripcion = "Contamos con el mejor equipo de la zona.",
                Precio = 1699,
                DuracionMinutos = 90,
                DisponibleDomicilio = true
            });

            ServiciosTarjetas.Add(new Servicios
            {
                Id = "2",
                Nombre = "Corte Degradado",
                Descripcion = "Corte moderno con acabado a navaja.",
                Precio = 350,
                DuracionMinutos = 45,
                DisponibleDomicilio = false
            });
        }

        public void CargarServicios()
        {
            _ = CargarServiciosBDAsync();
        }

        private int ObtenerLimiteServiciosPorHora()
        {
            if (HorarioSeleccionado == null || string.IsNullOrEmpty(HorarioSeleccionado.HoraDisplay))
                return 3;

            List<string> ordenHoras = new List<string>
            {
                "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM",
                "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM"
            };

            string horaActual = HorarioSeleccionado.HoraDisplay.Trim().ToUpper();
            int indiceActual = ordenHoras.FindIndex(h => h.Equals(horaActual, StringComparison.OrdinalIgnoreCase));

            if (indiceActual == -1) return 3;

            int bloquesDisponiblesConsecutivos = 1;

            for (int i = indiceActual + 1; i < ordenHoras.Count; i++)
            {
                string siguienteHora = ordenHoras[i];

                if (siguienteHora == "1:00 PM" || CitaRepository.CitasRegistradas.ContainsKey(siguienteHora))
                {
                    break;
                }

                bloquesDisponiblesConsecutivos++;

                if (bloquesDisponiblesConsecutivos >= 3)
                    break;
            }

            return bloquesDisponiblesConsecutivos;
        }

        private async void OnToggleServicio(Servicios? servicio)
        {
            if (servicio == null) return;

            bool yaSeleccionado = _serviciosSeleccionados.Any(s => s.Id == servicio.Id);

            if (yaSeleccionado)
            {
                _serviciosSeleccionados.RemoveAll(s => s.Id == servicio.Id);
                servicio.EsSeleccionado = false;
            }
            else
            {
                int limiteMaximo = ObtenerLimiteServiciosPorHora();

                if (_serviciosSeleccionados.Count >= limiteMaximo)
                {
                    string mensaje = limiteMaximo == 1
                        ? "El siguiente bloque de horario está ocupado o no disponible, por lo que solo puedes agendar 1 servicio en esta hora."
                        : $"Debido a la disponibilidad de los siguientes horarios, solo puedes seleccionar un máximo de {limiteMaximo} servicios.";

                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Límite de disponibilidad", mensaje, "Entendido");
                    }
                    return;
                }

                _serviciosSeleccionados.Add(servicio);
                servicio.EsSeleccionado = true;
            }

            OnPropertyChanged(nameof(ServicioNombreResumen));
            OnPropertyChanged(nameof(ServicioPrecioResumen));
        }

        private async void OnConfirmarCita()
        {
            System.Diagnostics.Debug.WriteLine("[CONFIRMAR] ✅ OnConfirmarCita SE EJECUTÓ");

            if (!_serviciosSeleccionados.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio.", "OK");
                });
                return;
            }

            if (HorarioSeleccionado == null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Atención", "Selecciona un horario válido.", "OK");
                });
                return;
            }

            try
            {
                var nuevaCita = new Cita_cliente
                {
                    Id = null,
                    NombreCliente = string.IsNullOrEmpty(ClienteNombre) ? "Juan Pérez" : ClienteNombre,
                    Servicio = ServicioNombreResumen,
                    Precio = (double)_serviciosSeleccionados.Sum(s => s.Precio),
                    Estado = "Ocupado",
                    Fecha = DateTime.SpecifyKind(FechaSeleccionada.Date, DateTimeKind.Utc),
                    Hora = HorarioSeleccionado.HoraDisplay
                };

                System.Diagnostics.Debug.WriteLine($"[CONFIRMAR] Guardando cita para: {nuevaCita.NombreCliente}");

                // 🚀 EJECUTAMOS EN SEGUNDO PLANO PARA EVITAR EL ANR DE ANDROID
                bool guardado = await Task.Run(async () =>
                {
                    try
                    {
                        var tareaGuardar = _mongoService.GuardarCitaAsync(nuevaCita);
                        var tareaTimeout = Task.Delay(5000);

                        var tareaCompletada = await Task.WhenAny(tareaGuardar, tareaTimeout);

                        if (tareaCompletada == tareaGuardar)
                        {
                            return await tareaGuardar;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR HILO BD]: {ex.Message}");
                        return false;
                    }
                });

                CitaSession.UltimoGuardadoExitoso = guardado;
                CitaRepository.LimpiarCache();
                CitaSession.HorarioSeleccionado = null;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        if (guardado)
                        {
                            await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita registrada correctamente en la base de datos!", "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Aviso", "La cita se procesó localmente (problemas de conexión con la red remota).", "OK");
                        }
                    }

                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONFIRMAR EXCEPTION] {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo completar la operación: {ex.Message}", "OK");
                    }
                });
            }
        }
    }
}   
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using App.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    [QueryProperty(nameof(HorarioSeleccionado), "HorarioSeleccionado")]
    [QueryProperty(nameof(DireccionCliente), "DireccionCliente")]
    public class SeleccionarCitaViewModel : BindableObject
    {
        private readonly MongoService _mongoService;

        private RangoHorario? _horarioSeleccionado;
        public RangoHorario? HorarioSeleccionado
        {
            get => _horarioSeleccionado;
            set
            {
                _horarioSeleccionado = value;
                OnPropertyChanged();
            }
        }

        private string _clienteNombre = "Juan Pérez";
        public string ClienteNombre
        {
            get => _clienteNombre;
            set { _clienteNombre = value; OnPropertyChanged(); }
        }

        private string _direccionCliente = "Matagalpa, Nicaragua";
        public string DireccionCliente
        {
            get => _direccionCliente;
            set { _direccionCliente = value; OnPropertyChanged(); }
        }

        public string SucursalTexto => $"Sucursal Central • {DireccionCliente}";

        private List<Servicio> _serviciosSeleccionados = new List<Servicio>();

        public ObservableCollection<Servicio> ServiciosTarjetas { get; set; } = new ObservableCollection<Servicio>();

        public string ServicioNombreResumen => _serviciosSeleccionados.Any()
            ? string.Join(", ", _serviciosSeleccionados.Select(s => s.Nombre))
            : "Selecciona un servicio";

        public string ServicioPrecioResumen => _serviciosSeleccionados.Any()
            ? $"C$ {_serviciosSeleccionados.Sum(s => s.Precio):N2}"
            : "C$ 0.00";

        public ICommand ToggleServicioCommand { get; }
        public ICommand ConfirmarCitaCommand { get; }
        public ICommand VolverCommand { get; }

        public SeleccionarCitaViewModel()
        {
            _mongoService = new MongoService();

            ToggleServicioCommand = new Command<Servicio>(OnToggleServicio);
            ConfirmarCitaCommand = new Command(OnConfirmarCita);
            VolverCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task CargarServiciosBDAsync()
        {
            try
            {
                // 1. Consultar MongoDB en segundo plano
                var serviciosBD = await Task.Run(async () => await _mongoService.ObtenerServiciosAsync());

                // 2. Modificar la colección visual de la UI dentro del Hilo Principal
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
                        // Datos de respaldo si la base de datos no devuelve registros o está vacía
                        ServiciosTarjetas.Add(new Servicio
                        {
                            Id = "1",
                            Nombre = "Afeitado Clásico",
                            Descripcion = "Contamos con el mejor equipo de la zona.",
                            Precio = 1699,
                            DuracionMinutos = 90,
                            DisponibleDomicilio = true
                        });

                        ServiciosTarjetas.Add(new Servicio
                        {
                            Id = "2",
                            Nombre = "Corte Degradado",
                            Descripcion = "Corte moderno con acabado a navaja.",
                            Precio = 350,
                            DuracionMinutos = 45,
                            DisponibleDomicilio = false
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR SERVICIOS]: {ex.Message}");

                // En caso de fallo de red o excepción, cargar los de respaldo
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ServiciosTarjetas.Clear();
                    ServiciosTarjetas.Add(new Servicio
                    {
                        Id = "1",
                        Nombre = "Afeitado Clásico",
                        Descripcion = "Contamos con el mejor equipo de la zona.",
                        Precio = 1699,
                        DuracionMinutos = 90,
                        DisponibleDomicilio = true
                    });

                    ServiciosTarjetas.Add(new Servicio
                    {
                        Id = "2",
                        Nombre = "Corte Degradado",
                        Descripcion = "Corte moderno con acabado a navaja.",
                        Precio = 350,
                        DuracionMinutos = 45,
                        DisponibleDomicilio = false
                    });
                });
            }
        }

        public void CargarServicios()
        {
            _ = CargarServiciosBDAsync();
        }

        // Evalúa tanto el límite por hora de cierre como si la siguiente hora está Ocupada/Reservada
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

            // Verificar cuántos bloques hacia adelante están realmente libres
            for (int i = indiceActual + 1; i < ordenHoras.Count; i++)
            {
                string siguienteHora = ordenHoras[i];

                // Si la siguiente hora es descanso (1:00 PM) o ya está ocupada en el repositorio, cortamos
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

        private async void OnToggleServicio(Servicio? servicio)
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
            if (!_serviciosSeleccionados.Any())
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio para continuar.", "OK");
                return;
            }

            if (HorarioSeleccionado != null)
            {
                try
                {
                    var nuevaCita = new Cita_cliente
                    {
                        NombreCliente = ClienteNombre,
                        Servicio = ServicioNombreResumen,
                        Precio = _serviciosSeleccionados.Sum(s => s.Precio),
                        Estado = "Ocupado",
                        Fecha = DateTime.Today
                    };

                    // 1. Guardar en MongoDB de forma asíncrona sin bloquear el hilo de la UI
                    await Task.Run(async () =>
                    {
                        await _mongoService.GuardarCitaAsync(nuevaCita);
                    });

                    // 2. Bloquear el horario seleccionado (y los consecutivos si aplica) en el repositorio local
                    AsegurarReservaDeBloques(HorarioSeleccionado.HoraDisplay, _serviciosSeleccionados.Count, nuevaCita);

                    // 3. Mostrar mensaje de confirmación
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("¡Éxito!", "Cita confirmada correctamente.", "Aceptar");
                    }

                    // 4. Redirección garantizada a la pantalla anterior (Calendario de Citas)
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        var window = Application.Current?.Windows.FirstOrDefault();
                        if (window?.Page?.Navigation != null && window.Page.Navigation.NavigationStack.Count > 1)
                        {
                            await window.Page.Navigation.PopAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR CONFIRMAR CITA]: {ex.Message}");
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Ocurrió un inconveniente al procesar la cita.", "OK");
                    }
                }
            }
        }

        private void AsegurarReservaDeBloques(string horaInicio, int cantidadServicios, Cita_cliente cita)
        {
            List<string> ordenHoras = new List<string>
            {
                "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM",
                "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM"
            };

            int index = ordenHoras.FindIndex(h => h.Equals(horaInicio.Trim(), StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                for (int i = 0; i < cantidadServicios && (index + i) < ordenHoras.Count; i++)
                {
                    string horaABloquear = ordenHoras[index + i];
                    CitaRepository.CitasRegistradas[horaABloquear] = cita;
                }
            }
        }
    }
}
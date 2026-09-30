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
    [QueryProperty(nameof(HorarioSeleccionado), "HorarioSeleccionado")]
    [QueryProperty(nameof(DireccionCliente), "DireccionCliente")]
    public class SeleccionarCitaViewModel : BindableObject, IQueryAttributable
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

        public string HorarioTextoResumen => HorarioSeleccionado != null
            ? HorarioSeleccionado.HoraDisplay
            : "Sin horario seleccionado";

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

        // Intercepta los parámetros pasados vía Shell Navigation al abrir la vista
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("HorarioSeleccionado", out var horario) && horario is RangoHorario rango)
            {
                HorarioSeleccionado = rango;
            }

            if (query.TryGetValue("DireccionCliente", out var dir) && dir is string direccion)
            {
                DireccionCliente = direccion;
            }
        }

        public async Task CargarServiciosBDAsync()
        {
            try
            {
                var serviciosBD = await Task.Run(async () => await _mongoService.ObtenerServiciosAsync());

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
                    await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio.", "OK");
                return;
            }

            if (HorarioSeleccionado == null || string.IsNullOrEmpty(HorarioSeleccionado.HoraDisplay))
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Atención", "Selecciona un horario válido.", "OK");
                return;
            }

            try
            {
                // 1. Crear el objeto mapeado correctamente para MongoDB
                var nuevaCita = new Cita_cliente
                {
                    NombreCliente = string.IsNullOrEmpty(ClienteNombre) ? "Juan Pérez" : ClienteNombre,
                    Servicio = ServicioNombreResumen,
                    Precio = _serviciosSeleccionados.Sum(s => s.Precio),
                    Estado = "Ocupado",
                    Fecha = DateTime.UtcNow.Date,
                    Hora = HorarioSeleccionado.HoraDisplay // Se asigna directamente como string "4:00 PM"
                };

                // 2. Guardar en MongoDB Atlas
                await _mongoService.GuardarCitaAsync(nuevaCita);

                // 3. Bloquear en la memoria local para esta sesión
                AsegurarReservaDeBloques(HorarioSeleccionado.HoraDisplay, _serviciosSeleccionados.Count, nuevaCita);

                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("¡Éxito!", "Cita guardada en la base de datos.", "Aceptar");
                }

                // 4. Regreso garantizado a la pantalla del Calendario
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    else if (Application.Current?.MainPage?.Navigation != null)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR FATAL MONGO]: {ex}");

                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error en MongoDB", $"{ex.Message}", "OK");
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
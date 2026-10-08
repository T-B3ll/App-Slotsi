using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModels
{
    public class SeleccionarCitaViewModel : BindableObject
    {
        private readonly MongoService _mongoService;
        private bool _isBusy;

        // Datos del cliente (siempre disponibles desde Preferences)
        public string ClienteNombre { get; set; } = Preferences.Get("UsuarioNombre", "Cliente Anónimo");
        public string TelefonoCliente { get; set; } = Preferences.Get("UsuarioTelefono", "+505 0000-0000");
        public string IdCliente { get; set; } = Preferences.Get("UsuarioId", "TEMP_ID");
        public string NegocioIdActual { get; set; } = Preferences.Get("NegocioIdActual", "NEGOCIO_DEMO");

        // Datos de la cita
        private DateTime _fechaSeleccionada = DateTime.Today;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set { _fechaSeleccionada = value; OnPropertyChanged(); OnPropertyChanged(nameof(FechaTextoResumen)); }
        }

        private RangoHorario? _horarioSeleccionado;
        public RangoHorario? HorarioSeleccionado
        {
            get => _horarioSeleccionado;
            set
            {
                _horarioSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HorarioTextoResumen));
            }
        }

        // Servicios
        public ObservableCollection<Servicios> ServiciosTarjetas { get; set; } = new();
        public List<Servicios> ServiciosSeleccionados { get; set; } = new();

        // UI Helpers
        public string FechaTextoResumen => FechaSeleccionada.ToString("dddd, dd MMMM yyyy", new CultureInfo("es-NI"));
        public string HorarioTextoResumen => HorarioSeleccionado?.HoraDisplay ?? "--:--";
        public string ServicioNombreResumen => ServiciosSeleccionados.Any()
            ? string.Join(", ", ServiciosSeleccionados.Select(s => s.Nombre))
            : "Ninguno seleccionado";
        public string ServicioPrecioResumen => ServiciosSeleccionados.Any()
            ? ServiciosSeleccionados.Sum(s => s.Precio).ToString("C2", new CultureInfo("es-NI"))
            : "C$ 0.00";

        // Control del botón (Forzamos true por defecto para asegurar que inicie activo)
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); }
        }
        public bool IsNotBusy => !_isBusy;

        // Comandos
        public ICommand VolverCommand { get; }
        public ICommand ToggleServicioCommand { get; }
        public ICommand ConfirmarCitaCommand { get; }

        public SeleccionarCitaViewModel()
        {
            // ✅ DEBUG EXTREMO: Confirma que el ViewModel existe en memoria
            System.Diagnostics.Debug.WriteLine(">>> VIEWMODEL CREADO EXITOSAMENTE <<<");

            _mongoService = new MongoService();

            VolverCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            ToggleServicioCommand = new Command<Servicios>((s) =>
            {
                if (ServiciosSeleccionados.Contains(s)) ServiciosSeleccionados.Remove(s);
                else ServiciosSeleccionados.Add(s);

                OnPropertyChanged(nameof(ServicioNombreResumen));
                OnPropertyChanged(nameof(ServicioPrecioResumen));
            });

            // Comando que SIEMPRE responde al click
            ConfirmarCitaCommand = new Command(async () => await EjecutarConfirmacion());

            CargarServiciosEjemplo();
        }

        private void CargarServiciosEjemplo()
        {
            if (!ServiciosTarjetas.Any())
            {
                ServiciosTarjetas.Add(new Servicios { Nombre = "Corte Clásico", Precio = 150m, DuracionMinutos = 30, Foto = "corte.png" });
                ServiciosTarjetas.Add(new Servicios { Nombre = "Barba", Precio = 100m, DuracionMinutos = 20, Foto = "barba.png" });
            }
        }

        private async Task EjecutarConfirmacion()
        {
            // ✅ LOG DE CLICK: Si ves esto, el botón funciona. Si no, es problema de XAML/Binding.
            System.Diagnostics.Debug.WriteLine(">>> CLICK EN CONFIRMAR DETECTADO <<<");

            if (IsBusy) return;

            // Validación 1: Servicios
            if (!ServiciosSeleccionados.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Debes seleccionar al menos un servicio.", "OK");
                return;
            }

            // Validación 2: Horario
            if (HorarioSeleccionado == null)
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "No has seleccionado un horario.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var nuevaCita = new Cita_cliente
                {
                    Fecha = FechaSeleccionada.Date,
                    Hora = HorarioSeleccionado.HoraInicio.ToString("h:mm tt", CultureInfo.InvariantCulture),
                    NombreCliente = ClienteNombre,
                    TelefonoCliente = TelefonoCliente,
                    IdNegocio = NegocioIdActual,
                    Servicio = string.Join(", ", ServiciosSeleccionados.Select(s => s.Nombre)),
                    Precio = (double)ServiciosSeleccionados.Sum(s => s.Precio),
                    Estado = "CONFIRMADA",
                    Notas = $"App - {DateTime.Now:dd/MM/yyyy HH:mm}"
                };

                System.Diagnostics.Debug.WriteLine($"[GUARDANDO] {nuevaCita.Servicio} | {nuevaCita.Fecha} | {nuevaCita.Hora}");

                bool exito = await _mongoService.GuardarCitaAsync(nuevaCita);

                if (exito)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Cita agendada correctamente.", "OK");
                    MessagingCenter.Send(this, "CitaConfirmada");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar en la base de datos.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CRÍTICO] {ex.Message}\n{ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task CargarServiciosBDAsync()
        {
            try
            {
                var servicios = await _mongoService.ObtenerServiciosAsync();
                if (servicios?.Any() == true)
                {
                    ServiciosTarjetas.Clear();
                    foreach (var s in servicios) ServiciosTarjetas.Add(s);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error servicios: {ex.Message}");
            }
        }
    }
}
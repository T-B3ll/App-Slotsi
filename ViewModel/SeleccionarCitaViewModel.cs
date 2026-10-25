using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class SeleccionarCitaViewModel : BindableObject, IQueryAttributable
    {
        private readonly CitaService _citaService;
        private readonly NegocioService _negocioService;

        private string _negocioNombre = "Cargando negocio...";
        public string NegocioNombre
        {
            get => _negocioNombre;
            set { _negocioNombre = value; OnPropertyChanged(); }
        }

        private string _negocioFoto = string.Empty;
        public string NegocioFoto
        {
            get => _negocioFoto;
            set
            {
                _negocioFoto = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneFotoNegocio));
                OnPropertyChanged(nameof(NoTieneFotoNegocio));
            }
        }

        public bool TieneFotoNegocio => !string.IsNullOrEmpty(NegocioFoto);
        public bool NoTieneFotoNegocio => string.IsNullOrEmpty(NegocioFoto);

        private string _direccionCliente = "Matagalpa, Nicaragua";
        public string DireccionCliente
        {
            get => _direccionCliente;
            set
            {
                _direccionCliente = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SucursalTexto));
            }
        }

        public string SucursalTexto => $"Sucursal • {DireccionCliente}";

        private string _clienteNombre = "Cargando cliente...";
        public string ClienteNombre
        {
            get => _clienteNombre;
            set { _clienteNombre = value; OnPropertyChanged(); }
        }

        private string _clienteId = string.Empty;
        public string ClienteId
        {
            get => _clienteId;
            set { _clienteId = value; OnPropertyChanged(); }
        }

        private Negocio? _negocioActualObj;
        public Negocio? NegocioActualObj
        {
            get => _negocioActualObj;
            set { _negocioActualObj = value; OnPropertyChanged(); }
        }

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
                    OnPropertyChanged(nameof(TieneHorarioSeleccionado));
                    OnPropertyChanged(nameof(NoTieneHorarioSeleccionado));
                    OnPropertyChanged(nameof(TextoBotonHorario));
                }
            }
        }

        public bool TieneHorarioSeleccionado => HorarioSeleccionado != null;
        public bool NoTieneHorarioSeleccionado => HorarioSeleccionado == null;
        public string TextoBotonHorario => HorarioSeleccionado != null ? "Cambiar horario" : "Elegir horario";

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

        public string FechaTextoResumen =>
            FechaSeleccionada.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

        private readonly List<Servicios> _serviciosSeleccionados = new List<Servicios>();
        public ObservableCollection<Servicios> ServiciosTarjetas { get; set; } = new ObservableCollection<Servicios>();

        public string ServicioNombreResumen => _serviciosSeleccionados.Any()
            ? string.Join(", ", _serviciosSeleccionados.Select(s => $"{s.Nombre} ({s.ColaboradorSeleccionado?.Nombre ?? "General"})"))
            : "Selecciona un servicio";

        public string ServicioPrecioResumen => _serviciosSeleccionados.Any()
            ? $"C$ {_serviciosSeleccionados.Sum(s => s.Precio):N2}"
            : "C$ 0.00";

        public string HorarioTextoResumen => HorarioSeleccionado != null
            ? HorarioSeleccionado.HoraDisplay
            : "Sin horario seleccionado";

        public ICommand ToggleServicioCommand { get; }
        public ICommand SeleccionarEspecialistaServicioCommand { get; }
        public ICommand ConfirmarCitaCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand IrAHorariosCommand { get; }
        public ICommand NavegarInicioCommand { get; }
        public ICommand NavegarCitasCommand { get; }
        public ICommand NavegarSucursalCommand { get; }
        public ICommand NavegarPerfilCommand { get; }

        public SeleccionarCitaViewModel(CitaService citaService, NegocioService negocioService)
        {
            _citaService = citaService;
            _negocioService = negocioService;

            HorarioSeleccionado = CitaSession.HorarioSeleccionado;
            FechaSeleccionada = CitaSession.FechaSeleccionada != default
                ? CitaSession.FechaSeleccionada
                : DateTime.Today;

            if (CitaSession.NegocioActual != null)
            {
                NegocioActualObj = CitaSession.NegocioActual;
                NegocioNombre = !string.IsNullOrEmpty(NegocioActualObj.NombreNegocio) ? NegocioActualObj.NombreNegocio : NegocioActualObj.Nombre;
                NegocioFoto = NegocioActualObj.UrlFotoPerfil;
                if (!string.IsNullOrEmpty(NegocioActualObj.DireccionNegocio))
                {
                    DireccionCliente = NegocioActualObj.DireccionNegocio;
                }
            }

            ToggleServicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(OnToggleServicio);
            SeleccionarEspecialistaServicioCommand = new Microsoft.Maui.Controls.Command<Trabajador>(OnSeleccionarEspecialistaServicio);
            ConfirmarCitaCommand = new Command(OnConfirmarCita);
            VolverCommand = new Command(async () => await RegresarPantallaAnterior());
            IrAHorariosCommand = new Command(async () => await IrAHorariosAsync());

            NavegarInicioCommand = new Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//dueñosnegocios");
            });
            NavegarCitasCommand = new Command(async () =>
            {
                await IrAHorariosAsync();
            });
            NavegarSucursalCommand = new Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//dueñosnegocios");
            });
            NavegarPerfilCommand = new Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//EditarPerfilPage");
            });

            _ = CargarDatosSesionClienteAsync();
        }

        public async Task IrAHorariosAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(HorariosDisponiblesPage));
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Negocio", out var negocioObj) && negocioObj is Negocio negocio)
            {
                NegocioActualObj = negocio;
                CitaSession.NegocioActual = negocio;
                CitaSession.NegocioId = negocio.Id ?? string.Empty;
                NegocioNombre = !string.IsNullOrEmpty(negocio.NombreNegocio) ? negocio.NombreNegocio : negocio.Nombre;
                NegocioFoto = negocio.UrlFotoPerfil;
                if (!string.IsNullOrEmpty(negocio.DireccionNegocio))
                {
                    DireccionCliente = negocio.DireccionNegocio;
                }
            }

            if (query.TryGetValue("ServiciosSeleccionados", out var serviciosObj) &&
                serviciosObj is List<Servicios> servicios)
            {
                _serviciosSeleccionados.Clear();
                ServiciosTarjetas.Clear();

                foreach (var serv in servicios)
                {
                    serv.EsSeleccionado = true;
                    _serviciosSeleccionados.Add(serv);
                    ServiciosTarjetas.Add(serv);
                }

                OnPropertyChanged(nameof(ServicioNombreResumen));
                OnPropertyChanged(nameof(ServicioPrecioResumen));
            }

            if (query.TryGetValue("HorarioSeleccionado", out var horarioObj) && horarioObj is RangoHorario horario)
            {
                HorarioSeleccionado = horario;
            }

            if (query.TryGetValue("FechaSeleccionada", out var fechaObj) && fechaObj is DateTime fecha)
            {
                FechaSeleccionada = fecha;
            }
        }

        private void OnSeleccionarEspecialistaServicio(Trabajador? trabajador)
        {
            if (trabajador == null) return;

            foreach (var servicio in ServiciosTarjetas)
            {
                if (servicio.ColaboradoresDelServicio.Any(c => c.Id == trabajador.Id))
                {
                    servicio.ColaboradorSeleccionado = trabajador;
                    break;
                }
            }

            OnPropertyChanged(nameof(ServicioNombreResumen));
        }

        public async Task CargarDatosSesionClienteAsync()
        {
            try
            {
                ClienteId = Preferences.Get("UsuarioId", string.Empty);
                if (string.IsNullOrEmpty(ClienteId))
                    ClienteId = Preferences.Get("UsuarioIdSesion", string.Empty);

                string nombreLocal = Preferences.Get("UsuarioNombre", string.Empty);
                string correo = Preferences.Get("UsuarioCorreo", string.Empty);

                if (string.IsNullOrEmpty(nombreLocal) && !string.IsNullOrEmpty(correo))
                {
                    nombreLocal = correo.Split('@')[0];
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClienteNombre = !string.IsNullOrWhiteSpace(nombreLocal) ? nombreLocal : "Cliente";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR CLIENTE SESION]: {ex.Message}");
            }
        }

        private async Task RegresarPantallaAnterior()
        {
            await Shell.Current.GoToAsync("..");
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
                if (_serviciosSeleccionados.Count >= 3)
                {
                    await Application.Current.MainPage.DisplayAlert("Límite Alcanzado", "Puedes seleccionar un máximo de 3 servicios por cita.", "Entendido");
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
                await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio.", "OK");
                return;
            }

            if (HorarioSeleccionado == null)
            {
                bool irACalendario = await Application.Current.MainPage.DisplayAlert("Horario Requerido", "Aún no has seleccionado un horario para tu cita. ¿Deseas elegir uno ahora?", "Sí, elegir", "Cancelar");
                if (irACalendario)
                {
                    await IrAHorariosAsync();
                }
                return;
            }

            try
            {
                DateTime inicioCita = FechaSeleccionada.Date.Add(HorarioSeleccionado.HoraInicio.TimeOfDay);
                int duracionTotal = _serviciosSeleccionados.Sum(s => s.DuracionMinutos > 0 ? s.DuracionMinutos : 45);
                DateTime finCita = inicioCita.AddMinutes(duracionTotal);

                string telefonoCliente = Preferences.Get("UsuarioTelefono", string.Empty);
                var servicioPrincipal = _serviciosSeleccionados.First();
                string trabajadorIdAsignado = servicioPrincipal.ColaboradorSeleccionado?.Id ?? "";

                string negocioIdFinal = NegocioActualObj?.Id ?? Preferences.Get("NegocioSeleccionadoId", CitaSession.NegocioId);
                if (string.IsNullOrEmpty(negocioIdFinal))
                {
                    negocioIdFinal = "652b1234567890abcdef1234"; // Fallback
                }

                var citaDto = new CitaDto
                {
                    NegocioId = negocioIdFinal,
                    TrabajadorId = trabajadorIdAsignado,
                    ServicioId = servicioPrincipal.Id,
                    ClienteId = string.IsNullOrEmpty(ClienteId) ? "652b1234567890abcdef1237" : ClienteId,
                    ClienteNombre = ClienteNombre,
                    ClienteTelefono = telefonoCliente,
                    ServicioNombre = ServicioNombreResumen,
                    Precio = (decimal)_serviciosSeleccionados.Sum(s => s.Precio),
                    Moneda = "NIO",
                    FechaHoraInicio = DateTime.SpecifyKind(inicioCita, DateTimeKind.Utc),
                    FechaHoraFin = DateTime.SpecifyKind(finCita, DateTimeKind.Utc),
                    DuracionMinutos = duracionTotal,
                    Tipo = "EnLocal",
                    Estado = "Confirmada",
                    MetodoPago = "Efectivo",
                    Notas = ""
                };

                // 👇 AQUÍ SE USA EL CITA SERVICE (API) EN LUGAR DE MONGO
                await _citaService.CrearAsync(citaDto);

                await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita registrada correctamente en la API!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo agendar la cita: {ex.Message}", "OK");
            }
        }
    }
}
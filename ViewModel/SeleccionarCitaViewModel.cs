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
        private readonly UsuarioService _usuarioService;
        private readonly CitaService _citaService;
        private readonly NegocioService _negocioService;
        private readonly CatalogoService _catalogoService;

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
        public bool EsEdicion { get; set; } = false;
        public string CitaIdEdicion { get; set; } = string.Empty;

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
            ? string.Join(", ", _serviciosSeleccionados.Select(s => s.Nombre))
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
        public ICommand NadaCommand { get; }

<<<<<<< HEAD
        public SeleccionarCitaViewModel(CitaService citaService, NegocioService negocioService, CatalogoService catalogoService)
        {
            _citaService = citaService;
            _negocioService = negocioService;
            _catalogoService = catalogoService;
=======
        public SeleccionarCitaViewModel(CitaService citaService, NegocioService negocioService, UsuarioService usuarioService)
        {
            _citaService = citaService;
            _negocioService = negocioService;
            _usuarioService = usuarioService;
>>>>>>> origin/main

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
            NadaCommand = new Command(() => { });

            _ = CargarDatosSesionClienteAsync();
        }

        public async Task IrAHorariosAsync()
        {
            if (!_serviciosSeleccionados.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio antes de elegir un horario.", "OK");
                return;
            }

            if (Shell.Current != null)
            {
<<<<<<< HEAD
                if (NegocioActualObj != null && !string.IsNullOrEmpty(NegocioActualObj.Id))
                {
                    Preferences.Set("NegocioSeleccionadoId", NegocioActualObj.Id);
                }

                var navParams = new Dictionary<string, object>
                {
                    { "FechaSeleccionada", FechaSeleccionada }
                };

                if (NegocioActualObj != null)
                {
                    navParams["Negocio"] = NegocioActualObj;
                }

                await Shell.Current.GoToAsync(nameof(HorariosDisponiblesPage), navParams);
=======
                var parametros = new Dictionary<string, object>
                {
                    { "ServiciosSeleccionados", _serviciosSeleccionados },
                    { "Negocio", NegocioActualObj ?? new Negocio() }
                };

                await Shell.Current.GoToAsync(nameof(CalendarioPage), parametros);
>>>>>>> origin/main
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
                serviciosObj is List<Servicios> servicios && servicios.Any())
            {
                _serviciosSeleccionados.Clear();
                ServiciosTarjetas.Clear();

                foreach (var serv in servicios)
                {
                    serv.EsSeleccionado = true;
                    _serviciosSeleccionados.Add(serv);
                    ServiciosTarjetas.Add(serv);

                    if (serv.ColaboradoresDelServicio != null)
                    {
                        foreach (var t in serv.ColaboradoresDelServicio)
                        {
                            t.ServicioPadre = serv;
                        }
                    }
                }
                CitaSession.ServiciosSeleccionados = servicios;

                OnPropertyChanged(nameof(ServicioNombreResumen));
                OnPropertyChanged(nameof(ServicioPrecioResumen));
            }
            else if (query.TryGetValue("EsEdicion", out var esEdicionObj) && esEdicionObj is bool esEdicion && esEdicion)
            {
                string nombreServicio = string.Empty;
                if (query.TryGetValue("ServicioNombreEdicion", out var servicioNombreObj) && servicioNombreObj is string sn)
                {
                    nombreServicio = sn;
                }
                _ = CargarServiciosParaEdicionAsync(nombreServicio);
            }

            if (query.TryGetValue("HorarioSeleccionado", out var horarioObj) && horarioObj is RangoHorario horario)
            {
                HorarioSeleccionado = horario;
            }

            if (query.TryGetValue("FechaSeleccionada", out var fechaObj) && fechaObj is DateTime fecha)
            {
                FechaSeleccionada = fecha;
            }

            if (query.TryGetValue("EsEdicion", out var esEdicionObj2) && esEdicionObj2 is bool esEdicion2)
            {
                EsEdicion = esEdicion2;
            }

            if (query.TryGetValue("CitaId", out var citaIdObj) && citaIdObj is string citaId)
            {
                CitaIdEdicion = citaId;
            }
        }

        private async Task CargarServiciosParaEdicionAsync(string nombreServicio)
        {
            try
            {
                if (NegocioActualObj == null) return;

                var resultados = await _catalogoService.ObtenerServiciosPorNegocioAsync(NegocioActualObj.Id);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _serviciosSeleccionados.Clear();
                    ServiciosTarjetas.Clear();

                    // 👇 BUSCAMOS EL SERVICIO POR COINCIDENCIA PARCIAL
                    string nombreBuscado = nombreServicio.Trim().ToLower();
                    var servicio = resultados.FirstOrDefault(s =>
                        s.Nombre.Trim().ToLower() == nombreBuscado)
                        ?? resultados.FirstOrDefault(s =>
                            nombreBuscado.Contains(s.Nombre.Trim().ToLower()) ||
                            s.Nombre.Trim().ToLower().Contains(nombreBuscado));

                    if (servicio != null)
                    {
                        servicio.EsSeleccionado = true;
                        _serviciosSeleccionados.Add(servicio);
                        ServiciosTarjetas.Add(servicio);

                        if (servicio.ColaboradoresDelServicio != null)
                        {
                            foreach (var t in servicio.ColaboradoresDelServicio)
                            {
                                t.ServicioPadre = servicio;
                            }
                        }
                    }

                    OnPropertyChanged(nameof(ServicioNombreResumen));
                    OnPropertyChanged(nameof(ServicioPrecioResumen));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR SERVICIOS EDICION]: {ex.Message}");
            }
        }

        private async void OnSeleccionarEspecialistaServicio(Trabajador? trabajador)
        {
            if (trabajador?.ServicioPadre == null) return;

            var servicioTocado = trabajador.ServicioPadre;
            bool estabaSeleccionado = servicioTocado.ColaboradorSeleccionado?.Id == trabajador.Id;

            if (estabaSeleccionado)
            {
                servicioTocado.ColaboradorSeleccionado = null;
            }
            else
            {
                var serviciosConEspecialista = ServiciosTarjetas
                    .Where(s => s.ColaboradorSeleccionado != null)
                    .ToList();

                if (serviciosConEspecialista.Any() &&
                    serviciosConEspecialista.All(s => s.ColaboradorSeleccionado?.Id != trabajador.Id))
                {
                    await Application.Current.MainPage.DisplayAlert("Especialista único",
                        "Solo puedes seleccionar un especialista por cita. Si quieres cambiar, primero deselecciona el actual.", "OK");
                    return;
                }

                foreach (var s in ServiciosTarjetas)
                {
                    if (s.ColaboradorSeleccionado?.Id == trabajador.Id)
                    {
                        s.ColaboradorSeleccionado = null;
                    }
                }
                servicioTocado.ColaboradorSeleccionado = trabajador;
            }

            bool seleccionadoEnAlgunServicio = ServiciosTarjetas.Any(s =>
                s.ColaboradorSeleccionado?.Id == trabajador.Id);

            foreach (var servicio in ServiciosTarjetas)
            {
                var t = servicio.ColaboradoresDelServicio.FirstOrDefault(c => c.Id == trabajador.Id);
                if (t == null) continue;

                if (servicio.ColaboradorSeleccionado?.Id == trabajador.Id)
                {
                    t.EsSeleccionado = true;
                    t.EstaDeshabilitado = false;
                }
                else
                {
                    t.EsSeleccionado = false;
                    t.EstaDeshabilitado = seleccionadoEnAlgunServicio;
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

                string nombreLocal = Preferences.Get("NombreUsuario", string.Empty);
                if (string.IsNullOrWhiteSpace(nombreLocal))
                    nombreLocal = Preferences.Get("UsuarioNombre", string.Empty); 

                string telefono = Preferences.Get("UsuarioTelefono", string.Empty);
                string correo = Preferences.Get("UsuarioCorreo", string.Empty);

                if ((string.IsNullOrWhiteSpace(nombreLocal) || string.IsNullOrWhiteSpace(telefono))
                    && !string.IsNullOrEmpty(correo))
                {
                    try
                    {
                        var usuario = await _usuarioService.ObtenerPorCorreoAsync(correo);
                        if (usuario != null)
                        {
                            if (string.IsNullOrWhiteSpace(nombreLocal))
                            {
                                nombreLocal = usuario.NombreCompleto ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(nombreLocal))
                                    Preferences.Set("NombreUsuario", nombreLocal);
                            }

                            if (string.IsNullOrWhiteSpace(telefono))
                            {
                                telefono = usuario.Telefono ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(telefono))
                                    Preferences.Set("UsuarioTelefono", telefono);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
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

            var servicioSinTrabajador = _serviciosSeleccionados.FirstOrDefault(s =>
                s.ColaboradoresDelServicio != null &&
                s.ColaboradoresDelServicio.Count > 0 &&
                s.ColaboradorSeleccionado == null);

            if (servicioSinTrabajador != null)
            {
                await Application.Current.MainPage.DisplayAlert("Especialista requerido",
                    $"Por favor, selecciona un especialista para el servicio '{servicioSinTrabajador.Nombre}'.", "OK");
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

                string trabajadorIdAsignado = servicioPrincipal.ColaboradorSeleccionado?.Id;

                if (string.IsNullOrEmpty(ClienteId))
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "No se encontró tu sesión. Vuelve a iniciar sesión.", "OK");
                    return;
                }

                string negocioIdFinal = NegocioActualObj?.Id ?? Preferences.Get("NegocioSeleccionadoId", CitaSession.NegocioId);
                if (string.IsNullOrEmpty(negocioIdFinal))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el negocio seleccionado.", "OK");
                    return;
                }

                var citaDto = new CitaDto
                {
                    Id = EsEdicion ? CitaIdEdicion : null,
                    NegocioId = negocioIdFinal,
                    TrabajadorId = trabajadorIdAsignado,
                    ServicioId = servicioPrincipal.Id,
                    ClienteId = ClienteId,
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

                if (string.IsNullOrEmpty(CitaIdEdicion))
                {
                    EsEdicion = false;
                }

                if (EsEdicion && !string.IsNullOrEmpty(CitaIdEdicion))
                {
                    citaDto.Id = CitaIdEdicion;
                    await _citaService.ActualizarEstadoAsync(citaDto.Id, citaDto);
                    await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita actualizada correctamente!", "OK");
                }
                else
                {
                    citaDto.Id = null;
                    await _citaService.CrearAsync(citaDto);
                    await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita registrada correctamente!", "OK");
                }

<<<<<<< HEAD
=======
                await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita registrada correctamente", "OK");
>>>>>>> origin/main
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la cita: {ex.Message}", "OK");
            }
        }
    }
}
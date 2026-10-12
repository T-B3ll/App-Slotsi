using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class SeleccionarCitaViewModel : BindableObject, IQueryAttributable
    {
        private readonly MongoService _mongoService;

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

        public SeleccionarCitaViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            HorarioSeleccionado = CitaSession.HorarioSeleccionado;
            FechaSeleccionada = CitaSession.FechaSeleccionada;

            ToggleServicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(OnToggleServicio);
            SeleccionarEspecialistaServicioCommand = new Microsoft.Maui.Controls.Command<Trabajador>(OnSeleccionarEspecialistaServicio);
            ConfirmarCitaCommand = new Command(OnConfirmarCita);
            VolverCommand = new Command(async () => await RegresarPantallaAnterior());

            _ = CargarDatosSesionClienteAsync();
        }

        public SeleccionarCitaViewModel() : this(new MongoService())
        {
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Negocio", out var negocioObj) && negocioObj is Negocio negocio)
            {
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

                _ = CargarColaboradoresParaServiciosAsync();
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

        public async Task CargarColaboradoresParaServiciosAsync()
        {
            try
            {
                var clienteMongo = new MongoClient(MongoDbSettings.ConnectionString);
                var bd = clienteMongo.GetDatabase(MongoDbSettings.DatabaseName);

                var coleccionTrab = bd.GetCollection<BsonDocument>("Trabajadores");
                var listaDocs = await coleccionTrab.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();

                var mapaTrabajadores = new Dictionary<string, Trabajador>();
                foreach (var doc in listaDocs)
                {
                    string idStr = doc.Contains("_id") ? doc["_id"].ToString() ?? "" : "";
                    string nombreStr = doc.Contains("nombre") ? doc["nombre"].AsString : (doc.Contains("Nombre") ? doc["Nombre"].AsString : "Especialista");

                    var trab = new Trabajador
                    {
                        Id = idStr,
                        Nombre = nombreStr,
                        Especialidad = doc.Contains("especialidad") ? doc["especialidad"].AsString : "Especialista",
                        Foto = doc.Contains("foto") ? doc["foto"].AsString : ""
                    };

                    if (!string.IsNullOrEmpty(idStr))
                        mapaTrabajadores[idStr] = trab;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var servicio in ServiciosTarjetas)
                    {
                        servicio.ColaboradoresDelServicio.Clear();
                        if (servicio.ColaboradoresHabilitados != null)
                        {
                            foreach (var idObj in servicio.ColaboradoresHabilitados)
                            {
                                string idKey = idObj.ToString();
                                if (mapaTrabajadores.TryGetValue(idKey, out var trab))
                                {
                                    servicio.ColaboradoresDelServicio.Add(trab);
                                }
                            }
                        }

                        if (servicio.ColaboradorSeleccionado == null && servicio.ColaboradoresDelServicio.Any())
                        {
                            servicio.ColaboradorSeleccionado = servicio.ColaboradoresDelServicio.First();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR]: {ex.Message}");
            }
        }

        private void OnSeleccionarEspecialistaServicio(Trabajador? trabajador)
        {
            if (trabajador == null) return;

            // Buscamos a qué servicio pertenece este trabajador y actualizamos su selección
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

                string correo = Preferences.Get("UsuarioCorreo", string.Empty);
                if (string.IsNullOrEmpty(correo))
                    correo = Preferences.Get("Correo", string.Empty);

                string nombreLocal = Preferences.Get("UsuarioNombre", string.Empty);

                bool esNombreInvalido = string.IsNullOrWhiteSpace(nombreLocal) ||
                                        nombreLocal.Equals("LoginPage", StringComparison.OrdinalIgnoreCase) ||
                                        nombreLocal.Equals("Cliente Básico", StringComparison.OrdinalIgnoreCase) ||
                                        nombreLocal.Equals("Cliente", StringComparison.OrdinalIgnoreCase);

                if (esNombreInvalido)
                {
                    var clienteMongo = new MongoClient(MongoDbSettings.ConnectionString);
                    var bd = clienteMongo.GetDatabase(MongoDbSettings.DatabaseName);

                    var coleccionUsuarios = bd.GetCollection<BsonDocument>("usuarios");
                    var filtros = new List<FilterDefinition<BsonDocument>>();

                    if (!string.IsNullOrEmpty(ClienteId))
                    {
                        if (ObjectId.TryParse(ClienteId, out ObjectId objectId))
                            filtros.Add(Builders<BsonDocument>.Filter.Eq("_id", objectId));
                        filtros.Add(Builders<BsonDocument>.Filter.Eq("_id", ClienteId));
                    }

                    if (!string.IsNullOrEmpty(correo))
                    {
                        filtros.Add(Builders<BsonDocument>.Filter.Eq("Correo", correo));
                        filtros.Add(Builders<BsonDocument>.Filter.Eq("correo", correo));
                    }

                    var usuarioDoc = await coleccionUsuarios.Find(Builders<BsonDocument>.Filter.Or(filtros)).FirstOrDefaultAsync();

                    if (usuarioDoc == null)
                    {
                        coleccionUsuarios = bd.GetCollection<BsonDocument>("Usuarios");
                        usuarioDoc = await coleccionUsuarios.Find(Builders<BsonDocument>.Filter.Or(filtros)).FirstOrDefaultAsync();
                    }

                    if (usuarioDoc != null)
                    {
                        if (usuarioDoc.Contains("NombreCompleto") && !usuarioDoc["NombreCompleto"].AsBsonValue.IsBsonNull)
                            nombreLocal = usuarioDoc["NombreCompleto"].AsString.Trim();
                        else if (usuarioDoc.Contains("Nombre") && !usuarioDoc["Nombre"].AsBsonValue.IsBsonNull)
                            nombreLocal = usuarioDoc["Nombre"].AsString.Trim();
                        else if (usuarioDoc.Contains("nombre") && !usuarioDoc["nombre"].AsBsonValue.IsBsonNull)
                            nombreLocal = usuarioDoc["nombre"].AsString.Trim();

                        if (usuarioDoc.Contains("Telefono") && !usuarioDoc["Telefono"].AsBsonValue.IsBsonNull)
                            Preferences.Set("UsuarioTelefono", usuarioDoc["Telefono"].AsString.Trim());

                        if (!string.IsNullOrEmpty(nombreLocal))
                            Preferences.Set("UsuarioNombre", nombreLocal);
                    }
                }

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

            _ = CargarColaboradoresParaServiciosAsync();
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
                await Application.Current.MainPage.DisplayAlert("Atención", "Selecciona un horario válido.", "OK");
                return;
            }

            try
            {
                DateTime inicioCita = FechaSeleccionada.Date.Add(HorarioSeleccionado.HoraInicio.TimeOfDay);
                int duracionTotal = _serviciosSeleccionados.Sum(s => s.DuracionMinutos > 0 ? s.DuracionMinutos : 45);
                DateTime finCita = inicioCita.AddMinutes(duracionTotal);

                string telefonoCliente = Preferences.Get("UsuarioTelefono", string.Empty);
                var servicioPrincipal = _serviciosSeleccionados.First();
                string trabajadorIdAsignado = servicioPrincipal.ColaboradorSeleccionado?.Id ?? "652b1234567890abcdef1235";

                var nuevaCita = new Citas
                {
                    Id = null,
                    NegocioId = Preferences.Get("NegocioSeleccionadoId", "652b1234567890abcdef1234"),
                    TrabajadorId = trabajadorIdAsignado,
                    ServicioId = servicioPrincipal.Id,
                    ClienteId = string.IsNullOrEmpty(ClienteId) ? "652b1234567890abcdef1237" : ClienteId,
                    Cliente = new ClienteSnapshot
                    {
                        Nombre = ClienteNombre,
                        Telefono = telefonoCliente
                    },
                    ServicioNombre = ServicioNombreResumen,
                    Precio = (decimal)_serviciosSeleccionados.Sum(s => s.Precio),
                    Moneda = "NIO",
                    FechaHoraInicio = DateTime.SpecifyKind(inicioCita, DateTimeKind.Utc),
                    FechaHoraFin = DateTime.SpecifyKind(finCita, DateTimeKind.Utc),
                    DuracionMinutos = duracionTotal,
                    Tipo = "EnLocal",
                    Estado = "Confirmada",
                    MetodoPago = "Efectivo",
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                bool guardado = await _mongoService.GuardarCitaAsync(nuevaCita);

                if (guardado)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "¡Cita registrada correctamente en MongoDB Atlas!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "No se pudo sincronizar con la nube. Revisa tu conexión.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo completar la operación: {ex.Message}", "OK");
            }
        }
    }
}
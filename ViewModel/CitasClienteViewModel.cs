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
using slotsi_citas.Pages;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class CitasClienteViewModel : BindableObject, IQueryAttributable
    {
        private readonly MongoService _mongoService;
        private readonly IMongoCollection<Negocio> _coleccionNegocios;

        private Negocio? _negocioActual;
        public Negocio? NegocioActual
        {
            get => _negocioActual;
            set
            {
                _negocioActual = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NegocioNombre));
            }
        }

        public string NegocioNombre => !string.IsNullOrEmpty(NegocioActual?.NombreNegocio)
            ? NegocioActual.NombreNegocio
            : (!string.IsNullOrEmpty(NegocioActual?.Nombre) ? NegocioActual.Nombre : "Negocio");

        private DateTime _fechaSeleccionada;
        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (_fechaSeleccionada.Date != value.Date)
                {
                    _fechaSeleccionada = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TextoFechaSeleccionada));
                    _ = CargarRangosHorariosAsync();
                }
            }
        }

        public string TextoFechaSeleccionada =>
            _fechaSeleccionada.ToString("d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        private bool _estaCerradoElDia;
        public bool EstaCerradoElDia
        {
            get => _estaCerradoElDia;
            set
            {
                _estaCerradoElDia = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstaAbiertoElDia));
            }
        }

        public bool EstaAbiertoElDia => !EstaCerradoElDia;

        private string _mensajeCerrado = string.Empty;
        public string MensajeCerrado
        {
            get => _mensajeCerrado;
            set { _mensajeCerrado = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RangoHorario> RangosHorarios { get; } = new ObservableCollection<RangoHorario>();

        public ICommand HoyCommand { get; }
        public ICommand AgendarHorarioCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand NavegarInicioCommand { get; }
        public ICommand NavegarCitasCommand { get; }
        public ICommand NavegarSucursalCommand { get; }
        public ICommand NavegarPerfilCommand { get; }

        public CitasClienteViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            var clienteMongo = new MongoClient(MongoDbSettings.ConnectionString);
            var database = clienteMongo.GetDatabase(MongoDbSettings.DatabaseName);
            _coleccionNegocios = database.GetCollection<Negocio>("Negocios");

            _fechaSeleccionada = CitaSession.FechaSeleccionada != default
                ? CitaSession.FechaSeleccionada
                : DateTime.Today;

            if (CitaSession.NegocioActual != null)
            {
                NegocioActual = CitaSession.NegocioActual;
            }

            HoyCommand = new Microsoft.Maui.Controls.Command(() => FechaSeleccionada = DateTime.Today);
            AgendarHorarioCommand = new Microsoft.Maui.Controls.Command<RangoHorario>(async (rango) => await OnAgendarHorarioAsync(rango));
            VolverCommand = new Microsoft.Maui.Controls.Command(async () => await RegresarAsync());

            NavegarInicioCommand = new Microsoft.Maui.Controls.Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//dueñosnegocios");
            });
            NavegarCitasCommand = new Microsoft.Maui.Controls.Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//Cita_UsuarioPage");
            });
            NavegarSucursalCommand = new Microsoft.Maui.Controls.Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//dueñosnegocios");
            });
            NavegarPerfilCommand = new Microsoft.Maui.Controls.Command(async () =>
            {
                if (Shell.Current != null) await Shell.Current.GoToAsync("//EditarPerfilPage");
            });

            _ = InicializarAsync();
        }

        public CitasClienteViewModel() : this(new MongoService())
        {
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Negocio", out var negocioObj) && negocioObj is Negocio negocio)
            {
                NegocioActual = negocio;
                CitaSession.NegocioActual = negocio;
            }

            if (query.TryGetValue("ServiciosSeleccionados", out var servObj) && servObj is List<Servicios> servs)
            {
                CitaSession.ServiciosSeleccionados = servs;
            }

            if (query.TryGetValue("FechaSeleccionada", out var fechaObj) && fechaObj is DateTime fecha)
            {
                _fechaSeleccionada = fecha;
                OnPropertyChanged(nameof(FechaSeleccionada));
                OnPropertyChanged(nameof(TextoFechaSeleccionada));
            }

            _ = CargarRangosHorariosAsync();
        }

        public async Task InicializarAsync()
        {
            if (NegocioActual == null)
            {
                await CargarNegocioActualAsync();
            }
            await CargarRangosHorariosAsync();
        }

        private async Task CargarNegocioActualAsync()
        {
            try
            {
                if (CitaSession.NegocioActual != null)
                {
                    NegocioActual = CitaSession.NegocioActual;
                    return;
                }

                string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);
                if (string.IsNullOrEmpty(negocioId))
                {
                    negocioId = CitaSession.NegocioId;
                }

                if (!string.IsNullOrEmpty(negocioId))
                {
                    FilterDefinition<Negocio> filter;
                    if (ObjectId.TryParse(negocioId, out ObjectId oId))
                    {
                        filter = Builders<Negocio>.Filter.Eq("_id", oId);
                    }
                    else
                    {
                        filter = Builders<Negocio>.Filter.Eq(n => n.Id, negocioId);
                    }

                    var negocioDb = await _coleccionNegocios.Find(filter).FirstOrDefaultAsync();
                    if (negocioDb != null)
                    {
                        NegocioActual = negocioDb;
                        CitaSession.NegocioActual = negocioDb;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR NEGOCIO EN CALENDARIO]: {ex.Message}");
            }
        }

        public async Task CargarRangosHorariosAsync()
        {
            try
            {
                EstaCargando = true;
                RangosHorarios.Clear();

                if (NegocioActual == null)
                {
                    await CargarNegocioActualAsync();
                }

                string diaKey = ObtenerClaveDia(FechaSeleccionada.DayOfWeek);
                string nombreDiaLargo = FechaSeleccionada.ToString("dddd", new CultureInfo("es-ES"));

                bool estaCerrado = false;
                Negocio.HorarioDia? horarioConfig = null;

                if (NegocioActual != null && NegocioActual.Horarios != null && NegocioActual.Horarios.Count > 0)
                {
                    // 1. Verificar si el día está en la lista de días cerrados
                    if (NegocioActual.DiasCerrados != null && NegocioActual.DiasCerrados.Any(d => d.Equals(diaKey, StringComparison.OrdinalIgnoreCase)))
                    {
                        estaCerrado = true;
                    }
                    // 2. Verificar si el diccionario contiene la configuración del día
                    else if (NegocioActual.Horarios.TryGetValue(diaKey, out var diaConfig))
                    {
                        horarioConfig = diaConfig;
                    }
                    else
                    {
                        estaCerrado = true;
                    }
                }
                else
                {
                    // Fallback para negocios que no tienen el campo Horarios configurado (ej. Barbería el pelón)
                    if (FechaSeleccionada.DayOfWeek == DayOfWeek.Sunday)
                    {
                        estaCerrado = true;
                    }
                    else
                    {
                        horarioConfig = new Negocio.HorarioDia
                        {
                            Apertura = "08:00",
                            Cierre = "17:00",
                            Pausas = new List<string> { "13:00", "14:00" }
                        };
                    }
                }

                if (estaCerrado || horarioConfig == null)
                {
                    EstaCerradoElDia = true;
                    MensajeCerrado = $"El negocio se encuentra cerrado los días {nombreDiaLargo}.";
                    return;
                }

                EstaCerradoElDia = false;
                MensajeCerrado = string.Empty;

                // Parsear Apertura y Cierre
                TimeSpan horaApertura = ParsearHora(horarioConfig.Apertura, new TimeSpan(8, 0, 0));
                TimeSpan horaCierre = ParsearHora(horarioConfig.Cierre, new TimeSpan(17, 0, 0));

                // Si la hora de cierre es menor o igual a la de apertura y menor a 12 (ej. 06:00 o 05:00 en formato 12h)
                if (horaCierre <= horaApertura && horaCierre.Hours < 12)
                {
                    horaCierre = horaCierre.Add(TimeSpan.FromHours(12));
                }

                // Parsear Pausas (descanso)
                bool tienePausa = false;
                TimeSpan pausaInicio = TimeSpan.Zero;
                TimeSpan pausaFin = TimeSpan.Zero;

                if (horarioConfig.Pausas != null && horarioConfig.Pausas.Count >= 2)
                {
                    pausaInicio = ParsearHora(horarioConfig.Pausas[0], new TimeSpan(13, 0, 0));
                    pausaFin = ParsearHora(horarioConfig.Pausas[1], new TimeSpan(14, 0, 0));

                    if (pausaInicio <= horaApertura && pausaInicio.Hours < 12)
                    {
                        pausaInicio = pausaInicio.Add(TimeSpan.FromHours(12));
                    }
                    if (pausaFin <= pausaInicio && pausaFin.Hours < 12)
                    {
                        pausaFin = pausaFin.Add(TimeSpan.FromHours(12));
                    }

                    tienePausa = true;
                }

                // Cargar citas existentes desde MongoDB para validar horarios ocupados
                var citasDelDia = await _mongoService.ObtenerCitasPorFechaAsync(FechaSeleccionada);
                string negocioIdFiltro = NegocioActual?.Id ?? Preferences.Get("NegocioSeleccionadoId", string.Empty);

                if (!string.IsNullOrEmpty(negocioIdFiltro))
                {
                    citasDelDia = citasDelDia.Where(c => c.NegocioId == negocioIdFiltro && c.Estado != "Cancelada").ToList();
                }

                // Generar los bloques de horario
                for (var hora = horaApertura; hora < horaCierre; hora = hora.Add(TimeSpan.FromHours(1)))
                {
                    var inicioBloque = FechaSeleccionada.Date.Add(hora);
                    var finBloque = inicioBloque.AddHours(1);

                    // Verificar pausa
                    bool esHorarioPausa = tienePausa && (hora >= pausaInicio && hora < pausaFin);

                    // Verificar citas ocupadas
                    var citaOcupada = citasDelDia.FirstOrDefault(c => c.FechaHoraInicio < finBloque && c.FechaHoraFin > inicioBloque);
                    bool esOcupado = citaOcupada != null;

                    bool esNoDisponible = esHorarioPausa;
                    bool esDisponible = !esHorarioPausa && !esOcupado;

                    string detallePausa = esHorarioPausa
                        ? $"({FormatearHora12(pausaInicio)} - {FormatearHora12(pausaFin)})"
                        : (esOcupado ? "(Ocupado)" : "");

                    string estadoTexto = esDisponible
                        ? "Agendar cita"
                        : (esHorarioPausa ? "No disponible" : "Ocupado");

                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = FormatearHora12(hora),
                        HoraInicio = inicioBloque,
                        HoraFin = finBloque,
                        EsDisponible = esDisponible,
                        EsOcupado = esOcupado,
                        EsNoDisponible = esNoDisponible,
                        EstadoTexto = estadoTexto,
                        DetallePausa = detallePausa,
                        Cita = citaOcupada
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR RANGOS HORARIOS]: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task OnAgendarHorarioAsync(RangoHorario? rango)
        {
            if (rango == null || !rango.EsDisponible) return;

            CitaSession.HorarioSeleccionado = rango;
            CitaSession.FechaSeleccionada = FechaSeleccionada;

            if (NegocioActual != null)
            {
                CitaSession.NegocioActual = NegocioActual;
                CitaSession.NegocioId = NegocioActual.Id ?? string.Empty;
            }

            var navParams = new Dictionary<string, object>
            {
                { "HorarioSeleccionado", rango },
                { "FechaSeleccionada", FechaSeleccionada }
            };

            if (NegocioActual != null)
            {
                navParams["Negocio"] = NegocioActual;
            }

            if (CitaSession.ServiciosSeleccionados != null && CitaSession.ServiciosSeleccionados.Any())
            {
                navParams["ServiciosSeleccionados"] = CitaSession.ServiciosSeleccionados;
            }

            try
            {
                var navStack = Shell.Current?.Navigation?.NavigationStack;
                bool estaEnPila = navStack != null && navStack.Any(p => p is SeleccionarCitaPage);

                if (estaEnPila)
                {
                    await Shell.Current!.GoToAsync("..", navParams);
                }
                else
                {
                    await Shell.Current!.GoToAsync("SeleccionarCitaPage", navParams);
                }
            }
            catch
            {
                await Shell.Current!.GoToAsync("SeleccionarCitaPage", navParams);
            }
        }

        private async Task RegresarAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private string ObtenerClaveDia(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mie",
                DayOfWeek.Thursday => "Jue",
                DayOfWeek.Friday => "Vie",
                DayOfWeek.Saturday => "Sab",
                DayOfWeek.Sunday => "Dom",
                _ => "Lun"
            };
        }

        private TimeSpan ParsearHora(string horaTexto, TimeSpan horaPorDefecto)
        {
            if (string.IsNullOrWhiteSpace(horaTexto)) return horaPorDefecto;

            if (TimeSpan.TryParse(horaTexto, out var ts))
            {
                return ts;
            }

            if (DateTime.TryParse(horaTexto, out var dt))
            {
                return dt.TimeOfDay;
            }

            return horaPorDefecto;
        }

        private string FormatearHora12(TimeSpan hora)
        {
            return DateTime.Today.Add(hora).ToString("h:mm tt", CultureInfo.InvariantCulture);
        }
    }
}
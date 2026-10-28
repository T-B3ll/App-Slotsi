using Microsoft.Maui.Storage;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using slotsi_citas.Pages;

namespace slotsi_citas.ViewModel
{
    public class CitasClienteViewModel : BindableObject, IQueryAttributable
    {
        private readonly CitaService _citaService;
        private readonly NegocioService _negocioService;
        private bool _cargandoHorarios = false;

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
        public ICommand EliminarCitaCommand { get; }
        public ICommand EditarCitaCommand { get; }

        public CitasClienteViewModel(CitaService citaService, NegocioService negocioService)
        {
            _citaService = citaService;
            _negocioService = negocioService;

            _fechaSeleccionada = CitaSession.FechaSeleccionada != default
                ? CitaSession.FechaSeleccionada
                : DateTime.Today;

            if (CitaSession.NegocioActual != null)
            {
                NegocioActual = CitaSession.NegocioActual;
            }

            HoyCommand = new Microsoft.Maui.Controls.Command(() => FechaSeleccionada = DateTime.Today);
            AgendarHorarioCommand = new Microsoft.Maui.Controls.Command<RangoHorario>(async (rango) => await OnAgendarHorarioAsync(rango));
            VolverCommand = new Microsoft.Maui.Controls.Command(async () => await Shell.Current.GoToAsync("//dueñosnegocios"));

            EliminarCitaCommand = new Microsoft.Maui.Controls.Command<RangoHorario>(async (rango) => await OnEliminarCitaAsync(rango));
            EditarCitaCommand = new Microsoft.Maui.Controls.Command<RangoHorario>(async (rango) => await OnEditarCitaAsync(rango));

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

            if (RangosHorarios.Count == 0)
            {
                await CargarRangosHorariosAsync();
            }
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
                    var negocioDb = await _negocioService.ObtenerPorIdAsync(negocioId);
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
            if (_cargandoHorarios) return;
            _cargandoHorarios = true;

            try
            {
                EstaCargando = true;
                RangosHorarios.Clear();

                // Movemos TODA la lógica de red y cálculo a un hilo de fondo
                await Task.Run(async () =>
                {
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
                        if (NegocioActual.DiasCerrados != null && NegocioActual.DiasCerrados.Any(d => d.Equals(diaKey, StringComparison.OrdinalIgnoreCase)))
                        {
                            estaCerrado = true;
                        }
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

                    // Variables para pasar al hilo principal si es necesario
                    bool setCerrado = estaCerrado || horarioConfig == null;
                    string msgCerrado = $"El negocio se encuentra cerrado los días {nombreDiaLargo}.";
                    var listaTemporal = new List<RangoHorario>();

                    if (!setCerrado)
                    {
                        TimeSpan horaApertura = ParsearHora(horarioConfig.Apertura, new TimeSpan(8, 0, 0));
                        TimeSpan horaCierre = ParsearHora(horarioConfig.Cierre, new TimeSpan(17, 0, 0));

                        if (horaCierre <= horaApertura && horaCierre.Hours < 12)
                        {
                            horaCierre = horaCierre.Add(TimeSpan.FromHours(12));
                        }

                        bool tienePausa = false;
                        TimeSpan pausaInicio = TimeSpan.Zero;
                        TimeSpan pausaFin = TimeSpan.Zero;

                        if (horarioConfig.Pausas != null && horarioConfig.Pausas.Count >= 2)
                        {
                            pausaInicio = ParsearHora(horarioConfig.Pausas[0], new TimeSpan(13, 0, 0));
                            pausaFin = ParsearHora(horarioConfig.Pausas[1], new TimeSpan(14, 0, 0));

                            if (pausaInicio <= horaApertura && pausaInicio.Hours < 12)
                                pausaInicio = pausaInicio.Add(TimeSpan.FromHours(12));
                            if (pausaFin <= pausaInicio && pausaFin.Hours < 12)
                                pausaFin = pausaFin.Add(TimeSpan.FromHours(12));

                            tienePausa = true;
                        }

                        // LLAMADA A LA API EN SEGUNDO PLANO
                        var citasDelDiaDto = await _citaService.ObtenerPorFechaAsync(FechaSeleccionada);
                        string negocioIdFiltro = NegocioActual?.Id ?? Preferences.Get("NegocioSeleccionadoId", string.Empty);

                        string usuarioLogueadoId = Preferences.Get("UsuarioIdSesion", string.Empty);
                        if (string.IsNullOrEmpty(usuarioLogueadoId)) usuarioLogueadoId = Preferences.Get("UsuarioId", string.Empty);

                        if (!string.IsNullOrEmpty(negocioIdFiltro))
                        {
                            citasDelDiaDto = citasDelDiaDto.Where(c => c.NegocioId == negocioIdFiltro && c.Estado != "Cancelada").ToList();
                        }

                        for (var hora = horaApertura; hora < horaCierre; hora = hora.Add(TimeSpan.FromHours(1)))
                        {
                            var inicioBloque = FechaSeleccionada.Date.Add(hora);
                            var finBloque = inicioBloque.AddHours(1);

                            bool esHorarioPausa = tienePausa && (hora >= pausaInicio && hora < pausaFin);
                            var citaOcupadaDto = citasDelDiaDto.FirstOrDefault(c => c.FechaHoraInicio < finBloque && c.FechaHoraFin > inicioBloque);
                            bool esOcupado = citaOcupadaDto != null;
                            bool esDisponible = !esHorarioPausa && !esOcupado;

                            var citaOcupada = citaOcupadaDto != null ? new Citas
                            {
                                Id = citaOcupadaDto.Id ?? string.Empty,
                                NegocioId = citaOcupadaDto.NegocioId,
                                ClienteId = citaOcupadaDto.ClienteId,
                                FechaHoraInicio = citaOcupadaDto.FechaHoraInicio,
                                FechaHoraFin = citaOcupadaDto.FechaHoraFin,
                                Estado = citaOcupadaDto.Estado,
                                Cliente = new ClienteSnapshot
                                {
                                    Nombre = citaOcupadaDto.ClienteNombre,
                                    Telefono = citaOcupadaDto.ClienteTelefono
                                }
                            } : null;

                            bool esMiCita = esOcupado && citaOcupada != null && citaOcupada.ClienteId == usuarioLogueadoId;

                            string detallePausa = esHorarioPausa
                                ? $"({FormatearHora12(pausaInicio)} - {FormatearHora12(pausaFin)})"
                                : (esOcupado ? "(Ocupado)" : "");

                            string estadoTexto = esDisponible ? "Agendar cita" : (esHorarioPausa ? "No disponible" : "Ocupado");

                            listaTemporal.Add(new RangoHorario
                            {
                                HoraDisplay = FormatearHora12(hora),
                                HoraInicio = inicioBloque,
                                HoraFin = finBloque,
                                EsDisponible = esDisponible,
                                EsOcupado = esOcupado,
                                EsNoDisponible = esHorarioPausa,
                                EstadoTexto = estadoTexto,
                                DetallePausa = detallePausa,
                                Cita = citaOcupada,
                                EsMiCita = esMiCita
                            });
                        }
                    }

                    // Volvemos al hilo principal para actualizar la UI
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        EstaCerradoElDia = setCerrado;
                        MensajeCerrado = setCerrado ? msgCerrado : string.Empty;

                        if (!setCerrado)
                        {
                            foreach (var item in listaTemporal)
                            {
                                RangosHorarios.Add(item);
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR RANGOS HORARIOS]: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
                _cargandoHorarios = false;
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

        private async Task OnEliminarCitaAsync(RangoHorario? rango)
        {
            if (rango?.Cita == null || string.IsNullOrEmpty(rango.Cita.Id)) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Eliminar Cita",
                $"¿Estás seguro de eliminar TU cita de las {rango.HoraDisplay}?",
                "Sí, eliminar", "Cancelar");

            if (!confirmar) return;

            bool eliminado = await _citaService.EliminarAsync(rango.Cita.Id);

            if (eliminado)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Cita eliminada correctamente.", "OK");
                await CargarRangosHorariosAsync();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar la cita.", "OK");
            }
        }

        private async Task OnEditarCitaAsync(RangoHorario? rango)
        {
            if (rango?.Cita == null) return;

            string accion = await Application.Current.MainPage.DisplayActionSheet(
                $"Editar tu Cita",
                "Cancelar",
                null,
                "Marcar como Completada",
                "Marcar como Cancelada");

            if (accion == "Cancelar" || string.IsNullOrEmpty(accion)) return;

            string nuevoEstado = accion == "Marcar como Completada" ? "Completada" : "Cancelada";

            var citaDto = new CitaDto
            {
                Id = rango.Cita.Id,
                NegocioId = rango.Cita.NegocioId,
                ClienteId = rango.Cita.ClienteId,
                ClienteNombre = rango.Cita.Cliente?.Nombre ?? "",
                ClienteTelefono = rango.Cita.Cliente?.Telefono ?? "",
                FechaHoraInicio = rango.Cita.FechaHoraInicio,
                FechaHoraFin = rango.Cita.FechaHoraFin,
                Estado = nuevoEstado,
                Precio = 0,
                Moneda = "NIO",
                DuracionMinutos = 45,
                Tipo = "EnLocal",
                MetodoPago = "Efectivo",
                ServicioNombre = "Servicio"
            };

            bool actualizado = await _citaService.ActualizarEstadoAsync(citaDto.Id!, citaDto);

            if (actualizado)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", $"Cita marcada como {nuevoEstado}.", "OK");
                await CargarRangosHorariosAsync();
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo actualizar la cita.", "OK");
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
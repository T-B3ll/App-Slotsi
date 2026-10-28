using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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

namespace slotsi_citas.ViewModel
{
    public class HorariosDisponiblesViewModel : BindableObject, IQueryAttributable
    {
        private readonly CitaService _citaService;
        private readonly NegocioService _negocioService;

        private Negocio? _negocioActual;
        public Negocio? NegocioActual
        {
            get => _negocioActual;
            set { _negocioActual = value; OnPropertyChanged(); }
        }

        private DateTime _fechaSeleccionada = DateTime.Today;
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
                    _ = CargarHorariosAsync();
                }
            }
        }

        public string TextoFechaSeleccionada =>
            _fechaSeleccionada.ToString("d 'de' MMMM 'de' yyyy", new CultureInfo("es-ES"));

        public ObservableCollection<RangoHorario> RangosHorarios { get; } = new();

        public ICommand VolverCommand { get; }
        public ICommand HoyCommand { get; }
        public ICommand SeleccionarHorarioCommand { get; }

        public HorariosDisponiblesViewModel(CitaService citaService, NegocioService negocioService)
        {
            _citaService = citaService;
            _negocioService = negocioService;

            VolverCommand = new Microsoft.Maui.Controls.Command(async () => await Shell.Current.GoToAsync("//dueñosnegocios"));
            HoyCommand = new Microsoft.Maui.Controls.Command(() => FechaSeleccionada = DateTime.Today);
            SeleccionarHorarioCommand = new Microsoft.Maui.Controls.Command<RangoHorario>(OnSeleccionarHorario);

            _ = CargarNegocioAsync();
        }

        public async Task CargarNegocioAsync()
        {
            if (CitaSession.NegocioActual != null)
            {
                NegocioActual = CitaSession.NegocioActual;
            }
            else
            {
                string negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);
                if (!string.IsNullOrEmpty(negocioId))
                {
                    NegocioActual = await _negocioService.ObtenerPorIdAsync(negocioId);
                }
            }

            await CargarHorariosAsync();
        }

        public async Task CargarHorariosAsync()
        {
            try
            {
                RangosHorarios.Clear();

                if (NegocioActual == null || NegocioActual.Horarios == null)
                {
                    return;
                }

                string diaKey = ObtenerClaveDia(FechaSeleccionada.DayOfWeek);
                string nombreDiaLargo = FechaSeleccionada.ToString("dddd", new CultureInfo("es-ES"));

                if (NegocioActual.DiasCerrados != null && NegocioActual.DiasCerrados.Any(d => d.Equals(diaKey, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (!NegocioActual.Horarios.TryGetValue(diaKey, out var horarioDia))
                {
                    return;
                }

                TimeSpan horaApertura = TimeSpan.Parse(horarioDia.Apertura);
                TimeSpan horaCierre = TimeSpan.Parse(horarioDia.Cierre);
                bool tienePausa = horarioDia.Pausas != null && horarioDia.Pausas.Count >= 2;
                TimeSpan pausaInicio = tienePausa ? TimeSpan.Parse(horarioDia.Pausas[0]) : TimeSpan.Zero;
                TimeSpan pausaFin = tienePausa ? TimeSpan.Parse(horarioDia.Pausas[1]) : TimeSpan.Zero;

                // Obtener citas de la API
                var citasDelDiaDto = await _citaService.ObtenerPorFechaAsync(FechaSeleccionada);
                citasDelDiaDto = citasDelDiaDto.Where(c => c.NegocioId == NegocioActual.Id).ToList();

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

                    RangosHorarios.Add(new RangoHorario
                    {
                        HoraDisplay = FormatearHora12(hora),
                        HoraInicio = inicioBloque,
                        HoraFin = finBloque,
                        EsDisponible = esDisponible,
                        EsOcupado = esOcupado,
                        EsNoDisponible = esHorarioPausa,
                        EstadoTexto = esDisponible ? "Agendar cita" : (esHorarioPausa ? "No disponible" : "Ocupado"),
                        DetallePausa = esHorarioPausa ? $"({FormatearHora12(pausaInicio)} - {FormatearHora12(pausaFin)})" : "",
                        Cita = citaOcupada
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR HORARIOS]: {ex.Message}");
            }
        }

        private void OnSeleccionarHorario(RangoHorario? rango)
        {
            if (rango == null || !rango.EsDisponible) return;

            CitaSession.HorarioSeleccionado = rango;
            CitaSession.FechaSeleccionada = FechaSeleccionada;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("..");
            });
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

        private string FormatearHora12(TimeSpan hora)
        {
            return DateTime.Today.Add(hora).ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query) { }
    }
}
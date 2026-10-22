using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class HorariosDisponiblesViewModel : BindableObject, IQueryAttributable
    {
        private readonly IMongoCollection<Negocio> _coleccionNegocios;
        private readonly IMongoCollection<Citas> _coleccionCitas;

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

        public HorariosDisponiblesViewModel()
        {
            var cliente = new MongoClient(MongoDbSettings.ConnectionString);
            var bd = cliente.GetDatabase(MongoDbSettings.DatabaseName);
            _coleccionNegocios = bd.GetCollection<Negocio>("Negocios");
            _coleccionCitas = bd.GetCollection<Citas>("Citas");

            // Usamos el nombre completo para evitar conflicto con MongoDB.Driver.Command
            VolverCommand = new Microsoft.Maui.Controls.Command(async () => await Shell.Current.GoToAsync(".."));
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
                    var filter = Builders<Negocio>.Filter.Eq(n => n.Id, negocioId);
                    NegocioActual = await _coleccionNegocios.Find(filter).FirstOrDefaultAsync();
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

                // Cargar citas existentes para este negocio y fecha
                var inicioDia = FechaSeleccionada.Date;
                var finDia = inicioDia.AddDays(1).AddTicks(-1);
                var filterCitas = Builders<Citas>.Filter.And(
                    Builders<Citas>.Filter.Eq(c => c.NegocioId, NegocioActual.Id),
                    Builders<Citas>.Filter.Gte(c => c.FechaHoraInicio, inicioDia),
                    Builders<Citas>.Filter.Lte(c => c.FechaHoraInicio, finDia)
                );
                var citasDelDia = await _coleccionCitas.Find(filterCitas).ToListAsync();

                for (var hora = horaApertura; hora < horaCierre; hora = hora.Add(TimeSpan.FromHours(1)))
                {
                    var inicioBloque = FechaSeleccionada.Date.Add(hora);
                    var finBloque = inicioBloque.AddHours(1);

                    bool esHorarioPausa = tienePausa && (hora >= pausaInicio && hora < pausaFin);
                    var citaOcupada = citasDelDia.FirstOrDefault(c => c.FechaHoraInicio < finBloque && c.FechaHoraFin > inicioBloque);
                    bool esOcupado = citaOcupada != null;

                    bool esDisponible = !esHorarioPausa && !esOcupado;

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
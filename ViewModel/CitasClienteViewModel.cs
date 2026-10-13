using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class CitasClienteViewModel : BindableObject
    {
        private readonly MongoService _mongoService;
        private DateTime _fechaSeleccionada;

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (_fechaSeleccionada != value)
                {
                    _fechaSeleccionada = value;
                    OnPropertyChanged();
                    _ = CargarRangosHorariosAsync();
                }
            }
        }

        public ObservableCollection<RangoHorario> RangosHorarios { get; } = new ObservableCollection<RangoHorario>();

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand CambiarAFechaDeHoyCommand { get; }
        public ICommand AgendarCitaClienteCommand { get; }

        public CitasClienteViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            _fechaSeleccionada = DateTime.Today;

            SemanaAnteriorCommand = new Command(() => FechaSeleccionada = FechaSeleccionada.AddDays(-1));
            SemanaSiguienteCommand = new Command(() => FechaSeleccionada = FechaSeleccionada.AddDays(1));
            CambiarAFechaDeHoyCommand = new Command(() => FechaSeleccionada = DateTime.Today);

            AgendarCitaClienteCommand = new Command<RangoHorario>(async (rango) => await ExecuteAgendarCita(rango));

            _ = CargarRangosHorariosAsync();
        }

        public CitasClienteViewModel() : this(new MongoService())
        {
        }

        public async Task CargarRangosHorariosAsync()
        {
            RangosHorarios.Clear();

            var citasDelDia = _mongoService != null
                ? await _mongoService.ObtenerCitasPorFechaAsync(FechaSeleccionada)
                : new List<Citas>();

            var horaInicioJornada = new TimeSpan(8, 0, 0);
            var horaFinJornada = new TimeSpan(17, 0, 0);

            for (var hora = horaInicioJornada; hora < horaFinJornada; hora = hora.Add(TimeSpan.FromHours(1)))
            {
                var inicioBloque = FechaSeleccionada.Date.Add(hora);
                var finBloque = inicioBloque.AddHours(1);

                var citaOcupada = citasDelDia.FirstOrDefault(c => c.FechaHoraInicio < finBloque && c.FechaHoraFin > inicioBloque);

                bool esHorarioDescanso = (hora.Hours == 13); // 1:00 PM
                bool esOcupado = citaOcupada != null;
                bool esNoDisponible = esHorarioDescanso;
                bool esDisponible = !esOcupado && !esNoDisponible;

                RangosHorarios.Add(new RangoHorario
                {
                    HoraDisplay = DateTime.Today.Add(hora).ToString("hh:mm tt"),
                    HoraInicio = inicioBloque,
                    HoraFin = finBloque,
                    EsDisponible = esDisponible,
                    EsOcupado = esOcupado,
                    EsNoDisponible = esNoDisponible,
                    Cita = citaOcupada
                });
            }
        }

        private async Task ExecuteAgendarCita(RangoHorario rango)
        {
            if (rango == null || !rango.EsDisponible) return;

            string clienteId = Preferences.Get("UsuarioId", string.Empty);

            if (string.IsNullOrEmpty(clienteId))
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Sesión Expirada", "Debes iniciar sesión para agendar.", "OK");
                }
                return;
            }

            CitaSession.HorarioSeleccionado = rango;
            CitaSession.FechaSeleccionada = FechaSeleccionada;

            var navigationParameters = new Dictionary<string, object>
            {
                { "HorarioSeleccionado", rango },
                { "FechaSeleccionada", FechaSeleccionada }
            };

            await Shell.Current.GoToAsync("SeleccionarCitaPage", navigationParameters);
        }
    }
}
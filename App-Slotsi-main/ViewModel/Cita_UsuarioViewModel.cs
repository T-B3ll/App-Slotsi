using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel
{
    public class Cita_UsuarioViewModel : BindableObject
    {
        private DateTime _fechaInicioSemana;
        private DateTime _fechaSeleccionada;

        public ObservableCollection<RangoHorario> RangosHorarios { get; set; }
        public ObservableCollection<DateTime> FechasSemana { get; set; }

        public DateTime FechaInicioSemana
        {
            get => _fechaInicioSemana;
            set
            {
                _fechaInicioSemana = value;
                OnPropertyChanged();
                ActualizarFechasSemana();
            }
        }

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                _fechaSeleccionada = value;
                OnPropertyChanged();
            }
        }

        public string RangoSemanal => $"{FechaInicioSemana:MMM dd} - {FechaInicioSemana.AddDays(6):dd, yyyy}";

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand AgendarCitaUsuarioCommand { get; }
        public ICommand VerDetallesCitaCommand { get; }

        public Cita_UsuarioViewModel()
        {
            RangosHorarios = new ObservableCollection<RangoHorario>();
            FechasSemana = new ObservableCollection<DateTime>();
            FechaInicioSemana = ObtenerInicioSemana(DateTime.Now);
            FechaSeleccionada = DateTime.Now;

            SemanaAnteriorCommand = new Command(() => FechaInicioSemana = FechaInicioSemana.AddDays(-7));
            SemanaSiguienteCommand = new Command(() => FechaInicioSemana = FechaInicioSemana.AddDays(7));
            AgendarCitaUsuarioCommand = new Command<RangoHorario>(AgendarCitaUsuario);
            VerDetallesCitaCommand = new Command<Cita_Usuario>(VerDetallesCita);

            InicializarRangosHorarios();
            ActualizarFechasSemana();
            CargarCitasUsuario();
        }

        private DateTime ObtenerInicioSemana(DateTime fecha)
        {
            int diferencia = (int)fecha.DayOfWeek - (int)DayOfWeek.Monday;
            if (diferencia < 0) diferencia += 7;
            return fecha.AddDays(-diferencia).Date;
        }

        private void ActualizarFechasSemana()
        {
            FechasSemana.Clear();
            for (int i = 0; i < 7; i++)
            {
                FechasSemana.Add(FechaInicioSemana.AddDays(i));
            }
            OnPropertyChanged(nameof(RangoSemanal));
        }

        private void InicializarRangosHorarios()
        {
            RangosHorarios.Clear();
            // De 8:00 AM a 4:00 PM
            for (int hora = 8; hora <= 16; hora++)
            {
                RangosHorarios.Add(new RangoHorario { Hora = new TimeSpan(hora, 0, 0) });
            }
        }

        public void CargarCitasUsuario()
        {
            // Simular cita ocupada
            var citaOcupada = RangosHorarios.FirstOrDefault(r => r.Hora.Hours == 12);
            if (citaOcupada != null)
            {
                citaOcupada.Cita = new Cita_Usuario
                {
                    Id = 1,
                    Fecha = FechaSeleccionada,
                    HoraInicio = new TimeSpan(12, 0, 0),
                    HoraFin = new TimeSpan(13, 0, 0),
                    Estado = "Ocupado",
                    NombreCliente = "Juan Pérez",
                    TelefonoCliente = "555-1234",
                    Notas = "Primera consulta"
                };
            }

            // Simular cita no disponible
            var citaNoDisponible = RangosHorarios.FirstOrDefault(r => r.Hora.Hours == 13);
            if (citaNoDisponible != null)
            {
                citaNoDisponible.Cita = new Cita_Usuario
                {
                    Id = 2,
                    Fecha = FechaSeleccionada,
                    HoraInicio = new TimeSpan(13, 0, 0),
                    HoraFin = new TimeSpan(14, 0, 0),
                    Estado = "NoDisponible"
                };
            }
        }

        private async void AgendarCitaUsuario(RangoHorario rango)
        {
            if (rango == null) return;

            var nuevaCita = new Cita_Usuario
            {
                Fecha = FechaSeleccionada,
                HoraInicio = rango.Hora,
                HoraFin = rango.Hora.Add(new TimeSpan(1, 0, 0)),
                Estado = "Disponible"
            };

            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    "Agendar Cita de Usuario",
                    $"Nueva cita para las {rango.HoraDisplay}",
                    "OK");
            }
        }

        private async void VerDetallesCita( Cita_Usuario cita)
        {
            if (cita != null && Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    "Detalles de Cita de Usuario",
                    $"Cliente: {cita.NombreCliente}\n" +
                    $"Hora: {cita.HoraInicio} - {cita.HoraFin}\n" +
                    $"Notas: {cita.Notas}",
                    "Cerrar");
            }
        }

        public void RefrescarCitasUsuario()
        {
            CargarCitasUsuario();
        }
    }
}
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
        private string _textoFechaBoton = string.Empty;

        public ObservableCollection<RangoHorario> RangosHorarios { get; set; }
        public ObservableCollection<DiaSemanaModel> FechasSemana { get; set; }

        public DateTime FechaInicioSemana
        {
            get => _fechaInicioSemana;
            set
            {
                _fechaInicioSemana = value;
                OnPropertyChanged();
            }
        }

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                _fechaSeleccionada = value.Date;
                OnPropertyChanged();

                FechaInicioSemana = ObtenerInicioSemana(_fechaSeleccionada);
                ActualizarFechasSemana();
                CargarCitasUsuario();
                ActualizarTextoBoton();
            }
        }
        

        public string TextoFechaBoton
        {
            get => _textoFechaBoton;
            set
            {
                _textoFechaBoton = value;
                OnPropertyChanged();
            }
        }

        public ICommand SemanaAnteriorCommand { get; }
        public ICommand SemanaSiguienteCommand { get; }
        public ICommand IrAHoyCommand { get; }
        public ICommand SeleccionarDiaCommand { get; }

        public Cita_UsuarioViewModel()
        {
            RangosHorarios = new ObservableCollection<RangoHorario>();
            FechasSemana = new ObservableCollection<DiaSemanaModel>();

            _fechaSeleccionada = DateTime.Today;
            _fechaInicioSemana = ObtenerInicioSemana(_fechaSeleccionada);

            SemanaAnteriorCommand = new Command(() =>
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(-7);
            });

            SemanaSiguienteCommand = new Command(() =>
            {
                FechaSeleccionada = FechaSeleccionada.AddDays(7);
            });

            IrAHoyCommand = new Command(() => FechaSeleccionada = DateTime.Today);

            SeleccionarDiaCommand = new Command<DiaSemanaModel>((dia) =>
            {
                if (dia != null)
                {
                    FechaSeleccionada = dia.Fecha;
                }
            });

            InicializarRangosHorarios();
            ActualizarFechasSemana();
            CargarCitasUsuario();
            ActualizarTextoBoton();
        }

        private DateTime ObtenerInicioSemana(DateTime fecha)
        {
            int diferencia = (int)fecha.DayOfWeek - (int)DayOfWeek.Monday;
            if (diferencia < 0) diferencia += 7;
            return fecha.AddDays(-diferencia).Date;
        }

        private void ActualizarTextoBoton()
        {
            // Muestra el día exacto seleccionado en el botón azul (Ej: "01 Oct, 2026")
            TextoFechaBoton = $"{FechaSeleccionada:dd MMM, yyyy}";
        }

        private void ActualizarFechasSemana()
        {
            FechasSemana.Clear();
            for (int i = 0; i < 7; i++)
            {
                var f = FechaInicioSemana.AddDays(i);
                FechasSemana.Add(new DiaSemanaModel
                {
                    Fecha = f,
                    EsSeleccionado = (f.Date == FechaSeleccionada.Date)
                });
            }
            OnPropertyChanged(nameof(FechasSemana));
        }

        private void InicializarRangosHorarios()
        {
            RangosHorarios.Clear();
            for (int hora = 8; hora <= 16; hora++)
            {
                RangosHorarios.Add(new RangoHorario { Hora = new TimeSpan(hora, 0, 0) });
            }
        }

        public void CargarCitasUsuario()
        {
            var listaNueva = new ObservableCollection<RangoHorario>();
            int diaSemana = (int)FechaSeleccionada.DayOfWeek;

            for (int hora = 8; hora <= 16; hora++)
            {
                var rango = new RangoHorario { Hora = new TimeSpan(hora, 0, 0) };

                if (diaSemana == (int)DayOfWeek.Sunday)
                {
                    rango.Cita = new Cita_Usuario { Estado = "NoDisponible" };
                }
                else
                {
                    // Simulación reactiva según el día exacto elegido
                    int horaOcupada = 8 + (FechaSeleccionada.Day % 5);
                    int horaBloqueada = 13 + (FechaSeleccionada.Day % 3);

                    if (hora == horaOcupada)
                    {
                        rango.Cita = new Cita_Usuario
                        {
                            Fecha = FechaSeleccionada,
                            HoraInicio = rango.Hora,
                            HoraFin = rango.Hora.Add(new TimeSpan(1, 0, 0)),
                            Estado = "Ocupado",
                            NombreCliente = $"Cliente ({FechaSeleccionada:dd/MM})",
                            Notas = "Consulta agendada"
                        };
                    }
                    else if (hora == horaBloqueada)
                    {
                        rango.Cita = new Cita_Usuario
                        {
                            Fecha = FechaSeleccionada,
                            HoraInicio = rango.Hora,
                            HoraFin = rango.Hora.Add(new TimeSpan(1, 0, 0)),
                            Estado = "NoDisponible"
                        };
                    }
                }

                listaNueva.Add(rango);
            }

            RangosHorarios = listaNueva;
            OnPropertyChanged(nameof(RangosHorarios));
        }
    }
}
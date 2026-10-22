using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Slotsi.Shared.Dtos;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class CrearResenaViewModel : BindableObject
    {
        private readonly CalificacionService _calificacionService;
        private readonly NegocioService _negocioService;

        private bool _esDuenoNegocio;

        public ObservableCollection<CitaElegibleModel> CitasElegibles { get; set; } = new();

        private CitaElegibleModel? _citaSeleccionada;
        public CitaElegibleModel? CitaSeleccionada
        {
            get => _citaSeleccionada;
            set
            {
                _citaSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneCitaSeleccionada));
            }
        }

        public bool TieneCitaSeleccionada => CitaSeleccionada != null;

        private int _puntuacion = 5;
        public int Puntuacion
        {
            get => _puntuacion;
            set
            {
                _puntuacion = Math.Clamp(value, 1, 5);
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstrellasSeleccionadas));
            }
        }
        public string EstrellasSeleccionadas => new string('★', Puntuacion) + new string('☆', 5 - Puntuacion);

        private string _comentario = string.Empty;
        public string Comentario
        {
            get => _comentario;
            set { _comentario = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        public ICommand SeleccionarCitaCommand { get; }
        public ICommand CambiarPuntuacionCommand { get; }
        public ICommand GuardarResenaCommand { get; }

        public CrearResenaViewModel(CalificacionService calificacionService, NegocioService negocioService)
        {
            _calificacionService = calificacionService ?? throw new ArgumentNullException(nameof(calificacionService));
            _negocioService = negocioService ?? throw new ArgumentNullException(nameof(negocioService));

            SeleccionarCitaCommand = new Microsoft.Maui.Controls.Command<CitaElegibleModel>(OnSeleccionarCita);
            CambiarPuntuacionCommand = new Microsoft.Maui.Controls.Command<string>(OnCambiarPuntuacion);
            GuardarResenaCommand = new Command(async () => await GuardarResenaAsync());
        }

        public async Task CargarCitasCompletadasAsync()
        {
            try
            {
                EstaCargando = true;
                string usuarioIdSesion = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                if (string.IsNullOrWhiteSpace(usuarioIdSesion))
                {
                    CitasElegibles.Clear();
                    return;
                }

                _esDuenoNegocio = Preferences.Get("EsDuenoNegocio", false);

                var listaElegibles = new List<CitaElegibleModel>();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CitasElegibles.Clear();
                    foreach (var item in listaElegibles)
                    {
                        CitasElegibles.Add(item);
                    }

                    if (CitasElegibles.Any())
                    {
                        var primeraCita = CitasElegibles.First();
                        primeraCita.EstaSeleccionada = true;
                        CitaSeleccionada = primeraCita;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrearResena] Error: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void OnSeleccionarCita(CitaElegibleModel cita)
        {
            if (cita == null) return;

            CitaSeleccionada = cita;
            foreach (var item in CitasElegibles)
            {
                item.EstaSeleccionada = (item.CitaId == cita.CitaId);
            }
        }

        private void OnCambiarPuntuacion(string ptsStr)
        {
            if (int.TryParse(ptsStr, out int pts))
            {
                Puntuacion = pts;
            }
        }

        private async Task GuardarResenaAsync()
        {
            if (CitaSeleccionada == null)
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Selecciona una cita para evaluar.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Comentario))
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Escribe un comentario sobre la atención recibida.", "OK");
                return;
            }

            try
            {
                string usuarioIdSesion = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                var nuevaResenaDto = new CalificacionesDtos
                {
                    Id = null,
                    IdNegocio = CitaSeleccionada.NegocioId,
                    IdColaborador = string.IsNullOrEmpty(CitaSeleccionada.TrabajadorId) ? null : CitaSeleccionada.TrabajadorId,
                    IdUsuarioCliente = usuarioIdSesion,
                    RangoCalificacion = Puntuacion,
                    NegocioCalificado = !_esDuenoNegocio,
                    NombreNegocio = CitaSeleccionada.NombreNegocio,
                    NombreUsuario = CitaSeleccionada.NombreCliente,
                    Comentario = Comentario.Trim()
                };

                await _calificacionService.CrearAsync(nuevaResenaDto);

                await Application.Current.MainPage.DisplayAlert("¡Gracias!", "Tu reseña ha sido registrada exitosamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la reseña: {ex.Message}", "OK");
            }
        }
    }

    public class CitaElegibleModel : BindableObject
    {
        public string CitaId { get; set; } = string.Empty;
        public string NegocioId { get; set; } = string.Empty;
        public string TrabajadorId { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string FechaTexto => FechaHora.ToString("dd 'de' MMM, yyyy - hh:mm tt");
        public bool EsDuenoNegocio { get; set; }
        public string TituloCard => EsDuenoNegocio ? NombreCliente : NombreNegocio;
        public string TextoEvaluacionResumen => EsDuenoNegocio ? "Evaluando al cliente:" : "Evaluando atención en:";
        public string NombreEvaluadoResumen => EsDuenoNegocio ? NombreCliente : NombreNegocio;

        private bool _estaSeleccionada;
        public bool EstaSeleccionada
        {
            get => _estaSeleccionada;
            set
            {
                if (_estaSeleccionada != value)
                {
                    _estaSeleccionada = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorBordeCard));
                    OnPropertyChanged(nameof(GrosorBorde));
                }
            }
        }

        public Brush ColorBordeCard => EstaSeleccionada ? new SolidColorBrush(Color.FromArgb("#2563EB")) : new SolidColorBrush(Color.FromArgb("#CBD5E1"));
        public double GrosorBorde => EstaSeleccionada ? 2.5 : 1.0;
    }
}
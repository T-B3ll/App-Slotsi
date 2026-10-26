using Slotsi.Shared.Dtos;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.ViewModel
{
    public class CrearResenaViewModel : BindableObject
    {
        private static readonly string[] EstadosFinalizados = { "Completada", "Finalizada", "Terminada" };

        private readonly CalificacionService _calificacionService;
        private readonly NegocioService _negocioService;
        private readonly CitaService _citaService;
        private readonly ResumenService _resumenService;

        private readonly Dictionary<string, string> _nombresNegocio = new();
        private bool _guardando;

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

        public CrearResenaViewModel(
            CalificacionService calificacionService,
            NegocioService negocioService,
            CitaService citaService,
            ResumenService resumenService)
        {
            _calificacionService = calificacionService ?? throw new ArgumentNullException(nameof(calificacionService));
            _negocioService = negocioService ?? throw new ArgumentNullException(nameof(negocioService));
            _citaService = citaService ?? throw new ArgumentNullException(nameof(citaService));
            _resumenService = resumenService ?? throw new ArgumentNullException(nameof(resumenService));

            SeleccionarCitaCommand = new Microsoft.Maui.Controls.Command<CitaElegibleModel>(OnSeleccionarCita);
            CambiarPuntuacionCommand = new Microsoft.Maui.Controls.Command<string>(OnCambiarPuntuacion);
            GuardarResenaCommand = new Command(async () => await GuardarResenaAsync());
        }

        public async Task CargarCitasCompletadasAsync()
        {
            if (EstaCargando) return;

            try
            {
                EstaCargando = true;

                string usuarioId = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));
                if (string.IsNullOrWhiteSpace(usuarioId))
                {
                    MainThread.BeginInvokeOnMainThread(() => AplicarLista(new List<CitaElegibleModel>()));
                    return;
                }

                bool esDueno = Preferences.Get("EsDuenoNegocio", false);
                DateTime hasta = DateTime.Now;
                DateTime desde = hasta.AddMonths(-1);

                var lista = new List<CitaElegibleModel>();
                string? error = null;

                try
                {
                    lista.AddRange(await CargarComoClienteAsync(usuarioId, desde, hasta));
                }
                catch (Exception ex)
                {
                    error = ApiException.MensajePara(ex);
                    System.Diagnostics.Debug.WriteLine($"[CrearResena] Error cliente: {ex.Message}");
                }

                if (esDueno)
                {
                    try
                    {
                        lista.AddRange(await CargarComoDuenoAsync(usuarioId, desde, hasta));
                    }
                    catch (Exception ex)
                    {
                        error ??= ApiException.MensajePara(ex);
                        System.Diagnostics.Debug.WriteLine($"[CrearResena] Error dueño: {ex.Message}");
                    }
                }

                var ordenada = lista.OrderByDescending(c => c.FechaHora).ToList();
                MainThread.BeginInvokeOnMainThread(() => AplicarLista(ordenada));

                if (error != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error", $"No se pudieron cargar todas las citas: {error}", "OK");
                }
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task<List<CitaElegibleModel>> CargarComoClienteAsync(string usuarioId, DateTime desde, DateTime hasta)
        {
            var citas = await _citaService.ObtenerPorUsuarioAsync(usuarioId) ?? new List<CitaDto>();
            var resenas = await _calificacionService.ObtenerPorUsuarioAsync(usuarioId) ?? new List<CalificacionesDtos>();

            var yaCalificados = resenas
                .Where(r => r.NegocioCalificado)
                .Select(r => r.IdNegocio)
                .ToHashSet();

            var elegibles = citas
                .Where(c => EsElegible(c, desde, hasta) && !yaCalificados.Contains(c.NegocioId))
                .GroupBy(c => c.NegocioId)
                .Select(g => g.OrderByDescending(c => c.FechaHoraInicio).First())
                .ToList();

            var lista = new List<CitaElegibleModel>();
            foreach (var c in elegibles)
            {
                string nombreNegocio = await ObtenerNombreNegocioAsync(c.NegocioId);
                lista.Add(Mapear(c, nombreNegocio, esDueno: false));
            }
            return lista;
        }

        private async Task<List<CitaElegibleModel>> CargarComoDuenoAsync(string usuarioId, DateTime desde, DateTime hasta)
        {
            string negocioId = Preferences.Get("negocio_id", string.Empty);
            if (string.IsNullOrEmpty(negocioId))
            {
                var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(usuarioId);
                negocioId = negocio?.Id ?? string.Empty;
                if (!string.IsNullOrEmpty(negocioId))
                    Preferences.Set("negocio_id", negocioId);
            }
            if (string.IsNullOrEmpty(negocioId)) return new List<CitaElegibleModel>();

            var resumen = await _resumenService.ObtenerResumenAsync(negocioId, desde.Date, hasta.Date.AddDays(1));
            var citas = resumen?.Citas ?? new List<CitaDto>();

            var resenas = await _calificacionService.ObtenerPorNegocioAsync(negocioId) ?? new List<CalificacionesDtos>();

            var yaCalificados = resenas
                .Where(r => !r.NegocioCalificado)
                .Select(r => r.IdUsuarioCliente)
                .ToHashSet();

            var elegibles = citas
                .Where(c => EsElegible(c, desde, hasta) && !yaCalificados.Contains(c.ClienteId))
                .GroupBy(c => c.ClienteId)
                .Select(g => g.OrderByDescending(c => c.FechaHoraInicio).First())
                .ToList();

            string nombreNegocio = await ObtenerNombreNegocioAsync(negocioId);
            return elegibles.Select(c => Mapear(c, nombreNegocio, esDueno: true)).ToList();
        }

        private static bool EsElegible(CitaDto c, DateTime desde, DateTime hasta)
        {
            return !string.IsNullOrEmpty(c.Id)
                && EstadosFinalizados.Contains(c.Estado ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                && c.FechaHoraInicio >= desde
                && c.FechaHoraInicio <= hasta;
        }

        private static CitaElegibleModel Mapear(CitaDto c, string nombreNegocio, bool esDueno)
        {
            return new CitaElegibleModel
            {
                CitaId = c.Id ?? string.Empty,
                NegocioId = c.NegocioId,
                ClienteId = c.ClienteId,
                TrabajadorId = c.TrabajadorId ?? string.Empty,
                ServicioNombre = c.ServicioNombre,
                NombreNegocio = nombreNegocio,
                NombreCliente = string.IsNullOrWhiteSpace(c.ClienteNombre) ? "Cliente" : c.ClienteNombre,
                FechaHora = c.FechaHoraInicio,
                EsDuenoNegocio = esDueno
            };
        }

        private async Task<string> ObtenerNombreNegocioAsync(string negocioId)
        {
            if (string.IsNullOrEmpty(negocioId)) return "Negocio";
            if (_nombresNegocio.TryGetValue(negocioId, out var nombre)) return nombre;

            var negocio = await _negocioService.ObtenerPorIdAsync(negocioId);
            nombre = negocio?.Nombre ?? "Negocio"; 
            _nombresNegocio[negocioId] = nombre;
            return nombre;
        }

        private void AplicarLista(List<CitaElegibleModel> lista)
        {
            CitasElegibles.Clear();
            foreach (var item in lista)
                CitasElegibles.Add(item);

            var primera = CitasElegibles.FirstOrDefault();
            foreach (var item in CitasElegibles)
                item.EstaSeleccionada = ReferenceEquals(item, primera);

            CitaSeleccionada = primera;
        }


        private void OnSeleccionarCita(CitaElegibleModel cita)
        {
            if (cita == null) return;

            CitaSeleccionada = cita;
            foreach (var item in CitasElegibles)
                item.EstaSeleccionada = ReferenceEquals(item, cita);
        }

        private void OnCambiarPuntuacion(string ptsStr)
        {
            if (int.TryParse(ptsStr, out int pts))
                Puntuacion = pts;
        }

        private async Task GuardarResenaAsync()
        {
            if (_guardando) return;

            var cita = CitaSeleccionada;
            if (cita == null)
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
                _guardando = true;

                var nuevaResenaDto = new CalificacionesDtos
                {
                    Id = null,
                    IdNegocio = cita.NegocioId,
                    IdColaborador = string.IsNullOrEmpty(cita.TrabajadorId) ? null : cita.TrabajadorId,
                    IdUsuarioCliente = cita.ClienteId,
                    RangoCalificacion = Puntuacion,
                    NegocioCalificado = !cita.EsDuenoNegocio,
                    NombreNegocio = cita.NombreNegocio,
                    NombreUsuario = cita.NombreCliente,
                    Comentario = Comentario.Trim()
                };

                await _calificacionService.CrearAsync(nuevaResenaDto);

                await Application.Current.MainPage.DisplayAlert("¡Gracias!", "Tu reseña ha sido registrada exitosamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la reseña: {ApiException.MensajePara(ex)}", "OK");
            }
            finally
            {
                _guardando = false;
            }
        }
    }

    public class CitaElegibleModel : BindableObject
    {
        public string CitaId { get; set; } = string.Empty;
        public string NegocioId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
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

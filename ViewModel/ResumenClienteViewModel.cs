using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Slotsi.Shared.Dtos;

namespace slotsi_citas.ViewModel
{
    public partial class ResumenClienteViewModel : ObservableObject
    {
        private readonly UsuarioService _usuarioService;
        private readonly NegocioService _negocioService;
        private readonly ResumenService _service;

        private string UsuarioId => Preferences.Get("UsuarioId", string.Empty);
        private string NombreCompleto => Preferences.Get("NombreUsuario", "Cliente");

        [ObservableProperty] private string saludo = string.Empty;
        [ObservableProperty] private string iniciales = string.Empty;

        [ObservableProperty] private int citasProximas;
        [ObservableProperty] private int citasPendientes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TieneProxima))]
        private CitaClienteItemViewModel? proximaCita;

        [ObservableProperty] private bool isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string? error;

        public bool TieneProxima => proximaCita is not null;
        public bool HasError => !string.IsNullOrEmpty(Error);

        public ObservableCollection<CitaClienteItemViewModel> Citas { get; } = new();

        public ResumenClienteViewModel(ResumenService service, NegocioService negocioService, UsuarioService usuarioService)
        {
            _service = service;
            _negocioService = negocioService;
            _usuarioService = usuarioService;
        }

        private async Task CargarEncabezadoAsync()
        {
            var nombre = Preferences.Get("NombreUsuario", string.Empty);

            if (string.IsNullOrWhiteSpace(nombre))
            {
                var correo = Preferences.Get("UsuarioCorreo", string.Empty);
                if (!string.IsNullOrEmpty(correo))
                {
                    try
                    {
                        var usuario = await _usuarioService.ObtenerPorCorreoAsync(correo);
                        nombre = usuario?.NombreCompleto ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(nombre))
                            Preferences.Set("NombreUsuario", nombre);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Cliente";

            var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Saludo = $"¡Hola, {partes.FirstOrDefault() ?? "Cliente"}!";
            Iniciales = string.Concat(partes.Take(2).Select(p => char.ToUpper(p[0])));
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Error = null;
                await CargarEncabezadoAsync();

                if (string.IsNullOrEmpty(UsuarioId))
                {
                    Error = "No se encontró tu sesión. Vuelve a iniciar sesión.";
                    return;
                }

                var citas = await _service.ObtenerCitasClienteAsync(UsuarioId);

                var ahora = DateTime.Now;
                var vigentesDto = citas
                    .Where(c => c.FechaHoraInicio >= ahora && c.Estado != "Cancelada")
                    .OrderBy(c => c.FechaHoraInicio)
                    .ToList();

                var nombres = new Dictionary<string, string>();
                foreach (var id in vigentesDto.Select(c => c.NegocioId).Distinct())
                {
                    try
                    {
                        var negocio = await _negocioService.ObtenerPorIdAsync(id);
                        nombres[id] = negocio?.NombreNegocio ?? "Negocio";
                    }
                    catch
                    {
                        nombres[id] = "Negocio";
                    }
                }

                var vigentes = vigentesDto
                    .Select(c => new CitaClienteItemViewModel(c, nombres.GetValueOrDefault(c.NegocioId, "Negocio")))
                    .ToList();

                Citas.Clear();
                foreach (var cita in vigentes)
                    Citas.Add(cita);

                ProximaCita = vigentes.FirstOrDefault();
                CitasProximas = vigentes.Count(c => c.EsConfirmada);
                CitasPendientes = vigentes.Count(c => !c.EsConfirmada);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Error = $"No se pudieron cargar tus citas: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task IrAAsync(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return;

            try
            {
                await Shell.Current.GoToAsync(ruta);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Error = "No se pudo abrir la pantalla.";
            }
        }
    }

    public class CitaClienteItemViewModel
    {
        private static readonly Color Verde = Color.FromArgb("#10B981");
        private static readonly Color VerdeClaro = Color.FromArgb("#DCFCE7");
        private static readonly Color VerdeOscuro = Color.FromArgb("#15803D");
        private static readonly Color Naranja = Color.FromArgb("#F59E0B");
        private static readonly Color NaranjaClaro = Color.FromArgb("#FEF3C7");
        private static readonly Color NaranjaOscuro = Color.FromArgb("#B45309");

        public CitaClienteItemViewModel(CitaDto cita, string negocioNombre)
        {
            var cultura = new CultureInfo("es-NI");

            Negocio = negocioNombre;
            Servicio = cita.ServicioNombre;
            Fecha = cita.FechaHoraInicio.ToString("ddd d MMM", cultura);
            Hora = cita.FechaHoraInicio.ToString("h:mm tt", CultureInfo.InvariantCulture);
            FechaHoraInicio = cita.FechaHoraInicio;

            EstadoTexto = (cita.Estado ?? string.Empty).ToUpperInvariant();
            EsConfirmada = cita.Estado == "Confirmada";

            ColorPrincipal = EsConfirmada ? Verde : Naranja;
            ColorChipFondo = EsConfirmada ? VerdeClaro : NaranjaClaro;
            ColorChipTexto = EsConfirmada ? VerdeOscuro : NaranjaOscuro;
        }

        public DateTime FechaHoraInicio { get; }
        public string Negocio { get; }
        public string Servicio { get; }
        public string Fecha { get; }
        public string Hora { get; }
        public string EstadoTexto { get; }
        public bool EsConfirmada { get; }
        public Color ColorPrincipal { get; }
        public Color ColorChipFondo { get; }
        public Color ColorChipTexto { get; }
    }

}
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class CatalogoServiciosViewModel : BindableObject
    {
        private readonly CatalogoService _catalogoService;
        private readonly TrabajadorService _trabajadorService;

        private List<Servicios> _todosLosServiciosCliente = new();
        public ObservableCollection<Servicios> Servicios { get; set; } = new();

        private readonly List<Servicios> _serviciosSeleccionados = new();

        private Negocio? _negocioActual;
        public Negocio? NegocioActual
        {
            get => _negocioActual;
            set { _negocioActual = value; OnPropertyChanged(); }
        }

        private int _serviciosActivosCount;
        public int ServiciosActivosCount
        {
            get => _serviciosActivosCount;
            set { _serviciosActivosCount = value; OnPropertyChanged(); }
        }

        private string _textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                AplicarFiltrosCliente();
            }
        }

        private string _modalidadSeleccionada = "Todos";
        public string ModalidadSeleccionada
        {
            get => _modalidadSeleccionada;
            set
            {
                _modalidadSeleccionada = value;
                OnPropertyChanged();
                ActualizarColoresBotones();
                AplicarFiltrosCliente();
            }
        }

        public int CantidadSeleccionados => _serviciosSeleccionados.Count;
        public bool TieneServiciosSeleccionados => _serviciosSeleccionados.Any();
        public decimal TotalPrecioSeleccionado => _serviciosSeleccionados.Sum(s => s.Precio);
        public string TextoResumenSeleccion => $"{CantidadSeleccionados} de 3 servicio(s) seleccionado(s)";
        public string TextoTotalPrecio => $"Total: C$ {TotalPrecioSeleccionado:N2}";

        public Color ColorBtnTodos => ModalidadSeleccionada == "Todos" ? Color.FromArgb("#2563EB") : Color.FromArgb("#F1F5F9");
        public Color TextColorBtnTodos => ModalidadSeleccionada == "Todos" ? Colors.White : Color.FromArgb("#475569");

        public Color ColorBtnLocal => ModalidadSeleccionada == "Local" ? Color.FromArgb("#2563EB") : Color.FromArgb("#F1F5F9");
        public Color TextColorBtnLocal => ModalidadSeleccionada == "Local" ? Colors.White : Color.FromArgb("#475569");

        public Color ColorBtnDomicilio => ModalidadSeleccionada == "Domicilio" ? Color.FromArgb("#2563EB") : Color.FromArgb("#F1F5F9");
        public Color TextColorBtnDomicilio => ModalidadSeleccionada == "Domicilio" ? Colors.White : Color.FromArgb("#475569");

        public ICommand FiltrarModalidadCommand { get; }
        public ICommand ToggleSeleccionServicioCommand { get; }
        public ICommand ContinuarAgendarCommand { get; }
        public ICommand CambiarEstadoActivoCommand { get; }
        public ICommand CreaServicioCommand { get; }
        public ICommand CargarServiciosCommand { get; }
        public ICommand EditarservicioCommand { get; }
        public ICommand CambiarFotoServicio { get; }

        public CatalogoServiciosViewModel(CatalogoService catalogoService, TrabajadorService trabajadorService)
        {
            _catalogoService = catalogoService ?? throw new ArgumentNullException(nameof(catalogoService));
            _trabajadorService = trabajadorService ?? throw new ArgumentNullException(nameof(trabajadorService));

            FiltrarModalidadCommand = new Microsoft.Maui.Controls.Command<string>((modalidad) => ModalidadSeleccionada = modalidad ?? "Todos");
            ToggleSeleccionServicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(OnToggleSeleccionServicio);
            ContinuarAgendarCommand = new Microsoft.Maui.Controls.Command(async () => await OnContinuarAgendarAsync());

            CambiarEstadoActivoCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await CambiarEstadoActivoAsync(servicio));
            CreaServicioCommand = new Microsoft.Maui.Controls.Command(async () => await CrearServicioAsync());
            CargarServiciosCommand = new Microsoft.Maui.Controls.Command(async () => await CargarServiciosAsync());
            EditarservicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await EditarServicioAsync(servicio));
            CambiarFotoServicio = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await SeleccionarYGuardarFotoAsync(servicio));
        }

        public async Task CargarservicioClienteAsync(string? negocioId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(negocioId))
                {
                    negocioId = Preferences.Get("NegocioSeleccionadoId", string.Empty);
                }

                if (string.IsNullOrEmpty(negocioId))
                {
                    negocioId = await ObtenerNegocioIdDeSesionAsync();
                }

                if (string.IsNullOrEmpty(negocioId)) return;

                NegocioActual = await _catalogoService.ObtenerNegocioPorIdAsync(negocioId);

                var resultados = await _catalogoService.ObtenerServiciosPorNegocioAsync(negocioId);

                _todosLosServiciosCliente = resultados.Where(s => s.Activo).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AplicarFiltrosCliente();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar servicios activos: {ex.Message}", "OK");
            }
        }

        private void OnToggleSeleccionServicio(Servicios? servicio)
        {
            if (servicio == null) return;

            bool yaExiste = _serviciosSeleccionados.Any(s => s.Id == servicio.Id);

            if (yaExiste)
            {
                _serviciosSeleccionados.RemoveAll(s => s.Id == servicio.Id);
                servicio.EsSeleccionado = false;
            }
            else
            {
                if (_serviciosSeleccionados.Count >= 3)
                {
                    Application.Current?.MainPage?.DisplayAlert("Límite alcanzado", "Puedes seleccionar un máximo de 3 servicios por cita.", "Entendido");
                    return;
                }

                _serviciosSeleccionados.Add(servicio);
                servicio.EsSeleccionado = true;
            }

            NotificarCambiosSeleccion();
        }

        private void NotificarCambiosSeleccion()
        {
            OnPropertyChanged(nameof(CantidadSeleccionados));
            OnPropertyChanged(nameof(TieneServiciosSeleccionados));
            OnPropertyChanged(nameof(TotalPrecioSeleccionado));
            OnPropertyChanged(nameof(TextoResumenSeleccion));
            OnPropertyChanged(nameof(TextoTotalPrecio));
        }

        private async Task OnContinuarAgendarAsync()
        {
            if (!_serviciosSeleccionados.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Por favor selecciona al menos un servicio para continuar.", "OK");
                return;
            }

            if (NegocioActual != null)
            {
                CitaSession.NegocioActual = NegocioActual;
                CitaSession.NegocioId = NegocioActual.Id ?? string.Empty;
                CitaSession.DireccionCliente = string.IsNullOrEmpty(NegocioActual.DireccionNegocio)
                    ? "Matagalpa, Nicaragua"
                    : NegocioActual.DireccionNegocio;

                if (!string.IsNullOrEmpty(NegocioActual.Id))
                {
                    Preferences.Set("NegocioSeleccionadoId", NegocioActual.Id);
                }
            }

            string serviciosIdsJson = string.Join(",", _serviciosSeleccionados.Select(s => s.Id));
            Preferences.Set("ServiciosSeleccionadosIds", serviciosIdsJson);

            var parametrosNavegacion = new Dictionary<string, object>
            {
                { "ServiciosSeleccionados", _serviciosSeleccionados },
                { "Negocio", NegocioActual ?? new Negocio() }
            };

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("CalendarioPage", parametrosNavegacion);
            }
        }

        private void AplicarFiltrosCliente()
        {
            if (_todosLosServiciosCliente == null) return;

            var filtrados = _todosLosServiciosCliente.AsEnumerable();

            if (ModalidadSeleccionada == "Local")
            {
                filtrados = filtrados.Where(s => !s.DisponibleDomicilio);
            }
            else if (ModalidadSeleccionada == "Domicilio")
            {
                filtrados = filtrados.Where(s => s.DisponibleDomicilio);
            }

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string busqueda = TextoBusqueda.ToLower().Trim();
                filtrados = filtrados.Where(s =>
                    (s.Nombre != null && s.Nombre.ToLower().Contains(busqueda)) ||
                    (s.Categoria != null && s.Categoria.ToLower().Contains(busqueda))
                );
            }

            Servicios.Clear();
            foreach (var serv in filtrados)
            {
                serv.EsSeleccionado = _serviciosSeleccionados.Any(s => s.Id == serv.Id);
                Servicios.Add(serv);
            }
        }

        private void ActualizarColoresBotones()
        {
            OnPropertyChanged(nameof(ColorBtnTodos));
            OnPropertyChanged(nameof(TextColorBtnTodos));
            OnPropertyChanged(nameof(ColorBtnLocal));
            OnPropertyChanged(nameof(TextColorBtnLocal));
            OnPropertyChanged(nameof(ColorBtnDomicilio));
            OnPropertyChanged(nameof(TextColorBtnDomicilio));
        }

        private async Task<string?> ObtenerNegocioIdDeSesionAsync()
        {
            string usuarioIdSesion = Preferences.Get("UsuarioIdSesion", string.Empty);
            if (string.IsNullOrEmpty(usuarioIdSesion))
            {
                usuarioIdSesion = Preferences.Get("UsuarioId", string.Empty);
            }

            if (string.IsNullOrEmpty(usuarioIdSesion))
            {
                await Application.Current.MainPage.DisplayAlert("Sesión Expirada", "No hay un usuario activo.", "OK");
                return null;
            }

            var negocio = await _catalogoService.ObtenerNegocioPorUsuarioAsync(usuarioIdSesion);

            if (negocio == null || string.IsNullOrEmpty(negocio.Id))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "No se encontró ningún negocio registrado para este usuario.", "OK");
                return null;
            }

            NegocioActual = negocio;
            return negocio.Id;
        }

        public async Task CargarServiciosAsync()
        {
            try
            {         
                string negocioId = Preferences.Get("negocio_id", string.Empty);

                if (string.IsNullOrEmpty(negocioId))
                {
                    negocioId = await ObtenerNegocioIdDeSesionAsync() ?? string.Empty;
                    if (string.IsNullOrEmpty(negocioId)) return;
                    Preferences.Set("negocio_id", negocioId);
                }

                var resultados = await _catalogoService.ObtenerServiciosPorNegocioAsync(negocioId);

                System.Diagnostics.Debug.WriteLine($"[SERVICIOS] negocioId={negocioId}, cantidad={resultados.Count}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Servicios.Clear();
                    foreach (var serv in resultados)
                        Servicios.Add(serv);
                    ServiciosActivosCount = Servicios.Count(s => s.Activo);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR SERVICIOS]: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current!.MainPage!.DisplayAlert("Error", $"Error al cargar servicios: {ex.Message}", "OK");
                });
            }
        }
        public async Task CambiarEstadoActivoAsync(Servicios servicio)
        {
            if (servicio == null) return;

            if (!servicio.Activo)
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Desactivar servicio",
                    $"¿Estás seguro de que deseas desactivar '{servicio.Nombre}'? Los clientes no podrán agendarlo.",
                    "Desactivar",
                    "Cancelar");

                if (!confirmar)
                {
                    servicio.Activo = true;
                    return;
                }
            }

            bool actualizado = await _catalogoService.ActualizarEstadoServicioAsync(servicio.Id!, servicio.Activo);
            if (actualizado)
            {
                ServiciosActivosCount = Servicios.Count(s => s.Activo);
            }
            else
            {
                servicio.Activo = !servicio.Activo;
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo actualizar el estado del servicio.", "OK");
            }
        }
        public async Task CrearServicioAsync()
        {
            string? negocioId = await ObtenerNegocioIdDeSesionAsync();
            if (string.IsNullOrEmpty(negocioId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo identificar el negocio", "OK");
                return;
            }

            var paginaCreacion = new Pages.EditarServicio(null, _catalogoService, _trabajadorService, negocioId);
            await Application.Current.MainPage.Navigation.PushModalAsync(paginaCreacion);
            await CargarServiciosAsync();
        }

        public async Task SeleccionarYGuardarFotoAsync(Servicios servicio)
        {
            if (servicio == null) return;

            try
            {
                var resultado = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona una imagen para el servicio",
                    FileTypes = FilePickerFileType.Images
                });

                if (resultado != null)
                {
                    using var stream = await resultado.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] bytesImagen = memoryStream.ToArray();

                    string extension = Path.GetExtension(resultado.FileName).ToLower().Replace(".", "");
                    if (string.IsNullOrEmpty(extension)) extension = "jpeg";

                    string base64Imagen = $"data:image/{extension};base64,{Convert.ToBase64String(bytesImagen)}";
                    servicio.Foto = base64Imagen;

                    bool guardado = await _catalogoService.ActualizarServicioAsync(servicio.Id!, servicio);

                    if (guardado)
                    {
                        await CargarServiciosAsync();
                        await Application.Current.MainPage.DisplayAlert("Éxito", "La imagen del servicio fue actualizada correctamente.", "OK");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar la imagen en el servidor.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo actualizar la imagen: {ex.Message}", "OK");
            }
        }

        public async Task EditarServicioAsync(Servicios servicio)
        {
            if (servicio == null) return;

            var paginaEdicion = new Pages.EditarServicio(servicio, _catalogoService, _trabajadorService, servicio.NegocioId);
            await Application.Current.MainPage.Navigation.PushModalAsync(paginaEdicion);
        }
    }
}
using slotsi_citas.Models;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Slotsi.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.ViewModel
{
    public class ColaboradorSeleccionable : BindableObject
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        private bool _isSeleccionado;
        public bool IsSeleccionado
        {
            get => _isSeleccionado;
            set { _isSeleccionado = value; OnPropertyChanged(); }
        }
    }

    public class EditarServicioViewModel : BindableObject
    {
        private readonly CatalogoService _catalogoService;
        private readonly TrabajadorService _trabajadorService;
        private readonly string _negocioId;
        private readonly Servicios? _servicioOriginal;
        private readonly bool _esNuevo;

        public string TituloPantalla => _esNuevo ? "Nuevo Servicio" : "Editar Servicio";
        public string TextoBotonGuardar => _esNuevo ? "Crear Servicio" : "Guardar Cambios";

        public string Id { get; set; } = string.Empty;

        private string _nombre = string.Empty;
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }
        private string _precioTexto = string.Empty;
        public string PrecioTexto { get => _precioTexto; set { _precioTexto = value; OnPropertyChanged(); } }

        private string _duracionTexto = string.Empty;
        public string DuracionTexto { get => _duracionTexto; set { _duracionTexto = value; OnPropertyChanged(); } }

        private string _categoria = string.Empty;
        public string Categoria { get => _categoria; set { _categoria = value; OnPropertyChanged(); } }

        private string _descripcion = string.Empty;
        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

        private bool _disponibleDomicilio;
        public bool DisponibleDomicilio { get => _disponibleDomicilio; set { _disponibleDomicilio = value; OnPropertyChanged(); } }

        private string _foto = string.Empty;
        public string Foto { get => _foto; set { _foto = value; OnPropertyChanged(); } }

        public ObservableCollection<ColaboradorSeleccionable> Colaboradores { get; set; } = new();

        public ICommand CambiarFotoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarServicioViewModel(Servicios? servicio, CatalogoService catalogoService, TrabajadorService trabajadorService, string negocioId = "")
        {
            _catalogoService = catalogoService ?? throw new ArgumentNullException(nameof(catalogoService));
            _trabajadorService = trabajadorService ?? throw new ArgumentNullException(nameof(trabajadorService));
            _servicioOriginal = servicio;
            _esNuevo = (servicio == null);

            _negocioId = !_esNuevo && servicio != null ? servicio.NegocioId : negocioId;

            if (!_esNuevo && servicio != null)
            {
                Id = servicio.Id ?? string.Empty;
                Nombre = servicio.Nombre;
                PrecioTexto = servicio.Precio.ToString("0.##", CultureInfo.InvariantCulture);
                DuracionTexto = servicio.DuracionMinutos.ToString(CultureInfo.InvariantCulture);
                Categoria = servicio.Categoria;
                Descripcion = servicio.Descripcion;
                DisponibleDomicilio = servicio.DisponibleDomicilio;
                Foto = servicio.Foto;
            }

            CambiarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            GuardarCommand = new Command(async () => await GuardarCambiosAsync());
            CancelarCommand = new Command(async () => await Application.Current.MainPage.Navigation.PopModalAsync());

            _ = CargarColaboradoresAsync();
        }

        private async Task CargarColaboradoresAsync()
        {
            if (string.IsNullOrEmpty(_negocioId)) return;

            try
            {
                var listaDtos = await _trabajadorService.ObtenerPorNegocioAsync(_negocioId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Colaboradores.Clear();
                    foreach (var colab in listaDtos)
                    {
                        bool estaHabilitado = _servicioOriginal != null
                            && _servicioOriginal.ColaboradoresHabilitados != null
                            && _servicioOriginal.ColaboradoresHabilitados.Contains(colab.Id);

                        Colaboradores.Add(new ColaboradorSeleccionable
                        {
                            Id = colab.Id,
                            Nombre = colab.Nombre ?? "Sin Nombre",
                            IsSeleccionado = estaHabilitado
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar colaboradores: {ex.Message}", "OK");
            }
        }

        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var resultado = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona una foto para el servicio",
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

                    Foto = $"data:image/{extension};base64,{Convert.ToBase64String(bytesImagen)}";
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo seleccionar la foto: {ex.Message}", "OK");
            }
        }

        private List<string> Validar(out decimal precio, out int duracion)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(Nombre))
                errores.Add("Nombre del servicio");

            if (!decimal.TryParse(PrecioTexto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out precio) || precio <= 0)
                errores.Add("Precio (debe ser mayor que 0)");

            if (!int.TryParse(DuracionTexto, NumberStyles.None, CultureInfo.InvariantCulture, out duracion) || duracion <= 0)
                errores.Add("Duración en minutos (debe ser mayor que 0)");

            if (string.IsNullOrWhiteSpace(Categoria))
                errores.Add("Categoría");

            if (string.IsNullOrWhiteSpace(Descripcion))
                errores.Add("Descripción");

            if (string.IsNullOrWhiteSpace(Foto))
                errores.Add("Foto del servicio");

            if (!Colaboradores.Any(c => c.IsSeleccionado))
                errores.Add("Al menos un colaborador habilitado");

            return errores;
        }

        private async Task GuardarCambiosAsync()
        {
            var errores = Validar(out var precio, out var duracion);
            if (errores.Count > 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Faltan datos",
                    "Completa los siguientes campos:\n\n• " + string.Join("\n• ", errores),
                    "OK");
                return;
            }

            var nombre = Nombre.Trim();
            var categoria = Categoria.Trim();
            var descripcion = Descripcion.Trim();

            var colaboradoresSeleccionados = Colaboradores
                .Where(c => c.IsSeleccionado)
                .Select(c => c.Id)
                .ToList();

            try
            {
                if (_esNuevo)
                {
                    if (string.IsNullOrEmpty(_negocioId))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "El ID del negocio no se recibió en esta pantalla.", "OK");
                        return;
                    }

                    var nuevoServicio = new Servicios
                    {
                        NegocioId = _negocioId,
                        Nombre = nombre,
                        Precio = precio,
                        DuracionMinutos = duracion,
                        Categoria = categoria,
                        Descripcion = descripcion,
                        DisponibleDomicilio = DisponibleDomicilio,
                        Foto = Foto,
                        Activo = true,
                        ColaboradoresHabilitados = colaboradoresSeleccionados
                    };

                    bool creado = await _catalogoService.CrearServicioAsync(nuevoServicio);
                    if (!creado)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo crear el servicio en el servidor.", "OK");
                        return;
                    }
                }
                else
                {
                    var servicioActualizado = new Servicios
                    {
                        Id = Id,
                        NegocioId = _negocioId,
                        Nombre = nombre,
                        Precio = precio,
                        DuracionMinutos = duracion,
                        Categoria = categoria,
                        Descripcion = descripcion,
                        DisponibleDomicilio = DisponibleDomicilio,
                        Foto = Foto,
                        Activo = _servicioOriginal?.Activo ?? true,
                        ColaboradoresHabilitados = colaboradoresSeleccionados
                    };

                    bool actualizado = await _catalogoService.ActualizarServicioAsync(Id, servicioActualizado);
                    if (!actualizado)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron actualizar los cambios en el servidor.", "OK");
                        return;
                    }

                    if (_servicioOriginal != null)
                    {
                        _servicioOriginal.Nombre = nombre;
                        _servicioOriginal.Precio = precio;
                        _servicioOriginal.DuracionMinutos = duracion;
                        _servicioOriginal.Categoria = categoria;
                        _servicioOriginal.Descripcion = descripcion;
                        _servicioOriginal.DisponibleDomicilio = DisponibleDomicilio;
                        _servicioOriginal.Foto = Foto;
                        _servicioOriginal.ColaboradoresHabilitados = colaboradoresSeleccionados;
                    }
                }

                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la información: {ex.Message}", "OK");
            }
        }
    }
}
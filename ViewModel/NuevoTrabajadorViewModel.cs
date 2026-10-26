using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using slotsi_citas.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace slotsi_citas.ViewModels
{
    [QueryProperty(nameof(TrabajadorAEditar), "TrabajadorEditar")]
    public class NuevoTrabajadorViewModel : INotifyPropertyChanged
    {
        private readonly NegocioService _negocioService;
        private readonly TrabajadorService _trabajadorService;
        private string _trabajadorId;

        private Trabajador _trabajadorAEditar;
        private string _archivoProfesionalBase64;
        private string _nombreDocumento = "Subir documento del título profesional";
        private string _nombre;
        private string _apellido;
        private string _especialidad;
        private string _correo;
        private string _telefono;
        private string _cedula;
        private bool _activo = true;
        private string _fotoBase64;
        private ImageSource _fotoPrevisualizacion;
        private string _tituloPagina = "Nuevo Trabajador";

        private ObservableCollection<HorarioTrabajoVm> _listaHorariosUI;

        public ICommand SubirDocumentoCommand { get; }
        public ICommand SeleccionarFotoCommand { get; }
        public ICommand GuardarTrabajadorCommand { get; }

        public string TituloPagina
        {
            get => _tituloPagina;
            set { _tituloPagina = value; OnPropertyChanged(); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        public string Apellido
        {
            get => _apellido;
            set { _apellido = value; OnPropertyChanged(); }
        }

        public string Especialidad
        {
            get => _especialidad;
            set { _especialidad = value; OnPropertyChanged(); }
        }

        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        public string Cedula
        {
            get => _cedula;
            set { _cedula = value; OnPropertyChanged(); }
        }

        public string NombreDocumento
        {
            get => _nombreDocumento;
            set { _nombreDocumento = value; OnPropertyChanged(); }
        }

        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        public ImageSource FotoPrevisualizacion
        {
            get => _fotoPrevisualizacion;
            set
            {
                _fotoPrevisualizacion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneFoto));
                OnPropertyChanged(nameof(NoTieneFoto));
            }
        }

        public bool TieneFoto => FotoPrevisualizacion != null;
        public bool NoTieneFoto => FotoPrevisualizacion == null;

        public ObservableCollection<HorarioTrabajoVm> ListaHorariosUI
        {
            get => _listaHorariosUI;
            set { _listaHorariosUI = value; OnPropertyChanged(); }
        }

        private bool _datosCargados;

        public Trabajador TrabajadorAEditar
        {
            get => _trabajadorAEditar;
            set
            {
                if (value == null || _datosCargados) return;

                _trabajadorAEditar = value;
                _datosCargados = true;
                OnPropertyChanged();
                CargarDatosTrabajador(value);
            }
        }

        public NuevoTrabajadorViewModel(TrabajadorService trabajadorService, NegocioService negocioService)
        {
            _trabajadorService = trabajadorService;
            _negocioService = negocioService;

            SubirDocumentoCommand = new Command(async () => await SubirDocumentoAsync());
            SeleccionarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            GuardarTrabajadorCommand = new Command(async () => await GuardarTrabajadorAsync());

            CargarHorariosEnUI();
        }

        private void CargarDatosTrabajador(Trabajador trabajador)
        {
            _trabajadorId = trabajador.Id;
            TituloPagina = "Editar Trabajador";

            if (!string.IsNullOrWhiteSpace(trabajador.Nombre))
            {
                var partes = trabajador.Nombre.Split(new[] { ' ' }, 2);
                Nombre = partes[0];
                Apellido = partes.Length > 1 ? partes[1] : string.Empty;
            }

            Especialidad = trabajador.Especialidad;
            Telefono = trabajador.Telefono;
            Correo = trabajador.Correo;
            Cedula = trabajador.Cedula;
            Activo = trabajador.Activo;
            _fotoBase64 = trabajador.Foto;
            _archivoProfesionalBase64 = trabajador.ArchivoProfesional;

            if (!string.IsNullOrEmpty(_archivoProfesionalBase64))
            {
                NombreDocumento = "Documento cargado (Toca para cambiar)";
            }

            if (!string.IsNullOrEmpty(_fotoBase64))
            {
                CargarImagenDesdeBase64(_fotoBase64);
            }

            CargarHorariosEnUI(trabajador.HorarioTrabajo);
        }

        private void CargarHorariosEnUI(List<HorarioTrabajo> horariosExistentes = null)
        {
            string[] diasSemana = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            ListaHorariosUI = new ObservableCollection<HorarioTrabajoVm>();

            foreach (var dia in diasSemana)
            {
                var coincidencia = horariosExistentes?.FirstOrDefault(h => h.Dia.Equals(dia, StringComparison.OrdinalIgnoreCase));

                if (coincidencia != null)
                {
                    ListaHorariosUI.Add(new HorarioTrabajoVm
                    {
                        Dia = dia,
                        EsLaboral = true,
                        HoraInicio = TimeSpan.TryParse(coincidencia.Inicio, out var ini) ? ini : new TimeSpan(8, 0, 0),
                        HoraFin = TimeSpan.TryParse(coincidencia.Fin, out var fin) ? fin : new TimeSpan(17, 0, 0)
                    });
                }
                else
                {
                    ListaHorariosUI.Add(new HorarioTrabajoVm
                    {
                        Dia = dia,
                        EsLaboral = (dia != "Domingo"),
                        HoraInicio = new TimeSpan(8, 0, 0),
                        HoraFin = new TimeSpan(17, 0, 0)
                    });
                }
            }
        }

        private async Task SubirDocumentoAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/pdf", "image/*" } },
                        { DevicePlatform.iOS, new[] { "com.adobe.pdf", "public.image" } },
                        { DevicePlatform.WinUI, new[] { ".pdf", ".jpg", ".jpeg", ".png" } }
                    });

                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona el Título o Certificado Profesional",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                    NombreDocumento = result.FileName;

                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] bytes = memoryStream.ToArray();

                    string mimeType = result.ContentType ?? "application/pdf";
                    _archivoProfesionalBase64 = $"data:{mimeType};base64,{Convert.ToBase64String(bytes)}";
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo seleccionar el archivo: {ex.Message}", "OK");
            }
        }

        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var resultado = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona la foto de perfil",
                    FileTypes = FilePickerFileType.Images
                });

                if (resultado != null)
                {
                    using var stream = await resultado.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    byte[] imageBytes = memoryStream.ToArray();
                    _fotoBase64 = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";

                    FotoPrevisualizacion = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            }
        }

        private void CargarImagenDesdeBase64(string base64String)
        {
            try
            {
                string pureBase64 = base64String.Contains(",")
                    ? base64String.Split(',')[1]
                    : base64String;

                byte[] imageBytes = Convert.FromBase64String(pureBase64);
                FotoPrevisualizacion = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch
            {
                FotoPrevisualizacion = null;
            }
        }

        private List<HorarioTrabajo> ObtenerHorariosParaMongo()
        {
            return ListaHorariosUI
                .Where(h => h.EsLaboral)
                .Select(h => new HorarioTrabajo
                {
                    Dia = h.Dia,
                    Inicio = h.HoraInicio.ToString(@"hh\:mm"),
                    Fin = h.HoraFin.ToString(@"hh\:mm")
                })
                .ToList();
        }

        /// <summary>
        /// Revisa los campos obligatorios (los que tienen * en la pantalla).
        /// Devuelve la lista de lo que falta o está mal (vacía si todo está correcto).
        /// Para quitar una obligatoriedad, borra su línea.
        /// </summary>
        private List<string> Validar()
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(Nombre))
                errores.Add("Nombre y apellido");

            if (string.IsNullOrWhiteSpace(Especialidad))
                errores.Add("Especialidad");

            if (string.IsNullOrWhiteSpace(Telefono))
                errores.Add("Número telefónico");

            if (string.IsNullOrWhiteSpace(Correo))
                errores.Add("Correo electrónico");
            else if (!CorreoValido(Correo.Trim()))
                errores.Add("Correo electrónico (formato no válido, ej. nombre@gmail.com)");

            if (string.IsNullOrWhiteSpace(Cedula))
                errores.Add("Número de cédula");

            if (string.IsNullOrWhiteSpace(_archivoProfesionalBase64))
                errores.Add("Título o certificado profesional");

            var diasLaborales = ListaHorariosUI.Where(h => h.EsLaboral).ToList();
            if (diasLaborales.Count == 0)
                errores.Add("Horario de trabajo (activa al menos un día)");
            else
                foreach (var h in diasLaborales.Where(h => h.HoraFin <= h.HoraInicio))
                    errores.Add($"Horario del {h.Dia} (la salida debe ser después de la entrada)");

            return errores;
        }

        private static bool CorreoValido(string correo)
        {
            try
            {
                var m = new System.Net.Mail.MailAddress(correo);
                return m.Address == correo && correo.Split('@').Last().Contains('.');
            }
            catch
            {
                return false;
            }
        }

        private async Task GuardarTrabajadorAsync()
        {
            var errores = Validar();
            if (errores.Count > 0)
            {
                await Shell.Current.DisplayAlert(
                    "Faltan datos",
                    "Completa los siguientes campos:\n\n• " + string.Join("\n• ", errores),
                    "OK");
                return;
            }

            try
            {

                string negocioIdActual = _trabajadorAEditar?.NegocioId ?? Preferences.Get("negocio_id", string.Empty);

                if (string.IsNullOrEmpty(negocioIdActual))
                {
                    string usuarioId = Preferences.Get("UsuarioId", string.Empty);
                    if (!string.IsNullOrEmpty(usuarioId))
                    {
                        var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(usuarioId);
                        negocioIdActual = negocio?.Id ?? string.Empty;
                        if (!string.IsNullOrEmpty(negocioIdActual))
                            Preferences.Set("negocio_id", negocioIdActual);
                    }
                }

                if (string.IsNullOrEmpty(negocioIdActual))
                {
                    await Shell.Current.DisplayAlert("Error de Sesión", "No se encontró ningún negocio asociado al usuario en sesión.", "OK");
                    return;
                }

                string nombreCompleto = string.IsNullOrWhiteSpace(Apellido)
                    ? Nombre.Trim()
                    : $"{Nombre.Trim()} {Apellido.Trim()}";

                var trabajador = new Trabajador
                {
                    Id = _trabajadorId,
                    NegocioId = negocioIdActual,
                    Nombre = nombreCompleto,
                    Especialidad = Especialidad.Trim(),
                    Correo = Correo.Trim(),
                    Telefono = Telefono.Trim(),
                    Cedula = Cedula.Trim(),
                    Foto = _fotoBase64 ?? string.Empty,
                    ArchivoProfesional = _archivoProfesionalBase64 ?? string.Empty,
                    Activo = Activo,
                    HorarioTrabajo = ObtenerHorariosParaMongo()
                };

                var dto = trabajador.ToDto();

                if (string.IsNullOrEmpty(_trabajadorId))
                    await _trabajadorService.CrearAsync(dto);
                else
                    await _trabajadorService.ActualizarAsync(dto);

                _trabajadorAEditar = null;

                await Shell.Current.DisplayAlert("Éxito", "Trabajador guardado correctamente.", "OK");

                if (Shell.Current.Navigation.NavigationStack.Count > 1)
                    await Shell.Current.Navigation.PopAsync();
                else
                    await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo guardar: {ApiException.MensajePara(ex)}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModels
{
    [QueryProperty(nameof(TrabajadorAEditar), "TrabajadorEditar")]
    public class NuevoTrabajadorViewModel : INotifyPropertyChanged
    {
        private readonly IMongoCollection<Trabajador> _trabajadoresCollection;
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

        public Trabajador TrabajadorAEditar
        {
            get => _trabajadorAEditar;
            set
            {
                _trabajadorAEditar = value;
                OnPropertyChanged();
                if (_trabajadorAEditar != null)
                {
                    CargarDatosTrabajador(_trabajadorAEditar);
                }
            }
        }

        public NuevoTrabajadorViewModel()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);
            _trabajadoresCollection = database.GetCollection<Trabajador>("Trabajadores");

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

        private async Task GuardarTrabajadorAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Correo) ||
                string.IsNullOrWhiteSpace(Telefono) ||
                string.IsNullOrWhiteSpace(Cedula) ||
                string.IsNullOrWhiteSpace(Especialidad))
            {
                await Shell.Current.DisplayAlert("Campos requeridos", "Debes rellenar todos los campos de información personal. No los dejes en blanco.", "OK");
                return;
            }

            string negocioIdActual = Preferences.Get("negocio_id", string.Empty);

            if (string.IsNullOrEmpty(negocioIdActual))
            {
                string usuarioId = Preferences.Get("UsuarioId", string.Empty);

                if (!string.IsNullOrEmpty(usuarioId) && MongoDB.Bson.ObjectId.TryParse(usuarioId, out var usuarioObjectId))
                {
                    try
                    {
                        var client = new MongoClient(MongoDbSettings.ConnectionString);
                        var database = client.GetDatabase(MongoDbSettings.DatabaseName);
                        var negociosCollection = database.GetCollection<MongoDB.Bson.BsonDocument>("Negocios");

                        var filtroNegocio = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("UsuarioId", usuarioObjectId);
                        var negocioDoc = await negociosCollection.Find(filtroNegocio).FirstOrDefaultAsync();

                        if (negocioDoc != null)
                        {
                            negocioIdActual = negocioDoc["_id"].ToString();
                            Preferences.Set("negocio_id", negocioIdActual);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR FETCH NEGOCIO] {ex.Message}");
                    }
                }
            }

            if (string.IsNullOrEmpty(negocioIdActual))
            {
                await Shell.Current.DisplayAlert("Error de Sesión", "No se encontró ningún negocio asociado al usuario en sesión.", "OK");
                return;
            }

            try
            {
                string nombreCompleto = string.IsNullOrWhiteSpace(Apellido)
                    ? Nombre.Trim()
                    : $"{Nombre.Trim()} {Apellido.Trim()}";

                var horariosConfigurados = ObtenerHorariosParaMongo();

                if (string.IsNullOrEmpty(_trabajadorId))
                {
                    var nuevoTrabajador = new Trabajador
                    {
                        NegocioId = negocioIdActual,
                        Nombre = nombreCompleto,
                        Especialidad = Especialidad.Trim(),
                        Correo = Correo.Trim(),
                        Telefono = Telefono.Trim(),
                        Cedula = Cedula.Trim(),
                        Foto = _fotoBase64 ?? string.Empty,
                        ArchivoProfesional = _archivoProfesionalBase64 ?? string.Empty,
                        Activo = Activo,
                        HorarioTrabajo = horariosConfigurados
                    };

                    await _trabajadoresCollection.InsertOneAsync(nuevoTrabajador);
                }
                else
                {
                    var filter = Builders<Trabajador>.Filter.Eq(t => t.Id, _trabajadorId);
                    var update = Builders<Trabajador>.Update
                        .Set(t => t.Nombre, nombreCompleto)
                        .Set(t => t.Especialidad, Especialidad.Trim())
                        .Set(t => t.Correo, Correo.Trim())
                        .Set(t => t.Telefono, Telefono.Trim())
                        .Set(t => t.Cedula, Cedula.Trim())
                        .Set(t => t.Foto, _fotoBase64 ?? string.Empty)
                        .Set(t => t.ArchivoProfesional, _archivoProfesionalBase64 ?? string.Empty)
                        .Set(t => t.Activo, Activo)
                        .Set(t => t.HorarioTrabajo, horariosConfigurados);

                    await _trabajadoresCollection.UpdateOneAsync(filter, update);
                }
                _trabajadorAEditar = null;

                await Shell.Current.DisplayAlert("Éxito", "Trabajador guardado correctamente.", "OK");

                if (Shell.Current.Navigation.NavigationStack.Count > 1)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo guardar en la base de datos: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
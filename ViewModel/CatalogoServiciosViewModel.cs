using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class CatalogoServiciosViewModel : BindableObject
    {
        private readonly IMongoCollection<Servicios> _coleccionServicios;
        private readonly IMongoCollection<Negocio> _coleccionNegocios;

        public ObservableCollection<Servicios> Servicios { get; set; } = new();

        private int _serviciosActivosCount;
        public int ServiciosActivosCount
        {
            get => _serviciosActivosCount;
            set { _serviciosActivosCount = value; OnPropertyChanged(); }
        }

        public ICommand CambiarEstadoActivoCommand { get; }
        public ICommand CreaServicioCommand { get; }
        public ICommand CargarServiciosCommand { get; }
        public ICommand EditarservicioCommand { get; }
        public ICommand CambiarFotoServicio { get; }

        public CatalogoServiciosViewModel()
        {
            var cliente = new MongoClient(MongoDbSettings.ConnectionString);
            var bd = cliente.GetDatabase(MongoDbSettings.DatabaseName);

            _coleccionServicios = bd.GetCollection<Servicios>("servicios");
            _coleccionNegocios = bd.GetCollection<Negocio>("Negocios");

            CambiarEstadoActivoCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await CambiarEstadoActivoAsync(servicio));
            CreaServicioCommand = new Command(async () => await CrearServicioAsync());
            CargarServiciosCommand = new Command(async () => await CargarServiciosAsync());
            EditarservicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await EditarServicioAsync(servicio));
            CambiarFotoServicio = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await SeleccionarYGuardarFotoAsync(servicio));
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

            var filtroNegocio = Builders<Negocio>.Filter.Eq(n => n.UsuarioId, usuarioIdSesion);

            var negocio = await _coleccionNegocios.Find(filtroNegocio).FirstOrDefaultAsync();

            if (negocio == null || string.IsNullOrEmpty(negocio.Id))
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "No se encontró ningún negocio registrado para este usuario.", "OK");
                return null;
            }

            return negocio.Id;
        }

        public async Task CargarServiciosAsync()
        {
            try
            {
                string? negocioId = await ObtenerNegocioIdDeSesionAsync();
                if (string.IsNullOrEmpty(negocioId)) return;

                var filtroServicios = Builders<Servicios>.Filter.Eq(s => s.NegocioId, negocioId);

                var resultados = await _coleccionServicios.Find(filtroServicios).ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Servicios.Clear();
                    foreach (var serv in resultados)
                    {
                        Servicios.Add(serv);
                    }
                    ServiciosActivosCount = Servicios.Count(s => s.Activo);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar servicios: {ex.Message}", "OK");
            }
        }

        public async Task CargarservicioClienteAsync()
        {
            try
            {
                string? negocioId = await ObtenerNegocioIdDeSesionAsync();
                if (string.IsNullOrEmpty(negocioId)) return;

                FilterDefinition<Servicios> filtroNegocio;

                if (ObjectId.TryParse(negocioId, out ObjectId negocioObjectId))
                {
                    filtroNegocio = Builders<Servicios>.Filter.Or(
                        Builders<Servicios>.Filter.Eq("negocio_id", negocioObjectId),
                        Builders<Servicios>.Filter.Eq(s => s.NegocioId, negocioId)
                    );
                }
                else
                {
                    filtroNegocio = Builders<Servicios>.Filter.Eq(s => s.NegocioId, negocioId);
                }

                var filtroActivo = Builders<Servicios>.Filter.Eq(s => s.Activo, true);
                var filtroFinal = Builders<Servicios>.Filter.And(filtroNegocio, filtroActivo);

                var resultados = await _coleccionServicios.Find(filtroFinal).ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Servicios.Clear();
                    foreach (var serv in resultados)
                    {
                        Servicios.Add(serv);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar servicios activos: {ex.Message}", "OK");
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

            try
            {
                var filtro = Builders<Servicios>.Filter.Eq(s => s.Id, servicio.Id);
                var actualizacion = Builders<Servicios>.Update.Set(s => s.Activo, servicio.Activo);

                await _coleccionServicios.UpdateOneAsync(filtro, actualizacion);

                ServiciosActivosCount = Servicios.Count(s => s.Activo);
            }
            catch (Exception ex)
            {
                servicio.Activo = !servicio.Activo;
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar: {ex.Message}", "OK");
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

            var paginaCreacion = new Pages.EditarServicio(null, _coleccionServicios, negocioId);
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

                    var filtro = Builders<Servicios>.Filter.Eq(s => s.Id, servicio.Id);
                    var actualizacion = Builders<Servicios>.Update.Set(s => s.Foto, base64Imagen);

                    await _coleccionServicios.UpdateOneAsync(filtro, actualizacion);

                    await CargarServiciosAsync();
                    await Application.Current.MainPage.DisplayAlert("Éxito", "La imagen del servicio fue actualizada correctamente.", "OK");
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

            var paginaEdicion = new Pages.EditarServicio(servicio, _coleccionServicios);
            await Application.Current.MainPage.Navigation.PushModalAsync(paginaEdicion);
        }
    }
}
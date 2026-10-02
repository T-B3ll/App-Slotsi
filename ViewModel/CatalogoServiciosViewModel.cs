using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiKeyboard = Microsoft.Maui.Keyboard;


namespace slotsi_citas.ViewModel
{
    public class CatalogoServiciosViewModel : BindableObject
    {
        private readonly IMongoCollection<Servicios> _coleccionServicios;

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

            CambiarEstadoActivoCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await CambiarEstadoActivoAsync(servicio));
            CreaServicioCommand = new Microsoft.Maui.Controls.Command(async () => await CrearServicioAsync());
            CargarServiciosCommand = new Command(async () => await CargarServiciosAsync());
            EditarservicioCommand = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await EditarServicioAsync(servicio));
            CambiarFotoServicio = new Microsoft.Maui.Controls.Command<Servicios>(async (servicio) => await SeleccionarYGuardarFotoAsync(servicio)); ;
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

        public async Task CargarservicioClienteAsync()
        {
            try
            {
                var filtroactivo = Builders<Servicios>.Filter.Eq(s => s.Activo, true);
                var resultados = await _coleccionServicios.Find(filtroactivo).ToListAsync();

                Servicios.Clear();
                foreach (var serv in resultados)
                {
                    Servicios.Add(serv);
                }
            }
            catch ( Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al cargar servicios activos: {ex.Message}", "OK");
            }
        }

        public async Task CrearServicioAsync()
        {
            var paginaCreacion = new Pages.EditarServicio(null, _coleccionServicios);
            await Application.Current.MainPage.Navigation.PushModalAsync(paginaCreacion);

            await CargarServiciosAsync();
        }

        public async Task CargarServiciosAsync()
        {
            try
            {
                var resultados = await _coleccionServicios.Find(_ => true).ToListAsync();
                Servicios.Clear();

                foreach (var serv in resultados)
                {
                    Servicios.Add(serv);
                }

                ServiciosActivosCount = Servicios.Count(s => s.Activo);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
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

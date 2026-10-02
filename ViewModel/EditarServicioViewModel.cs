using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class EditarServicioViewModel: BindableObject
    {
        private readonly IMongoCollection<Servicios> _coleccionServicios;
        private readonly Servicios _servicioOriginal;
        private readonly bool _esNuevo;

        public string TituloPantalla => _esNuevo ? "Nuevo Servicio" : "Editar Servicio";
        public string TextoBotonGuardar => _esNuevo ? "Crear Servicio" : "Guardar Cambios";

        public string Id { get; set; }

        private string _nombre;
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(); } }

        private decimal _precio;
        public decimal Precio { get => _precio; set { _precio = value; OnPropertyChanged(); } }

        private int _duracionMinutos = 30;
        public int DuracionMinutos { get => _duracionMinutos; set { _duracionMinutos = value; OnPropertyChanged(); } }

        private string _categoria;
        public string Categoria { get => _categoria; set { _categoria = value; OnPropertyChanged(); } }

        private string _descripcion;
        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

        private bool _disponibleDomicilio;
        public bool DisponibleDomicilio { get => _disponibleDomicilio; set { _disponibleDomicilio = value; OnPropertyChanged(); } }

        private string _foto;
        public string Foto { get => _foto; set { _foto = value; OnPropertyChanged(); } }

        public ICommand CambiarFotoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarServicioViewModel(Servicios servicio, IMongoCollection<Servicios> coleccion)
        {
            _coleccionServicios = coleccion;
            _servicioOriginal = servicio;
            _esNuevo = (servicio == null);

            if (!_esNuevo)
            {
                Id = servicio.Id;
                Nombre = servicio.Nombre;
                Precio = servicio.Precio;
                DuracionMinutos = servicio.DuracionMinutos;
                Categoria = servicio.Categoria;
                Descripcion = servicio.Descripcion;
                DisponibleDomicilio = servicio.DisponibleDomicilio;
                Foto = servicio.Foto;
            }

            CambiarFotoCommand = new Microsoft.Maui.Controls.Command(async () => await SeleccionarFotoAsync());
            GuardarCommand = new Microsoft.Maui.Controls.Command(async () => await GuardarCambiosAsync());
            CancelarCommand = new Microsoft.Maui.Controls.Command(async () => await Application.Current.MainPage.Navigation.PopModalAsync());
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

        private async Task GuardarCambiosAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "El nombre del servicio es obligatorio.", "OK");
                return;
            }

            try
            {
                if (_esNuevo)
                {
                    var nuevoServicio = new Servicios
                    {
                        Nombre = Nombre,
                        Precio = Precio,
                        DuracionMinutos = DuracionMinutos,
                        Categoria = Categoria,
                        Descripcion = Descripcion,
                        DisponibleDomicilio = DisponibleDomicilio,
                        Foto = Foto,
                        Activo = true,
                        ColaboradoresHabilitados = new List<ObjectId>()
                    };

                    await _coleccionServicios.InsertOneAsync(nuevoServicio);
                }
                else
                {
                    var filtro = Builders<Servicios>.Filter.Eq(s => s.Id, Id);
                    var actualizacion = Builders<Servicios>.Update
                        .Set(s => s.Nombre, Nombre)
                        .Set(s => s.Precio, Precio)
                        .Set(s => s.DuracionMinutos, DuracionMinutos)
                        .Set(s => s.Categoria, Categoria)
                        .Set(s => s.Descripcion, Descripcion)
                        .Set(s => s.DisponibleDomicilio, DisponibleDomicilio)
                        .Set(s => s.Foto, Foto);

                    await _coleccionServicios.UpdateOneAsync(filtro, actualizacion);

                    _servicioOriginal.Nombre = Nombre;
                    _servicioOriginal.Precio = Precio;
                    _servicioOriginal.DuracionMinutos = DuracionMinutos;
                    _servicioOriginal.Categoria = Categoria;
                    _servicioOriginal.Descripcion = Descripcion;
                    _servicioOriginal.DisponibleDomicilio = DisponibleDomicilio;
                    _servicioOriginal.Foto = Foto;
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

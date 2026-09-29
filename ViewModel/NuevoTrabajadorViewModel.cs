using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModels
{
    public class NuevoTrabajadorViewModel : INotifyPropertyChanged
    {
        private readonly IMongoCollection<Trabajador> _trabajadoresCollection;

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Especialidad { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Cedula { get; set; }

        private string _fotoBase64;
        private ImageSource _fotoPrevisualizacion;

        public ImageSource FotoPrevisualizacion
        {
            get => _fotoPrevisualizacion;
            set { _fotoPrevisualizacion = value; OnPropertyChanged(); }
        }

        public ICommand SeleccionarFotoCommand { get; }
        public ICommand GuardarTrabajadorCommand { get; }

        public NuevoTrabajadorViewModel()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);
            _trabajadoresCollection = database.GetCollection<Trabajador>("Trabajadores");

            SeleccionarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            GuardarTrabajadorCommand = new Command(async () => await GuardarTrabajadorAsync());
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

        private async Task GuardarTrabajadorAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Correo))
            {
                await Shell.Current.DisplayAlert("Validación", "Completa los campos requeridos.", "OK");
                return;
            }

            try
            {
                // Crear el horario estándar de 6 días (Array (6))
                var horariosPorDefecto = new List<HorarioTrabajo>
                {
                    new HorarioTrabajo { Dia = "Lunes", Inicio = "08:00", Fin = "17:00" },
                    new HorarioTrabajo { Dia = "Martes", Inicio = "08:00", Fin = "17:00" },
                    new HorarioTrabajo { Dia = "Miércoles", Inicio = "08:00", Fin = "17:00" },
                    new HorarioTrabajo { Dia = "Jueves", Inicio = "08:00", Fin = "17:00" },
                    new HorarioTrabajo { Dia = "Viernes", Inicio = "08:00", Fin = "17:00" },
                    new HorarioTrabajo { Dia = "Sábado", Inicio = "08:00", Fin = "17:00" }
                };

                var nuevoTrabajador = new Trabajador
                {
     
                    NegocioId = "6ab59dce7557e344b191bd43",
                    Nombre = string.IsNullOrWhiteSpace(Apellido) ? Nombre : $"{Nombre} {Apellido}".Trim(),
                    Especialidad = string.IsNullOrWhiteSpace(Especialidad) ? "Barbero Principal" : Especialidad,
                    Correo = Correo,
                    HorarioTrabajo = horariosPorDefecto,
                    Foto = _fotoBase64, // Cadena Base64 de la imagen seleccionada desde la computadora
                    Activo = true
                };

                await _trabajadoresCollection.InsertOneAsync(nuevoTrabajador);

                await Shell.Current.DisplayAlert("Éxito", "Trabajador guardado correctamente en MongoDB Atlas.", "OK");
                await Shell.Current.GoToAsync("..");
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
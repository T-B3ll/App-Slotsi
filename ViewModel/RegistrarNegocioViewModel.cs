using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class RegistrarNegocioViewModel : INotifyPropertyChanged
    {

        private readonly SupabaseStorageService _storageService;
        private readonly NegocioService _negocioService;


        private string _nombre = string.Empty;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        private string _correo = string.Empty;
        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        private string _telefono = string.Empty;
        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        private string _cedulaJuridica = string.Empty;
        public string CedulaJuridica
        {
            get => _cedulaJuridica;
            set { _cedulaJuridica = value; OnPropertyChanged(); }
        }

        private string _numeroRuc = string.Empty;
        public string NumeroRuc
        {
            get => _numeroRuc;
            set { _numeroRuc = value; OnPropertyChanged(); }
        }

        private string _direccion = string.Empty;
        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); }
        }

        private string _tipoServicio = "Sede"; 
        public string TipoServicio
        {
            get => _tipoServicio;
            set { _tipoServicio = value; OnPropertyChanged(); }
        }

        private string _urlFotoPerfil = string.Empty;
        public string UrlFotoPerfil
        {
            get => _urlFotoPerfil;
            set { _urlFotoPerfil = value; OnPropertyChanged(); }
        }

        private string _urlDocumentoTitulo = string.Empty;
        public string UrlDocumentoTitulo
        {
            get => _urlDocumentoTitulo;
            set { _urlDocumentoTitulo = value; OnPropertyChanged(); }
        }

        public Command SeleccionarFotoCommand { get; }
        public Command SeleccionarDocumentoCommand { get; }
        public Command RegistrarCommand { get; }


        public RegistrarNegocioViewModel()
        {
            _storageService = new SupabaseStorageService();
            _negocioService = new NegocioService();

         
            SeleccionarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            SeleccionarDocumentoCommand = new Command(async () => await SeleccionarDocumentoAsync());
            RegistrarCommand = new Command(async () => await RegistrarNegocioAsync());
        }



        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona foto de perfil",
                    FileTypes = FilePickerFileType.Images
                });

                if (file != null)
                {
                    
                    await Application.Current.MainPage.DisplayAlert("Subiendo...", "Por favor espera mientras se sube la foto.", "OK");

                    using var stream = await file.OpenReadAsync();
                    string fileName = $"perfil_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                   
                    UrlFotoPerfil = await _storageService.SubirArchivoAsync(stream, "perfiles", fileName);

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Foto subida correctamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo subir la foto: {ex.Message}", "OK");
            }
        }



        private async Task SeleccionarDocumentoAsync()
        {
            try
            {
               
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } }
                });

                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona documento PDF (Título/RUC)",
                    FileTypes = customFileType
                });

                if (file != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Subiendo...", "Por favor espera mientras se sube el documento.", "OK");

                    using var stream = await file.OpenReadAsync();
                    string fileName = $"doc_{Guid.NewGuid()}.pdf";

                  
                    UrlDocumentoTitulo = await _storageService.SubirArchivoAsync(stream, "documentos", fileName);

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Documento subido correctamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo subir el documento: {ex.Message}", "OK");
            }
        }


        private async Task RegistrarNegocioAsync()
        {
            // 1. Validaciones básicas
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Correo))
            {
                await Application.Current.MainPage.DisplayAlert("Campos obligatorios", "Nombre y Correo son requeridos.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(UrlFotoPerfil) || string.IsNullOrEmpty(UrlDocumentoTitulo))
            {
                await Application.Current.MainPage.DisplayAlert("Archivos faltantes", "Debes subir la foto de perfil y el documento PDF antes de registrar.", "OK");
                return;
            }

            try
            {
                
                var nuevoNegocio = new Negocio
                {
                    Nombre = Nombre,
                    Correo = Correo,
                    Telefono = Telefono,
                    CedulaJuridica = CedulaJuridica,
                    NumeroRuc = NumeroRuc,
                    DireccionNegocio = Direccion,
                    TipoServicio = TipoServicio,

                    
                    UrlFotoPerfil = UrlFotoPerfil,
                    UrlDocumentoTitulo = UrlDocumentoTitulo,

                  
                    TipoUsuario = true, 
                    EstaActivo = true,
                    FechaRegistro = DateTime.UtcNow
                };

     
                await _negocioService.CrearAsync(nuevoNegocio);

                await Application.Current.MainPage.DisplayAlert("¡Registro Exitoso!", "Tu negocio ha sido registrado correctamente.", "OK");

             

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error de Registro", ex.Message, "OK");
            }
        }

      
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    }


}

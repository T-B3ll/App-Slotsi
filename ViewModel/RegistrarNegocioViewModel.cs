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
        private readonly UsuarioService _usuarioService;


        private Usuario _usuarioData = new Usuario();
        private Negocio _negocioData = new Negocio();

      
        public Command SeleccionarFotoCommand { get; }
        public Command SeleccionarDocumentoCommand { get; }
        public Command RegistrarCommand { get; }

        public RegistrarNegocioViewModel()
        {
            _storageService = new SupabaseStorageService();
            _negocioService = new NegocioService();
            _usuarioService = new UsuarioService();

            SeleccionarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            SeleccionarDocumentoCommand = new Command(async () => await SeleccionarDocumentoAsync());
            RegistrarCommand = new Command(async () => await RegistrarNegocioAsync());
        }

    
        public string Nombre
        {
            get => _usuarioData.NombreCompleto;
            set { _usuarioData.NombreCompleto = value; OnPropertyChanged(); }
        }
        public string Correo
        {
            get => _usuarioData.Correo;
            set { _usuarioData.Correo = value; OnPropertyChanged(); }
        }
        public string Telefono
        {
            get => _usuarioData.Telefono;
            set { _usuarioData.Telefono = value; OnPropertyChanged(); }
        }
        public string CedulaJuridica
        {
            get => _usuarioData.Cedula;
            set { _usuarioData.Cedula = value; OnPropertyChanged(); }
        }
        public string Contrasena
        {
            get => _usuarioData.Contrasena;
            set { _usuarioData.Contrasena = value; OnPropertyChanged(); }
        }

        
        public string NumeroRuc
        {
            get => _negocioData.NumeroRuc;
            set { _negocioData.NumeroRuc = value; OnPropertyChanged(); }
        }
        public string Direccion
        {
            get => _negocioData.DireccionNegocio;
            set { _negocioData.DireccionNegocio = value; OnPropertyChanged(); }
        }
        public string TipoServicio
        {
            get => _negocioData.TipoServicio;
            set { _negocioData.TipoServicio = value; OnPropertyChanged(); }
        }
        public string UrlFotoPerfil
        {
            get => _negocioData.UrlFotoPerfil;
            set { _negocioData.UrlFotoPerfil = value; OnPropertyChanged(); }
        }
        public string UrlDocumentoTitulo
        {
            get => _negocioData.UrlDocumentoTitulo;
            set { _negocioData.UrlDocumentoTitulo = value; OnPropertyChanged(); }
        }

       
        private async Task RegistrarNegocioAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Contrasena))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Faltan datos obligatorios", "OK");
                return;
            }
            if (string.IsNullOrEmpty(UrlFotoPerfil) || string.IsNullOrEmpty(UrlDocumentoTitulo))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debes subir foto y documento", "OK");
                return;
            }

            try
            {
             
                _usuarioData.TipoUsuario = true;
                _usuarioData.EstaActivo = true;
                await _usuarioService.CrearAsync(_usuarioData);

                if (string.IsNullOrEmpty(_usuarioData.Id))
                    throw new Exception("Error al generar ID de usuario.");

       
                _negocioData.UsuarioId = _usuarioData.Id;
                _negocioData.FechaRegistro = DateTime.UtcNow;
                await _negocioService.CrearAsync(_negocioData);

               
                await Application.Current.MainPage.DisplayAlert("Éxito", "Cuenta creada correctamente", "OK");

                
                await Application.Current.MainPage.Navigation.PopToRootAsync();

           
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }

        }


        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Selecciona foto", FileTypes = FilePickerFileType.Images });
                if (file != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Subiendo...", "Espera.", "OK");
                    using var stream = await file.OpenReadAsync();
                    string fileName = $"perfil_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    UrlFotoPerfil = await _storageService.SubirArchivoAsync(stream, "perfiles", fileName);
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Foto subida.", "OK");
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async Task SeleccionarDocumentoAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.Android, new[] { "application/pdf" } }, { DevicePlatform.iOS, new[] { "com.adobe.pdf" } } });
                var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Selecciona PDF", FileTypes = customFileType });
                if (file != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Subiendo...", "Espera.", "OK");
                    using var stream = await file.OpenReadAsync();
                    string fileName = $"doc_{Guid.NewGuid()}.pdf";
                    UrlDocumentoTitulo = await _storageService.SubirArchivoAsync(stream, "documentos", fileName);
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Doc subido.", "OK");
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
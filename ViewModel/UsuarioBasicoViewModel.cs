using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using slotsi_citas.Models; 
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel 
{
    public class UsuarioBasicoViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;

        private string _nombreCompleto = string.Empty;


        private Negocio _negocioData = new Negocio();
        private Usuario _usuarioData = new Usuario();
        public string NombreCompleto
        {
            get => _nombreCompleto;
            set { _nombreCompleto = value; OnPropertyChanged(); }
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

        private string _cedula = string.Empty;
        public string Cedula
        {
            get => _cedula;
            set { _cedula = value; OnPropertyChanged(); }
        }

        private string _contrasena = string.Empty;
        public string Contrasena
        {
            get => _contrasena;
            set { _contrasena = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }
        public string CategoriaNegocio
        {
            get => _negocioData.Categoria;
            set { _negocioData.Categoria = value; OnPropertyChanged(); }
        }
        public ICommand RegistrarCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UsuarioBasicoViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;

            RegistrarCommand = new Command(async () =>
            {
                var resultado = await RegistrarUsuarioAsync();

                if (resultado.Exito)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", resultado.Mensaje, "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", resultado.Mensaje, "OK");
                }
            });
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto) ||
                string.IsNullOrWhiteSpace(Correo) ||
                string.IsNullOrWhiteSpace(Contrasena))
            {
                return (false, "Por favor completa los campos obligatorios (*).");
            }

            if (Contrasena.Length < 8)
            {
                return (false, "La contraseña debe tener al menos 8 caracteres.");
            }

            EstaCargando = true;

            try
            {
                var usuarioExistente = await _usuarioService.ObtenerPorCorreoAsync(Correo);
                if (usuarioExistente != null)
                {
                    return (false, "Este correo electrónico ya está registrado.");
                }

                var nuevoUsuario = new Usuario
                {
                    NombreCompleto = NombreCompleto.Trim(),
                    Correo = Correo.Trim().ToLower(),
                    Telefono = Telefono.Trim(),
                    Cedula = Cedula.Trim(),
                    Contrasena = Contrasena,
                    TipoUsuario = false,
                    EstaActivo = true
                };

                await _usuarioService.CrearAsync(nuevoUsuario);

                return (true, "¡Cuenta creada exitosamente!");
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }
    }
}
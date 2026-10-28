using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class RegistroUsuariosViewModel : BindableObject
    {
        private readonly UsuarioService _usuarioService;
        private List<Usuario> _listaCompletaUsuarios = new();
        private string _textoBusqueda = string.Empty;
        private string? _letraSeleccionada = null;

        private bool _cargandoUsuarios = false;
        private readonly HashSet<string> _usuariosEnActualizacion = new();

        public ObservableCollection<Usuario> Usuarios { get; set; }
        public ObservableCollection<InicialLetraModel> Iniciales { get; set; }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (_textoBusqueda != value)
                {
                    _textoBusqueda = value;
                    OnPropertyChanged();
                    AplicarFiltros();
                }
            }
        }

        public ICommand RegresarCommand { get; }
        public ICommand RegistrarNuevoCommand { get; }
        public ICommand CambiarEstadoCommand { get; }
        public ICommand VerDetallesUsuarioCommand { get; }

        // Constructor con inyección de dependencias
        public RegistroUsuariosViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            Usuarios = new ObservableCollection<Usuario>();
            Iniciales = new ObservableCollection<InicialLetraModel>();

            GenerarAlfabeto();

            RegresarCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.Navigation.PopAsync();
            });

            RegistrarNuevoCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Slotsi", "Registrar nuevo usuario seleccionado", "OK");
            });

            CambiarEstadoCommand = new Command<Usuario>(OnCambiarEstadoComando);
            VerDetallesUsuarioCommand = new Command<Usuario>(OnVerDetallesUsuarioComando);
        }

        public async Task CargarUsuariosBDAsync()
        {
            if (_cargandoUsuarios) return;
            _cargandoUsuarios = true;

            try
            {
                // Llamamos a la API en segundo plano
                var datos = await Task.Run(async () => await _usuarioService.ObtenerTodosAsync());
                _listaCompletaUsuarios = datos ?? new List<Usuario>();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AplicarFiltros();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CARGAR USUARIOS]: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los usuarios: {ex.Message}", "OK");
                });
            }
            finally
            {
                _cargandoUsuarios = false;
            }
        }

        private async void OnCambiarEstadoComando(Usuario? usuario)
        {
            if (usuario == null || string.IsNullOrEmpty(usuario.Id)) return;
            if (_usuariosEnActualizacion.Contains(usuario.Id)) return;

            _usuariosEnActualizacion.Add(usuario.Id);

            try
            {
                bool nuevoEstado = !usuario.EstaActivo;

                string accion = nuevoEstado ? "activar" : "desactivar";
                string titulo = nuevoEstado ? "Activar Usuario" : "Desactivar Usuario";
                string mensaje = $"¿Estás seguro de que deseas {accion} a {usuario.NombreCompleto}?";

                bool confirmar = await Application.Current.MainPage.DisplayAlert(titulo, mensaje, "Sí", "Cancelar");

                if (!confirmar) return;

                // Llamamos a la API
                await Task.Run(async () => await _usuarioService.CambiarEstadoAsync(usuario.Id, nuevoEstado));

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    usuario.EstaActivo = nuevoEstado;
                    var index = Usuarios.IndexOf(usuario);
                    if (index >= 0)
                    {
                        Usuarios[index] = usuario;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR CAMBIAR ESTADO]: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error al actualizar", ex.Message, "OK");
                });
            }
            finally
            {
                _usuariosEnActualizacion.Remove(usuario.Id);
            }
        }

        private async void OnVerDetallesUsuarioComando(Usuario? usuario)
        {
            if (usuario == null || Application.Current?.MainPage == null) return;

            string tipoStr = usuario.TipoUsuario ? "Administrador / Personal" : "Cliente / Usuario";
            string estadoStr = usuario.EstaActivo ? "Activo" : "Inactivo";

            string mensaje = $"ID: {usuario.Id}\n\n" +
                             $"Nombre: {usuario.NombreCompleto}\n" +
                             $"Correo: {usuario.Correo}\n" +
                             $"Teléfono: {usuario.Telefono}\n" +
                             $"Cédula: {usuario.Cedula}\n" +
                             $"Tipo de Usuario: {tipoStr}\n" +
                             $"Estado: {estadoStr}";

            await Application.Current.MainPage.DisplayAlert("Detalles del Usuario", mensaje, "Cerrar");
        }

        private void GenerarAlfabeto()
        {
            Iniciales.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Iniciales.Add(new InicialLetraModel
                {
                    Letra = c.ToString(),
                    EsSeleccionada = false
                });
            }
        }

        public void OnFiltrarPorInicial(InicialLetraModel? item)
        {
            if (item == null) return;

            if (_letraSeleccionada == item.Letra)
                _letraSeleccionada = null;
            else
                _letraSeleccionada = item.Letra;

            foreach (var inicial in Iniciales)
            {
                inicial.EsSeleccionada = (inicial.Letra == _letraSeleccionada);
            }

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            var resultado = _listaCompletaUsuarios.AsEnumerable();

            if (!string.IsNullOrEmpty(_letraSeleccionada))
            {
                resultado = resultado.Where(u => !string.IsNullOrEmpty(u.NombreCompleto) &&
                                                 u.NombreCompleto.TrimStart().StartsWith(_letraSeleccionada, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string busqueda = TextoBusqueda.Trim();
                resultado = resultado.Where(u =>
                    (!string.IsNullOrEmpty(u.NombreCompleto) && u.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Correo) && u.Correo.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Cedula) && u.Cedula.Contains(busqueda, StringComparison.OrdinalIgnoreCase)));
            }

            var listaFiltrada = resultado.ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Usuarios.Clear();
                foreach (var usr in listaFiltrada)
                {
                    Usuarios.Add(usr);
                }
            });
        }
    }
}
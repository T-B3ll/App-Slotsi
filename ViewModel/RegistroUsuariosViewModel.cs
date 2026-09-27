using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel; // <--- NECESARIO PARA MainThread
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class RegistroUsuariosViewModel : BindableObject
    {
        private readonly MongoService _mongoService;
        private List<Usuario> _listaCompletaUsuarios = new();
        private string _textoBusqueda = string.Empty;
        private string? _letraSeleccionada = null;

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
        public ICommand VerDetallesCommand { get; }
        public ICommand ImportarListaCommand { get; }
        public ICommand RegistrarNuevoCommand { get; }

        public RegistroUsuariosViewModel()
        {
            _mongoService = new MongoService();
            Usuarios = new ObservableCollection<Usuario>();
            Iniciales = new ObservableCollection<InicialLetraModel>();

            GenerarAlfabeto();

            RegresarCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.Navigation.PopAsync();
            });

            VerDetallesCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Slotsi", "Ver detalles seleccionado", "OK");
            });

            ImportarListaCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Slotsi", "Importar lista seleccionado", "OK");
            });

            RegistrarNuevoCommand = new Command(async () =>
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Slotsi", "Registrar nuevo usuario seleccionado", "OK");
            });
        }

        public async Task CargarUsuariosBDAsync()
        {
            try
            {
                var datos = await _mongoService.ObtenerUsuariosAsync();
                _listaCompletaUsuarios = datos ?? new List<Usuario>();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AplicarFiltros();
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error MongoDB", ex.Message, "OK");
                });
            }
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
            {
                _letraSeleccionada = null;
            }
            else
            {
                _letraSeleccionada = item.Letra;
            }

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

            Usuarios.Clear();
            foreach (var usr in resultado)
            {
                Usuarios.Add(usr);
            }
        }
    }
}
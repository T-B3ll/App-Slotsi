using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class EditarPerfilViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private readonly NegocioService _negocioService;
        private readonly SupabaseStorageService _storageService;

        private Usuario _usuario;
        private bool _esDueno;
        private bool _isBusy;
        private Negocio _negocioActual;

        // --- DATOS PERSONALES ---
        public Usuario Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        // --- DATOS DEL NEGOCIO (Propiedades individuales para Binding directo) ---
        private string _nombreNegocio;
        public string NombreNegocio
        {
            get => _nombreNegocio;
            set { _nombreNegocio = value; OnPropertyChanged(); }
        }

        private string _direccion;
        public string Direccion
        {
            get => _direccion;
            set { _direccion = value; OnPropertyChanged(); }
        }

        private string _ruc;
        public string Ruc
        {
            get => _ruc;
            set { _ruc = value; OnPropertyChanged(); }
        }

        // --- HORARIOS ---
        public ObservableCollection<HorarioTrabajoVm> ListaHorarios { get; set; }
            = new ObservableCollection<HorarioTrabajoVm>();

        // --- CONTROLADORES UI ---
        public bool EsDuenoNegocio => _esDueno;
        public bool PuedeEditar => true;

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string TituloPagina => _esDueno ? "Editar Perfil de Negocio" : "Mi Perfil";

        // --- COMANDOS ---
        public Command GuardarCambiosCommand { get; }
        public Command SeleccionarFotoCommand { get; }
        public Command SubirPdfCommand { get; }

        public EditarPerfilViewModel(UsuarioService usuarioService, NegocioService negocioService)
        {
            _usuarioService = usuarioService;
            _negocioService = negocioService;
            _storageService = new SupabaseStorageService();

            GuardarCambiosCommand = new Command(async () => await GuardarCambiosAsync());
            SeleccionarFotoCommand = new Command(async () => await SeleccionarFotoAsync());
            SubirPdfCommand = new Command(async () => await SubirPdfAsync());

            CargarDatosAsync();
        }

        private async void CargarDatosAsync()
        {
            IsBusy = true;
            try
            {
                var idUsuario = Preferences.Get("UsuarioId", string.Empty);
                _esDueno = Preferences.Get("EsDuenoNegocio", false);

                if (!string.IsNullOrEmpty(idUsuario))
                {
                    // 1. CARGAR USUARIO
                    var usuarios = await _usuarioService.ObtenerTodosAsync();
                    var usuarioEncontrado = usuarios.FirstOrDefault(u => u.Id == idUsuario);
                    if (usuarioEncontrado != null) Usuario = usuarioEncontrado;

                    // 2. CARGAR NEGOCIO Y HORARIOS
                    if (_esDueno)
                    {
                        var todosLosNegocios = await _negocioService.ObtenerTodosAsync();
                        _negocioActual = todosLosNegocios.FirstOrDefault(n => n.UsuarioId == idUsuario);

                        if (_negocioActual != null)
                        {
                            // Asignar valores a las propiedades individuales
                            NombreNegocio = _negocioActual.NombreNegocio;
                            Direccion = _negocioActual.DireccionNegocio;
                            Ruc = _negocioActual.NumeroRuc;

                            // Cargar horarios desde el modelo
                            CargarHorariosDesdeModelo(_negocioActual);
                        }
                    }

                    // Notificar cambios a la UI
                    OnPropertyChanged(nameof(EsDuenoNegocio));
                    OnPropertyChanged(nameof(TituloPagina));
                    // Las propiedades NombreNegocio, Direccion, Ruc ya notifican en su setter
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ✅ MÉTODO DE GUARDADO CORREGIDO
        private async Task GuardarCambiosAsync()
        {
            if (IsBusy || _usuario == null) return;
            IsBusy = true;

            try
            {
                // 1. Actualizar datos básicos del usuario
                await _usuarioService.ActualizarAsync(_usuario);

                // 2. Actualizar negocio y reconstruir horarios si es dueño
                if (_esDueno && _negocioActual != null)
                {
                    _negocioActual.NombreNegocio = NombreNegocio;
                    _negocioActual.DireccionNegocio = Direccion;
                    _negocioActual.NumeroRuc = Ruc;

                    // Reconstruir diccionario de horarios desde la lista editable
                    _negocioActual.Horarios = new Dictionary<string, Negocio.HorarioDia>();
                    _negocioActual.DiasCerrados = new List<string>();

                    var mapInverso = new Dictionary<string, string>
                    {
                        {"Lunes","Lun"}, {"Martes","Mar"}, {"Miércoles","Mie"},
                        {"Jueves","Jue"}, {"Viernes","Vie"}, {"Sábado","Sab"}, {"Domingo","Dom"}
                    };

                    foreach (var item in ListaHorarios)
                    {
                        if (mapInverso.TryGetValue(item.Dia, out string clave))
                        {
                            if (item.EsLaboral)
                            {
                                // Guardar horario (sin pausas por ahora)
                                _negocioActual.Horarios[clave] = new Negocio.HorarioDia
                                {
                                    Apertura = item.HoraInicio.ToString(@"hh\:mm"),
                                    Cierre = item.HoraFin.ToString(@"hh\:mm"),
                                    Pausas = new List<string>()
                                };
                            }
                            else
                            {
                                // Si NO está marcado, va a la lista de cerrados
                                _negocioActual.DiasCerrados.Add(clave);
                            }
                        }
                    }

                    await _negocioService.ActualizarAsync(_negocioActual);
                }

                await Application.Current.MainPage.DisplayAlert("Éxito", "Cambios guardados correctamente", "OK");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- LÓGICA DE ARCHIVOS (SUPABASE) ---
        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                { PickerTitle = "Seleccionar foto", FileTypes = FilePickerFileType.Images });

                if (file != null && _negocioActual != null)
                {
                    using var stream = await file.OpenReadAsync();
                    string fileName = $"perfil_{_usuario.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    string url = await _storageService.SubirArchivoAsync(stream, "perfiles", fileName);

                    _negocioActual.UrlFotoPerfil = url;
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Foto subida.", "OK");
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async Task SubirPdfAsync()
        {
            try
            {
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } }
                });

                var file = await FilePicker.PickAsync(new PickOptions
                { PickerTitle = "Seleccionar PDF", FileTypes = customFileType });

                if (file != null && _negocioActual != null)
                {
                    using var stream = await file.OpenReadAsync();
                    string fileName = $"doc_{_usuario.Id}_{Guid.NewGuid()}.pdf";
                    string url = await _storageService.SubirArchivoAsync(stream, "documentos", fileName);

                    _negocioActual.UrlDocumentoTitulo = url;
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Documento subido.", "OK");
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); }
        }

        // ✅ TU MÉTODO DE CARGA DE HORARIOS (INTACTO Y CORREGIDO)
        private void CargarHorariosDesdeModelo(Negocio negocio)
        {
            ListaHorarios.Clear();

            var diasMapa = new[]
            {
                ("Lun", "Lunes"), ("Mar", "Martes"), ("Mie", "Miércoles"),
                ("Jue", "Jueves"), ("Vie", "Viernes"), ("Sab", "Sábado"), ("Dom", "Domingo")
            };

            foreach (var (claveDb, nombrePantalla) in diasMapa)
            {
                var vm = new HorarioTrabajoVm { Dia = nombrePantalla };

                if (negocio.Horarios != null && negocio.Horarios.ContainsKey(claveDb))
                {
                    var horarioDb = negocio.Horarios[claveDb];

                    if (TimeSpan.TryParse(horarioDb.Apertura, out TimeSpan apertura))
                        vm.HoraInicio = apertura;

                    if (TimeSpan.TryParse(horarioDb.Cierre, out TimeSpan cierre))
                        vm.HoraFin = cierre;

                    bool estaCerrado = negocio.DiasCerrados != null &&
                                       negocio.DiasCerrados.Contains(claveDb);
                    vm.EsLaboral = !estaCerrado;
                }
                else
                {
                    vm.EsLaboral = false;
                    vm.HoraInicio = new TimeSpan(8, 0, 0);
                    vm.HoraFin = new TimeSpan(18, 0, 0);
                }

                ListaHorarios.Add(vm);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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

        public Usuario Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

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

        private string _nuevaPassword;
        public string NuevaPassword
        {
            get => _nuevaPassword;
            set { _nuevaPassword = value; OnPropertyChanged(); }
        }

        private string _modalidadActual;
        public string ModalidadActual
        {
            get => _modalidadActual;
            set { _modalidadActual = value; OnPropertyChanged(); }
        }

        public string UrlFotoPerfil
        {
            get => _negocioActual?.UrlFotoPerfil;
            set
            {
                if (_negocioActual != null && _negocioActual.UrlFotoPerfil != value)
                {
                    _negocioActual.UrlFotoPerfil = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<HorarioTrabajoVm> ListaHorarios { get; set; }
            = new ObservableCollection<HorarioTrabajoVm>();

        public ObservableCollection<string> DocumentosSubidos { get; set; } = new ObservableCollection<string>();
        public bool HayDocumentos => DocumentosSubidos.Count > 0;

        public bool EsDuenoNegocio => _esDueno;
        public bool PuedeEditar => true;

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string TituloPagina => _esDueno ? "Editar Perfil de Negocio" : "Mi Perfil";

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

        private string EncriptarPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public async Task CargarDatosAsync()
        {
            IsBusy = true;
            try
            {
                DocumentosSubidos.Clear();
                OnPropertyChanged(nameof(HayDocumentos));

                var idUsuario = Preferences.Get("UsuarioId", string.Empty).Trim();
                _esDueno = Preferences.Get("EsDuenoNegocio", false);

                if (!string.IsNullOrEmpty(idUsuario))
                {
                    var usuarios = await _usuarioService.ObtenerTodosAsync();
                    var usuarioEncontrado = usuarios.FirstOrDefault(u => u.Id.Trim() == idUsuario);

                    if (usuarioEncontrado != null)
                    {
                        Usuario = usuarioEncontrado;
                    }

                    if (_esDueno)
                    {
                        var todosLosNegocios = await _negocioService.ObtenerTodosAsync();

                        _negocioActual = todosLosNegocios.FirstOrDefault(n =>
                            !string.IsNullOrEmpty(n.UsuarioId) &&
                            n.UsuarioId.Trim().ToLower() == idUsuario.ToLower());

                        if (_negocioActual != null)
                        {
                            NombreNegocio = _negocioActual.NombreNegocio;
                            Direccion = _negocioActual.DireccionNegocio;
                            Ruc = _negocioActual.NumeroRuc;
                            ModalidadActual = _negocioActual.TipoServicio;

                            OnPropertyChanged(nameof(UrlFotoPerfil));
                            CargarHorariosDesdeModelo(_negocioActual);
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Aviso", $"No se encontró el negocio.\nApp busca el ID: {idUsuario}\nRevisa que en Compass el UsuarioId sea idéntico.", "OK");
                            CargarHorariosDesdeModelo(new Negocio());
                        }
                    }

                    OnPropertyChanged(nameof(EsDuenoNegocio));
                    OnPropertyChanged(nameof(TituloPagina));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error al cargar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GuardarCambiosAsync()
        {
            if (IsBusy || _usuario == null) return;
            IsBusy = true;

            try
            {
                if (!string.IsNullOrEmpty(NuevaPassword))
                {
                    if (NuevaPassword.Length < 8)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "La contraseña debe tener al menos 8 caracteres.", "OK");
                        return;
                    }

                    _usuario.Contrasena = EncriptarPassword(NuevaPassword);
                }

                await _usuarioService.ActualizarAsync(_usuario);

                if (_esDueno && _negocioActual != null)
                {
                    _negocioActual.TipoServicio = ModalidadActual;
                    _negocioActual.NombreNegocio = NombreNegocio;
                    _negocioActual.DireccionNegocio = Direccion;
                    _negocioActual.NumeroRuc = Ruc;
                    _negocioActual.UsuarioId = _usuario.Id;

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
                                _negocioActual.Horarios[clave] = new Negocio.HorarioDia
                                {
                                    Apertura = item.HoraInicio.ToString(@"hh\:mm"),
                                    Cierre = item.HoraFin.ToString(@"hh\:mm"),
                                    Pausas = item.TienePausa
                                        ? new List<string> { item.PausaInicio.ToString(@"hh\:mm"), item.PausaFin.ToString(@"hh\:mm") }
                                        : new List<string>()
                                };
                            }
                            else
                            {
                                _negocioActual.DiasCerrados.Add(clave);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_negocioActual.Id))
                    {
                        await _negocioService.CrearAsync(_negocioActual);
                    }
                    else
                    {
                        await _negocioService.ActualizarAsync(_negocioActual);
                    }
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

        private async Task SeleccionarFotoAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                { PickerTitle = "Seleccionar foto", FileTypes = FilePickerFileType.Images });

                if (file != null)
                {
                    if (_negocioActual == null) _negocioActual = new Negocio();

                    using var stream = await file.OpenReadAsync();
                    string fileName = $"perfil_{_usuario.Id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    string url = await _storageService.SubirArchivoAsync(stream, "perfiles", fileName);

                    UrlFotoPerfil = url;
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Foto actualizada.", "OK");
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

                if (file != null)
                {
                    if (_negocioActual == null) _negocioActual = new Negocio();

                    using var stream = await file.OpenReadAsync();
                    string fileName = $"doc_{_usuario.Id}_{Guid.NewGuid()}.pdf";
                    string url = await _storageService.SubirArchivoAsync(stream, "documentos", fileName);

                    _negocioActual.UrlDocumentoTitulo = url;

                    DocumentosSubidos.Add(file.FileName);
                    OnPropertyChanged(nameof(HayDocumentos));

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Documento subido.", "OK");
                }
            }
            catch (Exception ex) { await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); }
        }

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

                if (negocio != null && negocio.Horarios != null && negocio.Horarios.ContainsKey(claveDb))
                {
                    var horarioDb = negocio.Horarios[claveDb];

                    if (TimeSpan.TryParse(horarioDb.Apertura, out TimeSpan apertura))
                        vm.HoraInicio = apertura;

                    if (TimeSpan.TryParse(horarioDb.Cierre, out TimeSpan cierre))
                        vm.HoraFin = cierre;

                    if (horarioDb.Pausas != null && horarioDb.Pausas.Count >= 2)
                    {
                        vm.TienePausa = true;
                        TimeSpan.TryParse(horarioDb.Pausas[0], out TimeSpan pInicio);
                        TimeSpan.TryParse(horarioDb.Pausas[1], out TimeSpan pFin);
                        vm.PausaInicio = pInicio;
                        vm.PausaFin = pFin;
                    }
                    else
                    {
                        vm.TienePausa = false;
                    }

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
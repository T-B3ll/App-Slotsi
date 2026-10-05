using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class EditarPerfilViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private readonly NegocioService _negocioService;

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

        
        public ObservableCollection<HorarioTrabajoVm> ListaHorarios { get; set; }
            = new ObservableCollection<HorarioTrabajoVm>();

     
        public bool EsDuenoNegocio => _esDueno;
        public bool PuedeEditar => true;

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string TituloPagina => _esDueno ? "Editar Perfil de Negocio" : "Mi Perfil";

        public event PropertyChangedEventHandler PropertyChanged;

        public EditarPerfilViewModel(UsuarioService usuarioService, NegocioService negocioService)
        {
            _usuarioService = usuarioService;
            _negocioService = negocioService;
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
                   
                    var usuarios = await _usuarioService.ObtenerTodosAsync();
                    var usuarioEncontrado = usuarios.FirstOrDefault(u => u.Id == idUsuario);
                    if (usuarioEncontrado != null) Usuario = usuarioEncontrado;

                 
                    if (_esDueno)
                    {
                        var todosLosNegocios = await _negocioService.ObtenerTodosAsync();
                        _negocioActual = todosLosNegocios.FirstOrDefault(n => n.UsuarioId == idUsuario);

                        if (_negocioActual != null)
                        {
                            NombreNegocio = _negocioActual.NombreNegocio;
                            Direccion = _negocioActual.DireccionNegocio;
                            Ruc = _negocioActual.NumeroRuc;

                            
                            CargarHorariosDesdeModelo(_negocioActual);
                        }
                    }

                    OnPropertyChanged(nameof(EsDuenoNegocio));
                    OnPropertyChanged(nameof(NombreNegocio));
                    OnPropertyChanged(nameof(Direccion));
                    OnPropertyChanged(nameof(Ruc));
                    OnPropertyChanged(nameof(TituloPagina));
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

       
        private void CargarHorariosDesdeModelo(Negocio negocio)
        {
            ListaHorarios.Clear();

            
            var diasMapa = new[]
            {
        ("Lun", "Lunes"),
        ("Mar", "Martes"),
        ("Mie", "Miércoles"),
        ("Jue", "Jueves"),
        ("Vie", "Viernes"),
        ("Sab", "Sábado"),
        ("Dom", "Domingo")
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

                    
                    bool estaEnListaCerrados = negocio.DiasCerrados != null &&
                                               negocio.DiasCerrados.Contains(claveDb);

                    vm.EsLaboral = !estaEnListaCerrados;
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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
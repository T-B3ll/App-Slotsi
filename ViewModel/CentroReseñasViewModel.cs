using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Slotsi.Shared.Dtos;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static slotsi_citas.Models.Calificaciones;

namespace slotsi_citas.ViewModel
{
    public enum TipoRolUsuario
    {
        Cliente,
        Negocio
    }

    public class CentroReseñasViewModel : BindableObject
    {
        private readonly CalificacionService _calificacionService;
        private readonly NegocioService _negocioService;

        public ObservableCollection<CalificacionModel> ListaCalificaciones { get; set; } = new();

        private TipoRolUsuario _rolActual = TipoRolUsuario.Cliente;

        public bool MostrarBotonCrearResena => !MostrarRecibidas;
        public ICommand IrACrearResenaCommand { get; }
        private bool _mostrarRecibidas = true;

        public bool MostrarRecibidas
        {
            get => _mostrarRecibidas;
            set
            {
                _mostrarRecibidas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColorBotonRecibidas));
                OnPropertyChanged(nameof(ColorBotonRealizadas));
                OnPropertyChanged(nameof(TextoBotonRecibidas));
                OnPropertyChanged(nameof(TextoBotonRealizadas));
                OnPropertyChanged(nameof(MostrarBotonCrearResena));
                _ = CargarCalificacionesAsync();
            }
        }

        private int _totalCalificaciones;
        public int TotalCalificaciones
        {
            get => _totalCalificaciones;
            set { _totalCalificaciones = value; OnPropertyChanged(); }
        }

        private double _promedioCalificacion;
        public double PromedioCalificacion
        {
            get => _promedioCalificacion;
            set { _promedioCalificacion = value; OnPropertyChanged(); }
        }

        public Color ColorBotonRecibidas => _mostrarRecibidas ? Color.FromArgb("#1976D2") : Color.FromArgb("#E0E0E0");
        public Color ColorBotonRealizadas => !_mostrarRecibidas ? Color.FromArgb("#1976D2") : Color.FromArgb("#E0E0E0");
        public Color TextoBotonRecibidas => _mostrarRecibidas ? Colors.White : Color.FromArgb("#444444");
        public Color TextoBotonRealizadas => !_mostrarRecibidas ? Colors.White : Color.FromArgb("#444444");

        public ICommand SeleccionarPestanaCommand { get; }

        public CentroReseñasViewModel(CalificacionService calificacionService, NegocioService negocioService)
        {
            _calificacionService = calificacionService ?? throw new ArgumentNullException(nameof(calificacionService));
            _negocioService = negocioService ?? throw new ArgumentNullException(nameof(negocioService));

            IrACrearResenaCommand = new Command(async () => await NavegarACrearResena());
            SeleccionarPestanaCommand = new Microsoft.Maui.Controls.Command<string>((pestana) =>
            {
                MostrarRecibidas = (pestana == "Recibidas");
            });
        }

        private async Task NavegarACrearResena()
        {
            await Shell.Current.GoToAsync(nameof(CrearResena));
        }

        public async Task CargarCalificacionesAsync()
        {
            try
            {
                bool esDuenoNegocio = Preferences.Get("EsDuenoNegocio", false);
                _rolActual = esDuenoNegocio ? TipoRolUsuario.Negocio : TipoRolUsuario.Cliente;

                string usuarioIdActivo = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                if (string.IsNullOrWhiteSpace(usuarioIdActivo))
                {
                    LimpiarLista();
                    return;
                }

                List<CalificacionesDtos> dtos = new();

                if (_rolActual == TipoRolUsuario.Negocio)
                {
                    string negocioId = usuarioIdActivo;
                    try
                    {
                        var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(usuarioIdActivo);
                        if (negocio != null && !string.IsNullOrEmpty(negocio.Id))
                        {
                            negocioId = negocio.Id;
                        }
                    }
                    catch
                    {

                    }

                    var todasNegocio = await _calificacionService.ObtenerPorNegocioAsync(negocioId);
                    bool estadoCalificado = MostrarRecibidas;
                    dtos = todasNegocio.Where(c => c.NegocioCalificado == estadoCalificado).ToList();

                    try
                    {
                        var promedioDto = await _calificacionService.ObtenerPromedioNegocioAsync(negocioId);
                        if (promedioDto != null && MostrarRecibidas)
                        {
                            TotalCalificaciones = promedioDto.Total;
                            PromedioCalificacion = promedioDto.Promedio;
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    var todasCliente = await _calificacionService.ObtenerPorUsuarioAsync(usuarioIdActivo);
                    bool estadoCalificado = !MostrarRecibidas;
                    dtos = todasCliente.Where(c => c.NegocioCalificado == estadoCalificado).ToList();
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ListaCalificaciones.Clear();

                    if (dtos != null && dtos.Count > 0)
                    {
                        foreach (var dto in dtos)
                        {
                            var model = new CalificacionModel
                            {
                                Id = dto.Id,
                                IdNegocio = dto.IdNegocio,
                                IdColaborador = dto.IdColaborador,
                                IdUsuarioCliente = dto.IdUsuarioCliente,
                                RangoCalificacion = dto.RangoCalificacion,
                                NegocioCalificado = dto.NegocioCalificado,
                                NombreNegocio = dto.NombreNegocio ?? string.Empty,
                                NombreUsuario = dto.NombreUsuario ?? string.Empty,
                                Comentario = dto.Comentario ?? string.Empty
                            };

                            if (_rolActual == TipoRolUsuario.Negocio)
                            {
                                model.TituloMostrado = $"Cliente: {model.NombreUsuario}";
                                model.SubtituloMostrado = $"Negocio: {model.NombreNegocio}";
                            }
                            else
                            {
                                model.TituloMostrado = $"Negocio: {model.NombreNegocio}";
                                model.SubtituloMostrado = $"Cliente: {model.NombreUsuario}";
                            }

                            ListaCalificaciones.Add(model);
                        }

                        if (_rolActual != TipoRolUsuario.Negocio || !MostrarRecibidas)
                        {
                            TotalCalificaciones = dtos.Count;
                            PromedioCalificacion = dtos.Count > 0 ? Math.Round(dtos.Average(c => c.RangoCalificacion), 1) : 0;
                        }
                    }
                    else
                    {
                        LimpiarLista();
                        if (_rolActual != TipoRolUsuario.Negocio || !MostrarRecibidas)
                        {
                            TotalCalificaciones = 0;
                            PromedioCalificacion = 0;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CentroReseñas] Error: {ex.Message}");
            }
        }

        private void LimpiarLista()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ListaCalificaciones.Clear();
                TotalCalificaciones = 0;
                PromedioCalificacion = 0;
            });
        }
    }
}
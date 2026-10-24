using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
using slotsi_citas.Services.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class ListaTrabjadoresViewModel : INotifyPropertyChanged
    {
        private readonly TrabajadorService _trabajadorService;
        private readonly NegocioService _negocioService;

        public ICommand EditarTrabajadorCommand { get; }
        public ICommand IrANuevoTrabajadorCommand { get; }
        public ICommand CambiarEstadoActivoCommand { get; }

        private int _trabajadoresActivosCount;
        public int TrabajadoresActivosCount
        {
            get => _trabajadoresActivosCount;
            set
            {
                if (_trabajadoresActivosCount != value)
                {
                    _trabajadoresActivosCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Trabajador> Trabajadores { get; set; } = new ObservableCollection<Trabajador>();

        public ListaTrabjadoresViewModel(TrabajadorService trabajadorService, NegocioService negocioService)
        {
            _trabajadorService = trabajadorService;
            _negocioService = negocioService;

            CambiarEstadoActivoCommand = new Command<Trabajador>(async (t) => await CambiarEstadoActivoAsync(t));
            IrANuevoTrabajadorCommand = new Command(async () => await IrANuevoTrabajadorAsync());
            EditarTrabajadorCommand = new Command<Trabajador>(async (t) => await EditarTrabajadorAsync(t));
        }

        private async Task EditarTrabajadorAsync(Trabajador trabajador)
        {
            if (trabajador == null) return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "TrabajadorEditar", trabajador }
            };

            await Shell.Current.GoToAsync(nameof(NuevoTrabajador), navigationParameters);
        }

        private async Task IrANuevoTrabajadorAsync()
        {
            await Shell.Current.GoToAsync(nameof(NuevoTrabajador));
        }

        public async Task CargarTrabajadoresAsync()
        {
            try
            {
                string usuarioId = Preferences.Get("UsuarioId", string.Empty);

                if (string.IsNullOrEmpty(usuarioId))
                {
                    Debug.WriteLine("[SESIÓN ERROR] No se encontró 'UsuarioId' en Preferences.");
                    return;
                }

                var negocioId = Preferences.Get("negocio_id", string.Empty);

                if (string.IsNullOrEmpty(negocioId))
                {
                    var negocio = await _negocioService.ObtenerPorUsuarioIdAsync(usuarioId);
                    if (negocio == null || string.IsNullOrEmpty(negocio.Id))
                    {
                        Debug.WriteLine("[API] No se encontró ningún negocio vinculado a este UsuarioId.");
                        return;
                    }
                    negocioId = negocio.Id;
                    Preferences.Set("negocio_id", negocioId);
                }

                var dtos = await _trabajadorService.ObtenerPorNegocioAsync(negocioId);
                var lista = dtos.Select(d => d.ToModel()).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Trabajadores.Clear();
                    foreach (var trabajador in lista)
                    {
                        Trabajadores.Add(trabajador);
                    }
                    TrabajadoresActivosCount = Trabajadores.Count(t => t.Activo);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR API TRABAJADORES] {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", ApiException.MensajePara(ex), "OK");
            }
        }

        public async Task CambiarEstadoActivoAsync(Trabajador trabajador)
        {
            if (trabajador == null) return;

            if (!trabajador.Activo)
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Desactivar trabajador",
                    $"¿Estás seguro de que deseas desactivar '{trabajador.Nombre}'? No podrá acceder al sistema.",
                    "Desactivar",
                    "Cancelar");

                if (!confirmar)
                {
                    trabajador.Activo = true;
                    return;
                }
            }

            try
            {
                await _trabajadorService.CambiarEstadoAsync(trabajador.Id, trabajador.Activo);
                trabajador.Activo = trabajador.Activo;
                TrabajadoresActivosCount = Trabajadores.Count(t => t.Activo);
            }
            catch (Exception ex)
            {
                trabajador.Activo = !trabajador.Activo;
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar: {ApiException.MensajePara(ex)}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
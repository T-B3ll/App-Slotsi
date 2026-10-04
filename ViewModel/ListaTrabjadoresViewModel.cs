using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Pages;
using slotsi_citas.Services;
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
        private readonly IMongoCollection<Trabajador> _trabajadoresCollection;

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

        public ListaTrabjadoresViewModel()
        {
            CambiarEstadoActivoCommand = new Microsoft.Maui.Controls.Command<Trabajador>(async (trabajador) => await CambiarEstadoActivoAsync(trabajador));
            IrANuevoTrabajadorCommand = new Command(async () => await IrANuevoTrabajadorAsync());
            EditarTrabajadorCommand = new Microsoft.Maui.Controls.Command<Trabajador>(async (trabajador) => await EditarTrabajadorAsync(trabajador));

            try
            {
                var client = new MongoClient(MongoDbSettings.ConnectionString);
                var database = client.GetDatabase(MongoDbSettings.DatabaseName);

                _trabajadoresCollection = database.GetCollection<Trabajador>("Trabajadores");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR MONGODB INIT] {ex.Message}");
            }
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
            if (_trabajadoresCollection == null) return;

            try
            {
                string usuarioId = Preferences.Get("UsuarioId", string.Empty);

                if (string.IsNullOrEmpty(usuarioId))
                {
                    Debug.WriteLine("[SESIÓN ERROR] No se encontró 'UsuarioId' en Preferences.");
                    return;
                }

                if (!ObjectId.TryParse(usuarioId, out ObjectId usuarioObjectId))
                {
                    Debug.WriteLine($"[MONGODB ERROR] El UsuarioId '{usuarioId}' no es un ObjectId válido.");
                    return;
                }

                var client = new MongoClient(MongoDbSettings.ConnectionString);
                var database = client.GetDatabase(MongoDbSettings.DatabaseName);
                var negociosCollection = database.GetCollection<BsonDocument>("Negocios");

                var filtroNegocio = Builders<BsonDocument>.Filter.Eq("UsuarioId", usuarioObjectId);
                var negocioDoc = await negociosCollection.Find(filtroNegocio).FirstOrDefaultAsync();

                if (negocioDoc == null)
                {
                    Debug.WriteLine("[MONGODB ERROR] No se encontró ningún negocio vinculado a este UsuarioId.");
                    return;
                }

                string negocioIdActual = negocioDoc["_id"].ToString();
                Preferences.Set("negocio_id", negocioIdActual);

                var filtroTrabajadores = Builders<Trabajador>.Filter.Eq(t => t.NegocioId, negocioIdActual);
                var lista = await _trabajadoresCollection.Find(filtroTrabajadores).ToListAsync();

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
                Debug.WriteLine($"[ERROR MONGODB FETCH] {ex.Message}");
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
                var filtro = Builders<Trabajador>.Filter.Eq(t => t.Id, trabajador.Id);
                var actualizacion = Builders<Trabajador>.Update.Set(t => t.Activo, trabajador.Activo);

                await _trabajadoresCollection.UpdateOneAsync(filtro, actualizacion);

                TrabajadoresActivosCount = Trabajadores.Count(t => t.Activo);
            }
            catch (Exception ex)
            {
                trabajador.Activo = !trabajador.Activo;
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al actualizar: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
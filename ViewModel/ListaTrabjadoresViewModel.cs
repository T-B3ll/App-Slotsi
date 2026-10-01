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
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace slotsi_citas.ViewModel
{
    public class ListaTrabjadoresViewModel : INotifyPropertyChanged
    {
        private readonly IMongoCollection<Trabajador> _trabajadoresCollection;

        public ICommand EditarTrabajadorCommand { get; }
        public ICommand IrANuevoTrabajadorCommand { get; }

        public ObservableCollection<Trabajador> Trabajadores { get; set; } = new ObservableCollection<Trabajador>();

        public ListaTrabjadoresViewModel()
        {
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
                Debug.WriteLine("[MONGODB] Consultando colección Trabajadores...");

                var lista = await _trabajadoresCollection.Find(_ => true).ToListAsync();

                Debug.WriteLine($"[MONGODB] Documentos encontrados: {lista.Count}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Trabajadores.Clear();
                    foreach (var trabajador in lista)
                    {
                        Trabajadores.Add(trabajador);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR MONGODB FETCH] {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
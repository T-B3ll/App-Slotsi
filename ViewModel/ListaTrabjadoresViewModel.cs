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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace slotsi_citas.ViewModel
{
    public class ListaTrabjadoresViewModel : INotifyPropertyChanged
    {
        public ICommand IrANuevoTrabajadorCommand { get; }
        private readonly IMongoCollection<Trabajador> _trabajadoresCollection;

        public ObservableCollection<Trabajador> Trabajadores { get; set; } = new ObservableCollection<Trabajador>();

        public ListaTrabjadoresViewModel()
        {
            IrANuevoTrabajadorCommand = new Command(async () => await IrANuevoTrabajadorAsync());
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

        private async Task IrANuevoTrabajadorAsync()
        {
        
            await Shell.Current.GoToAsync(nameof(NuevoTrabajador));
     
        }

        public async Task CargarTrabajadoresAsync()
        {
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
                    OnPropertyChanged(nameof(Trabajadores));
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class DueñosNegociosViewModel : INotifyPropertyChanged
    {
        private readonly NegocioService _negocioService;
        private ObservableCollection<Negocio> _listaDueños;

        public ObservableCollection<Negocio> ListaDueños
        {
            get => _listaDueños;
            set { _listaDueños = value; OnPropertyChanged(); }
        }

        public Command RefreshCommand { get; }

        public DueñosNegociosViewModel()
        {
            _negocioService = new NegocioService();
            ListaDueños = new ObservableCollection<Negocio>();
            RefreshCommand = new Command(async () => await CargarDatosAsync());

         
            MainThread.BeginInvokeOnMainThread(async () => await CargarDatosAsync());
        }

        private async Task CargarDatosAsync()
        {
            try
            {
               
                var negocios = await _negocioService.ObtenerTodosAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ListaDueños.Clear();
                    foreach (var negocio in negocios)
                    {
                        ListaDueños.Add(negocio);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los dueños: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using slotsi_citas.Models;
using slotsi_citas.Services;

namespace slotsi_citas.ViewModel
{
    public class DueñosNegociosViewModel : INotifyPropertyChanged
    {
        private readonly NegocioService _negocioService;
        private readonly CalificacionService _calificacionService;
        private ObservableCollection<Negocio> _listaDueños;

        public ObservableCollection<Negocio> ListaDueños
        {
            get => _listaDueños;
            set { _listaDueños = value; OnPropertyChanged(); }
        }

        public Command RefreshCommand { get; }

        public DueñosNegociosViewModel(NegocioService negocioService, CalificacionService calificacionService)
        {
            _negocioService = negocioService;
            _calificacionService = calificacionService;
            ListaDueños = new ObservableCollection<Negocio>();
            RefreshCommand = new Command(async () => await CargarNegociosAsync());
        }

        public async Task CargarNegociosAsync()
        {
            try
            {
                var negocios = await _negocioService.ObtenerTodosAsync() ?? new List<Negocio>();

                await CargarCalificacionesAsync(negocios);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ListaDueños.Clear();
                    foreach (var negocio in negocios)
                        ListaDueños.Add(negocio);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los negocios: {ex.Message}", "OK");
            }
        }

        private async Task CargarCalificacionesAsync(List<Negocio> negocios)
        {
            using var limitador = new SemaphoreSlim(5); // máx. 5 peticiones simultáneas

            var tareas = negocios.Select(async n =>
            {
                if (string.IsNullOrEmpty(n.Id)) return;

                await limitador.WaitAsync();
                try
                {
                    var p = await _calificacionService.ObtenerPromedioNegocioAsync(n.Id);
                    n.PromedioCalificacion = p?.Promedio ?? 0;
                    n.TotalResenas = p?.Total ?? 0;
                }
                catch
                {
                  
                }
                finally
                {
                    limitador.Release();
                }
            });

            await Task.WhenAll(tareas);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
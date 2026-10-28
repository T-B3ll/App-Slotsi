using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.ViewModel
{
    public class MejoresNegociosViewModel
    {
        private readonly NegocioService _negocioService;
        private readonly CalificacionService _calificacionService;

        private List<Negocio> _todos = new();
        private string _categoria = "todos";
        private int _top = 10;

        public ObservableCollection<NegocioRanckingItem> Ranking { get; } = new();

        private string _textoResultados = "";
        public string TextoResultados
        {
            get => _textoResultados;
            set { _textoResultados = value; OnPropertyChanged(); }
        }

        private bool _cargando;
        public bool Cargando
        {
            get => _cargando;
            set { _cargando = value; OnPropertyChanged(); }
        }

        public MejoresNegociosViewModel(NegocioService negocioService, CalificacionService calificacionService)
        {
            _negocioService = negocioService;
            _calificacionService = calificacionService;
        }

        public async Task CargarAsync()
        {
            if (Cargando) return;
            Cargando = true;
            try
            {
                var negocios = await _negocioService.ObtenerTodosAsync() ?? new List<Negocio>();

                using var limitador = new SemaphoreSlim(5);
                await Task.WhenAll(negocios.Select(async n =>
                {
                    if (string.IsNullOrEmpty(n.Id)) return;
                    await limitador.WaitAsync();
                    try
                    {
                        var p = await _calificacionService.ObtenerPromedioNegocioAsync(n.Id);
                        n.PromedioCalificacion = p?.Promedio ?? 0;
                        n.TotalResenas = p?.Total ?? 0;
                    }
                    catch { }
                    finally { limitador.Release(); }
                }));

                // Un ranking solo tiene sentido con negocios que ya tienen reseñas
                _todos = negocios.Where(n => n.TotalResenas > 0).ToList();
                Aplicar();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"No se pudo cargar el ranking: {ex.Message}", "OK");
            }
            finally
            {
                Cargando = false;
            }
        }

        public void CambiarCategoria(string categoria)
        {
            _categoria = Normalizar(categoria);
            Aplicar();
        }

        public void CambiarTop(int top)
        {
            _top = top;
            Aplicar();
        }

        private void Aplicar()
        {
            var filtrados = _todos
                .Where(CoincideCategoria)
                .OrderByDescending(n => n.PromedioCalificacion)
                .ThenByDescending(n => n.TotalResenas)
                .ThenBy(n => n.NombreNegocio)
                .ToList();

            var mostrados = filtrados.Take(_top).ToList();

            Ranking.Clear();
            foreach (var n in mostrados)
                Ranking.Add(new NegocioRanckingItem { Negocio = n });

            TextoResultados = filtrados.Count > mostrados.Count
                ? $"Top {mostrados.Count} de {filtrados.Count} negocios"
                : $"{filtrados.Count} negocios encontrados";
        }

        private bool CoincideCategoria(Negocio n)
        {
            if (_categoria == "todos") return true;
            if (string.IsNullOrEmpty(n.Categoria)) return false;

            string cat = Normalizar(n.Categoria);
            return _categoria switch
            {
                "salones" => cat.Contains("salon") || cat.Contains("belleza") || cat.Contains("estetica"),
                "barberia" => cat.Contains("barber"),
                "spa" => cat.Contains("spa"),
                _ => cat.Contains(_categoria)
            };
        }

        private static string Normalizar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.ToLower().Replace("á", "a").Replace("é", "e").Replace("í", "i")
                        .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

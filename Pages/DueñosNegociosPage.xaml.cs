using System.Linq;
using System.Threading.Tasks;
using slotsi_citas.ViewModel;
using slotsi_citas.Models;

namespace slotsi_citas.Pages;

public partial class DueñosNegociosPage : ContentPage
{
    private bool _ordenarPorCalificacion = false;
    private string _categoriaActual = "todos";
    private string _textoBusqueda = "";
    private List<Negocio> _todosLosNegocios = new();
    private DueñosNegociosViewModel? _viewModel;
    private bool _isNavigating = false;

    private CancellationTokenSource? _searchCancellationTokenSource;

    public DueñosNegociosPage(DueñosNegociosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string original = e.NewTextValue ?? "";
        BtnLimpiarBusqueda.IsVisible = !string.IsNullOrEmpty(original);

        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();
        var token = _searchCancellationTokenSource.Token;

        try
        {
            await Task.Delay(300, token);

            _textoBusqueda = NormalizarTexto(original);
            AplicarFiltros();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        EntryBusqueda.Text = "";
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        foreach (var b in new[] { BtnTodos, BtnSalones, BtnBarberia })
        {
            b.BackgroundColor = Colors.White;
            b.TextColor = Color.FromArgb("#4B5563");
        }
        btn.BackgroundColor = Color.FromArgb("#2563EB");
        btn.TextColor = Colors.White;

        _categoriaActual = NormalizarTexto(btn.Text);
        AplicarFiltros();
    }

    private void OnOrdenCalificacionClicked(object sender, EventArgs e)
    {
        _ordenarPorCalificacion = !_ordenarPorCalificacion;

        BtnMejorCalificados.BackgroundColor = _ordenarPorCalificacion
            ? Color.FromArgb("#2563EB") : Colors.White;
        BtnMejorCalificados.TextColor = _ordenarPorCalificacion
            ? Colors.White : Color.FromArgb("#4B5563");

        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        var query = _todosLosNegocios
            .Where(n => CoincideCategoria(n) && CoincideBusqueda(n));

        if (_ordenarPorCalificacion)
        {
            query = query
                .OrderByDescending(n => n.PromedioCalificacion)
                .ThenByDescending(n => n.TotalResenas)
                .ThenBy(n => n.NombreNegocio);
        }

        var resultados = query.ToList();

        LblResultados.Text = $"{resultados.Count} negocios encontrados";
        ActualizarLista(resultados);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel == null) return;

        indicadorCarga.IsRunning = indicadorCarga.IsVisible = true;
        ListaNegocios.IsVisible = false;

        try
        {
            await _viewModel.CargarNegociosAsync();
            _todosLosNegocios = _viewModel.ListaDueños.ToList();
            AplicarFiltros();
        }
        finally
        {
            indicadorCarga.IsRunning = indicadorCarga.IsVisible = false;
            ListaNegocios.IsVisible = true;
        }
    }

    private bool CoincideCategoria(Negocio n)
    {
        if (_categoriaActual == "todos") return true;
        if (string.IsNullOrEmpty(n.Categoria)) return false;

        string cat = NormalizarTexto(n.Categoria);
        return _categoriaActual switch
        {
            "salones" => cat.Contains("salon") || cat.Contains("belleza") || cat.Contains("estetica"),
            "barberia" => cat.Contains("barber"),
            "spa" => cat.Contains("spa"),
            _ => cat.Contains(_categoriaActual)
        };
    }

    private bool CoincideBusqueda(Negocio n)
    {
        if (string.IsNullOrEmpty(_textoBusqueda)) return true;
        string nombre = NormalizarTexto(n.NombreNegocio ?? n.Nombre ?? "");
        string categoria = NormalizarTexto(n.Categoria ?? "");
        return nombre.Contains(_textoBusqueda) || categoria.Contains(_textoBusqueda);
    }

    private async void OnNegocioSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Negocio negocioSeleccionado)
        {
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }

            if (string.IsNullOrEmpty(negocioSeleccionado.Id))
            {
                await DisplayAlert("Aviso", $"El negocio '{negocioSeleccionado.NombreNegocio}' no tiene un ID registrado.", "OK");
                return;
            }

            Preferences.Set("NegocioSeleccionadoId", negocioSeleccionado.Id);
            await Shell.Current.GoToAsync(nameof(CatalogoCliente));
        }
    }

    private string NormalizarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        string lower = texto.ToLower();
        lower = lower.Replace("á", "a").Replace("é", "e").Replace("í", "i")
                     .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
        return lower;
    }

    private void ActualizarLista(List<Negocio> lista)
    {
        if (_viewModel == null) return;

        Dispatcher.Dispatch(() =>
        {
            _viewModel.ListaDueños.Clear();
            foreach (var item in lista)
            {
                _viewModel.ListaDueños.Add(item);
            }
        });
    }

    private async void OnNegocioTapped(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            Negocio? negocioSeleccionado = null;

            if (e is TappedEventArgs tappedArgs)
            {
                negocioSeleccionado = tappedArgs.Parameter as Negocio;
            }

            if (negocioSeleccionado == null && sender is BindableObject bindable)
            {
                negocioSeleccionado = bindable.BindingContext as Negocio;
            }

            if (negocioSeleccionado == null) return;

            string idValido = negocioSeleccionado.Id;

            if (string.IsNullOrEmpty(idValido))
            {
                await DisplayAlert("Aviso", $"El negocio '{negocioSeleccionado.NombreNegocio ?? negocioSeleccionado.Nombre}' no posee un ID de MongoDB registrado.", "OK");
                return;
            }

            Preferences.Set("NegocioSeleccionadoId", idValido);

            if (Shell.Current != null)
            {
                await Task.Yield();
                await Shell.Current.GoToAsync("CatalogoCliente", false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR NAVEGACION]: {ex.Message}");
            await DisplayAlert("Error", $"No se pudo abrir el catálogo: {ex.Message}", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}
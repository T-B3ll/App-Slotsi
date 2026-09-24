using System.Linq;
using slotsi_citas.ViewModel;
using slotsi_citas.Models;

namespace slotsi_citas.Pages;

public partial class DueñosNegociosPage : ContentPage
{
   
    private List<Negocio> _todosLosNegocios = new();
    private DueñosNegociosViewModel? _viewModel;

    public DueñosNegociosPage()
    {
        InitializeComponent();
        _viewModel = new DueñosNegociosViewModel();
        BindingContext = _viewModel;
    }


    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string textoOriginal = e.NewTextValue ?? "";
        string texto = NormalizarTexto(textoOriginal);

        BtnLimpiarBusqueda.IsVisible = !string.IsNullOrEmpty(textoOriginal);

   
        if (_todosLosNegocios == null || !_todosLosNegocios.Any())
            return;

       
        var resultados = _todosLosNegocios.Where(n =>
        {
            string nombre = NormalizarTexto(n.NombreNegocio ?? n.Nombre ?? "");
            string categoria = NormalizarTexto(n.Categoria ?? "");

     
            return nombre.Contains(texto) || categoria.Contains(texto);
        }).ToList();

        ActualizarLista(resultados);
    }

    private void OnClearSearchClicked(object sender, EventArgs e)
    {
        
        EntryBusqueda.TextChanged -= OnSearchTextChanged;

        EntryBusqueda.Text = "";
        BtnLimpiarBusqueda.IsVisible = false;

        ActualizarLista(_todosLosNegocios);

  
        EntryBusqueda.TextChanged += OnSearchTextChanged;
    }

 
    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;

        BtnTodos.BackgroundColor = Colors.White; BtnTodos.TextColor = Color.FromArgb("#4B5563");
        BtnSalones.BackgroundColor = Colors.White; BtnSalones.TextColor = Color.FromArgb("#4B5563");
        BtnBarberia.BackgroundColor = Colors.White; BtnBarberia.TextColor = Color.FromArgb("#4B5563");

     
        btn.BackgroundColor = Color.FromArgb("#2563EB");
        btn.TextColor = Colors.White;

        IEnumerable<Negocio> resultados;
        string categoriaBoton = btn.Text.ToLower();

        if (categoriaBoton == "todos")
        {
            resultados = _todosLosNegocios;
        }
        else
        {
            resultados = _todosLosNegocios.Where(n =>
            {
                if (n.Categoria == null) return false;
                string cat = NormalizarTexto(n.Categoria);

            
                if (categoriaBoton == "salones")
                    return cat.Contains("salon") || cat.Contains("belleza") || cat.Contains("estetica");

                if (categoriaBoton == "barberia") 
                    return cat.Contains("barberia") || cat.Contains("barber");

                return cat.Contains(categoriaBoton);
            });
        }

        ActualizarLista(resultados.ToList());
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

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _viewModel.ListaDueños.Clear();
            foreach (var item in lista)
            {
                _viewModel.ListaDueños.Add(item);
            }
        });
    }

   
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        while (_viewModel != null && _viewModel.ListaDueños.Count == 0)
        {
            await Task.Delay(100);
        }

        if (_viewModel != null)
        {
            _todosLosNegocios = _viewModel.ListaDueños.ToList();
        }
    }
}
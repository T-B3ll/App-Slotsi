using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using System.Text;

namespace slotsi_citas.Pages
{
    public partial class EditarServicio : ContentPage
    {
        public EditarServicio(Servicios? servicio, CatalogoService catalogoService, TrabajadorService trabajadorService, string negocioId = "")
        {
            InitializeComponent();
            BindingContext = new EditarServicioViewModel(servicio, catalogoService, trabajadorService, negocioId);
        }
    }

    // Estas clases van FUERA de EditarServicio para que el XAML las encuentre.
    public enum TipoFiltro
    {
        SoloLetras,
        SoloEnteros,
        SoloDecimales
    }

    public class FiltroEntryBehavior : Behavior<Entry>
    {
        public TipoFiltro Tipo { get; set; } = TipoFiltro.SoloLetras;

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.TextChanged += OnTextChanged;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry || string.IsNullOrEmpty(e.NewTextValue)) return;

            var filtrado = Filtrar(e.NewTextValue);
            if (filtrado != e.NewTextValue)
                entry.Text = filtrado;
        }

        private string Filtrar(string texto)
        {
            var sb = new StringBuilder();

            switch (Tipo)
            {
                case TipoFiltro.SoloLetras:
                    foreach (var c in texto)
                        if (char.IsLetter(c) || c == ' ') sb.Append(c);
                    break;

                case TipoFiltro.SoloEnteros:
                    foreach (var c in texto)
                        if (c >= '0' && c <= '9') sb.Append(c);
                    break;

                case TipoFiltro.SoloDecimales:
                    var hayPunto = false;
                    var decimales = 0;
                    foreach (var c in texto.Replace(',', '.'))
                    {
                        if (c >= '0' && c <= '9')
                        {
                            if (hayPunto)
                            {
                                if (decimales >= 2) continue;
                                decimales++;
                            }
                            sb.Append(c);
                        }
                        else if (c == '.' && !hayPunto)
                        {
                            hayPunto = true;
                            sb.Append(c);
                        }
                    }
                    break;
            }

            return sb.ToString();
        }
    }
}
using System.Linq;
using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using slotsi_citas.Services;

namespace slotsi_citas.Pages;

public partial class PagoSuscripcionPage : ContentPage
{
    private bool _isUpdating = false;
    private PagoSuscripcionViewModel _viewModel;

    public PagoSuscripcionPage()
    {
        InitializeComponent();
        _viewModel = new PagoSuscripcionViewModel(new SuscripcionService());
        BindingContext = _viewModel;
    }


    private void EntryCard_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (digits.Length > 16) digits = digits.Substring(0, 16);

        string formatted = "";
        for (int i = 0; i < digits.Length; i++)
        {
            if (i > 0 && i % 4 == 0) formatted += " ";
            formatted += digits[i];
        }

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;

      
            EntryCard.Text = formatted;
            EntryCard.CursorPosition = formatted.Length;

  
            _viewModel.NumeroTarjeta = digits;

            _isUpdating = false;
        });
    }


    private void EntryExpiry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string text = e.NewTextValue.Replace("/", "");
        if (!string.IsNullOrEmpty(text) && !text.All(char.IsDigit)) return;
        if (text.Length > 4) text = text.Substring(0, 4);

        string formatted = text;
        if (text.Length >= 3) formatted = text.Insert(2, "/");

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;
            EntryExpiry.Text = formatted;
            EntryExpiry.CursorPosition = formatted.Length;

           
            _viewModel.FechaExpiracion = formatted;

            _isUpdating = false;
        });
    }

 
    private void OnMetodoPagoClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;

        BtnDebito.BackgroundColor = Colors.White;
        BtnDebito.TextColor = Color.FromArgb("#4B5563");
        BtnCredito.BackgroundColor = Colors.White;
        BtnCredito.TextColor = Color.FromArgb("#4B5563");

        btn.BackgroundColor = Color.FromArgb("#2563EB");
        btn.TextColor = Colors.White;

        _viewModel.MetodoSeleccionado = btn.Text;
    }

   
    private async void OnPagarClicked(object sender, EventArgs e)
    {
        if (_viewModel.PagarCommand.CanExecute(null))
            _viewModel.PagarCommand.Execute(null);
    }
}
using System.Linq;
using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using slotsi_citas.Services;

namespace slotsi_citas.Pages;

public partial class PagoSuscripcionPage : ContentPage
{
    private bool _isUpdating = false;
    private PagoSuscripcionViewModel _viewModel;
    private readonly SuscripcionService _servicioLocal = new SuscripcionService();


    public PagoSuscripcionPage()
    {
        InitializeComponent();
        _viewModel = new PagoSuscripcionViewModel(new SuscripcionService());
        BindingContext = _viewModel;

        CargarFechaProximoCobroAsync().ConfigureAwait(false);
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


    private async Task CargarFechaProximoCobroAsync()
    {
        var usuarioId = Preferences.Get("UsuarioId", string.Empty);
        if (string.IsNullOrEmpty(usuarioId)) return;

        try
        {
            var sub = await _servicioLocal.ObtenerSuscripcionActivaAsync(usuarioId);

            if (sub != null && sub.ProximoVencimiento > DateTime.UtcNow)
            {
                LblProximoCobro.Text = $"Próximo cobro: {sub.ProximoVencimiento:dd MMM, yyyy}";
            }
            else
            {
                LblProximoCobro.Text = "Próximo cobro: Al realizar el pago";
            }
        }
        catch
        {
            LblProximoCobro.Text = "Próximo cobro: Pendiente";
        }
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
        var usuarioId = Preferences.Get("UsuarioId", string.Empty);

        // Validamos directamente con nuestro servicio local
        bool yaPago = await _servicioLocal.YaPagoEsteMesAsync(usuarioId);

        if (yaPago)
        {
            await DisplayAlert(
                "Ya realizaste este pago",
                "Tu suscripción está activa este mes. No es necesario pagar nuevamente.",
                "Entendido");
            return;
        }

        if (_viewModel.PagarCommand.CanExecute(null))
        {
            _viewModel.PagarCommand.Execute(null);
        }



    }
}
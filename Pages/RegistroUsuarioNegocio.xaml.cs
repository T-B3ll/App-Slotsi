using System.Linq;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Pages;

public partial class RegistroUsuarioNegocio : ContentPage
{
    private bool _isUpdating = false;
    private bool _isPasswordVisible = false;
    private string _tipoNegocio = "";

    public RegistroUsuarioNegocio() => InitializeComponent();

    // ✅ ESTE ES EL MÉTODO QUE FALTABA Y CAUSABA EL ERROR
    private void OnTipoNegocioSelected(object sender, TappedEventArgs e)
    {
        string opcion = e.Parameter?.ToString();
        if (string.IsNullOrEmpty(opcion)) return;

        _tipoNegocio = opcion;

        // Resetear todos a gris/blanco
        FrameSede.BorderColor = Color.FromArgb("#E5E7EB");
        FrameSede.BackgroundColor = Colors.White;

        FrameDomicilio.BorderColor = Color.FromArgb("#E5E7EB");
        FrameDomicilio.BackgroundColor = Colors.White;

        FrameMixta.BorderColor = Color.FromArgb("#E5E7EB");
        FrameMixta.BackgroundColor = Colors.White;

        // Activar el seleccionado en azul
        Frame activo = opcion switch
        {
            "Sede" => FrameSede,
            "Domicilio" => FrameDomicilio,
            "Mixta" => FrameMixta,
            _ => null
        };

        if (activo != null)
        {
            activo.BorderColor = Color.FromArgb("#2563EB");
            activo.BackgroundColor = Color.FromArgb("#EFF6FF");
        }
    }

    // --- FORMATO TELÉFONO ---
    private void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (digits.Length > 8) digits = digits.Substring(0, 8);
        string formatted = digits.Length > 4 ? digits.Insert(4, "-") : digits;

        Device.BeginInvokeOnMainThread(() => {
            _isUpdating = true;
            EntryTelefono.Text = formatted;
            EntryTelefono.CursorPosition = formatted.Length;
            _isUpdating = false;
        });
    }

    // --- FORMATO CÉDULA ---
    private void OnCedulaChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        string clean = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (clean.Length > 13) clean = clean.Substring(0, 13);

        string formatted = clean;
        if (clean.Length > 4) formatted = formatted.Insert(4, "-");
        if (clean.Length > 8) formatted = formatted.Insert(9, "-");

        Device.BeginInvokeOnMainThread(() => {
            _isUpdating = true;
            EntryCedula.Text = formatted;
            EntryCedula.CursorPosition = formatted.Length;
            _isUpdating = false;
        });
    }

    // --- CONTRASEÑA ---
    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        ((ImageButton)sender).Source = _isPasswordVisible ? "eye_closed.png" : "eye_open.png";
    }

    // --- BOTONES ---
    private async void OnSubirPdfClicked(object sender, EventArgs e) => await DisplayAlert("Info", "Pendiente", "OK");
    private async void OnCancelarClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnCrearCuentaClicked(object sender, EventArgs e) => await DisplayAlert("Listo", "Datos validados", "OK");
}
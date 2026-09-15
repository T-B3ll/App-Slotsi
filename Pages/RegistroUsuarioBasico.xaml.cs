using System.Linq;

namespace slotsi_citas.Pages;

public partial class RegistroUsuarioBasico : ContentPage
{
    private bool _isUpdating = false;
    private bool _isPasswordVisible = false;

    public RegistroUsuarioBasico() => InitializeComponent();

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (digits.Length > 8) digits = digits.Substring(0, 8);

        string formatted = digits;
        if (digits.Length > 4) formatted = digits.Insert(4, "-");

        // Usamos BeginInvokeOnMainThread para evitar crash en Android
        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;
            EntryTelefono.Text = formatted;
            EntryTelefono.CursorPosition = formatted.Length;
            _isUpdating = false;
        });
    }

    private void OnCedulaChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string clean = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (clean.Length > 13) clean = clean.Substring(0, 13);

        string formatted = clean;
        if (clean.Length > 4) formatted = formatted.Insert(4, "-");
        if (clean.Length > 8) formatted = formatted.Insert(9, "-");

        // Usamos BeginInvokeOnMainThread para evitar crash en Android
        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;
            EntryCedula.Text = formatted;
            EntryCedula.CursorPosition = formatted.Length;
            _isUpdating = false;
        });
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;

        var btn = (ImageButton)sender;
        btn.Source = _isPasswordVisible ? "eye_closed.png" : "eye_open.png";
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
        => await DisplayAlert("Slotsi", "Registrando usuario...", "OK");
}
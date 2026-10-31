using slotsi_citas.ViewModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Pages;

public partial class RegistroUsuarioBasico : ContentPage
{
    private readonly UsuarioBasicoViewModel _viewModel;
    private bool _isUpdating = false;

    public RegistroUsuarioBasico(UsuarioBasicoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        var btn = (ImageButton)sender;
        btn.Source = PasswordEntry.IsPassword ? "eye_open.png" : "eye_closed.png";
    }

    private async void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        if (digits.Length > 8) digits = digits.Substring(0, 8);

        string formatted = digits;
        if (digits.Length > 4)
            formatted = digits.Insert(4, "-");

        if (EntryTelefono.Text == formatted) return;

        _isUpdating = true;

        await Task.Delay(10);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            EntryTelefono.Text = formatted;
            if (EntryTelefono.Text?.Length > 0)
            {
                EntryTelefono.CursorPosition = EntryTelefono.Text.Length;
            }
            _viewModel.Telefono = digits;
            _isUpdating = false;
        });
    }

    private void OnCedulaChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        _isUpdating = true;

        string clean = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

        if (clean.Length > 14) clean = clean.Substring(0, 14);

        if (clean.Length == 14 && char.IsDigit(clean[13]))
        {
            clean = clean.Substring(0, 13);
        }

        string formatted = clean;
        if (clean.Length > 3)
            formatted = formatted.Insert(3, "-");
        if (clean.Length > 9)
            formatted = formatted.Insert(10, "-");

        if (clean.Length >= 13)
        {
            EntryCedula.Keyboard = Keyboard.Text;
        }
        else
        {
            EntryCedula.Keyboard = Keyboard.Numeric;
        }

        Device.BeginInvokeOnMainThread(() =>
        {
            EntryCedula.Text = formatted;
            EntryCedula.CursorPosition = formatted.Length;
            _viewModel.Cedula = clean;
            _isUpdating = false;
        });
    }

    private bool EsContrasenaSegura(string contrasena, out string mensaje)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            mensaje = "La contraseña no puede estar vacía.";
            return false;
        }

        if (contrasena.Length < 8)
        {
            mensaje = "❌ Debe tener al menos 8 caracteres.";
            return false;
        }

        if (!contrasena.Any(char.IsUpper))
        {
            mensaje = "❌ Debe incluir al menos una letra MAYÚSCULA.";
            return false;
        }

        if (!contrasena.Any(char.IsLower))
        {
            mensaje = "❌ Debe incluir al menos una letra minúscula.";
            return false;
        }

        if (!contrasena.Any(char.IsDigit))
        {
            mensaje = "❌ Debe incluir al menos un NÚMERO.";
            return false;
        }

        if (!contrasena.Any(c => !char.IsLetterOrDigit(c)))
        {
            mensaje = "❌ Debe incluir al menos un SÍMBOLO (!@#$%^&*).";
            return false;
        }

        mensaje = "✅ Contraseña segura.";
        return true;
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var contrasena = PasswordEntry.Text ?? "";

        bool esSegura = EsContrasenaSegura(contrasena, out string mensaje);

        lblSeguridadPassword.Text = mensaje;
        lblSeguridadPassword.TextColor = esSegura ? Colors.Green : Colors.Red;
    }

    private async void OnRegistrarseClicked(object sender, EventArgs e)
    {
        var campos = new (string Nombre, string Valor)[]
        {
            ("nombre completo", _viewModel.NombreCompleto),
            ("correo electrónico", _viewModel.Correo),
            ("número de teléfono", EntryTelefono.Text),
            ("número de cédula", EntryCedula.Text),
            ("contraseña", PasswordEntry.Text)
        };

        foreach (var (nombre, valor) in campos)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                await DisplayAlert("Campo vacío", $"Por favor, ingresa tu {nombre}.", "OK");
                return;
            }
        }

        if (!_viewModel.Correo.Contains("@") || !_viewModel.Correo.Contains(".") || _viewModel.Correo.Length < 5)
        {
            await DisplayAlert("Correo inválido", "Por favor, ingresa un correo electrónico válido (ejemplo: usuario@gmail.com).", "OK");
            return;
        }

        if (!EsContrasenaSegura(PasswordEntry.Text, out string mensajePassword))
        {
            await DisplayAlert("Contraseña insegura", mensajePassword, "OK");
            PasswordEntry.Focus();
            return;
        }

        _viewModel.Telefono = EntryTelefono.Text?.Replace("-", "").Trim();
        _viewModel.Cedula = EntryCedula.Text?.Replace("-", "").Trim();
        _viewModel.Contrasena = PasswordEntry.Text;

        if (_viewModel.RegistrarCommand.CanExecute(null))
        {
            _viewModel.RegistrarCommand.Execute(null);
        }

        await Task.Delay(2000);

        try
        {
            await Navigation.PopToRootAsync();
        }
        catch
        {
            try
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("No se pudo navegar automáticamente al Login.");
            }
        }
    }
}
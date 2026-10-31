using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using System.Linq;
using System.Threading.Tasks;

namespace slotsi_citas.Pages;

public partial class EditarPerfilPage : ContentPage
{
    private readonly EditarPerfilViewModel _viewModel;

    private bool _isUpdatingTelefono = false;
    private bool _isUpdatingCedula = false;
    private bool _isUpdatingRuc = false;
    private bool _isPasswordVisible = false;

    public EditarPerfilPage(EditarPerfilViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        FrameSede.GestureRecognizers.Add(new TapGestureRecognizer
        { Command = new Command(() => SeleccionarModalidad("Sede")) });

        FrameDomicilio.GestureRecognizers.Add(new TapGestureRecognizer
        { Command = new Command(() => SeleccionarModalidad("Domicilio")) });

        FrameMixta.GestureRecognizers.Add(new TapGestureRecognizer
        { Command = new Command(() => SeleccionarModalidad("Mixta")) });

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(EditarPerfilViewModel.ModalidadActual))
            {
                ActualizarVisualizacionModalidad();
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(500);
        ActualizarVisualizacionModalidad();
    }

    private void SeleccionarModalidad(string modalidad)
    {
        var colorActivo = Color.FromArgb("#2563EB");
        var colorInactivo = Color.FromArgb("#E5E7EB");

        FrameSede.Stroke = modalidad == "Sede" ? colorActivo : colorInactivo;
        FrameDomicilio.Stroke = modalidad == "Domicilio" ? colorActivo : colorInactivo;
        FrameMixta.Stroke = modalidad == "Mixta" ? colorActivo : colorInactivo;

        _viewModel.ModalidadActual = modalidad;
    }

    private void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel.Usuario == null || string.IsNullOrEmpty(e.NewTextValue)) return;
        if (_isUpdatingTelefono || sender is not Entry entry) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());
        if (digits.Length > 8) digits = digits.Substring(0, 8);
        string formatted = digits.Length > 4 ? digits.Insert(4, "-") : digits;

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdatingTelefono = true;
            entry.Text = formatted;
            entry.CursorPosition = formatted.Length;
            _viewModel.Usuario.Telefono = digits;
            _isUpdatingTelefono = false;
        });
    }

    private void OnCedulaChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel.Usuario == null || string.IsNullOrEmpty(e.NewTextValue)) return;
        if (_isUpdatingCedula || sender is not Entry entry) return;

        string clean = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (clean.Length > 14) clean = clean.Substring(0, 14);
        if (clean.Length == 14 && char.IsDigit(clean[13])) clean = clean.Substring(0, 13);

        string formatted = clean;
        if (clean.Length > 3) formatted = formatted.Insert(3, "-");
        if (clean.Length > 9) formatted = formatted.Insert(10, "-");

        if (clean.Length >= 13)
            entry.Keyboard = Keyboard.Text;
        else
            entry.Keyboard = Keyboard.Numeric;

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdatingCedula = true;
            entry.Text = formatted;
            entry.CursorPosition = formatted.Length;
            _viewModel.Usuario.Cedula = clean;
            _isUpdatingCedula = false;
        });
    }

    private void OnRucChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingRuc || sender is not Entry entry) return;

        string clean = new string(e.NewTextValue.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
        if (clean.Length > 11) clean = clean.Substring(0, 11);

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdatingRuc = true;
            entry.Text = clean;
            entry.CursorPosition = clean.Length;
            _viewModel.Ruc = clean;
            _isUpdatingRuc = false;
        });
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        if (PasswordEntry != null)
        {
            PasswordEntry.IsPassword = !_isPasswordVisible;
        }
        ((ImageButton)sender).Source = _isPasswordVisible ? "eye_closed.png" : "eye_open.png";
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var contrasena = _viewModel.NuevaPassword ?? "";

        if (string.IsNullOrEmpty(contrasena))
        {
            lblSeguridadPassword.Text = "";
            return;
        }

        bool esSegura = EsContrasenaSegura(contrasena, out string mensaje);

        lblSeguridadPassword.Text = mensaje;
        lblSeguridadPassword.TextColor = esSegura ? Colors.Green : Colors.Red;
    }

    private void ActualizarVisualizacionModalidad()
    {
        var colorActivo = Color.FromArgb("#2563EB");
        var colorInactivo = Color.FromArgb("#E5E7EB");

        FrameSede.Stroke = colorInactivo;
        FrameDomicilio.Stroke = colorInactivo;
        FrameMixta.Stroke = colorInactivo;

        switch (_viewModel.ModalidadActual?.Trim().ToLower())
        {
            case "sede":
                FrameSede.Stroke = colorActivo;
                break;
            case "domicilio":
            case "a domicilio":
                FrameDomicilio.Stroke = colorActivo;
                break;
            case "mixta":
                FrameMixta.Stroke = colorActivo;
                break;
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
      
        PasswordEntry.Text = string.Empty;
        lblSeguridadPassword.Text = string.Empty;
        await _viewModel.CargarDatosAsync();
        await Navigation.PopAsync();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        string telefono = _viewModel.Usuario?.Telefono ?? "";
        if (!string.IsNullOrEmpty(telefono) && telefono.Length != 8)
        {
            await DisplayAlert("Teléfono inválido", "El número de teléfono debe tener exactamente 8 dígitos.", "OK");
            return;
        }

        string cedula = _viewModel.Usuario?.Cedula ?? "";
        if (!string.IsNullOrEmpty(cedula) && (cedula.Length != 14 || !char.IsLetter(cedula.Last())))
        {
            await DisplayAlert("Cédula inválida", "La cédula debe tener el formato 000-000000-0000A (14 caracteres, terminando en letra).", "OK");
            return;
        }

        string correo = _viewModel.Usuario?.Correo ?? "";
        if (!string.IsNullOrEmpty(correo) && !EsCorreoValido(correo))
        {
            await DisplayAlert("Correo inválido", "Por favor, ingresa un correo electrónico válido (ejemplo: usuario@gmail.com).", "OK");
            return;
        }

        string nuevaPass = _viewModel.NuevaPassword ?? "";
        if (!string.IsNullOrEmpty(nuevaPass))
        {
            if (!EsContrasenaSegura(nuevaPass, out string mensajePassword))
            {
                await DisplayAlert("Contraseña insegura", mensajePassword, "OK");
                return;
            }
        }

        if (_viewModel.GuardarCambiosCommand.CanExecute(null))
        {
            _viewModel.GuardarCambiosCommand.Execute(null);
        }
    }

    private bool EsCorreoValido(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(correo);
            return addr.Address == correo.Trim();
        }
        catch
        {
            return false;
        }
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
}
using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using Syncfusion.Maui.Toolkit.Carousel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace slotsi_citas.Pages;

public partial class RegistroUsuarioNegocio : ContentPage
{
    private bool _isUpdating = false;
    private bool _isPasswordVisible = false;
    private string _tipoNegocio = "";

    private RegistrarNegocioViewModel _viewModel;

    public RegistroUsuarioNegocio(RegistrarNegocioViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnTipoNegocioSelected(object sender, TappedEventArgs e)
    {
        string opcion = e.Parameter?.ToString();
        if (string.IsNullOrEmpty(opcion)) return;

        _viewModel.TipoServicio = opcion;

        FrameSede.BorderColor = Color.FromArgb("#E5E7EB");
        FrameSede.BackgroundColor = Colors.White;
        FrameDomicilio.BorderColor = Color.FromArgb("#E5E7EB");
        FrameDomicilio.BackgroundColor = Colors.White;
        FrameMixta.BorderColor = Color.FromArgb("#E5E7EB");
        FrameMixta.BackgroundColor = Colors.White;

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

    private async void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        if (digits.Length > 8) digits = digits.Substring(0, 8);

        string formatted = digits.Length > 4 ? digits.Insert(4, "-") : digits;

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
            _isUpdating = false;
        });

        _viewModel.Telefono = digits;
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
            _viewModel.CedulaJuridica = clean;
            _isUpdating = false;
        });
    }

    private void OnTipoNegocioChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        if (picker.SelectedItem != null)
        {
            _viewModel.CategoriaNegocio = picker.SelectedItem.ToString();
        }
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        ((ImageButton)sender).Source = _isPasswordVisible ? "eye_closed.png" : "eye_open.png";
    }

    private void OnPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var contrasena = PasswordEntry.Text ?? "";

        bool esSegura = EsContrasenaSegura(contrasena, out string mensaje);

        lblSeguridadPassword.Text = mensaje;
        lblSeguridadPassword.TextColor = esSegura ? Colors.Green : Colors.Red;
    }

    private async void OnSubirPdfClicked(object sender, EventArgs e)
    {
        _viewModel.SeleccionarDocumentoCommand.Execute(null);
    }

    private void SincronizarHorarioManual()
    {
        if (_viewModel.ListaDiasHorario.Count == 0) return;

        var mapeo = new[]
        {
            (CheckLun, TP_Lun_Abre, TP_Lun_Cierra, SwitchLun, TP_Lun_PausaIni, TP_Lun_PausaFin, 0),
            (CheckMar, TP_Mar_Abre, TP_Mar_Cierra, SwitchMar, TP_Mar_PausaIni, TP_Mar_PausaFin, 1),
            (CheckMie, TP_Mie_Abre, TP_Mie_Cierra, SwitchMie, TP_Mie_PausaIni, TP_Mie_PausaFin, 2),
            (CheckJue, TP_Jue_Abre, TP_Jue_Cierra, SwitchJue, TP_Jue_PausaIni, TP_Jue_PausaFin, 3),
            (CheckVie, TP_Vie_Abre, TP_Vie_Cierra, SwitchVie, TP_Vie_PausaIni, TP_Vie_PausaFin, 4),
            (CheckSab, TP_Sab_Abre, TP_Sab_Cierra, SwitchSab, TP_Sab_PausaIni, TP_Sab_PausaFin, 5),
            (CheckDom, TP_Dom_Abre, TP_Dom_Cierra, SwitchDom, TP_Dom_PausaIni, TP_Dom_PausaFin, 6)
        };

        foreach (var (check, tpAbre, tpCierra, swPausa, tpPausaIni, tpPausaFin, index) in mapeo)
        {
            if (index < _viewModel.ListaDiasHorario.Count)
            {
                var dia = _viewModel.ListaDiasHorario[index];
                dia.EstaActivo = check.IsChecked;
                dia.HoraApertura = tpAbre.Time;
                dia.HoraCierre = tpCierra.Time;
                dia.TienePausa = swPausa.IsToggled;
                dia.PausaInicio = tpPausaIni.Time;
                dia.PausaFin = tpPausaFin.Time;
            }
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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

    private async void OnCrearCuentaClicked(object sender, EventArgs e)
    {
        var campos = new (string Nombre, Entry Entry)[]
        {
            ("nombre completo", EntryNombre),
            ("número de teléfono", EntryTelefono),
            ("número de cédula", EntryCedula),
            ("correo electrónico", EntryCorreo),
            ("contraseña", PasswordEntry),
            ("ubicación exacta", EntryUbicacion),
            ("nombre del negocio", EntryNombreNegocio),
            ("número RUC", EntryRuc)
        };

        foreach (var (nombre, entry) in campos)
        {
            if (string.IsNullOrWhiteSpace(entry.Text))
            {
                await DisplayAlert("Campo vacío", $"Por favor, ingresa tu {nombre}.", "OK");
                entry.Focus();
                return;
            }
        }

        if (PickerTipoNegocio.SelectedItem == null)
        {
            await DisplayAlert("Campo vacío", "Por favor, selecciona el tipo de negocio (Barbería o Salón de Belleza).", "OK");
            PickerTipoNegocio.Focus();
            return;
        }

        if (!EsCorreoValido(EntryCorreo.Text))
        {
            await DisplayAlert("Correo inválido", "Por favor, ingresa un correo electrónico válido (ejemplo: usuario@gmail.com).", "OK");
            EntryCorreo.Focus();
            return;
        }

        if (!EsContrasenaSegura(PasswordEntry.Text, out string mensajePassword))
        {
            await DisplayAlert("Contraseña insegura", mensajePassword, "OK");
            PasswordEntry.Focus();
            return;
        }

        if (string.IsNullOrEmpty(_viewModel.UrlFotoPerfil))
        {
            await DisplayAlert("Foto faltante", "Debes subir una foto de perfil para tu negocio.", "OK");
            return;
        }

        if (!_viewModel.HasDocumentos)
        {
            await DisplayAlert("Documentos faltantes", "Debes subir al menos un documento de verificación (PDF).", "OK");
            return;
        }

        _viewModel.Nombre = EntryNombre.Text.Trim();
        _viewModel.Correo = EntryCorreo.Text.Trim();
        _viewModel.Telefono = EntryTelefono.Text?.Replace("-", "").Trim();
        _viewModel.Contrasena = PasswordEntry.Text;
        _viewModel.CedulaJuridica = EntryCedula.Text?.Replace("-", "").Trim();
        _viewModel.Direccion = EntryUbicacion.Text.Trim();
        _viewModel.NumeroRuc = EntryRuc.Text?.Trim();
        _viewModel.NombreNegocio = EntryNombreNegocio.Text.Trim();
        _viewModel.CategoriaNegocio = PickerTipoNegocio.SelectedItem.ToString();

        SincronizarHorarioManual();
        _viewModel.RegistrarCommand.Execute(null);
    }

    private async void OnSubirFotoClicked(object sender, EventArgs e)
    {
        _viewModel.SeleccionarFotoCommand.Execute(null);
    }
}
using Microsoft.Maui.Controls;
using slotsi_citas.ViewModel;
using Syncfusion.Maui.Toolkit.Carousel;
using System.Linq;


namespace slotsi_citas.Pages;

public partial class RegistroUsuarioNegocio : ContentPage
{
    private bool _isUpdating = false;
    private bool _isPasswordVisible = false;
    private string _tipoNegocio = "";





    private RegistrarNegocioViewModel _viewModel;


    public RegistroUsuarioNegocio()
    {
        InitializeComponent();

        _viewModel = new RegistrarNegocioViewModel();
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


            _viewModel.Telefono = digits;

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

        Device.BeginInvokeOnMainThread(() => {
            _isUpdating = true;
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
    private async void OnCrearCuentaClicked(object sender, EventArgs e)
    {

        _viewModel.Nombre = EntryNombre.Text;
        _viewModel.Correo = EntryCorreo.Text;
        _viewModel.Telefono = EntryTelefono.Text?.Replace("-", "");
        _viewModel.Contrasena = PasswordEntry.Text;
        _viewModel.CedulaJuridica = EntryCedula.Text?.Replace("-", "");
        _viewModel.Direccion = EntryUbicacion.Text;
        _viewModel.NumeroRuc = EntryRuc.Text;
        _viewModel.NombreNegocio = EntryNombreNegocio.Text;

        SincronizarHorarioManual();

        _viewModel.RegistrarCommand.Execute(null);
    }

    private async void OnSubirFotoClicked(object sender, EventArgs e)
    {
        _viewModel.SeleccionarFotoCommand.Execute(null);
    }
}
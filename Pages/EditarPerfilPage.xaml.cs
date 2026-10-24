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


        FrameSede.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => _viewModel.ModalidadActual = "Sede")
        });

        FrameDomicilio.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => _viewModel.ModalidadActual = "Domicilio")
        });

        FrameMixta.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => _viewModel.ModalidadActual = "Mixta")
        });

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
        if (clean.Length > 13) clean = clean.Substring(0, 13);
        string formatted = clean;
        if (clean.Length > 4) formatted = formatted.Insert(4, "-");
        if (clean.Length > 8) formatted = formatted.Insert(9, "-");

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

    private void ActualizarVisualizacionModalidad()
    {
        var colorActivo = Color.FromArgb("#2563EB");
        var colorInactivo = Color.FromArgb("#E5E7EB");

        // Resetea todos a gris primero
        FrameSede.Stroke = colorInactivo;
        FrameDomicilio.Stroke = colorInactivo;
        FrameMixta.Stroke = colorInactivo;

        // Pinta de azul solo el que coincide con la BD
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
        await Navigation.PopAsync();
    }
}
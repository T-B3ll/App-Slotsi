using slotsi_citas.ViewModel; 

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


    private void OnTelefonoChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        string digits = new string(e.NewTextValue.Where(char.IsDigit).ToArray());

        if (digits.Length > 8) digits = digits.Substring(0, 8);

        string formatted = digits;
        if (digits.Length > 4)
            formatted = digits.Insert(4, "-");
        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;
            EntryTelefono.Text = formatted;
            EntryTelefono.CursorPosition = formatted.Length;

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
        if (clean.Length > 9) formatted = formatted.Insert(9, "-");

        Device.BeginInvokeOnMainThread(() =>
        {
            _isUpdating = true;
            EntryCedula.Text = formatted;
            EntryCedula.CursorPosition = formatted.Length;

   
            _viewModel.Cedula = clean;

            _isUpdating = false;
        });
    }

    private async void OnRegistrarseClicked(object sender, EventArgs e)
    {
  
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
namespace slotsi_citas.Pages;
using slotsi_citas.Services;
using Microsoft.Maui.Controls;

public partial class NuevaContra : ContentPage
{
    private bool _isNewPassVisible = false;
    private bool _isConfirmPassVisible = false;


    private string _solicitudId;
    private string _correoUsuario;
    public NuevaContra()
    {
        InitializeComponent();
    }


    public NuevaContra(string solicitudId, string correo)
    {
        InitializeComponent();

        _solicitudId = solicitudId;
        _correoUsuario = correo;
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }


    private void OnToggleNewPasswordVisibility(object sender, EventArgs e)
    {
        _isNewPassVisible = !_isNewPassVisible;
        EntryNewPassword.IsPassword = !_isNewPassVisible;
        ((ImageButton)sender).Source = _isNewPassVisible ? "eye_closed.png" : "eye_open.png";
    }


    private void OnToggleConfirmPasswordVisibility(object sender, EventArgs e)
    {
        _isConfirmPassVisible = !_isConfirmPassVisible;
        EntryConfirmPassword.IsPassword = !_isConfirmPassVisible;
        ((ImageButton)sender).Source = _isConfirmPassVisible ? "eye_closed.png" : "eye_open.png";
    }


    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {

    }
    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        var recuperacionService = new RecuperacionService();

      
        await Navigation.PushAsync(new RecuperarContra(recuperacionService));
    }




    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var nuevaPass = EntryNewPassword.Text?.Trim();
        var confirmPass = EntryConfirmPassword.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nuevaPass) || nuevaPass.Length < 8)
        {
            await DisplayAlert("Atención", "La contraseña debe tener al menos 8 caracteres.", "OK");
            return;
        }

        if (nuevaPass != confirmPass)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        try
        {
            // Creamos las instancias de los servicios aquí mismo para no romper tu MauiProgram
            var usuarioService = new UsuarioService();
            var recuperacionService = new RecuperacionService();

            // Actualizamos la contraseña del usuario en Mongo
            await usuarioService.ActualizarContrasenaAsync(_correoUsuario, nuevaPass);

            // Marcamos el código de recuperación como usado para que no se pueda reutilizar
            await recuperacionService.MarcarComoUsadoAsync(_solicitudId);

            await DisplayAlert("Éxito", "Tu contraseña ha sido actualizada correctamente.", "OK");

            // Regresamos al inicio o login
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }

    }
}
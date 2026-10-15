namespace slotsi_citas.Pages;
using slotsi_citas.Services;
using Microsoft.Maui.Controls;
using System.Security.Cryptography;
using System.Text;

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


    private string EncriptarPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
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
            var usuarioService = Application.Current.Handler.MauiContext.Services.GetRequiredService<UsuarioService>(); var recuperacionService = new RecuperacionService();

            var usuario = await usuarioService.ObtenerPorCorreoAsync(_correoUsuario);

            if (usuario == null)
            {
                await DisplayAlert("Error", "No se encontró el usuario asociado a este correo.", "OK");
                return;
            }


            string passwordEncriptada = EncriptarPassword(nuevaPass);

          
            usuario.Contrasena = passwordEncriptada;
            await usuarioService.ActualizarAsync(usuario);

         
            await recuperacionService.MarcarComoUsadoAsync(_solicitudId);

            await DisplayAlert("Éxito", "Tu contraseña ha sido actualizada correctamente.", "OK");
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }
}
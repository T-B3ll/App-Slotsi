namespace slotsi_citas.Pages;

using slotsi_citas.Services.Api;
using Slotsi.Shared.Dtos;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

public partial class NuevaContra : ContentPage
{
    private bool _isNewPassVisible = false;
    private bool _isConfirmPassVisible = false;
    private string _correoUsuario;
    private string _token;

    public NuevaContra(string correo, string token)
    {
        InitializeComponent();
        _correoUsuario = correo;
        _token = token;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

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

    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e) { }

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
            var authService = Application.Current.Handler.MauiContext.Services.GetRequiredService<IAuthApiService>();

           
            var dto = new RestablecerContrasenaDto
            {
                Email = _correoUsuario,
                Token = _token,
                NuevaContrasena = nuevaPass
            };

            await authService.RestablecerContrasenaAsync(dto);

            await DisplayAlert("Éxito", "Tu contraseña ha sido actualizada correctamente.", "OK");
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }
}
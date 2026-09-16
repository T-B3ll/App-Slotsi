namespace slotsi_citas.Pages;

public partial class NuevaContra : ContentPage
{
    private bool _isNewPassVisible = false;
    private bool _isConfirmPassVisible = false;

    public NuevaContra()
    {
        InitializeComponent();
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
        await Navigation.PushAsync(new RecuperarContra());
    }




    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EntryNewPassword.Text) || string.IsNullOrWhiteSpace(EntryConfirmPassword.Text))
        {
            await DisplayAlert("Error", "Por favor completa ambos campos.", "OK");
            return;
        }

        if (EntryNewPassword.Text != EntryConfirmPassword.Text)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        if (EntryNewPassword.Text.Length < 8)
        {
            await DisplayAlert("Error", "La contraseña debe tener al menos 8 caracteres.", "OK");
            return;
        }


        await DisplayAlert("Éxito", "Contraseña actualizada correctamente.", "OK");

    }
}
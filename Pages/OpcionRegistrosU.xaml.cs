namespace slotsi_citas.Pages;

using slotsi_citas.Services;
using slotsi_citas.ViewModel;
using Microsoft.Extensions.DependencyInjection;

public partial class OpcionRegistrosU : ContentPage
{
    private bool _isNavigating = false;

    public OpcionRegistrosU()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;
        try { await Navigation.PopAsync(); }
        finally { _isNavigating = false; }
    }

    private async void OnBasicUserSelected(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            var pagina = Application.Current?.Handler?.MauiContext?.Services
                .GetRequiredService<RegistroUsuarioBasico>();

            if (pagina != null)
            {
                await Navigation.PushAsync(pagina, false);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el registro: {ex.Message}", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async void OnBusinessOwnerSelected(object sender, EventArgs e)
    {
        if (_isNavigating) return;
        _isNavigating = true;

        try
        {
            await Task.Yield();

            var pagina = Application.Current?.Handler?.MauiContext?.Services
                .GetRequiredService<RegistroUsuarioNegocio>();

            if (pagina != null)
            {
                await Navigation.PushAsync(pagina, false);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo abrir el registro: {ex.Message}", "OK");
        }
        finally
        {
            _isNavigating = false;
        }
    }
}
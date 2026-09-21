using Microsoft.Extensions.DependencyInjection;
using slotsi_citas.Pages;
using slotsi_citas.ViewModel;
using System;
namespace slotsi_citas.Pages;

public partial class LoginPage : ContentPage
{

    private readonly IServiceProvider _serviceProvider;
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent(); 
        BindingContext = viewModel;
    }
    public LoginPage(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _serviceProvider = serviceProvider;
    }
    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
       
        var paginaRecuperar = _serviceProvider.GetRequiredService<RecuperarContra>();

        await Navigation.PushAsync(paginaRecuperar);

    }

    private async void OnCreateAccountTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OpcionRegistrosU());
    }
    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        
      
    }

}
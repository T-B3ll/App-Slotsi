using System;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.ViewModel;
using slotsi_citas.Services;

namespace slotsi_citas.Pages;

public partial class RegistroUsuariosPage : ContentPage
{
    private RegistroUsuariosViewModel? ViewModel => BindingContext as RegistroUsuariosViewModel;

    public RegistroUsuariosPage()
    {
        InitializeComponent();
        BindingContext = new RegistroUsuariosViewModel();
    }

    public RegistroUsuariosPage(RegistroUsuariosViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (ViewModel != null)
        {
            await ViewModel.CargarUsuariosBDAsync();
        }
    }

    private void OnLetraTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is InicialLetraModel letraModel)
        {
            ViewModel?.OnFiltrarPorInicial(letraModel);
        }
    }
}
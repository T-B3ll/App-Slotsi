using System;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class Cita_UsuarioPage : ContentPage
{
    private Cita_UsuarioViewModel? ViewModel => BindingContext as Cita_UsuarioViewModel;

    public Cita_UsuarioPage()
    {
        InitializeComponent();
    }

    public Cita_UsuarioPage(Cita_UsuarioViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel?.CargarCitasUsuario();
    }

    private void OnSemanaAnteriorClicked(object sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.FechaSeleccionada = ViewModel.FechaSeleccionada.AddDays(-7);
        }
    }

    private void OnSemanaSiguienteClicked(object sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.FechaSeleccionada = ViewModel.FechaSeleccionada.AddDays(7);
        }
    }

    private void OnDiaSemanaTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is DiaSemanaModel diaModel)
        {
            if (ViewModel != null)
            {
                ViewModel.FechaSeleccionada = diaModel.Fecha;
            }
        }
    }
}
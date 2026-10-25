using System;
using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages;

public partial class Cita_UsuarioPage : ContentPage
{
    private CitasClienteViewModel? ViewModel => BindingContext as CitasClienteViewModel ;

    public Cita_UsuarioPage()
    {
        InitializeComponent();
    }

    public Cita_UsuarioPage(CitasClienteViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
       
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
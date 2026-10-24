using Microsoft.Maui.Controls;
using slotsi_citas.Models;
using slotsi_citas.Services;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages
{
    public partial class EditarServicio : ContentPage
    {
        public EditarServicio(Servicios? servicio, CatalogoService catalogoService, TrabajadorService trabajadorService, string negocioId = "")
        {
            InitializeComponent();
            BindingContext = new EditarServicioViewModel(servicio, catalogoService, trabajadorService, negocioId);
        }
    }
}
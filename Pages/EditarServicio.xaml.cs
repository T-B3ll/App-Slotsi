using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.ViewModel;

namespace slotsi_citas.Pages 
{ 

 public partial class EditarServicio : ContentPage
 {
	public EditarServicio(Servicios servicio, IMongoCollection<Servicios> coleccion)
	{
		InitializeComponent();
        BindingContext = new EditarServicioViewModel(servicio, coleccion);
    }
 }
}
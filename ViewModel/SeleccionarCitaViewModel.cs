using System.Collections.ObjectModel;
using System.Windows.Input;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel;

public class SeleccionarCitaViewModel
{
    public string ClienteNombre { get; set; } = "Juan Pérez";
    public string SucursalTexto { get; set; } = "Cita en Barbería Santa Cruz";

    public ObservableCollection<ServicioItem> Servicios { get; set; }
    public ObservableCollection<DetallePedidoItem> DetallesPedido { get; set; }
    public ObservableCollection<MetodoPagoItem> MetodosPago { get; set; }

    public ICommand ConfirmarCitaCommand { get; }
    public ICommand AgregarExtrasCommand { get; }

    public SeleccionarCitaViewModel()
    {
        Servicios = new ObservableCollection<ServicioItem>
        {
            new ServicioItem { Nombre = "Corte Clásico", EsSeleccionado = false },
            new ServicioItem { Nombre = "Barba y Toque (Hot Towel)", EsSeleccionado = true },
            new ServicioItem { Nombre = "Corte + Barba", EsSeleccionado = false }
        };

        DetallesPedido = new ObservableCollection<DetallePedidoItem>
        {
            new DetallePedidoItem
            {
                Tag = "Recomendado",
                Titulo = "Corte Clásico",
                Duracion = "Duración: 30-40 min",
                PlaceholderImagen = "Imagen de un estilista realizando un corte clásico"
            },
            new DetallePedidoItem
            {
                Tag = "",
                Titulo = "Barba y Toque (Hot Towel)",
                Duracion = "Duración: 20-30 min",
                PlaceholderImagen = "Imagen de vapor/hot towel sobre la barba"
            }
        };

        MetodosPago = new ObservableCollection<MetodoPagoItem>
        {
            new MetodoPagoItem { Titulo = "Tarjeta de crédito/débito", EsSeleccionado = true },
            new MetodoPagoItem { Titulo = "Transferencia bancaria", EsSeleccionado = false }
        };

        ConfirmarCitaCommand = new Command(async () =>
        {
            await Application.Current.MainPage.DisplayAlert("Slotsi", "¡Cita confirmada con éxito!", "OK");
        });

        AgregarExtrasCommand = new Command(async () =>
        {
            await Application.Current.MainPage.DisplayAlert("Slotsi", "Sección de agregar extras", "OK");
        });
    }
}
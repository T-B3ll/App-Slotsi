using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel;

[QueryProperty(nameof(HorarioSeleccionado), "HorarioSeleccionado")]
[QueryProperty(nameof(DireccionCliente), "DireccionCliente")]
public class SeleccionarCitaViewModel : BindableObject
{
    private string _clienteNombre = "Juan Pérez";
    private string _sucursalTexto = "Sucursal Central • Matagalpa";
    private RangoHorario? _horarioSeleccionado;
    private string _direccionCliente = string.Empty;

    public string ClienteNombre
    {
        get => _clienteNombre;
        set { _clienteNombre = value; OnPropertyChanged(); }
    }

    public string SucursalTexto
    {
        get => _sucursalTexto;
        set { _sucursalTexto = value; OnPropertyChanged(); }
    }

    public RangoHorario? HorarioSeleccionado
    {
        get => _horarioSeleccionado;
        set
        {
            _horarioSeleccionado = value;
            OnPropertyChanged();
        }
    }

    public string DireccionCliente
    {
        get => _direccionCliente;
        set
        {
            _direccionCliente = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ServicioItem> Servicios { get; set; }
    public ObservableCollection<DetallePedidoItem> DetallesPedido { get; set; }
    public ObservableCollection<MetodoPagoItem> MetodosPago { get; set; }

    public ICommand VolverCommand { get; }
    public ICommand AgregarExtrasCommand { get; }
    public ICommand ConfirmarCitaCommand { get; }

    public SeleccionarCitaViewModel()
    {
        _clienteNombre = Preferences.Default.Get("nombre_usuario", "Juan Pérez");

        // Comando de navegación defensivo para prevenir NullReferenceException en Shell.Current
        VolverCommand = new Command(async () =>
        {
            try
            {
                // 1. Obtener SÓLO la pantalla activa superior (no toda la aplicación)
                var paginaActiva = Shell.Current?.CurrentPage;

                if (paginaActiva != null)
                {
                    // 2. Desvanecer solo la pantalla de Seleccionar Cita
                    await paginaActiva.FadeTo(0, 150, Easing.CubicOut);
                }

                // 3. Ejecutar la navegación hacia atrás
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..", false);
                }
                else
                {
                    var nav = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;
                    if (nav != null)
                    {
                        await nav.PopAsync(false);
                    }
                }
            }
            catch
            {
                // Resguardo de seguridad en caso de error de rutas
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
        });

        AgregarExtrasCommand = new Command(() => { /* Lógica para agregar extras */ });
        ConfirmarCitaCommand = new Command(() => { /* Lógica para confirmar cita */ });

        Servicios = new ObservableCollection<ServicioItem>
        {
            new ServicioItem { Nombre = "Corte + Barba", EsSeleccionado = true },
            new ServicioItem { Nombre = "Solo Corte", EsSeleccionado = false },
            new ServicioItem { Nombre = "Perfilado de Barba", EsSeleccionado = false }
        };

        DetallesPedido = new ObservableCollection<DetallePedidoItem>
        {
            new DetallePedidoItem { Titulo = "Corte Clásico", Duracion = "30 min", Tag = "Popular", PlaceholderImagen = "Imagen Servicio 1" },
            new DetallePedidoItem { Titulo = "Barba Completa", Duracion = "20 min", Tag = "", PlaceholderImagen = "Imagen Servicio 2" }
        };

        MetodosPago = new ObservableCollection<MetodoPagoItem>
        {
            new MetodoPagoItem { Titulo = "Tarjeta (Débito/Crédito)", Icono = "💳", EsSeleccionado = true },
            new MetodoPagoItem { Titulo = "Efectivo en local", Icono = "💵", EsSeleccionado = false }
        };
    }
}
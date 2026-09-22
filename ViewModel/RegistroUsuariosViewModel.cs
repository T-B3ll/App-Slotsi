using System.Collections.ObjectModel;
using System.Windows.Input;
using slotsi_citas.Models;

namespace slotsi_citas.ViewModel;

public class RegistroUsuariosViewModel
{
    public ObservableCollection<UsuarioItem> Usuarios { get; set; }

    public ICommand VerDetallesCommand { get; }
    public ICommand ImportarListaCommand { get; }
    public ICommand RegistrarNuevoCommand { get; }

    public RegistroUsuariosViewModel()
    {
        Usuarios = new ObservableCollection<UsuarioItem>
        {
            new UsuarioItem { Nombre = "Ana Pérez", Email = "ana.perez@email.com", Id = 1001 },
            new UsuarioItem { Nombre = "Bruno López", Email = "bruno.lopez@email.com", Id = 1002 },
            new UsuarioItem { Nombre = "Carla Medina", Email = "carla.medina@email.com", Id = 1003 },
            new UsuarioItem { Nombre = "Diego Romero", Email = "diego.romero@email.com", Id = 1004 },
            new UsuarioItem { Nombre = "Elena Torres", Email = "elena.torres@email.com", Id = 1005 }
        };

        VerDetallesCommand = new Command(async () =>
            await Application.Current.MainPage.DisplayAlert("Slotsi", "Ver detalles seleccionado", "OK"));

        ImportarListaCommand = new Command(async () =>
            await Application.Current.MainPage.DisplayAlert("Slotsi", "Importar lista seleccionado", "OK"));

        RegistrarNuevoCommand = new Command(async () =>
            await Application.Current.MainPage.DisplayAlert("Slotsi", "Registrar nuevo usuario seleccionado", "OK"));
    }
}
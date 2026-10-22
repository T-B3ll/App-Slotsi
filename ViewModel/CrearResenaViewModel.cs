using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static slotsi_citas.Models.Calificaciones;

namespace slotsi_citas.ViewModel
{
    public class CrearResenaViewModel : BindableObject
    {
        private readonly IMongoCollection<BsonDocument> _coleccionCitas;
        private readonly IMongoCollection<BsonDocument> _coleccionNegocios;
        private readonly IMongoCollection<CalificacionModel> _coleccionCalificaciones;

        private bool _esDuenoNegocio;

        public ObservableCollection<CitaElegibleModel> CitasElegibles { get; set; } = new();

        private CitaElegibleModel? _citaSeleccionada;
        public CitaElegibleModel? CitaSeleccionada
        {
            get => _citaSeleccionada;
            set
            {
                _citaSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneCitaSeleccionada));
            }
        }

        public bool TieneCitaSeleccionada => CitaSeleccionada != null;

        private int _puntuacion = 5;
        public int Puntuacion
        {
            get => _puntuacion;
            set
            {
                _puntuacion = Math.Clamp(value, 1, 5);
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstrellasSeleccionadas));
            }
        }
        public string EstrellasSeleccionadas => new string('★', Puntuacion) + new string('☆', 5 - Puntuacion);

        private string _comentario = string.Empty;
        public string Comentario
        {
            get => _comentario;
            set { _comentario = value; OnPropertyChanged(); }
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        public ICommand SeleccionarCitaCommand { get; }
        public ICommand CambiarPuntuacionCommand { get; }
        public ICommand GuardarResenaCommand { get; }

        public CrearResenaViewModel()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);

            _coleccionCitas = database.GetCollection<BsonDocument>("Citas");
            _coleccionNegocios = database.GetCollection<BsonDocument>("Negocios");
            _coleccionCalificaciones = database.GetCollection<CalificacionModel>("Calificaciones");

            SeleccionarCitaCommand = new Microsoft.Maui.Controls.Command<CitaElegibleModel>(OnSeleccionarCita);
            CambiarPuntuacionCommand = new Microsoft.Maui.Controls.Command<string>(OnCambiarPuntuacion);
            GuardarResenaCommand = new Command(async () => await GuardarResenaAsync());
        }

        public async Task CargarCitasCompletadasAsync()
        {
            try
            {
                EstaCargando = true;
                string usuarioIdSesion = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                if (string.IsNullOrWhiteSpace(usuarioIdSesion))
                {
                    CitasElegibles.Clear();
                    return;
                }

                var builder = Builders<BsonDocument>.Filter;

                var filtroNegocioCheck = builder.Or(
                    builder.Eq("usuario_id", usuarioIdSesion),
                    builder.Eq("id_usuario", usuarioIdSesion),
                    builder.Eq("UsuarioId", usuarioIdSesion)
                );
                if (ObjectId.TryParse(usuarioIdSesion, out var userObjCheck))
                {
                    filtroNegocioCheck = builder.Or(
                        filtroNegocioCheck,
                        builder.Eq("usuario_id", userObjCheck),
                        builder.Eq("id_usuario", userObjCheck),
                        builder.Eq("UsuarioId", userObjCheck),
                        builder.Eq("_id", userObjCheck)
                    );
                }
                else
                {
                    filtroNegocioCheck = builder.Or(
                        filtroNegocioCheck,
                        builder.Eq("_id", usuarioIdSesion)
                    );
                }

                var negocioDoc = await _coleccionNegocios.Find(filtroNegocioCheck).FirstOrDefaultAsync();
                _esDuenoNegocio = (negocioDoc != null);

                DateTime ahoraUtc = DateTime.UtcNow;
                DateTime limiteUnMesAtras = ahoraUtc.AddDays(-30);

                FilterDefinition<BsonDocument> filtroUsuario;

                if (_esDuenoNegocio)
                {
                    string negocioId = negocioDoc["_id"].ToString() ?? usuarioIdSesion;
                    var possibleNegocioIds = new List<string> { negocioId, usuarioIdSesion };

                    var listNegocioFilters = new List<FilterDefinition<BsonDocument>>();
                    foreach (var id in possibleNegocioIds.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        listNegocioFilters.Add(builder.Eq("NegocioId", id));
                        listNegocioFilters.Add(builder.Eq("negocio_id", id));
                        if (ObjectId.TryParse(id, out var objId))
                        {
                            listNegocioFilters.Add(builder.Eq("NegocioId", objId));
                            listNegocioFilters.Add(builder.Eq("negocio_id", objId));
                        }
                    }
                    filtroUsuario = builder.Or(listNegocioFilters);
                }
                else
                {
                    if (ObjectId.TryParse(usuarioIdSesion, out var clienteObjId))
                    {
                        filtroUsuario = builder.Or(
                            builder.Eq("ClienteId", clienteObjId),
                            builder.Eq("ClienteId", usuarioIdSesion),
                            builder.Eq("cliente_id", clienteObjId),
                            builder.Eq("cliente_id", usuarioIdSesion)
                        );
                    }
                    else
                    {
                        filtroUsuario = builder.Or(
                            builder.Eq("ClienteId", usuarioIdSesion),
                            builder.Eq("cliente_id", usuarioIdSesion)
                        );
                    }
                }

                var filtroFecha = builder.And(
                    builder.Gte("FechaHoraInicio", limiteUnMesAtras),
                    builder.Lte("FechaHoraInicio", ahoraUtc)
                );

                var filtroFinal = builder.And(filtroUsuario, filtroFecha);

                var docsCitas = await _coleccionCitas.Find(filtroFinal).SortByDescending(c => c["FechaHoraInicio"]).ToListAsync();

                var listaElegibles = new List<CitaElegibleModel>();

                foreach (var doc in docsCitas)
                {
                    string citaId = doc.Contains("_id") ? doc["_id"].ToString() ?? "" : "";
                    string negocioId = doc.Contains("NegocioId") ? doc["NegocioId"].ToString() ?? "" : (doc.Contains("negocio_id") ? doc["negocio_id"].ToString() ?? "" : "");
                    string trabajadorId = doc.Contains("TrabajadorId") ? doc["TrabajadorId"].ToString() ?? "" : (doc.Contains("trabajador_id") ? doc["trabajador_id"].ToString() ?? "" : "");
                    string servicioNombre = doc.Contains("ServicioNombre") ? doc["ServicioNombre"].AsString : "Servicio";
                    DateTime fechaHora = doc.Contains("FechaHoraInicio") ? doc["FechaHoraInicio"].ToUniversalTime() : DateTime.UtcNow;

                    string clienteNombre = "Cliente";
                    if (doc.Contains("Cliente") && doc["Cliente"].IsBsonDocument)
                    {
                        var objCliente = doc["Cliente"].AsBsonDocument;
                        if (objCliente.Contains("Nombre")) clienteNombre = objCliente["Nombre"].AsString;
                    }

                    string nombreNegocio = "Negocio";
                    if (!string.IsNullOrEmpty(negocioId))
                    {
                        FilterDefinition<BsonDocument> filtroNegocio = ObjectId.TryParse(negocioId, out var negObj)
                            ? builder.Eq("_id", negObj)
                            : builder.Eq("_id", negocioId);

                        var docNegocio = await _coleccionNegocios.Find(filtroNegocio).FirstOrDefaultAsync();
                        if (docNegocio != null)
                        {
                            if (docNegocio.Contains("nombre_negocio")) nombreNegocio = docNegocio["nombre_negocio"].AsString;
                            else if (docNegocio.Contains("NombreNegocio")) nombreNegocio = docNegocio["NombreNegocio"].AsString;
                            else if (docNegocio.Contains("nombre")) nombreNegocio = docNegocio["nombre"].AsString;
                        }
                    }

                    listaElegibles.Add(new CitaElegibleModel
                    {
                        CitaId = citaId,
                        NegocioId = negocioId,
                        TrabajadorId = trabajadorId,
                        ServicioNombre = servicioNombre,
                        NombreNegocio = nombreNegocio,
                        NombreCliente = clienteNombre,
                        FechaHora = fechaHora.ToLocalTime(),
                        EsDuenoNegocio = _esDuenoNegocio,
                        EstaSeleccionada = false
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CitasElegibles.Clear();
                    foreach (var item in listaElegibles)
                    {
                        CitasElegibles.Add(item);
                    }

                    if (CitasElegibles.Any())
                    {
                        var primeraCita = CitasElegibles.First();
                        primeraCita.EstaSeleccionada = true;
                        CitaSeleccionada = primeraCita;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CrearResena] Error: {ex.Message}");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private void OnSeleccionarCita(CitaElegibleModel cita)
        {
            if (cita == null) return;

            CitaSeleccionada = cita;
            foreach (var item in CitasElegibles)
            {
                item.EstaSeleccionada = (item.CitaId == cita.CitaId);
            }
        }

        private void OnCambiarPuntuacion(string ptsStr)
        {
            if (int.TryParse(ptsStr, out int pts))
            {
                Puntuacion = pts;
            }
        }

        private async Task GuardarResenaAsync()
        {
            if (CitaSeleccionada == null)
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Selecciona una cita para evaluar.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Comentario))
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Escribe un comentario sobre la atención recibida.", "OK");
                return;
            }

            try
            {
                string usuarioIdSesion = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                var nuevaResena = new CalificacionModel
                {
                    Id = null,
                    IdNegocio = CitaSeleccionada.NegocioId,
                    IdColaborador = string.IsNullOrEmpty(CitaSeleccionada.TrabajadorId) ? null : CitaSeleccionada.TrabajadorId,
                    IdUsuarioCliente = usuarioIdSesion,
                    RangoCalificacion = Puntuacion,
                    NegocioCalificado = !_esDuenoNegocio,
                    NombreNegocio = CitaSeleccionada.NombreNegocio,
                    NombreUsuario = CitaSeleccionada.NombreCliente,
                    Comentario = Comentario.Trim()
                };

                await _coleccionCalificaciones.InsertOneAsync(nuevaResena);

                await Application.Current.MainPage.DisplayAlert("¡Gracias!", "Tu reseña ha sido registrada exitosamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo guardar la reseña: {ex.Message}", "OK");
            }
        }
    }

    public class CitaElegibleModel : BindableObject
    {
        public string CitaId { get; set; } = string.Empty;
        public string NegocioId { get; set; } = string.Empty;
        public string TrabajadorId { get; set; } = string.Empty;
        public string ServicioNombre { get; set; } = string.Empty;
        public string NombreNegocio { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string FechaTexto => FechaHora.ToString("dd 'de' MMM, yyyy - hh:mm tt");
        public bool EsDuenoNegocio { get; set; }
        public string TituloCard => EsDuenoNegocio ? NombreCliente : NombreNegocio;
        public string TextoEvaluacionResumen => EsDuenoNegocio ? "Evaluando al cliente:" : "Evaluando atención en:";
        public string NombreEvaluadoResumen => EsDuenoNegocio ? NombreCliente : NombreNegocio;

        private bool _estaSeleccionada;
        public bool EstaSeleccionada
        {
            get => _estaSeleccionada;
            set
            {
                if (_estaSeleccionada != value)
                {
                    _estaSeleccionada = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ColorBordeCard));
                    OnPropertyChanged(nameof(GrosorBorde));
                }
            }
        }

        public Brush ColorBordeCard => EstaSeleccionada ? new SolidColorBrush(Color.FromArgb("#2563EB")) : new SolidColorBrush(Color.FromArgb("#CBD5E1"));
        public double GrosorBorde => EstaSeleccionada ? 2.5 : 1.0;
    }
}
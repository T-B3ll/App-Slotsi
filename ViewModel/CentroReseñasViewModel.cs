using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using slotsi_citas.Models;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static slotsi_citas.Models.Calificaciones;

namespace slotsi_citas.ViewModel
{
    public enum TipoRolUsuario
    {
        Cliente,
        Negocio
    }

    public class CentroReseñasViewModel : BindableObject
    {
        private readonly IMongoCollection<CalificacionModel> _coleccionCalificaciones;
        private readonly IMongoCollection<BsonDocument> _coleccionNegociosBson;

        public ObservableCollection<CalificacionModel> ListaCalificaciones { get; set; } = new();

        private TipoRolUsuario _rolActual = TipoRolUsuario.Cliente;
        private bool _mostrarRecibidas = true;

        public bool MostrarRecibidas
        {
            get => _mostrarRecibidas;
            set
            {
                _mostrarRecibidas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColorBotonRecibidas));
                OnPropertyChanged(nameof(ColorBotonRealizadas));
                OnPropertyChanged(nameof(TextoBotonRecibidas));
                OnPropertyChanged(nameof(TextoBotonRealizadas));
                _ = CargarCalificacionesAsync();
            }
        }

        private int _totalCalificaciones;
        public int TotalCalificaciones
        {
            get => _totalCalificaciones;
            set { _totalCalificaciones = value; OnPropertyChanged(); }
        }

        private double _promedioCalificacion;
        public double PromedioCalificacion
        {
            get => _promedioCalificacion;
            set { _promedioCalificacion = value; OnPropertyChanged(); }
        }

        public Color ColorBotonRecibidas => _mostrarRecibidas ? Color.FromArgb("#1976D2") : Color.FromArgb("#E0E0E0");
        public Color ColorBotonRealizadas => !_mostrarRecibidas ? Color.FromArgb("#1976D2") : Color.FromArgb("#E0E0E0");
        public Color TextoBotonRecibidas => _mostrarRecibidas ? Colors.White : Color.FromArgb("#444444");
        public Color TextoBotonRealizadas => !_mostrarRecibidas ? Colors.White : Color.FromArgb("#444444");

        public ICommand SeleccionarPestanaCommand { get; }

        public CentroReseñasViewModel()
        {
            var client = new MongoClient(MongoDbSettings.ConnectionString);
            var database = client.GetDatabase(MongoDbSettings.DatabaseName);

            _coleccionCalificaciones = database.GetCollection<CalificacionModel>("Calificaciones");
            _coleccionNegociosBson = database.GetCollection<BsonDocument>("Negocios");

            SeleccionarPestanaCommand = new Microsoft.Maui.Controls.Command<string>((pestana) =>
            {
                MostrarRecibidas = (pestana == "Recibidas");
            });
        }

        public async Task CargarCalificacionesAsync()
        {
            try
            {
                bool esDuenoNegocio = Preferences.Get("EsDuenoNegocio", false);
                _rolActual = esDuenoNegocio ? TipoRolUsuario.Negocio : TipoRolUsuario.Cliente;

                string usuarioIdActivo = Preferences.Get("UsuarioIdSesion", Preferences.Get("UsuarioId", string.Empty));

                if (string.IsNullOrWhiteSpace(usuarioIdActivo))
                {
                    LimpiarLista();
                    return;
                }

                FilterDefinition<CalificacionModel> filtroFinal;

                if (_rolActual == TipoRolUsuario.Negocio)
                {
                    var builderNegocio = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> filtroUsuarioNegocio;

                    if (ObjectId.TryParse(usuarioIdActivo, out var userObj))
                    {
                        filtroUsuarioNegocio = builderNegocio.Or(
                            builderNegocio.Eq("usuario_id", userObj),
                            builderNegocio.Eq("usuario_id", usuarioIdActivo),
                            builderNegocio.Eq("id_usuario", userObj),
                            builderNegocio.Eq("id_usuario", usuarioIdActivo),
                            builderNegocio.Eq("UsuarioId", userObj),
                            builderNegocio.Eq("UsuarioId", usuarioIdActivo)
                        );
                    }
                    else
                    {
                        filtroUsuarioNegocio = builderNegocio.Or(
                            builderNegocio.Eq("usuario_id", usuarioIdActivo),
                            builderNegocio.Eq("id_usuario", usuarioIdActivo),
                            builderNegocio.Eq("UsuarioId", usuarioIdActivo)
                        );
                    }

                    var negocioDoc = await _coleccionNegociosBson.Find(filtroUsuarioNegocio).FirstOrDefaultAsync();

                    string negocioId = negocioDoc != null ? negocioDoc["_id"].ToString() : usuarioIdActivo;
                    bool estadoCalificado = MostrarRecibidas;

                    var builderCal = Builders<CalificacionModel>.Filter;
                    filtroFinal = builderCal.And(
                        builderCal.Or(
                            builderCal.Eq(c => c.IdNegocio, negocioId),
                            ObjectId.TryParse(negocioId, out var negObj)
                                ? builderCal.Eq("id_negocio", negObj)
                                : builderCal.Eq("id_negocio", negocioId)
                        ),
                        builderCal.Eq(c => c.NegocioCalificado, estadoCalificado)
                    );
                }
                else
                {
                    bool estadoCalificado = !MostrarRecibidas;

                    var builderCal = Builders<CalificacionModel>.Filter;
                    filtroFinal = builderCal.And(
                        builderCal.Or(
                            builderCal.Eq(c => c.IdUsuarioCliente, usuarioIdActivo),
                            ObjectId.TryParse(usuarioIdActivo, out var cliObj)
                                ? builderCal.Eq("id_usuario_cliente", cliObj)
                                : builderCal.Eq("id_usuario_cliente", usuarioIdActivo)
                        ),
                        builderCal.Eq(c => c.NegocioCalificado, estadoCalificado)
                    );
                }

                var lista = await _coleccionCalificaciones.Find(filtroFinal).ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ListaCalificaciones.Clear();

                    if (lista != null && lista.Count > 0)
                    {
                        foreach (var item in lista)
                        {
                            if (_rolActual == TipoRolUsuario.Negocio)
                            {
                                item.TituloMostrado = $"Cliente: {item.NombreUsuario}";
                                item.SubtituloMostrado = $"Negocio: {item.NombreNegocio}";
                            }
                            else
                            {
                                item.TituloMostrado = $"Negocio: {item.NombreNegocio}";
                                item.SubtituloMostrado = $"Cliente: {item.NombreUsuario}";
                            }

                            ListaCalificaciones.Add(item);
                        }
                        TotalCalificaciones = lista.Count;
                        PromedioCalificacion = Math.Round(lista.Average(c => c.RangoCalificacion), 1);
                    }
                    else
                    {
                        LimpiarLista();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CentroReseñas] Error: {ex.Message}");
            }
        }

        private void LimpiarLista()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ListaCalificaciones.Clear();
                TotalCalificaciones = 0;
                PromedioCalificacion = 0;
            });
        }
    }
}
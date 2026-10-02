using MongoDB.Driver;
using slotsi_citas.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static slotsi_citas.Models.Calificaciones;
using System.Diagnostics;

namespace slotsi_citas.ViewModel
{
    class CentroCalificacionesViewModel
    {
        public ObservableCollection<CalificacionModel> HistorialCalificaciones { get; set; }
            = new ObservableCollection<CalificacionModel>();

        public CentroCalificacionesViewModel()
        {
            // Carga automáticamente los datos al abrir la pantalla
            _ = CargarCalificacionesDesdeMongoDB();
        }

        private async Task CargarCalificacionesDesdeMongoDB()
        {
            try
            {
                var client = new MongoClient(MongoDbSettings.ConnectionString);
                var database = client.GetDatabase(MongoDbSettings.DatabaseName);
                var collection = database.GetCollection<CalificacionModel>("Calificaciones");

                var lista = await collection.Find(_ => true).ToListAsync();

                HistorialCalificaciones.Clear();

                if (lista != null && lista.Count > 0)
                {
                    foreach (var item in lista)
                    {
                        HistorialCalificaciones.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al conectar con MongoDB: {ex.Message}");
            }
        }
    }
}

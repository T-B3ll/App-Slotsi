using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;

namespace Slotsi.Api.Repositories;

public class NegocioRepository : INegocioRepository
{
    private readonly IMongoCollection<NegocioEntity> _negocios;
    private readonly string _nombreColeccion;

    public NegocioRepository(IMongoClient cliente, IOptions<MongoDbSettings> opciones)
    {
        var settings = opciones.Value;
        _nombreColeccion = settings.NegociosCollection;
        _negocios = cliente
            .GetDatabase(settings.DatabaseName)
            .GetCollection<NegocioEntity>(_nombreColeccion);
    }

    public async Task<List<NegocioEntity>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        var listaValida = new List<NegocioEntity>();

        var coleccionCruda = _negocios.Database
            .GetCollection<BsonDocument>(_nombreColeccion);
        var documentos = await coleccionCruda.Find(_ => true).ToListAsync(ct);

        foreach (var doc in documentos)
        {
            try
            {
                var entidad = MongoDB.Bson.Serialization.BsonSerializer
                    .Deserialize<NegocioEntity>(doc);
                listaValida.Add(entidad);
            }
            catch (Exception ex)
            {
                // ✅ Ya NO se silencia: queda rastro para saber qué documento falla
                System.Diagnostics.Debug.WriteLine(
                    $"⚠️ Documento ignorado {doc.GetValue("_id", "?")}: {ex.Message}");
            }
        }
        return listaValida;
    }

    public async Task<NegocioEntity?> ObtenerPorIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return null;
        return await _negocios.Find(n => n.Id == objectId).FirstOrDefaultAsync(ct);
    }

    public async Task<NegocioEntity?> ObtenerPorUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(usuarioId, out var usuarioOid)) return null;

       
        try
        {
            return await _negocios
                .Find(n => n.UsuarioId == usuarioOid)
                .FirstOrDefaultAsync(ct);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"⚠️ Error buscando negocio por usuario {usuarioId}: {ex.Message}");
            return null;
        }
    }

    public Task CrearAsync(NegocioEntity negocio, CancellationToken ct = default) =>
        _negocios.InsertOneAsync(negocio, cancellationToken: ct);

    public async Task<bool> ActualizarAsync(NegocioEntity negocio, CancellationToken ct = default)
    {
        var resultado = await _negocios.ReplaceOneAsync(n => n.Id == negocio.Id, negocio, cancellationToken: ct);
        return resultado.MatchedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return false;
        var resultado = await _negocios.DeleteOneAsync(n => n.Id == objectId, ct);
        return resultado.DeletedCount > 0;
    }
}
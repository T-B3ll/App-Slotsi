using MongoDB.Bson;

namespace Slotsi.Api.Infrastructure;

public static class IdValidator
{
    public static bool EsValido(string? id) => ObjectId.TryParse(id, out _);
}
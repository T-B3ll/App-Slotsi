using MongoDB.Bson.Serialization.Attributes;

namespace Slotsi.Api.Entities
{
    // Lista negra de tokens cerrados con logout. Se identifica por el claim "jti".
    // Un índice TTL sobre ExpiraEn borra el registro cuando el token ya habría caducado.

    public class TokenRevocadoEntity
    {
        [BsonId]
        public string Jti { get; set; } = string.Empty;

        [BsonElement("expiraEn")]
        public DateTime ExpiraEn { get; set; }
    }
}

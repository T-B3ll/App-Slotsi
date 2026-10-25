using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Slotsi.Api.Configuration;
using Slotsi.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Slotsi.Api.Service
{
    public class TokenService
    {
        private readonly JwtSetting _settings;
        public TokenService(IOptions<JwtSetting> opciones) => _settings = opciones.Value;

        public(String Token, DateTime ExpiraEn) Generar(UsuarioEntity usuario)
        {
            var expira = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(JwtRegisteredClaimNames.Name, usuario.NombreCompleto),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)), SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims,
                expires: expira, signingCredentials: credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
         
        }
    }
}

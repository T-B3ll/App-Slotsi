using System.Security.Cryptography;
using System.Text;
using Slotsi.Api.Entities;
using Slotsi.Api.Repositories;
using Slotsi.Api.Service;
using Slotsi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Slotsi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private static readonly TimeSpan VigenciaTokenReset = TimeSpan.FromMinutes(30);

        private readonly IUsuarioRepository _usuarios;
        private readonly TokenService _tokens;
        private readonly IPasswordHasher<UsuarioEntity> _hasher;
        private readonly IWebHostEnvironment _env;

        public AuthController(IUsuarioRepository usuarios, TokenService tokens,
            IPasswordHasher<UsuarioEntity> hasher, IWebHostEnvironment env)
        {
            _usuarios = usuarios;
            _tokens = tokens;
            _hasher = hasher;
            _env = env;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Registrar(RegistrarUsuarioDto dto, CancellationToken ct)
        {
            var usuario = new UsuarioEntity
            {
                NombreCompleto = dto.Nombre.Trim(),
                Correo = Normalizar(dto.Email),
            };

            usuario.Contrasena = _hasher.HashPassword(usuario, dto.Contrasena);

            if (!await _usuarios.CrearAsync(usuario, ct))
                return Problem(title: "Correo ya registrado", detail: "Ya existe un usuario con ese correo",
                    statusCode: StatusCodes.Status409Conflict);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenDto>> Login(LoginDto dto, CancellationToken ct)
        {
            var usuario = await _usuarios.ObtenerPorCorreoAsync(Normalizar(dto.Email), ct);

            bool valido = false;
            if (usuario is not null && !string.IsNullOrEmpty(usuario.Contrasena))
            {
                if (EsHashLegado(usuario.Contrasena))
                {
                    // Usuario creado con el esquema viejo (SHA-256 en hexadecimal).
                    valido = CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(Sha256Hex(dto.Contrasena)),
                        Encoding.UTF8.GetBytes(usuario.Contrasena.ToLowerInvariant()));

                    if (valido)
                    {
                        // Migra al hasher actual para que no vuelva a usarse el hash viejo.
                        usuario.Contrasena = _hasher.HashPassword(usuario, dto.Contrasena);
                        await _usuarios.ActualizarAsync(usuario, ct);
                    }
                }
                else
                {
                    valido = _hasher.VerifyHashedPassword(usuario, usuario.Contrasena, dto.Contrasena)
                             != PasswordVerificationResult.Failed;
                }
            }

            if (!valido)
            {
                return Problem(title: "Credenciales invalidas", detail: "Correo o contraseña incorrectos.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var (token, expira) = _tokens.Generar(usuario!);
            return Ok(new TokenDto { Token = token, ExpiraEn = expira });
        }

        /// <summary>Cierra la sesión: el token actual deja de ser válido.</summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var jti = User.FindFirst("jti")?.Value;
            var exp = User.FindFirst("exp")?.Value;
            if (jti is not null && long.TryParse(exp, out var expUnix))
                await _usuarios.RevocarTokenAsync(jti, DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime, ct);

            return NoContent();
        }

        /// <summary>Solicita un token para restablecer la contraseña.</summary>
        /// <remarks>
        /// La respuesta es siempre la misma exista o no el email. No hay servicio de correo:
        /// en Development el token se devuelve en la respuesta; en otros entornos debería
        /// enviarse por email.
        /// </remarks>
        [HttpPost("olvide-contrasena")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SolicitudRestablecimientoRespuestaDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SolicitarRestablecimientoDto>> OlvideContraseña(
            SolicitarRestablecimientoDto dto, CancellationToken ct)
        {
            var respuesta = new SolicitudRestablecimientoRespuestaDto
            {
                Mensaje = "Si el correo esta registrado, recibira intrucciones para restablecer la contraseña."
            };

            var usuario = await _usuarios.ObtenerPorCorreoAsync(Normalizar(dto.Email), ct);
            if (usuario is null)
                return Ok(respuesta);

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            usuario.ResetTokenHash = Hash(token);
            usuario.ResetTokenExpira = DateTime.UtcNow.Add(VigenciaTokenReset);
            await _usuarios.ActualizarAsync(usuario, ct);

            if (_env.IsDevelopment())
                respuesta.Token = token;

            return Ok(respuesta);

        }

        /// <summary>Establece una nueva contraseña con el token recibido.</summary>
        [HttpPost("restablecer-contrasena")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestablecerContraseña(RestablecerContrasenaDto dto, CancellationToken ct)
        {
            var usuario = await _usuarios.ObtenerPorCorreoAsync(Normalizar(dto.Email), ct);

            var valido = usuario?.ResetTokenHash is not null && usuario.ResetTokenExpira > DateTime.UtcNow
                && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(usuario.ResetTokenHash),
                Encoding.UTF8.GetBytes(Hash(dto.Token)));

            if (!valido)
                return Problem(title: "Token invalido", detail: "El token es incorrecto o ha caducado.",
                    statusCode: StatusCodes.Status400BadRequest);

            usuario!.Contrasena = _hasher.HashPassword(usuario, dto.NuevaContrasena);
            usuario.ResetTokenHash = null;
            usuario.ResetTokenExpira = null;
            await _usuarios.ActualizarAsync(usuario, ct);
            return NoContent();
        }

        private static string Normalizar(string Email) => Email.Trim().ToLowerInvariant();

        private static string Hash(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));

        private static bool EsHashLegado(string valor) => valor.Length == 64 && valor.All(Uri.IsHexDigit);

        private static string Sha256Hex(string valor) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor))).ToLowerInvariant();
    }
}

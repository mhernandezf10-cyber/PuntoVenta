
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Conexiones _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            Conexiones context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto solicitud)
        {
            if (string.IsNullOrWhiteSpace(solicitud.Nombre_usuario) ||
                string.IsNullOrWhiteSpace(solicitud.Password))
            {
                return BadRequest(new
                {
                    mensaje = "Ingrese el usuario y la contraseña."
                });
            }

            var nombreUsuario = solicitud.Nombre_usuario.Trim();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Nombre_usuario == nombreUsuario);

            if (usuario == null ||
                !usuario.Activo ||
                !VerificarPassword(
                    solicitud.Password,
                    usuario.Password_hash))
            {
                return Unauthorized(new
                {
                    mensaje = "Usuario o contraseña incorrectos."
                });
            }

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Falta configurar Jwt:Key.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Falta configurar Jwt:Issuer.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Falta configurar Jwt:Audience.");

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.Id_usuario.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Nombre_usuario)
            };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credenciales);

            var tokenGenerado =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenGenerado,
                tipo = "Bearer",
                expiraEnMinutos = 30,
                usuario = new
                {
                    usuario.Id_usuario,
                    usuario.Nombre_usuario,
                    usuario.Nombre_completo
                }
            });
        }

        private static bool VerificarPassword(
            string password,
            string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(hashAlmacenado))
                return false;

            var hasher = new PasswordHasher<Usuarios>();

            var resultado = hasher.VerifyHashedPassword(
                new Usuarios(),
                hashAlmacenado,
                password);

            return resultado != PasswordVerificationResult.Failed;
        }
    }

    public class LoginDto
    {
        public string Nombre_usuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}

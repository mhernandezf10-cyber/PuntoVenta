
using System.Net;
using ApiEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly Conexiones _context;
        private readonly IWebHostEnvironment _environment;
        private readonly PasswordHasher<Usuarios> _hasher = new();

        public UsuariosController(
            Conexiones context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private bool EsDesarrolloLocal()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;

            return _environment.IsDevelopment()
                && ip != null
                && IPAddress.IsLoopback(ip);
        }

        [HttpPost("crear-administrador-inicial")]
        public async Task<IActionResult> CrearAdministradorInicial(
            [FromBody] CrearUsuarioDto solicitud)
        {
            if (!EsDesarrolloLocal())
                return NotFound();

            if (await _context.Usuarios.AnyAsync())
            {
                return Conflict(new
                {
                    mensaje = "Ya existe un usuario. No se permite crear otro administrador inicial."
                });
            }

            if (solicitud == null ||
                string.IsNullOrWhiteSpace(solicitud.Nombre_usuario) ||
                string.IsNullOrWhiteSpace(solicitud.Nombre_completo) ||
                string.IsNullOrWhiteSpace(solicitud.Password))
            {
                return BadRequest(new
                {
                    mensaje = "Todos los campos son obligatorios."
                });
            }

            var nombreUsuario = solicitud.Nombre_usuario.Trim();
            var nombreCompleto = solicitud.Nombre_completo.Trim();

            if (nombreUsuario.Length > 50 ||
                nombreCompleto.Length > 100 ||
                solicitud.Password.Length < 12)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario o el nombre excede el límite permitido, o la contraseña tiene menos de 12 caracteres."
                });
            }

            var usuario = new Usuarios
            {
                Nombre_usuario = nombreUsuario,
                Nombre_completo = nombreCompleto,
                Activo = true
            };

            usuario.Password_hash = _hasher.HashPassword(
                usuario,
                solicitud.Password);

            _context.Usuarios.Add(usuario);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje = "No se pudo crear el usuario. Comprueba que el nombre no esté repetido."
                });
            }

            return Ok(new
            {
                mensaje = "Administrador inicial creado correctamente.",
                usuario = usuario.Nombre_usuario
            });
        }

        [HttpPut("cambiar-password-inicial")]
        public async Task<IActionResult> CambiarPasswordInicial(
            [FromBody] CambiarPasswordDto solicitud)
        {
            if (!EsDesarrolloLocal())
                return NotFound();

            if (solicitud == null ||
                string.IsNullOrWhiteSpace(solicitud.Nombre_usuario) ||
                string.IsNullOrEmpty(solicitud.Nueva_password))
            {
                return BadRequest(new
                {
                    mensaje = "Ingrese el usuario y la nueva contraseña."
                });
            }

            if (solicitud.Nueva_password.Length < 12)
            {
                return BadRequest(new
                {
                    mensaje = "La contraseña debe tener al menos 12 caracteres."
                });
            }

            var nombreUsuario = solicitud.Nombre_usuario.Trim();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(
                u => u.Nombre_usuario == nombreUsuario);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje = "Usuario no encontrado."
                });
            }

            usuario.Password_hash = _hasher.HashPassword(
                usuario,
                solicitud.Nueva_password);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Contraseña actualizada correctamente.",
                usuario = usuario.Nombre_usuario
            });
        }
    }

    public class CrearUsuarioDto
    {
        public string Nombre_usuario { get; set; } = string.Empty;
        public string Nombre_completo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CambiarPasswordDto
    {
        public string Nombre_usuario { get; set; } = string.Empty;
        public string Nueva_password { get; set; } = string.Empty;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmpresa.Models;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly Conexiones _context;

        public ProveedoresController(Conexiones context)
        {
            _context = context;
        }

        // Consultar todos los proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedores>>> GetProveedores()
        {
            return await _context.Proveedores
                .AsNoTracking()
                .ToListAsync();
        }

        // Consultar un proveedor por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedores>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound();

            return proveedor;
        }

        // Registrar un proveedor
        [HttpPost]
        public async Task<ActionResult<Proveedores>> PostProveedor(
            Proveedores proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProveedor),
                new { id = proveedor.Id_proveedor },
                proveedor);
        }

        // Editar un proveedor
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProveedor(
            int id,
            Proveedores proveedor)
        {
            if (id != proveedor.Id_proveedor)
                return BadRequest("El ID de la URL no coincide con el proveedor.");

            var existe = await _context.Proveedores
                .AnyAsync(p => p.Id_proveedor == id);

            if (!existe)
                return NotFound();

            _context.Entry(proveedor).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Eliminar un proveedor
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound();

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
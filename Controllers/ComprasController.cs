
using ApiEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly Conexiones _context;

        public ComprasController(Conexiones context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compras>>> GetCompras()
        {
            return await _context.Compras
                .AsNoTracking()
                .OrderByDescending(c => c.Id_compra)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Compras>> GetCompra(int id)
        {
            var compra = await _context.Compras.FindAsync(id);

            if (compra == null)
                return NotFound();

            return compra;
        }

        [HttpPost]
        public async Task<ActionResult<Compras>> CrearCompra(Compras compra)
        {
            var proveedorExiste = await _context.Proveedores
                .AnyAsync(p => p.Id_proveedor == compra.Id_proveedor);

            if (!proveedorExiste)
                return BadRequest("Seleccione un proveedor válido.");

            compra.Total = 0;

            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCompra),
                new { id = compra.Id_compra },
                compra);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCompra(
            int id, Compras compra)
        {
            if (id != compra.Id_compra)
                return BadRequest("El ID no coincide con la compra.");

            var existente = await _context.Compras.FindAsync(id);

            if (existente == null)
                return NotFound();

            var proveedorExiste = await _context.Proveedores
                .AnyAsync(p => p.Id_proveedor == compra.Id_proveedor);

            if (!proveedorExiste)
                return BadRequest("Seleccione un proveedor válido.");

            existente.Id_proveedor = compra.Id_proveedor;
            existente.Fecha = compra.Fecha;

            existente.Total = await _context.Compras_detalle
                .Where(d => d.Id_compra == id)
                .SumAsync(d => (decimal?)d.Subtotal) ?? 0m;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCompra(int id)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            var compra = await _context.Compras
                .FirstOrDefaultAsync(c => c.Id_compra == id);

            if (compra == null)
                return NotFound();

            var detalles = await _context.Compras_detalle
                .Where(d => d.Id_compra == id)
                .ToListAsync();

            _context.Compras_detalle.RemoveRange(detalles);
            _context.Compras.Remove(compra);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return NoContent();
        }
    }
}

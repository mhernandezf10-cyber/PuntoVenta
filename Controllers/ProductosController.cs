
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmpresa.Models;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly Conexiones _context;

        public ProductosController(Conexiones context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productos>>> GetProductos()
        {
            return await _context.Productos
                .AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Productos>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            return producto;
        }

        [HttpPost]
        public async Task<ActionResult<Productos>> PostProducto(Productos producto)
        {
            producto.Id_producto = 0;

            if (string.IsNullOrWhiteSpace(producto.Codigo))
                return BadRequest("El código del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return BadRequest("El nombre del producto es obligatorio.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            if (producto.Existencia < 0)
                return BadRequest("La existencia no puede ser negativa.");

            if (producto.Id_marca.HasValue)
            {
                bool marcaExiste = await _context.Marcas
                    .AnyAsync(m => m.Id_marca == producto.Id_marca.Value);

                if (!marcaExiste)
                    return BadRequest("La marca seleccionada no existe.");
            }

            _context.Productos.Add(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo registrar el producto. Comprueba que el código no esté repetido y que los datos sean válidos.");
            }

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.Id_producto },
                producto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Productos producto)
        {
            if (id != producto.Id_producto)
                return BadRequest("El ID del producto no coincide.");

            if (string.IsNullOrWhiteSpace(producto.Codigo))
                return BadRequest("El código del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return BadRequest("El nombre del producto es obligatorio.");

            if (producto.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            if (producto.Existencia < 0)
                return BadRequest("La existencia no puede ser negativa.");

            bool existe = await _context.Productos
                .AnyAsync(p => p.Id_producto == id);

            if (!existe)
                return NotFound();

            if (producto.Id_marca.HasValue)
            {
                bool marcaExiste = await _context.Marcas
                    .AnyAsync(m => m.Id_marca == producto.Id_marca.Value);

                if (!marcaExiste)
                    return BadRequest("La marca seleccionada no existe.");
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo actualizar el producto. Comprueba que el código no esté repetido y que los datos sean válidos.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se puede eliminar el producto porque está relacionado con otros registros.");
            }

            return NoContent();
        }
    }
}

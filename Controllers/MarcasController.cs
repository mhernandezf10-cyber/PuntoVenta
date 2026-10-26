using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmpresa.Models;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {
        private readonly Conexiones _context;

        public MarcasController(Conexiones context)
        {
            _context = context;
        }

        // CONSULTAR TODAS LAS MARCAS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marcas>>> GetMarcas()
        {
            return await _context.Marcas
                .AsNoTracking()
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        // CONSULTAR UNA MARCA
        [HttpGet("{id}")]
        public async Task<ActionResult<Marcas>> GetMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            return marca;
        }

        // REGISTRAR UNA MARCA
        [HttpPost]
        public async Task<ActionResult<Marcas>> PostMarca(Marcas marca)
        {
            marca.Id_marca = 0;

            _context.Marcas.Add(marca);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo registrar la marca. Comprueba que el nombre no esté repetido.");
            }

            return CreatedAtAction(
                nameof(GetMarca),
                new { id = marca.Id_marca },
                marca);
        }

        // EDITAR UNA MARCA
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarca(int id, Marcas marca)
        {
            if (id != marca.Id_marca)
            {
                return BadRequest("El ID de la marca no coincide.");
            }

            var existe = await _context.Marcas
                .AnyAsync(m => m.Id_marca == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Entry(marca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se pudo actualizar la marca. Comprueba que el nombre no esté repetido.");
            }

            return NoContent();
        }

        // ELIMINAR UNA MARCA
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            _context.Marcas.Remove(marca);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se puede eliminar la marca porque está relacionada con productos.");
            }

            return NoContent();
        }
    }
}

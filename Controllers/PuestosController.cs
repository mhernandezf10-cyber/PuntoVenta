using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmpresa.Models;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PuestosController : ControllerBase
    {
        private readonly Conexiones _context;

        public PuestosController(Conexiones context)
        {
            _context = context;
        }

        // GET: api/Puestos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Puestos>>> GetPuestos()
        {
            return await _context.Puestos
                .AsNoTracking()
                .ToListAsync();
        }

        // GET: api/Puestos/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Puestos>> GetPuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);

            if (puesto == null)
                return NotFound();

            return puesto;
        }

        // POST: api/Puestos
        [HttpPost]
        public async Task<ActionResult<Puestos>> PostPuesto(Puestos puesto)
        {
            _context.Puestos.Add(puesto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPuesto),
                new { id = puesto.Id_puesto },
                puesto);
        }

        // PUT: api/Puestos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPuesto(
            int id, Puestos puesto)
        {
            if (id != puesto.Id_puesto)
                return BadRequest();

            var existe = await _context.Puestos
                .AnyAsync(p => p.Id_puesto == id);

            if (!existe)
                return NotFound();

            _context.Entry(puesto).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Puestos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);

            if (puesto == null)
                return NotFound();

            _context.Puestos.Remove(puesto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "No se puede eliminar el puesto porque está relacionado con empleados.");
            }

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmpresa.Models;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private readonly Conexiones _context;

        public EmpleadosController(Conexiones context)
        {
            _context = context;
        }

        // GET: api/Empleados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empleados>>> GetEmpleados()
        {
            return await _context.Empleados.ToListAsync();
        }

        // GET: api/Empleados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Empleados>> GetEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);

            if (empleado == null)
                return NotFound();

            return empleado;
        }

        // POST: api/Empleados
        [HttpPost]
        public async Task<ActionResult<Empleados>> PostEmpleado(Empleados empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEmpleado),
                new { id = empleado.idempleados },
                empleado);
        }

        // PUT: api/Empleados/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmpleado(int id, Empleados empleado)
        {
            if (id != empleado.idempleados)
                return BadRequest();

            _context.Entry(empleado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Empleados.AnyAsync(
                    e => e.idempleados == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/Empleados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);

            if (empleado == null)
                return NotFound();

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

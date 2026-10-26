
using ApiEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly Conexiones _context;

        public VentasController(Conexiones context)
        {
            _context = context;
        }

        public class DetalleVentaDto
        {
            public int Id_producto { get; set; }
            public int Cantidad { get; set; }
        }

        public class VentaDto
        {
            public uint Id_cliente { get; set; }
            public List<DetalleVentaDto> Detalles { get; set; } = new();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ventas>>> GetVentas()
        {
            return await _context.Ventas
                .AsNoTracking()
                .OrderByDescending(v => v.Id_venta)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ventas>> GetVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                return NotFound("La venta no existe.");

            return venta;
        }

        [HttpGet("{id}/detalles")]
        public async Task<ActionResult<IEnumerable<Ventas_detalle>>> GetDetalles(int id)
        {
            var existe = await _context.Ventas
                .AnyAsync(v => v.Id_venta == id);

            if (!existe)
                return NotFound("La venta no existe.");

            return await _context.Ventas_detalle
                .AsNoTracking()
                .Where(d => d.Id_venta == id)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Ventas>> CrearVenta(VentaDto dto)
        {
            if (dto.Id_cliente == 0)
                return BadRequest("Seleccione un cliente válido.");

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest("Agregue al menos un producto.");

            if (dto.Detalles.Any(d => d.Id_producto <= 0 || d.Cantidad <= 0))
                return BadRequest("Los productos y las cantidades deben ser válidos.");

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                bool clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.Id_clientes == dto.Id_cliente);

                if (!clienteExiste)
                    return BadRequest("El cliente seleccionado no existe.");

                var cantidades = dto.Detalles
                    .GroupBy(d => d.Id_producto)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

                var ids = cantidades.Keys.ToList();

                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.Id_producto))
                    .ToListAsync();

                if (productos.Count != ids.Count)
                    return BadRequest("Uno o más productos no existen.");

                foreach (var producto in productos)
                {
                    if (producto.Existencia < cantidades[producto.Id_producto])
                        return BadRequest(
                            $"Existencia insuficiente para {producto.Nombre}. " +
                            $"Disponible: {producto.Existencia}.");
                }

                var venta = new Ventas
                {
                    Id_cliente = dto.Id_cliente,
                    Fecha = DateTime.Now,
                    Total = 0
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var producto in productos)
                {
                    int cantidad = cantidades[producto.Id_producto];

                    decimal precio = producto.Precio;
                    total += precio * cantidad;

                    producto.Existencia -= cantidad;

                    _context.Ventas_detalle.Add(new Ventas_detalle
                    {
                        Id_venta = venta.Id_venta,
                        Id_producto = producto.Id_producto,
                        Cantidad = cantidad,
                        Precio = precio
                    });
                }

                venta.Total = total;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return CreatedAtAction(
                    nameof(GetVenta),
                    new { id = venta.Id_venta },
                    venta);
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Problem(
                    title: "No se pudo registrar la venta.",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarVenta(
            int id, VentaDto dto)
        {
            if (dto.Id_cliente == 0)
                return BadRequest("Seleccione un cliente válido.");

            if (dto.Detalles == null || dto.Detalles.Count == 0)
                return BadRequest("Agregue al menos un producto.");

            if (dto.Detalles.Any(d => d.Id_producto <= 0 || d.Cantidad <= 0))
                return BadRequest("Los productos y las cantidades deben ser válidos.");

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = await _context.Ventas
                    .FirstOrDefaultAsync(v => v.Id_venta == id);

                if (venta == null)
                    return NotFound("La venta no existe.");

                bool clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.Id_clientes == dto.Id_cliente);

                if (!clienteExiste)
                    return BadRequest("El cliente seleccionado no existe.");

                var detallesAnteriores = await _context.Ventas_detalle
                    .Where(d => d.Id_venta == id)
                    .ToListAsync();

                var cantidadesNuevas = dto.Detalles
                    .GroupBy(d => d.Id_producto)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

                var idsProductos = detallesAnteriores
                    .Select(d => d.Id_producto)
                    .Concat(cantidadesNuevas.Keys)
                    .Distinct()
                    .ToList();

                var productos = await _context.Productos
                    .Where(p => idsProductos.Contains(p.Id_producto))
                    .ToListAsync();

                if (productos.Count != idsProductos.Count)
                    return BadRequest("Uno o más productos ya no existen.");

                // Primero se devuelve al inventario lo de la venta anterior.
                foreach (var detalle in detallesAnteriores)
                {
                    var producto = productos.First(
                        p => p.Id_producto == detalle.Id_producto);

                    producto.Existencia += detalle.Cantidad;
                }

                // Luego se comprueba la nueva venta contra el inventario restaurado.
                foreach (var producto in productos)
                {
                    int cantidadNueva = cantidadesNuevas
                        .GetValueOrDefault(producto.Id_producto, 0);

                    if (producto.Existencia < cantidadNueva)
                        return BadRequest(
                            $"Existencia insuficiente para {producto.Nombre}. " +
                            $"Disponible: {producto.Existencia}.");
                }

                _context.Ventas_detalle.RemoveRange(detallesAnteriores);

                decimal total = 0;

                foreach (var producto in productos)
                {
                    int cantidadNueva = cantidadesNuevas
                        .GetValueOrDefault(producto.Id_producto, 0);

                    if (cantidadNueva == 0)
                        continue;

                    decimal precio = producto.Precio;
                    total += precio * cantidadNueva;
                    producto.Existencia -= cantidadNueva;

                    _context.Ventas_detalle.Add(new Ventas_detalle
                    {
                        Id_venta = id,
                        Id_producto = producto.Id_producto,
                        Cantidad = cantidadNueva,
                        Precio = precio
                    });
                }

                venta.Id_cliente = dto.Id_cliente;
                venta.Total = total;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Problem(
                    title: "No se pudo actualizar la venta.",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarVenta(int id)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = await _context.Ventas
                    .FirstOrDefaultAsync(v => v.Id_venta == id);

                if (venta == null)
                    return NotFound("La venta no existe.");

                var detalles = await _context.Ventas_detalle
                    .Where(d => d.Id_venta == id)
                    .ToListAsync();

                var cantidades = detalles
                    .GroupBy(d => d.Id_producto)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

                var ids = cantidades.Keys.ToList();

                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.Id_producto))
                    .ToListAsync();

                if (productos.Count != ids.Count)
                    return BadRequest(
                        "No se puede restaurar el inventario porque falta un producto.");

                foreach (var producto in productos)
                    producto.Existencia += cantidades[producto.Id_producto];

                _context.Ventas_detalle.RemoveRange(detalles);
                _context.Ventas.Remove(venta);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Problem(
                    title: "No se pudo eliminar la venta.",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }
    }
}

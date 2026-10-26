using ApiEmpresa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiEmpresa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Compras_detalleController : ControllerBase
    {
        private readonly Conexiones _context;

        public Compras_detalleController(Conexiones context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await _context.Compras_detalle
                .AsNoTracking()
                .ToListAsync();

            return Ok(detalles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var detalle = await _context.Compras_detalle
                .FindAsync(id);

            if (detalle == null)
                return NotFound("No se encontró el detalle.");

            return Ok(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> CrearDetalle(
            [FromBody] Compras_detalle detalle)
        {
            if (detalle.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            if (detalle.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = await _context.Compras
                    .FirstOrDefaultAsync(c =>
                        c.Id_compra == detalle.Id_compra);

                if (compra == null)
                    return NotFound("La compra no existe.");

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id_producto == detalle.Id_producto);

                if (producto == null)
                    return NotFound("El producto no existe.");

                detalle.Subtotal = detalle.Cantidad * detalle.Precio;

                _context.Compras_detalle.Add(detalle);

                producto.Existencia += detalle.Cantidad;

                await _context.SaveChangesAsync();

                compra.Total = await _context.Compras_detalle
                    .Where(d => d.Id_compra == detalle.Id_compra)
                    .SumAsync(d => d.Subtotal);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return Ok(detalle);
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje = "No se pudo registrar el detalle.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarDetalle(
            int id,
            [FromBody] Compras_detalle detalle)
        {
            if (id != detalle.Id_detalle)
                return BadRequest("El ID no coincide.");

            if (detalle.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor que cero.");

            if (detalle.Precio < 0)
                return BadRequest("El precio no puede ser negativo.");

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var anterior = await _context.Compras_detalle
                    .FirstOrDefaultAsync(d => d.Id_detalle == id);

                if (anterior == null)
                    return NotFound("No se encontró el detalle.");

                var productoAnterior = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id_producto == anterior.Id_producto);

                var productoNuevo = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id_producto == detalle.Id_producto);

                var compraNueva = await _context.Compras
                    .FirstOrDefaultAsync(c =>
                        c.Id_compra == detalle.Id_compra);

                if (productoAnterior == null ||
                    productoNuevo == null ||
                    compraNueva == null)
                {
                    return BadRequest(
                        "El producto o la compra no existen.");
                }

                // Revertir la cantidad del detalle anterior.
                productoAnterior.Existencia -= anterior.Cantidad;

                // Aplicar la cantidad del detalle actualizado.
                productoNuevo.Existencia += detalle.Cantidad;

                anterior.Id_compra = detalle.Id_compra;
                anterior.Id_producto = detalle.Id_producto;
                anterior.Cantidad = detalle.Cantidad;
                anterior.Precio = detalle.Precio;
                anterior.Subtotal = detalle.Cantidad * detalle.Precio;

                await _context.SaveChangesAsync();

                var idsCompras = await _context.Compras_detalle
                    .Where(d => d.Id_detalle == id)
                    .Select(d => d.Id_compra)
                    .ToListAsync();

                foreach (var idCompra in idsCompras.Append(detalle.Id_compra).Distinct())
                {
                    var compra = await _context.Compras
                        .FirstOrDefaultAsync(c =>
                            c.Id_compra == idCompra);

                    if (compra != null)
                    {
                        compra.Total = await _context.Compras_detalle
                            .Where(d => d.Id_compra == idCompra)
                            .SumAsync(d => d.Subtotal);
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return Ok(anterior);
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje = "No se pudo actualizar el detalle.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDetalle(int id)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var detalle = await _context.Compras_detalle
                    .FirstOrDefaultAsync(d => d.Id_detalle == id);

                if (detalle == null)
                    return NotFound("No se encontró el detalle.");

                var compra = await _context.Compras
                    .FirstOrDefaultAsync(c =>
                        c.Id_compra == detalle.Id_compra);

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p =>
                        p.Id_producto == detalle.Id_producto);

                if (producto == null)
                    return NotFound("No se encontró el producto.");

                // Revertir la cantidad que ingresó este detalle.
                producto.Existencia -= detalle.Cantidad;

                _context.Compras_detalle.Remove(detalle);

                await _context.SaveChangesAsync();

                if (compra != null)
                {
                    compra.Total = await _context.Compras_detalle
                        .Where(d => d.Id_compra == compra.Id_compra)
                        .SumAsync(d => d.Subtotal);

                    await _context.SaveChangesAsync();
                }

                await transaccion.CommitAsync();

                return Ok(new
                {
                    mensaje = "Detalle eliminado y existencia actualizada."
                });
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje = "No se pudo eliminar el detalle.",
                    error = ex.Message
                });
            }
        }
    }
}
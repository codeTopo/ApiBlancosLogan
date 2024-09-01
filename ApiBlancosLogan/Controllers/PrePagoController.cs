using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrePagoController : ControllerBase
    {
        private readonly BlancosLoganContext _context;

        public PrePagoController(BlancosLoganContext context)
        {
            _context = context;
        }

        // Método para obtener el precio de un producto por ID
        private async Task<decimal> ObtenerPrecioProducto(long idProducto)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == idProducto);
            return producto?.Precio ?? 0;
        }

        // Endpoint para generar un pre-pago
        [HttpPost("agregar")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult> Prepago(PrepagoRequest model)
        {
            var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };

            // Validar el modelo recibido
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .Select(x => new
                    {
                        Campo = x.Key,
                        Errores = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();
                respuesta.Mensaje = "La solicitud contiene datos no válidos. Detalles:";
                respuesta.Data = errores;
                return BadRequest(respuesta);
            }
            // Validar la existencia del cliente
            var cliente = await _context.Clientes.FindAsync(model.IdCliente);
            if (cliente == null)
            {
                respuesta.Mensaje = "El IdCliente proporcionado no es válido.";
                return BadRequest(respuesta);
            }
            // Validar la existencia de la dirección
            var direccion = await _context.Direccions.FindAsync(model.IdDireccion);
            if (direccion == null)
            {
                respuesta.Mensaje = "El IdDireccion proporcionado no es válido.";
                return BadRequest(respuesta);
            }
            // Calcular el total del pedido
            decimal total = 0;
            foreach (var concepto in model.Pedido!)
            {
                var precioProducto = await ObtenerPrecioProducto((long)concepto.IdProducto!);
                if (precioProducto == 0)
                {
                    respuesta.Mensaje = $"El producto con Id {concepto.IdProducto} no existe.";
                    return BadRequest(respuesta);
                }
                // Validar y manejar valores nullable
                decimal cantidad = concepto.Cantidad ?? 0;
                total += cantidad * precioProducto;
            }
            // Generar un nuevo GUID para el pre-pago
            var idPrePago = Guid.NewGuid();
            // Crear y devolver la respuesta
            respuesta.Data = new { IdPrePago = idPrePago, Total = total };
            respuesta.Exito = 1;
            respuesta.Mensaje = "Pre-pago generado exitosamente.";
            return Ok(respuesta);
        }
    }
}

using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController(BlancosLoganContext context) : ControllerBase
    {
        //solicitudes http

        [HttpGet]
        [Authorize(Roles ="Master,Usuario")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la coneccion a la base de datos"
            };
            try
            {
                var productos = await context.Productos.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De productos";
                respuestas.Data = productos;
            }
            catch (Exception ex)
            {
                respuestas.Mensaje=ex.Message;
            }
            return Ok(respuestas);
        }
        [HttpPost("agregar")]
        [Authorize(Roles ="Master")]
        public async Task<IActionResult>Post(ProductoRequest model)
        {
            Respuestas respuestas = new ()
            {
                Exito = 0,
                Mensaje = "Error en la conexión a la base de datos"
            };
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

                respuestas.Mensaje = "La solicitud contiene datos no válidos. Detalles:";
                respuestas.Data = errores;

                return BadRequest(respuestas);
            }
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Producto prod = new()
                {
                    Nombre = model.Nombre!,
                    Descripcion = model.Descripcion!,
                    Precio = (decimal)model.Precio!,
                    Ubicacion = model.Ubicacion!,
                };
                context.Productos.Add(prod);
                await context.SaveChangesAsync();
                await transaction.CommitAsync(); // Confirma la transacción si todo está bien
                respuestas.Exito = 1;
                respuestas.Mensaje = "Producto agregado correctamente.";
                respuestas.Data = prod;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                respuestas.Mensaje = $"Error al intentar agregar el producto. {ex.Message}";
            }
            return Ok(respuestas);
        }
        [HttpPut("{idProducto}")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Put(long idProducto, [FromBody] ProductoUpdate model)
        {
            var respuesta = new Respuestas
            {
                Exito = 0,
                Mensaje = "Problemas con la conexión a la base de datos"
            };

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var producto = await context.Productos.FindAsync(idProducto); // Asincrónico para encontrar el producto
                if (producto == null)
                {
                    respuesta.Mensaje = "Producto no encontrado";
                    return NotFound(respuesta);
                }
                // Actualiza las propiedades del producto
                if (!string.IsNullOrEmpty(model.Nombre)) { producto.Nombre = model.Nombre; }
                if (!string.IsNullOrEmpty(model.Descripcion)) { producto.Descripcion = model.Descripcion; }
                if (model.Precio.HasValue) { producto.Precio = model.Precio.Value; }
                if (!string.IsNullOrEmpty(model.Ubicacion)) { producto.Ubicacion = model.Ubicacion; }
                context.Entry(producto).State = EntityState.Modified;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                respuesta.Exito = 1;
                respuesta.Mensaje = "Producto editado correctamente.";
                respuesta.Data = producto;
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                respuesta.Mensaje = $"Error al intentar editar el producto. {ex.Message}";
                return StatusCode(500, respuesta); // Retorna un estado de error más adecuado
            }
        }
        [HttpDelete("{idProducto}")]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Delete(long idProducto)
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conexión a la base de datos"
            };
            try
            {
                var producto = await context.Productos.FindAsync(idProducto);
                if (producto == null)
                {
                    respuestas.Mensaje = "Producto no encontrado";
                    return NotFound(respuestas);
                }
                context.Remove(producto);
                await context.SaveChangesAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Producto Eliminado Correctamente";
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
                respuestas.Mensaje = "Ocurrió un error al intentar eliminar el producto. Por favor, inténtelo de nuevo.";
            }
            return Ok(respuestas);
        }
    }
}

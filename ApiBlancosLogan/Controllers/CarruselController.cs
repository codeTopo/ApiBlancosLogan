using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CarruselController : ControllerBase
    {
        private readonly BlancosLoganContext context;
        public CarruselController(BlancosLoganContext _context)
        {
            context = _context;
        }
        //Solicitudes Http
        //Get General
        [HttpGet]
        [Authorize(Roles = "Master, Admin, Usuario")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conecxion con la base de datos"
            };
            try
            {
                var cliente = await context.Carrusels.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De Fotos";
                respuestas.Data = cliente;
            }
            catch (Exception ex)
            {
                respuestas.Exito = 0;
                respuestas.Mensaje = ex.Message;
                respuestas.Data = null;
            }
            return Ok(respuestas);
        }
       
        [HttpPost("agregar")]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> Post(CarruselRequest model)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al Conectar a la Base de Datos"
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
            var strategy = context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await context.Database.BeginTransactionAsync();
                    try
                    {
                        var carrusel
                        = new Carrusel
                        {
                            Nombre = model.Nombre!,
                            Imagen = model.Imagen!,
                        };
                        context.Carrusels.Add(carrusel);
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        respuestas.Exito = 1;
                        respuestas.Mensaje = "Foto Agregado Correctamente";
                        respuestas.Data = carrusel;

                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = $"Error al intentar agregar el producto. {ex.Message}";
            }
            return Ok(respuestas);
        }
       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al Conectar a la Base de Datos"
            };
            try
            {
                var cliente = await context.Carrusels.FindAsync(id);
                if (cliente == null)
                {
                    respuestas.Mensaje = "Foto no encontrado";
                    return NotFound(respuestas);
                }
                context.Remove(cliente);
                await context.SaveChangesAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Foto Eliminado Correctamente";
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }
    }
}

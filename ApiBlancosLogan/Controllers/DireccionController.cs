using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionController(BlancosLoganContext context) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Master, admin")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la coneccion a la base de datos"
            };
            try
            {
                var productos = await context.Direccions.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De Direcciones";
                respuestas.Data = productos;
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }
        [HttpGet("{telefono}")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult> GetByTelefono(string telefono)
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error al conectar a la base de datos"
            };
            try
            {
                // Verificar que el teléfono cumpla con los requisitos de validación
                if (!Regex.IsMatch(telefono, @"^\d{10}$"))
                {
                    respuestas.Mensaje = "El formato del teléfono es inválido. Debe contener exactamente 10 dígitos.";
                    return BadRequest(respuestas);
                }

                var direccion = await (from direc in context.Direccions
                                       where direc.Telefono == telefono
                                       select new DirecionRequest
                                       {
                                           IdDireccion = direc.IdDireccion,
                                           Cp = direc.CodigoPostal,
                                           Calle = direc.Calle!,
                                           Colonia = direc.Colonia!,
                                           Municipio = direc.Municipio!,
                                           Numero = direc.Numero,
                                           Estado = direc.Estado!,
                                           Telefono = direc.Telefono
                                       }).ToListAsync();
                if (direccion.Any())
                {
                    respuestas.Exito = 1;
                    respuestas.Mensaje = "Búsqueda exitosa";
                    respuestas.Data = direccion;
                }
                else
                {
                    respuestas.Mensaje = "No se encontró la dirección con el teléfono proporcionado";
                }
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }

        [HttpGet("idDireccion/{iddireccion}")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult> GetId(long idDireccion)
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error al conectar a la base de datos"
            };
            try
            {
                var direccion = await (from direc in context.Direccions
                                       where direc.IdDireccion == idDireccion
                                       select new DirecionRequest
                                       {
                                           IdDireccion = direc.IdDireccion,
                                           Cp = direc.CodigoPostal,
                                           Calle = direc.Calle!,
                                           Colonia = direc.Colonia!,
                                           Municipio = direc.Municipio!,
                                           Numero = direc.Numero,
                                           Estado = direc.Estado!,
                                           Telefono = direc.Telefono
                                       }).ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Búsqueda exitosa";
                respuestas.Data = direccion;
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }

        [HttpPost("agregar")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult>Post(DirecionRequest model)
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error al conectar a la base de datos"
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
                var direccion = new Direccion
                {
                    CodigoPostal = model.Cp!,
                    Estado = model.Estado,
                    Municipio = model.Municipio,
                    Calle = model.Calle,
                    Colonia = model.Colonia,
                    Numero = model.Numero!,
                    Telefono=model.Telefono!,
                };
                context.Direccions.Add(direccion);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Direccion Agregado Correctamente";
                respuestas.Data = direccion;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                respuestas.Mensaje = $"Error al intentar agregar el producto. {ex.Message}";
            }
            return Ok(respuestas);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult> Put(long id, [FromBody] DirecionRequest model)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al conectar a la base de datos"
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
            try
            {
                Direccion direccion = context.Direccions.Find(id)!;
                if (direccion == null)
                {
                    respuestas.Mensaje = "Dirección no encontrada";
                    return NotFound(respuestas);
                }
                direccion.CodigoPostal = model.Cp ?? direccion.CodigoPostal;
                direccion.Estado = model.Estado ?? direccion.Estado;
                direccion.Colonia = model.Colonia ?? direccion.Colonia;
                direccion.Municipio = model.Municipio ?? direccion.Municipio;
                direccion.Calle = model.Calle ?? direccion.Calle;
                direccion.Numero = model.Numero ?? direccion.Numero;

                context.Entry(direccion).State = EntityState.Modified;
                await context.SaveChangesAsync();

                respuestas.Exito = 1;
                respuestas.Mensaje = "Dirección Actualizada con Éxito";
                respuestas.Data = direccion;
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = $"Error al intentar actualizar la dirección. {ex.Message}";
            }
            return Ok(respuestas);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Master, admin")]
        public async Task<IActionResult>Delete(long id)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al Conectar a la Base de Datos"
            };
            try
            {
                var direccion = await context.Direccions.FindAsync(id);
                if (direccion == null)
                {
                    respuestas.Mensaje = "Direccion no encontrado";
                    return NotFound(respuestas);
                }
                context.Remove(direccion);
                await context.SaveChangesAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Direccion Eliminada Correctamente";
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }
    }
}

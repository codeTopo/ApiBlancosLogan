using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly BlancosLoganContext context;
        public ClientesController(BlancosLoganContext _context)
        {
            context = _context;
        }

        //Solicitudes Http
        //Get General
        [HttpGet]
        [Authorize(Roles ="Master, admin")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conecxion con la base de datos"
            };
            try
            {
                var cliente = await context.Clientes.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De Clientes";
                respuestas.Data = cliente;
            }
            catch (Exception ex)
            {
                respuestas.Exito = 0;
                respuestas.Mensaje=ex.Message;
                respuestas.Data = null;
            }
            return Ok(respuestas);
        }
        //Get por Telefono
        [HttpGet("{telefono}")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult> GetByTelefono(string telefono)
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conexión con la base de datos"
            };
            try
            {
                var cliente = await context.Clientes
                    .FirstOrDefaultAsync(c => c.Telefono == telefono);

                if (cliente != null)
                {
                    respuestas.Exito = 1;
                    respuestas.Mensaje = "Cliente encontrado";
                    respuestas.Data = cliente;
                }
                else
                {
                    respuestas.Mensaje = "Cliente no encontrado";
                }
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = $"Ocurrió un error: {ex.Message}";
                respuestas.Data = ex.StackTrace;  // Esto te dará más detalles del error.
                return StatusCode(500, respuestas); // Opcional: devolver explícitamente un código de estado 500
            }

            return Ok(respuestas);
        }
        //Get por id
        [HttpGet("idcliente/{idcliente}")]
        [Authorize(Roles = "Master,Usuario")]
        public async Task<IActionResult> GetId(long idcliente)
        {
            Respuestas respuestas = new Respuestas();
            respuestas.Exito = 0;
            using (var _context = new BlancosLoganContext())
            {
                try
                {
                    var cli = await (from clien in _context.Clientes
                                     where clien.IdCliente == idcliente
                                     select new ClienteRequest
                                     {
                                         IdCliente = clien.IdCliente,
                                         Nombre = clien.Nombre,
                                         Apellidos = clien.Apellidos,
                                         Telefono = clien.Telefono,
                                     }).ToListAsync();
                    respuestas.Exito = 1;
                    respuestas.Mensaje = "Busqueda Exitosa";
                    respuestas.Data = cli;

                }
                catch (Exception ex)
                {
                    respuestas.Mensaje = ex.Message;
                }
            }
            return Ok(respuestas);
        }
        [HttpPost("agregar")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult>Post(ClienteRequest model)
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
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var cliente = new Cliente
                {
                    Nombre = model.Nombre!,
                    Apellidos = model.Apellidos!,
                    Telefono = model.Telefono!
                };
                context.Clientes.Add(cliente);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Cliente Agregado Correctamente";
                respuestas.Data = cliente;

            }
            catch (Exception ex )
            {
                await transaction.RollbackAsync();
                respuestas.Mensaje = $"Error al intentar agregar el producto. {ex.Message}";
            }
            return Ok(respuestas);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Master, admin, Usuario")]
        public async Task<IActionResult>Put(long id, [FromBody] ClienteUpdate model)
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
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                Cliente nCliente = context.Clientes.Find(id)!;
                if (nCliente == null)
                {
                    respuestas.Mensaje = "Cliente no encontrado";
                    return NotFound(respuestas);
                }
                nCliente.Nombre = model.Nombre ?? nCliente.Nombre;
                nCliente.Apellidos = model.Apellidos ?? nCliente.Apellidos;
                nCliente.Telefono = model.Telefono ?? nCliente.Telefono;
                context.Entry(nCliente).State = EntityState.Modified;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Cliente Actualizado con Exito";
                respuestas.Data = nCliente;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                respuestas.Mensaje = $"Error al intentar agregar el producto. {ex.Message}";
            }
            return Ok(respuestas);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Master, admin")]
        public async Task<IActionResult> Delete(long id)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al Conectar a la Base de Datos"
            };
            try
            {
                var cliente = await context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    respuestas.Mensaje = "Cliente no encontrado";
                    return NotFound(respuestas);
                }
                context.Remove(cliente);
                await context.SaveChangesAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "Cliente Eliminado Correctamente";
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = ex.Message;
            }
            return Ok(respuestas);
        }
    }
}

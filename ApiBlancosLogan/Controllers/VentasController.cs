using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlancosLoganApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly BlancosLoganContext _context;

        public VentasController(BlancosLoganContext context)
        {
            _context = context;
        }
        private decimal ObtenerPrecioProducto(BlancosLoganContext context, long idProducto)
        {
            // Lógica para obtener el precio del producto de la base de datos
            var producto = context.Productos.FirstOrDefault(p => p.IdProducto == idProducto);
            return producto?.Precio ?? 0;
        }

        [HttpGet]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> Get()
        {
            var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
            try
            {
                var venta = await _context.Venta.ToListAsync();
                respuesta.Exito = 1;
                respuesta.Mensaje = "lista De ventas";
                respuesta.Data = venta;
            }
            catch (Exception ex)
            {
                respuesta.Mensaje = ex.Message;
            }
            return Ok(respuesta);

        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Master, Admin, Usuario")]
        public async Task<IActionResult>GetId(long id)
        {
            var respuestas = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
            var list = await (from ven in _context.Venta
                              where ven.IdVenta == id
                              select new VentaRequestGet
                              {
                                  IdVenta = ven.IdVenta,
                                  Total = (decimal)ven.Total!,
                                  clientesV = ((List<ClienteV>)(from cli in _context.Clientes
                                                                where cli.IdCliente == ven.IdCliente
                                                                select new ClienteV
                                                                {
                                                                    Nombre = cli.Nombre!,
                                                                    Apellido = cli.Apellidos!,
                                                                })),
                                  DireccionV = (((List<DireccionV>)(from dir in _context.Direccions
                                                                    where dir.IdDireccion == ven.IdDirecion
                                                                    select new DireccionV
                                                                    {
                                                                        Cp = dir.CodigoPostal,
                                                                        Estado = dir.Estado!,
                                                                        Municipio = dir.Municipio!,
                                                                        Calle = dir.Calle!,
                                                                        Colonia = dir.Colonia!,
                                                                        Numero = dir.Numero,
                                                                    }))),

                                  Pedido = ((List<ConceptosV>)(from ped in _context.Conceptos
                                                               where ped.IdVenta == ven.IdVenta
                                                               select new ConceptosV
                                                               {

                                                                   idProducto = (int)ped.IdProducto!,
                                                                   ProductoV = ((List<ProductoV>)(from prod in _context.Productos
                                                                                                  where ped.IdProducto == prod.IdProducto
                                                                                                  select new ProductoV
                                                                                                  {
                                                                                                      Nombre = prod.Nombre!,
                                                                                                      Ubicacion = prod.Ubicacion!,
                                                                                                  })),
                                                                   Cantidad = (int)ped.Cantidad!,
                                                                   Precio = ped.Precio,
                                                               })),


                              }).ToListAsync();
            respuestas.Exito = 1;
            respuestas.Mensaje = "Venta Agregada Con exito";
            respuestas.Data = list;
            return Ok(respuestas);
        }
        [HttpPost("agregar")]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> Add(VentaRequesPost model)
        {
            var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
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
            // Verificar si el IdCliente es válido
            var cliente = await _context.Clientes.FindAsync(model.IdCliente);
            if (cliente == null)
            {
                respuesta.Mensaje = "El IdCliente proporcionado no es válido.";
                return BadRequest(respuesta);
            }
            // Verificar si el IdDireccion es válido
            var direccion = await _context.Direccions.FindAsync(model.IdDireccion);
            if (direccion == null)
            {
                respuesta.Mensaje = "El IdDireccion proporcionado no es válido.";
                return BadRequest(respuesta);
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        using (BlancosLoganContext context = new BlancosLoganContext())
                        {
                            // Crear la venta
                            var ven = new Ventum
                            {
                                IdCliente = (long)model.IdCliente!,
                                IdDirecion = (long)model.IdDireccion!,
                                Pago = model.Pago!,
                                Fecha = DateTime.Now,
                                Total = (long)model.Pedido!.Sum(concepto => concepto.Cantidad * ObtenerPrecioProducto(context, (long)concepto.IdProducto!))!
                            };
                            context.Venta.Add(ven);
                            await context.SaveChangesAsync();
                            // Agregar los conceptos
                            foreach (var conceptos in model.Pedido!)
                            {
                                var concp = new Concepto
                                {
                                    IdProducto = (long)conceptos.IdProducto!,
                                    Cantidad = (int)conceptos.Cantidad!,
                                    Precio = ObtenerPrecioProducto(context, (long)conceptos.IdProducto!),
                                    IdVenta = ven.IdVenta
                                };
                                context.Conceptos.Add(concp);
                            }
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            var data = new
                            {
                                Venta = ven,
                                Concepto = model.Pedido
                            };
                            respuesta.Data = data;
                            respuesta.Exito = 1;
                            respuesta.Mensaje = "Venta agregada con éxito";
                        }
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
                respuesta.Mensaje = $"Error al intentar agregar la venta. {ex.Message}";
                return StatusCode(500, respuesta); // Retornar un error 500 en caso de excepción
            }
            return Ok(respuesta);
        }

        [HttpPost("addprepago/{idPrePago}")]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> AddDesdePrepago(Guid idPrePago)
        {
            var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
            // Obtener el PrePago usando el idPrePago
            var prePago = await _context.PrePagos
                .FirstOrDefaultAsync(p => p.IdPrePago == idPrePago);
            if (prePago == null)
            {
                respuesta.Mensaje = "No se encontró el pre-pago con el Id proporcionado.";
                return NotFound(respuesta);
            }
            if (prePago.EsFinalizado)
            {
                respuesta.Mensaje = "El pre-pago ya ha sido utilizado.";
                return BadRequest(respuesta);
            }
            // Verificar si el cliente y la dirección son válidos
            var cliente = await _context.Clientes.FindAsync(prePago.IdCliente);
            if (cliente == null)
            {
                respuesta.Mensaje = "El IdCliente asociado al pre-pago no es válido.";
                return BadRequest(respuesta);
            }
            var direccion = await _context.Direccions.FindAsync(prePago.IdDireccion);
            if (direccion == null)
            {
                respuesta.Mensaje = "El IdDireccion asociado al pre-pago no es válido.";
                return BadRequest(respuesta);
            }
            // Obtener los PreConceptos relacionados con el IdPrePago
            var preConceptos = await _context.PreConceptos
                .Where(pc => pc.IdPrePago == idPrePago)
                .ToListAsync();
            if (!preConceptos.Any())
            {
                respuesta.Mensaje = "No se encontraron conceptos asociados al pre-pago.";
                return NotFound(respuesta);
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        using (BlancosLoganContext context = new BlancosLoganContext())
                        {
                            var ven = new Ventum
                            {
                                IdCliente = prePago.IdCliente,
                                IdDirecion = prePago.IdDireccion,
                                Pago = "Prepago", // Puedes cambiar esto según sea necesario
                                Fecha = DateTime.Now,
                                Total = (long)preConceptos.Sum(concepto => concepto.Cantidad * ObtenerPrecioProducto(context, concepto.IdProducto))
                            };
                            context.Venta.Add(ven);
                            await context.SaveChangesAsync();
                            foreach (var concepto in preConceptos)
                            await _context.SaveChangesAsync();
                            prePago.EsFinalizado = true;
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync(); 
                            var data = new
                            {
                                Venta = ven,
                                Conceptos = preConceptos
                            };
                            respuesta.Data = data;
                            respuesta.Exito = 1;
                            respuesta.Mensaje = "Venta agregada con éxito";
                        }
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
                respuesta.Mensaje = $"Error al intentar agregar la venta. {ex.Message}";
                return StatusCode(500, respuesta);
            }

            return Ok(respuesta);
        }
    }
}

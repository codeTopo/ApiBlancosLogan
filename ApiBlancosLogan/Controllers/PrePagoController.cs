using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using ApiBlancosLogan.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PrePagoController : ControllerBase
{
    private readonly BlancosLoganContext _context;
    private readonly MercadoPagoService _mercadoPagoService;

    public PrePagoController(BlancosLoganContext context, MercadoPagoService mercadoPagoService)
    {
        _context = context;
        _mercadoPagoService = mercadoPagoService;
    }

    [HttpGet("vista")]
    [Authorize(Roles ="Master,Admin")]
    public async Task<IActionResult> Get()
    {
        var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
        try
        {
            var prepago = await _context.PrePagos.ToListAsync();
            respuesta.Exito = 1;
            respuesta.Mensaje = "lista De Clientes";
            respuesta.Data = prepago;
        }
        catch (Exception ex)
        {
            respuesta.Mensaje = ex.Message;
        }
        return Ok(respuesta);
    }
    //Metodo para el precio
    private decimal ObtenerPrecioProducto(long idProducto)
    {
        // Obtener el contexto si no está disponible en el alcance actual
        var context = _context; // Asegúrate de que '_context' esté disponible aquí
        // Lógica para obtener el precio del producto
        var producto = context.Productos.FirstOrDefault(p => p.IdProducto == idProducto);
        return producto?.Precio ?? 0;
    }


    [HttpGet("vista/{idPrePago}")]
    [Authorize(Roles ="Master, Admin")]
    public async Task<IActionResult> GetVista(Guid idPrePago)
    {
        var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
        try
        {
            var list = await (from pre in _context.PrePagos
                              where pre.IdPrePago == idPrePago
                              select new PrePagoRequestGet
                              {
                                  IdCliente = pre.IdCliente,
                                  IdDireccion = pre.IdDireccion,
                                 
                                  Pedido = (from prcon in _context.PreConceptos
                                            where prcon.IdPrePago == pre.IdPrePago
                                            select new PreConceptosG
                                            {
                                                IdPreConcepto = prcon.IdPreConcepto,
                                                IdProducto = prcon.IdProducto,
                                                Cantidad = prcon.Cantidad,
                                                ProductoV = (from producto in _context.Productos
                                                             where producto.IdProducto == prcon.IdProducto
                                                             select new PrProducto
                                                             {
                                                                 Nombre = producto.Nombre,
                                                                 Ubicacion = producto.Ubicacion
                                                             }).ToList()
                                            }).ToList()
                              }).ToListAsync();
            respuesta.Exito = 1;
            respuesta.Mensaje = "Datos obtenidos correctamente";
            respuesta.Data = list;
        }
        catch (Exception ex)
        {
            respuesta.Mensaje = ex.Message;
        }
        return Ok(respuesta);
    }
    [HttpPost("agregar")]
    [Authorize(Roles = "Master, Admin, Usuario")]
    public async Task<IActionResult> Prepago(PrepagoRequest model)
    {
        var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
        // Validación de modelo
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
        var cliente = await _context.Clientes.FindAsync(model.IdCliente);
        if (cliente == null)
        {
            respuesta.Mensaje = "El IdCliente proporcionado no es válido.";
            return BadRequest(respuesta);
        }
        var direccion = await _context.Direccions.FindAsync(model.IdDireccion);
        if (direccion == null)
        {
            respuesta.Mensaje = "El IdDireccion proporcionado no es válido.";
            return BadRequest(respuesta);
        }
        // Crear el id temporal
        var idPrePago = Guid.NewGuid();
        var strategy = _context.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var prePago = new PrePago
                    {
                        IdPrePago = idPrePago,
                        IdCliente = (long)model.IdCliente!,
                        IdDireccion = (long)model.IdDireccion!,
                    };
                    _context.PrePagos.Add(prePago);
                    decimal subtotal = 0;
                    await _context.SaveChangesAsync();
                    // Validación de productos
                    var idProductos = model.Pedido!.Select(c => c.IdProducto).ToList();
                    var productos = await _context.Productos.Where(p => idProductos.Contains(p.IdProducto)).ToListAsync();
                    foreach (var concepto in model.Pedido!)
                    {
                        var producto = productos.FirstOrDefault(p => p.IdProducto == concepto.IdProducto);
                        if (producto == null)
                        {
                            return BadRequest(new { mensaje = $"El producto con Id {concepto.IdProducto} no existe." });
                        }
                        var precioProducto = producto.Precio;
                        var preConcepto = new PreConcepto
                        {
                            IdPrePago = idPrePago,
                            IdProducto = (long)concepto.IdProducto!,
                            Cantidad = (decimal)concepto.Cantidad!
                        };
                        subtotal += precioProducto * (decimal)concepto.Cantidad!;
                        _context.PreConceptos.Add(preConcepto);
                    }
                    await _context.SaveChangesAsync();
                    // Creación de la preferencia de MercadoPago
                    string urlPago;
                    try
                    {
                        urlPago = await _mercadoPagoService.CrearPreferencia(
                            model.Email!,
                            cliente.Nombre,
                            cliente.Telefono,
                            cliente.Apellidos,
                            direccion.CodigoPostal,
                            idPrePago.ToString()
                        );
                    }
                    catch (Exception ex)
                    {
                        respuesta.Mensaje = $"Error al crear la preferencia de pago: {ex.Message}";
                        await transaction.RollbackAsync(); // Revertir cambios si falla MercadoPago
                        return StatusCode(500, respuesta);
                    }
                    await transaction.CommitAsync();
                    respuesta.Data = new { IdPrePago = idPrePago, UrlPago = urlPago, Subtotal = subtotal };
                    respuesta.Exito = 1;
                    respuesta.Mensaje = "Pre-pago generado exitosamente.";
                    return Ok(respuesta);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw; // Lanzar la excepción para manejarla en el bloque exterior
                }
            });
        }
        catch (Exception ex)
        {
            respuesta.Mensaje = $"Ocurrió un error durante el proceso: {ex.Message}";
            return StatusCode(500, respuesta);
        }
        return Ok(respuesta);
    }
    [HttpPut("{idPrePago}")]
    [Authorize(Roles = "Master, Admin")]
    public async Task<IActionResult> Put(Guid idPrePago, [FromBody] List<PreConceptos> nuevosConceptos)
    {
        var respuesta = new Respuestas { Exito = 0, Mensaje = "Error en la conexión a la base de datos" };
        // Verificar si el PrePago existe
        var prePago = await _context.PrePagos.FindAsync(idPrePago);
        if (prePago == null)
        {
            respuesta.Mensaje = "El IdPrePago proporcionado no es válido.";
            return BadRequest(respuesta);
        }
        if (prePago.EsFinalizado)
        {
            respuesta.Mensaje = "El pre-pago ya ha sido utilizado.";
            return BadRequest(respuesta);
        }
        // Validar los nuevos conceptos
        if (nuevosConceptos == null || !nuevosConceptos.Any())
        {
            respuesta.Mensaje = "Debe proporcionar al menos un concepto.";
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
                    decimal subtotal = 0;
                    foreach (var concepto in nuevosConceptos)
                    {
                        var precioProducto = ObtenerPrecioProducto(concepto.IdProducto!.Value);
                        var preConcepto = new PreConcepto
                        {
                            IdPrePago = idPrePago,
                            IdProducto = (long)concepto.IdProducto!,
                            Cantidad = (decimal)concepto.Cantidad!
                        };
                        subtotal += precioProducto * (decimal)concepto.Cantidad!;
                        _context.PreConceptos.Add(preConcepto);
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    respuesta.Exito = 1;
                    respuesta.Mensaje = "Pre-conceptos agregados exitosamente.";
                    respuesta.Data = new { Subtotal = subtotal };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    respuesta.Mensaje = $"Error durante el proceso: {ex.Message}";
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            respuesta.Mensaje = $"Ocurrió un error durante el proceso: {ex.Message}";
            return StatusCode(500, respuesta); // Retornar un error 500 en caso de excepción
        }
        return Ok(respuesta);
    }

    [HttpGet("validar/{idPrePago}")]
    [Authorize(Roles = "Master, Admin, Usuario")]
    public async Task<IActionResult> ObtenerDetallePrepago(Guid idPrePago)
    {
        var respuesta = new Respuestas { Exito = 0, Mensaje = "Error al recuperar los datos del pre-pago" };
        // Obtener el registro de PrePago usando el idPrePago
        var prePago = await _context.PrePagos.FirstOrDefaultAsync(p => p.IdPrePago == idPrePago);
        if (prePago == null)
        {
            respuesta.Mensaje = "No se encontró el pre-pago con el Id proporcionado.";
            return NotFound(respuesta);
        }
        // Obtener los PreConceptos relacionados con el IdPrePago
        var preConceptos = await _context.PreConceptos
            .Where(pc => pc.IdPrePago == idPrePago)
            .Select(pc => new
            {
                pc.IdPreConcepto,
                pc.IdProducto,
                pc.Cantidad
            })
            .ToListAsync();
        if (preConceptos == null || !preConceptos.Any())
        {
            respuesta.Mensaje = "No se encontraron conceptos asociados al pre-pago.";
            return NotFound(respuesta);
        }
        // Preparar la respuesta
        var detallePrepago = new
        {
            IdPrePago = prePago.IdPrePago,
            EsFinalizado = prePago.EsFinalizado,
            Conceptos = preConceptos
        };
        respuesta.Exito = 1;
        respuesta.Mensaje = "Datos del pre-pago";
        respuesta.Data = detallePrepago;
        return Ok(respuesta);
    }

}

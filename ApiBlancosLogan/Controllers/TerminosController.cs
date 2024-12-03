﻿using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerminosController : ControllerBase
    {
        private readonly BlancosLoganContext context;
        public TerminosController(BlancosLoganContext _context)
        {
            context = _context;

        }


        [HttpGet]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conecxion con la base de datos"
            };
            try
            {
                var cliente = await context.Terminos.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De Clientes";
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
        [Authorize(Roles = "Master, Admin, Usuario")]
        public async Task<IActionResult> Post(TerminosRequest model)
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
            var cliente = await context.Clientes.FindAsync(model.IdCliente);
            if (cliente == null)
            {
                respuestas.Exito = 0;
                respuestas.Mensaje = "El IdCliente proporcionado no es válido.";
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
                        var terminos = new Termino
                        {
                            IdCliente = model.IdCliente,
                            Fecha = DateTime.Now,
                            ArchivoVersion = model.ArchivoVersion,
                        };
                        await context.AddAsync(terminos);
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        respuestas.Exito = 1;
                        respuestas.Mensaje = $"Términos aceptados con la fecha {terminos.Fecha}";
                        respuestas.Data = terminos;
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
                return StatusCode(500, respuestas);
            }
            return Ok(respuestas);
        }

    }
}

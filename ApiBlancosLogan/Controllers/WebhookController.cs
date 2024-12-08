using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using ApiBlancosLogan.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly BlancosLoganContext _context;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly string tokenWbho;

        public WebhookController(BlancosLoganContext context, MercadoPagoService mercadoPagoService)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();

            tokenWbho = configuration["MercadoPago:WebhookSecret"] ?? throw new Exception("El Token del Webhook no está configurada.");
        }

        [HttpGet("pagos/{paymentId}")]
        [Authorize(Roles = "Master, Admin")]
        public async Task<IActionResult> ObtenerDetallesPago(long paymentId)
        {
            var respuesta = await _mercadoPagoService.ObtenerDetallesPago(paymentId);
            return Ok(respuesta);
        }
        [HttpGet("pagos")]
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
                var pagos = await _context.Pagos.ToListAsync();
                respuestas.Exito = 1;
                respuestas.Mensaje = "lista De Pagos";
                respuestas.Data = pagos;
            }
            catch (Exception ex)
            {
                respuestas.Exito = 0;
                respuestas.Mensaje = ex.Message;
                respuestas.Data = null;
            }
            return Ok(respuestas);
        }

        [HttpPost("anticipo")]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymentRequest webhook)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al procesar el webhook"
            };
            try
            {
                string expectedToken = tokenWbho;
                if (webhook.Type == "payment" && webhook.Action == "payment.created")
                {
                    string paymentId = webhook.Data?.Id ?? throw new Exception("ID del pago no encontrado");
                    var pago = new Pago
                    {
                        PaymentId = webhook.Data.Id,
                        Estado = "Pendiente", // Estado inicial
                        FechaCreacion = DateTime.UtcNow,
                        TipoEvento = webhook.Type,
                        Accion = webhook.Action,
                        ReferenciaInterna = "pago guarddado"
                    };
                    _context.Pagos.Add(pago);
                    await _context.SaveChangesAsync();
                    respuestas.Exito = 1;
                    respuestas.Mensaje = "Webhook procesado correctamente";
                    respuestas.Data = webhook.Data;
                }
                else
                {
                    respuestas.Mensaje = $"Evento no reconocido Type={webhook.Type}, Action={webhook.Action}";
                    return BadRequest(respuestas);
                } 
            }
            catch (Exception ex)
            {
                respuestas.Mensaje = $"Error: {ex.Message}";
            }
            return Ok(respuestas);
        }

    }
}

using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using ApiBlancosLogan.Tools;
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

        public WebhookController(BlancosLoganContext context, MercadoPagoService mercadoPagoService)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook()
        {
            return Ok();
        }
    }
}

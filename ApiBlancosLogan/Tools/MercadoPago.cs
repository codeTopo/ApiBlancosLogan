using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Client;
using MercadoPago.Client.Payment;
using ApiBlancosLogan.Request;


namespace ApiBlancosLogan.Tools
{
    public class MercadoPagoService
    {
        private readonly decimal _unitPrice;
        private readonly string exito;
        private readonly string fall;
        private readonly string pendiente;
        public MercadoPagoService()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            string token = configuration["MercadoPago:Token"]!;
            if (!string.IsNullOrEmpty(token))
            {
                MercadoPagoConfig.AccessToken = token;
            }
            else
            {
                throw new Exception("El token de MercadoPago no está configurado.");
            }
            if (!decimal.TryParse(configuration["MercadoPago:UnitPrice"], out _unitPrice))
            {
                throw new Exception("El UnitPrice esta en en valor 0 favor de verificarlo");
            }
            string _exito = configuration["MercadoPago:UrlReturnExito"]!;
            string _fall = configuration["MercadoPago:UrlReturnFaill"]!;
            string _pendiente = configuration["MercadoPago:UrlReturnPending"]!;

            exito = _exito;
            fall = _fall;
            pendiente = _pendiente;
        }

        // Metodo para crear una preferencia de pago
        public async Task<string> CrearPreferencia(string idPrepago, string email)
        {
            var requestOptions = new RequestOptions();
            requestOptions.CustomHeaders.Add("x-idempotency-key", Guid.NewGuid().ToString());
            // Crear un objeto de preferencia
            var preferenceRequest = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = "Anticipo Blancos Logan",
                        Description = $"Anticipo del pedido con ID: {idPrepago}",
                        CurrencyId = "MXN",
                        UnitPrice = _unitPrice,
                        Quantity = 1
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = exito,
                    Failure = fall,
                    Pending = pendiente
                },
                AutoReturn = "approved",
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentMethods = new List<PreferencePaymentMethodRequest>
                    {
                       new PreferencePaymentMethodRequest { Id = "amex" },
                    },
                    DefaultPaymentMethodId = "clabe",
                    ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                       // new PreferencePaymentTypeRequest { Id = "credit_card" },
                        new PreferencePaymentTypeRequest { Id = "digital_currency" }
                    },
                },
                StatementDescriptor = "Anticipo Para Blancos Logan",
                Expires = true,
                Payer = new PreferencePayerRequest
                {
                    Email = email
                }
            };
            // Crear la preferencia
            var preferenceClient = new PreferenceClient();
            var preference = await preferenceClient.CreateAsync(preferenceRequest, requestOptions);
            // Verificar si se creó correctamente
            if (preference.Id == null)
            {
                throw new Exception("Error al crear la preferencia de pago.");
            }
            // Devolver la URL de inicio del pago
            return preference.InitPoint;
        }

        //Metodo para buscar un pago realizado 
        public async Task<Respuestas> ObtenerDetallesPago(long paymentId)
        {
            var respuestas = new Respuestas { Exito = 0, Mensaje = "Error al Realizar la funcion" };
            var paymentClient = new PaymentClient();
            var payment = await paymentClient.GetAsync(paymentId);
            if (payment == null)
            {
                respuestas.Exito =0;
                respuestas.Mensaje = $"No se encontraron detalles para el pago con ID {paymentId}.";
            }
            else
            {
                respuestas.Exito = 1;
                respuestas.Mensaje = "Solicitud Exitosa datos";
                respuestas.Data = payment;
            }
            return respuestas;
        }
    }
}

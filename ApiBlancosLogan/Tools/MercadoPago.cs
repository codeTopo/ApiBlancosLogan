using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Client;

namespace ApiBlancosLogan.Tools
{
    public class MercadoPagoService
    {
        private readonly IConfiguration _configuration;
        public MercadoPagoService(IConfiguration configuration)
        {
            _configuration = configuration;
            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:token"];
        }

        // Método para crear una preferencia de pago
        public async Task<string> CrearPreferencia(string email, string nombre, string apellido, string telefono, string codigopostal, string idPrepago)
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
                        Id = idPrepago,
                        Title = "Blancos Logan Anticipo",
                        CurrencyId = "MXN",
                        UnitPrice = 1500,
                        Quantity=1
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://tusitio.com/exito",
                    Failure = "https://tusitio.com/fallo",
                    Pending = "https://tusitio.com/pendiente"
                },
                Payer = new PreferencePayerRequest
                {
                    Name = nombre,
                    Surname = apellido,  
                    Email = email,
                    Phone = new MercadoPago.Client.Common.PhoneRequest
                    {
                        AreaCode = "52", 
                        Number = telefono
                    },
                    Address = new MercadoPago.Client.Common.AddressRequest
                    {
                        ZipCode = codigopostal
                    }
                },
                // Configurar auto-retorno
                AutoReturn = "approved",
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentMethods = new List<PreferencePaymentMethodRequest>
                    {
                       new PreferencePaymentMethodRequest { Id = "amex" },
                    },
                    ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                        new PreferencePaymentTypeRequest { Id = "credit_card" },  // Excluye tarjetas de crédito
                        new PreferencePaymentTypeRequest { Id = "digital_currency" },  // Excluye criptomonedas
                    },
                },
                StatementDescriptor = "BLANCOS LOGAN",  // Texto que aparecerá en el estado de cuenta del cliente
                ExternalReference = idPrepago,  // Una referencia interna para tu sistema
                Metadata = new Dictionary<string, object>
                {
                    { "idPrepago", idPrepago }
                },
                Expires = true,
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
    }
}

using System.ComponentModel.DataAnnotations;

namespace ApiBlancosLogan.Request
{
    public class PaymentRequest
    {
        public long Id { get; set; }
        public bool LiveMode { get; set; }
        [Required]
        public string? Type { get; set; }
        public DateTime DateCreated { get; set; }
        public long UserId { get; set; }
        public string? ApiVersion { get; set; }
        [Required]
        public string? Action { get; set; }
        [Required]
        public WebhookData? Data { get; set; }

        public class WebhookData
        {
            [Required]
            public string? Id { get; set; }
        }
    }

}

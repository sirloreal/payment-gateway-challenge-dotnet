using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Banking
{
    public class BankPaymentResponse
    {
        [JsonPropertyName("authorized")]
        //[JsonConverter(typeof(StringToBoolConverter))]
        public bool Authorized { get; set; }
        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; }
    }
}

namespace PaymentGateway.Api.Models.Responses
{
    public class ValidationErrorResponse
    {
        public PaymentStatus Status { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

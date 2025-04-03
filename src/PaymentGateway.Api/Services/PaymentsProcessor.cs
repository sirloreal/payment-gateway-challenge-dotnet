using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Banking;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentsProcessor
    {
        Task<PaymentResponse> ProcessPaymentAsync(PostPaymentRequest request);
    }
    public class PaymentsProcessor : IPaymentsProcessor
    {
        private readonly IPaymentsRepository _paymentsRepository;
        private readonly HttpClient _httpClient;
        public PaymentsProcessor(IPaymentsRepository paymentsRepository, HttpClient httpClient)
        {
            _paymentsRepository = paymentsRepository;
            _httpClient = httpClient;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PostPaymentRequest request)
        {
            var bankPaymentRequest = new BankPaymentRequest()
            {
                CardNumber = request.CardNumber,
                ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
                Amount = request.Amount,
                Currency = request.Currency,
                Cvv = request.Cvv
            };

            var result = await _httpClient.PostAsJsonAsync("payments", bankPaymentRequest);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Payment failed");
            }

            var bankPaymentResponse = await result.Content.ReadFromJsonAsync<BankPaymentResponse>();

            var paymentResponse = new PaymentResponse
            {
                Id = Guid.NewGuid(),
                ExpiryYear = request.ExpiryYear,
                ExpiryMonth = request.ExpiryMonth,
                Amount = request.Amount,
                CardNumberLastFour = int.Parse(request.CardNumber.ToString().Substring(request.CardNumber.ToString().Length - 4)),
                Currency = request.Currency,
                Status = bankPaymentResponse!.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
            };

            //TODO: Maybe don't do this here?
            _paymentsRepository.Add(paymentResponse);
            return paymentResponse;
        }
    }
}

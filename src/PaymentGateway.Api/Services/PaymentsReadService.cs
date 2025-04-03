using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentsReadService
    {
        PaymentResponse? GetPayment(Guid id);
    }
    public class PaymentsReadService : IPaymentsReadService
    {
        private readonly IPaymentsRepository _paymentsRepository;

        public PaymentsReadService(IPaymentsRepository paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

        public PaymentResponse? GetPayment(Guid id)
        {
            return _paymentsRepository.Get(id);
        }
    }
}

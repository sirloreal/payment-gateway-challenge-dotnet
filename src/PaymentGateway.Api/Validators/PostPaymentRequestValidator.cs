using FluentValidation;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators
{
    public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
    {
        public PostPaymentRequestValidator()
        {
            // Stop validation on the first failure.  
            this.ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .WithMessage("Card number is required")
                .Must(cardNumber => cardNumber.ToString().Length >= 14 && cardNumber.ToString().Length <= 19)
                .WithMessage("Card number must be between 14 and 19 digits long")
                .Matches(@"^\d+$")
                .WithMessage("Card number must contain only digits");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty()
                .WithMessage("Expiry month is required")
                .InclusiveBetween(1, 12)
                .WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty()
                .WithMessage("Expiry year is required")
                .GreaterThanOrEqualTo(DateTime.Now.Year)
                .WithMessage("Expiry year must be greater than or equal to the current year");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Must(currency => new[] { "USD", "EUR", "GBP" }.Contains(currency.ToString()))
                .WithMessage("Currency must be one of the following: USD, EUR, GBP")
                .Must(currency => currency.ToString().Length == 3)
                .WithMessage("Currency must be 3 characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Cvv)
                .NotEmpty()
                .WithMessage("CVV is required")
                .Must(cvv => cvv.ToString().Length == 3)
                .WithMessage("CVV must be 3 characters");

            RuleFor(x => x)
                .Must(IsExpiryInTheFuture)
                .WithMessage("Expiry must be in the future");
        }

        private bool IsExpiryInTheFuture(PostPaymentRequest request)
        {
            // Assume the expiry date is the first day of the month after the given expiry month
            var currentDate = DateTime.Now;
            var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);
            return currentDate < expiryDate;
        }
    }
}

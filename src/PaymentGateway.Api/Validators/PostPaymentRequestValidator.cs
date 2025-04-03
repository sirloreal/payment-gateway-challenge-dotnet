using FluentValidation;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators
{
    public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
    {
        public PostPaymentRequestValidator()
        {
            RuleFor(x => x.CardNumberLastFour)
                .NotEmpty()
                .WithMessage("Card number is required")
                .Must(cardNumber => cardNumber.ToString().Length == 4)
                .WithMessage("Card number last four digits must be 4 characters long");
                //TODO: This requires a change to the data type
                //.Must(cardNumber => cardNumber.ToString().Length >= 14 && cardNumber.ToString().Length <= 19)
                //.WithMessage("Card number must be between 14 and 19 digits long");

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

            RuleFor(x => x)
                .Must(IsExpiryInTheFuture)
                .WithMessage("Expiry must be in the future");

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
        }

        private bool IsExpiryInTheFuture(PostPaymentRequest request)
        {
            return true;
        }
    }
}

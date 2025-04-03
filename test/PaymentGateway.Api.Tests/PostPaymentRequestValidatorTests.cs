using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests
{
    public class PostPaymentRequestValidatorTests
    {
        private PostPaymentRequestValidator _validator;

        public PostPaymentRequestValidatorTests()
        {
            _validator = new PostPaymentRequestValidator();
        }
            
        [Fact]
        public void Should_Validate_ValidRequest()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("123456789012")] // Card number too short
        [InlineData("12345678901234567890")] // Card number too long
        [InlineData("123456789012345a")] // Card number with invalid character
        public void Should_Validate_WithError_InvalidCardNumber(string cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = cardNumber,
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_InvalidExpiryMonth()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 13, // Invalid month
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_InvalidExpiryYear()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = 2020, // Invalid year
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_InvalidAmount()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year,
                Amount = -100, // Invalid amount
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_InvalidCurrency()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "US", // Invalid currency
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_InvalidCvv()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "USD",
                Cvv = 12 // Invalid CVV
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_CardExpired_LastYear()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.Month,
                ExpiryYear = DateTime.Now.AddYears(-1).Year, // Expired a year ago
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public void Should_Validate_WithError_CareExpired_LastMonth()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = DateTime.Now.AddMonths(-1).Month, // Expired a month ago
                ExpiryYear = DateTime.Now.Year,
                Amount = 100,
                Currency = "USD",
                Cvv = 123
            };
            // Act
            var result = _validator.Validate(request);
            // Assert
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.Errors);
        }
    }
}

using System.Net;
using System.Net.Http.Json;

using NSubstitute;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Utilities;

namespace PaymentGateway.Api.Tests
{
    public class PaymentsProcessorTests
    {
        private readonly PaymentsProcessor _paymentsProcessor;
        private readonly IPaymentsRepository _paymentsRepositoryMock;
        private readonly HttpClient _httpClientMock;
        private readonly MockHttpMessageHandler _httpMessageHandlerMock;

        public PaymentsProcessorTests()
        {
            _paymentsRepositoryMock = Substitute.For<IPaymentsRepository>();
            _httpMessageHandlerMock = Substitute.ForPartsOf<MockHttpMessageHandler>();
            _httpClientMock = new HttpClient(_httpMessageHandlerMock)
            {
                BaseAddress = new Uri("http://fakehost:8080")
            };
            _paymentsProcessor = new PaymentsProcessor(_paymentsRepositoryMock, _httpClientMock);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnAuthorizedPaymentResponse()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.Year + 1,
                Currency = "USD",
                Amount = 100,
                Cvv = 123
            };

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new
                {
                    authorized = true,
                    authorization_code = Guid.NewGuid()
                })
            };

            // This could be mocked so we only care about the PostPaymentRequest object as an input.
            _httpMessageHandlerMock.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(mockResponse);

            // Act
            var response = await _paymentsProcessor.ProcessPaymentAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(PaymentStatus.Authorized, response.Status);
            Assert.Equal(request.ExpiryMonth, response.ExpiryMonth);
            Assert.Equal(request.ExpiryYear, response.ExpiryYear);
            Assert.Equal(request.Amount, response.Amount);
            Assert.Equal(int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4)), response.CardNumberLastFour);
            Assert.Equal(request.Currency, response.Currency);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnDeclinedPaymentResponse_WhenNotAuthorized()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.Year + 1,
                Currency = "USD",
                Amount = 100,
                Cvv = 123
            };

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(new
                {
                    authorized = false,
                    authorization_code = string.Empty
                })
            };

            // This could be mocked so we only care about the PostPaymentRequest object as an input.
            _httpMessageHandlerMock.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(mockResponse);

            // Act
            var response = await _paymentsProcessor.ProcessPaymentAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(PaymentStatus.Declined, response.Status);
            Assert.Equal(request.ExpiryMonth, response.ExpiryMonth);
            Assert.Equal(request.ExpiryYear, response.ExpiryYear);
            Assert.Equal(request.Amount, response.Amount);
            Assert.Equal(int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4)), response.CardNumberLastFour);
            Assert.Equal(request.Currency, response.Currency);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldAddPaymentToRepository()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.Year + 1,
                Currency = "USD",
                Amount = 100,
                Cvv = 123
            };

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(new
                {
                    authorized = true,
                    authorization_code = Guid.NewGuid()
                })
            };

            _httpMessageHandlerMock.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(mockResponse);

            // Act
            var response = await _paymentsProcessor.ProcessPaymentAsync(request);

            // Assert
            _paymentsRepositoryMock.Received(1).Add(Arg.Is<PaymentResponse>(p =>
                p.CardNumberLastFour == int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4)) &&
                p.ExpiryMonth == request.ExpiryMonth &&
                p.ExpiryYear == request.ExpiryYear &&
                p.Currency == request.Currency &&
                p.Amount == request.Amount &&
                p.Status == PaymentStatus.Authorized
            ));
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldThrowException_WhenHttpRequestFails()
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                CardNumber = "1234567890123456",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.Year + 1,
                Currency = "USD",
                Amount = 100,
                Cvv = 123
            };
            _httpMessageHandlerMock.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            // Act & Assert
            await Assert.ThrowsAsync<PaymentGatewayException>(() => _paymentsProcessor.ProcessPaymentAsync(request));
        }
    }
}

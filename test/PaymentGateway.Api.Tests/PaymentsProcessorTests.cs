﻿using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using NSubstitute;
using PaymentGateway.Api.Models.Responses;
using System.Net.Http.Json;

namespace PaymentGateway.Api.Tests
{
    public class PaymentsProcessorTests
    {
        private readonly PaymentsProcessor _paymentsProcessor;
        private readonly IPaymentsRepository _paymentsRepositoryMock;
        private readonly HttpClient _httpClientMock; // TODO: Set up and verify

        public PaymentsProcessorTests()
        {
            _paymentsRepositoryMock = Substitute.For<IPaymentsRepository>();
            var mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();
            _httpClientMock = new HttpClient(mockHttpMessageHandler);
            _httpClientMock.BaseAddress = new Uri("http://fakehost:8080");
            //_httpClientMock.PostAsJsonAsync(Arg.Any<string>(), Arg.Any<PostPaymentRequest>())
            //    .Returns(Task.FromResult(

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonContent.Create(new
                {
                    authorized = true,
                    authorization_code = Guid.NewGuid()
                })
            };
            mockHttpMessageHandler.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                            .Returns(mockResponse);
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

            // Act
            var response = await _paymentsProcessor.ProcessPaymentAsync(request);

            // Assert
            _paymentsRepositoryMock.Received(1).Add(Arg.Is<PostPaymentResponse>(p =>
                p.CardNumberLastFour == int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4)) &&
                p.ExpiryMonth == request.ExpiryMonth &&
                p.ExpiryYear == request.ExpiryYear &&
                p.Currency == request.Currency &&
                p.Amount == request.Amount &&
                p.Status == PaymentStatus.Authorized
            ));
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(MockSend(request, cancellationToken));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return MockSend(request, cancellationToken);
        }

        public virtual HttpResponseMessage MockSend(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

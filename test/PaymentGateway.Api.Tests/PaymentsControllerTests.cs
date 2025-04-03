using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Utilities;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly HttpClient _httpClientMock;
    private readonly MockHttpMessageHandler _httpMessageHandlerMock;

    public PaymentsControllerTests()
    {
        _httpMessageHandlerMock = Substitute.ForPartsOf<MockHttpMessageHandler>();
        _httpClientMock = new HttpClient(_httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://localhost")
        };
    }

    [Fact]
    public async Task MakesAPaymentSuccessfully()
    {
        // Arrange
        var paymentRequest = new PostPaymentRequest
        {
            CardNumber = "123456789012345",
            ExpiryYear = DateTime.Now.Year,
            ExpiryMonth = DateTime.Now.Month,
            Amount = _random.Next(1, 10000),
            Currency = "GBP",
            Cvv = 123
        };

        var paymentResponse = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = paymentRequest.ExpiryYear,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            Amount = paymentRequest.Amount,
            CardNumberLastFour = int.Parse(paymentRequest.CardNumber[^4..]),
            Currency = paymentRequest.Currency,
            Status = PaymentStatus.Authorized
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

        _httpMessageHandlerMock.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(mockResponse);


        var paymentsRepository = new PaymentsRepository();
        var paymentsProcessor = new PaymentsProcessor(paymentsRepository, _httpClientMock);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton<IPaymentsRepository>(paymentsRepository)
                .AddScoped<IPaymentsReadService, PaymentsReadService>()
                .AddScoped(_ => paymentsProcessor)))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", paymentRequest);
        var actualPaymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(actualPaymentResponse);
        Assert.NotEqual(Guid.Empty, actualPaymentResponse!.Id);
        Assert.Equal(paymentRequest.Amount, actualPaymentResponse.Amount);
        Assert.Equal(paymentRequest.Currency, actualPaymentResponse.Currency);
    }
}

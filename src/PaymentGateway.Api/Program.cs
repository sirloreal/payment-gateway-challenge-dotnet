using FluentValidation;
using FluentValidation.AspNetCore;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IValidator<PostPaymentRequest>, PostPaymentRequestValidator>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
builder.Services.AddScoped<IPaymentsProcessor, PaymentsProcessor>();
builder.Services.AddHttpClient<IPaymentsProcessor, PaymentsProcessor>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8080");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

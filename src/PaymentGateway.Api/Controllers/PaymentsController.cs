using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IValidator<PostPaymentRequest> _validator;
    private readonly PaymentsRepository _paymentsRepository;

    public PaymentsController(IValidator<PostPaymentRequest> validator, PaymentsRepository paymentsRepository)
    {
        _validator = validator;
        _paymentsRepository = paymentsRepository;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id) //TODO: Is this the right response?
    {
        var payment = _paymentsRepository.Get(id);

        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<IActionResult> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if(!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        return Ok("Valid Request");
    }
}
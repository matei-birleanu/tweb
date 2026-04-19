using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Models.Enums;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllPayments()
    {
        var payments = await _paymentService.GetAllPaymentsAsync();
        return Ok(payments);
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"Payment with ID {id} not found" });
        }
        return Ok(payment);
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
        {
            return NotFound(new { message = $"Payment for order {orderId} not found" });
        }
        return Ok(payment);
    }

    /// <summary>
    /// Create a payment
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto paymentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var payment = await _paymentService.CreatePaymentAsync(
                paymentDto.OrderId,
                paymentDto.Amount,
                paymentDto.PaymentMethod
            );
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Process Stripe payment
    /// </summary>
    [HttpPost("stripe")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> ProcessStripePayment([FromBody] StripePaymentDto stripePaymentDto)
    {
        try
        {
            var payment = await _paymentService.ProcessStripePaymentAsync(
                stripePaymentDto.OrderId,
                stripePaymentDto.PaymentMethodId
            );
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create Stripe Checkout Session for an order
    /// </summary>
    [HttpPost("checkout/{orderId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateCheckoutSession(int orderId)
    {
        try
        {
            var url = await _paymentService.CreateCheckoutSessionAsync(orderId);
            return Ok(new { url });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify payment status after Stripe checkout redirect
    /// </summary>
    [HttpPost("verify/{orderId}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> VerifyPayment(int orderId)
    {
        try
        {
            var payment = await _paymentService.VerifyCheckoutAsync(orderId);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Stripe webhook for checkout session completion
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = Stripe.EventUtility.ParseEvent(json);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session?.Metadata != null && session.Metadata.TryGetValue("order_id", out var orderIdStr))
                {
                    if (int.TryParse(orderIdStr, out var orderId))
                    {
                        await _paymentService.CompleteCheckoutPaymentAsync(orderId, session.PaymentIntentId ?? session.Id);
                        _logger.LogInformation("Payment completed for order {OrderId}", orderId);
                    }
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }

    /// <summary>
    /// Update payment status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(int id, [FromBody] PaymentStatus status)
    {
        try
        {
            var payment = await _paymentService.UpdatePaymentStatusAsync(id, status);
            return Ok(payment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Payment with ID {id} not found" });
        }
    }

    /// <summary>
    /// Refund payment
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> RefundPayment(int id)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(id);
            return Ok(payment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Payment with ID {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreatePaymentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class StripePaymentDto
{
    public int OrderId { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
}

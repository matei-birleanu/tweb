using AutoMapper;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Models.Enums;
using OrderService.Repositories;
using Stripe;

namespace OrderService.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository repository,
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<PaymentService> logger)
    {
        _repository = repository;
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _repository.GetByIdAsync(id);
        return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
    {
        var payment = await _repository.GetByOrderIdAsync(orderId);
        return payment != null ? _mapper.Map<PaymentDto>(payment) : null;
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status)
    {
        var payments = await _repository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> CreatePaymentAsync(int orderId, decimal amount, string paymentMethod)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        }

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending
        };

        var createdPayment = await _repository.CreateAsync(payment);
        return _mapper.Map<PaymentDto>(createdPayment);
    }

    public async Task<PaymentDto> ProcessStripePaymentAsync(int orderId, string paymentMethodId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        }

        try
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalPrice * 100), // Convert to cents
                Currency = "usd",
                PaymentMethod = paymentMethodId,
                Confirm = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                }
            };

            var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalPrice,
                PaymentMethod = "Stripe",
                Status = paymentIntent.Status == "succeeded" ? PaymentStatus.Completed : PaymentStatus.Pending,
                StripePaymentIntentId = paymentIntent.Id,
                TransactionId = paymentIntent.Id,
                PaidAt = paymentIntent.Status == "succeeded" ? DateTime.UtcNow : null
            };

            var createdPayment = await _repository.CreateAsync(payment);

            if (payment.Status == PaymentStatus.Completed)
            {
                order.Status = OrderStatus.Processing;
                await _orderRepository.UpdateAsync(order);
            }

            return _mapper.Map<PaymentDto>(createdPayment);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment failed for order {OrderId}", orderId);
            throw new Exception($"Payment processing failed: {ex.Message}");
        }
    }

    public async Task<PaymentDto> UpdatePaymentStatusAsync(int id, PaymentStatus status)
    {
        var existingPayment = await _repository.GetByIdAsync(id);
        if (existingPayment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {id} not found");
        }

        existingPayment.Status = status;
        if (status == PaymentStatus.Completed && existingPayment.PaidAt == null)
        {
            existingPayment.PaidAt = DateTime.UtcNow;
        }

        var updatedPayment = await _repository.UpdateAsync(existingPayment);
        return _mapper.Map<PaymentDto>(updatedPayment);
    }

    public async Task<PaymentDto> RefundPaymentAsync(int id)
    {
        var payment = await _repository.GetByIdAsync(id);
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {id} not found");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed payments can be refunded");
        }

        try
        {
            if (!string.IsNullOrEmpty(payment.StripePaymentIntentId))
            {
                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = payment.StripePaymentIntentId
                };
                await refundService.CreateAsync(refundOptions);
            }

            payment.Status = PaymentStatus.Refunded;
            var updatedPayment = await _repository.UpdateAsync(payment);

            return _mapper.Map<PaymentDto>(updatedPayment);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed for payment {PaymentId}", id);
            throw new Exception($"Refund processing failed: {ex.Message}");
        }
    }
}

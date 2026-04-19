using OrderService.DTOs;
using OrderService.Models.Enums;

namespace OrderService.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
    Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status);
    Task<PaymentDto> CreatePaymentAsync(int orderId, decimal amount, string paymentMethod);
    Task<PaymentDto> ProcessStripePaymentAsync(int orderId, string paymentMethodId);
    Task<PaymentDto> UpdatePaymentStatusAsync(int id, PaymentStatus status);
    Task<PaymentDto> RefundPaymentAsync(int id);
    Task<string> CreateCheckoutSessionAsync(int orderId);
    Task CompleteCheckoutPaymentAsync(int orderId, string transactionId);
    Task<PaymentDto> VerifyCheckoutAsync(int orderId);
}

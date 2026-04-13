using OrderService.Models;

namespace OrderService.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(Order order);
    Task SendOrderStatusUpdateAsync(Order order);
    Task SendPaymentConfirmationAsync(Payment payment);
    Task SendEmailAsync(string to, string subject, string body);
}

using MailKit.Net.Smtp;
using MimeKit;
using OrderService.Models;

namespace OrderService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(Order order)
    {
        var subject = $"Order Confirmation - Order #{order.Id}";
        var body = $@"
            <h2>Order Confirmation</h2>
            <p>Thank you for your order!</p>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Product ID:</strong> {order.ProductId}</p>
            <p><strong>Quantity:</strong> {order.Quantity}</p>
            <p><strong>Total Price:</strong> ${order.TotalPrice:F2}</p>
            <p><strong>Order Type:</strong> {order.OrderType}</p>
            <p><strong>Status:</strong> {order.Status}</p>
            <p>We will notify you once your order is processed.</p>
        ";

        // In production, get email from user entity
        await SendEmailAsync("customer@example.com", subject, body);
    }

    public async Task SendOrderStatusUpdateAsync(Order order)
    {
        var subject = $"Order Status Update - Order #{order.Id}";
        var body = $@"
            <h2>Order Status Update</h2>
            <p>Your order status has been updated.</p>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>New Status:</strong> {order.Status}</p>
            <p><strong>Updated At:</strong> {order.UpdatedAt:F}</p>
        ";

        await SendEmailAsync("customer@example.com", subject, body);
    }

    public async Task SendPaymentConfirmationAsync(Payment payment)
    {
        var subject = $"Payment Confirmation - Payment #{payment.Id}";
        var body = $@"
            <h2>Payment Confirmation</h2>
            <p>Your payment has been processed successfully.</p>
            <p><strong>Payment ID:</strong> {payment.Id}</p>
            <p><strong>Order ID:</strong> {payment.OrderId}</p>
            <p><strong>Amount:</strong> ${payment.Amount:F2}</p>
            <p><strong>Payment Method:</strong> {payment.PaymentMethod}</p>
            <p><strong>Transaction ID:</strong> {payment.TransactionId}</p>
        ";

        await SendEmailAsync("customer@example.com", subject, body);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailSettings = _configuration.GetSection("Email");
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                emailSettings["FromName"] ?? "Shop Platform",
                emailSettings["FromEmail"] ?? "noreply@shopplatform.com"
            ));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                emailSettings["Host"] ?? "smtp.mailtrap.io",
                int.Parse(emailSettings["Port"] ?? "2525"),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                emailSettings["Username"],
                emailSettings["Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

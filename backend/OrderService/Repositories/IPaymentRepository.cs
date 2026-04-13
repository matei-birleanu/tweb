using OrderService.Models;
using OrderService.Models.Enums;

namespace OrderService.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

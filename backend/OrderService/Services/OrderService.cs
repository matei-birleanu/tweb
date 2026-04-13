using AutoMapper;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Models.Enums;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IMapper mapper,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _repository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var orders = await _repository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
    {
        var order = _mapper.Map<Order>(orderDto);
        order.Status = OrderStatus.Pending;

        var createdOrder = await _repository.CreateAsync(order);

        // Send confirmation email
        try
        {
            await _emailService.SendOrderConfirmationAsync(createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order confirmation email");
        }

        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var existingOrder = await _repository.GetByIdAsync(id);
        if (existingOrder == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        existingOrder.Status = status;
        var updatedOrder = await _repository.UpdateAsync(existingOrder);

        // Send status update email
        try
        {
            await _emailService.SendOrderStatusUpdateAsync(updatedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order status update email");
        }

        return _mapper.Map<OrderDto>(updatedOrder);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}

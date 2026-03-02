using EComAPI.DTOs.Order;

namespace EComAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Guid> CheckoutAsync(Guid userId);
        Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId);
    }
}

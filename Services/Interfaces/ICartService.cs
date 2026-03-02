using EComAPI.DTOs.Cart;

namespace EComAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(Guid userId, AddToCartRequest request);
        Task<IEnumerable<CartItemResponse>> GetMyCartAsync(Guid userId);
        Task RemoveAsync(Guid userId, Guid cartItemId);
    }
}

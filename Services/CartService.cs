namespace EComAPI.Services
{
    using EComAPI.DTOs.Cart;
    using EComAPI.Entities;
    using EComAPI.Repositories.Interfaces;
    using EComAPI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddToCartAsync(Guid userId, AddToCartRequest request)
        {
            var product = await _unitOfWork.Products
                .Query()
                .FirstOrDefaultAsync(x => x.Id == request.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            var existingItem = await _unitOfWork.CartItems
                .Query()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == request.ProductId);

            var newQuantity = request.Quantity;

            if (existingItem != null)
                newQuantity += existingItem.Quantity;

            if (product.Stock < newQuantity)
                throw new Exception("Not enough stock");

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
                _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };

                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CartItemResponse>> GetMyCartAsync(Guid userId)
        {
            return await _unitOfWork.CartItems
                .Query()
                .Where(x => x.UserId == userId)
                .Select(x => new CartItemResponse
                {
                    Id = x.Id,
                    ProductName = x.Product.Name,
                    Price = x.Product.Price,
                    Quantity = x.Quantity
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task RemoveAsync(Guid userId, Guid cartItemId)
        {
            var item = await _unitOfWork.CartItems
                .Query()
                .FirstOrDefaultAsync(x =>
                    x.Id == cartItemId &&
                    x.UserId == userId);

            if (item == null)
                throw new Exception("Cart item not found");

            _unitOfWork.CartItems.Remove(item);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
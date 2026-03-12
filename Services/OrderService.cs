using EComAPI.DTOs.Order;
using EComAPI.Entities;
using EComAPI.Repositories.Interfaces;
using EComAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CheckoutAsync(Guid userId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Lấy cart + product
                var cartItems = await _unitOfWork.CartItems
                    .Query()
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                    throw new Exception("Cart is empty");

                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending",
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in cartItems)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new Exception(
                            $"Not enough stock for {item.Product.Name}");

                    // Trừ tồn kho
                    item.Product.Stock -= item.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });

                    totalAmount += item.Product.Price * item.Quantity;
                }

                order.TotalAmount = totalAmount;

                await _unitOfWork.Orders.AddAsync(order);

                // Xóa cart
                foreach (var item in cartItems)
                {
                    _unitOfWork.CartItems.Remove(item);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return order.Id;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderResponse>> GetMyOrdersAsync(Guid userId)
        {
            return await _unitOfWork.Orders
                .Query()
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .Select(x => new OrderResponse
                {
                    Id = x.Id,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    Items = x.OrderItems.Select(i => new OrderItemResponse
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
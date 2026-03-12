using EComAPI.DTOs.Common;
using EComAPI.DTOs.Product;
using EComAPI.Entities;

namespace EComAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponse>> GetAllAsync(
            int page,
            int pageSize,
            string? search,
            decimal? minPrice,
            decimal? maxPrice);

        Task<ProductResponse?> GetByIdAsync(Guid id);

        Task<Guid> CreateAsync(CreateProductRequest request);

        Task UpdateAsync(Guid id, UpdateProductRequest request);

        Task DeleteAsync(Guid id);
    }
}

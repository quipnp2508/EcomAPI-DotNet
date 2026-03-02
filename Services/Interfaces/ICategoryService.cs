using EComAPI.DTOs.Category;

namespace EComAPI.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
        Task<Guid> CreateAsync(CreateCategoryRequest request);
        Task DeleteAsync(Guid id);
    }
}

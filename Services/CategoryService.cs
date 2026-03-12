namespace EComAPI.Services
{
    using EComAPI.DTOs.Category;
    using EComAPI.Entities;
    using EComAPI.Repositories.Interfaces;
    using EComAPI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            return await _unitOfWork.Categories
                .Query()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<Guid> CreateAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name
            };

            var exists = await _unitOfWork.Categories
                .Query()
                .AnyAsync(x => x.Name == request.Name);

            if (exists)
                throw new Exception("Category already exists");

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.Id;
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
                throw new Exception("Category not found");

            var hasProducts = await _unitOfWork.Products
                .Query()
                .AnyAsync(x => x.CategoryId == id);
            if (hasProducts)
                throw new Exception("Cannot delete category with products");

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

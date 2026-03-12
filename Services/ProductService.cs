namespace EComAPI.Services
{
    using EComAPI.DTOs.Common;
    using EComAPI.DTOs.Product;
    using EComAPI.Entities;
    using EComAPI.Repositories.Interfaces;
    using EComAPI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ProductResponse>> GetAllAsync(
             int page,
             int pageSize,
             string? search,
             decimal? minPrice,
             decimal? maxPrice)
        {
            IQueryable<Product> query = _unitOfWork.Products
                .Query()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x => x.Name.StartsWith(search));

            if (minPrice.HasValue)
                query = query.Where(x => x.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(x => x.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Stock = x.Stock,
                    CategoryName = x.Category.Name
                })
                .ToListAsync();

            return new PagedResult<ProductResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<ProductResponse?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Products
                .Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Stock = x.Stock,
                    CategoryName = x.Category.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Guid> CreateAsync(CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

using EComAPI.Data;
using EComAPI.Entities;
using EComAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace EComAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IGenericRepository<Product> Products { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Order> Orders { get; }
        public IGenericRepository<CartItem> CartItems { get; }
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new GenericRepository<Product>(context);
            Categories = new GenericRepository<Category>(context);
            Orders = new GenericRepository<Order>(context);
            CartItems = new GenericRepository<CartItem>(context);
            Users = new GenericRepository<User>(context);
            RefreshTokens = new GenericRepository<RefreshToken>(context);
        }

        public async Task<int> SaveChangesAsync()
             => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

}

using TienLuxury.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace TienLuxury.Services
{
    public class ProductService(DBContext hairSalonDbContext) : IProductService
    {
        private readonly DBContext _dbContext = hairSalonDbContext;

        public async Task CreateProduct(Product newProduct)
        {
            _dbContext.Products.Add(newProduct);
            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);

            _dbContext.SaveChanges();
        }
        public async Task<IEnumerable<Product>> GetAllProduct()
            => await _dbContext.Products.OrderByDescending(product => product.ProductName).ToListAsync();


        public async Task<Product?> GetProductById(ObjectId id)
            => await _dbContext.Products.FirstOrDefaultAsync(product => product.ID == id);

        public async Task UpdateProduct(Product updatedProduct)
        {
            Product productToUpdate = await GetProductById(updatedProduct.ID);

            if (productToUpdate == null)
            {
                throw new ArgumentException("The restaurant to update cannot be found.");
            }

            productToUpdate.ProductName = updatedProduct.ProductName;
            productToUpdate.QuantityInStock = updatedProduct.QuantityInStock;
            productToUpdate.Price = updatedProduct.Price;
            productToUpdate.Description = updatedProduct.Description;
            productToUpdate.ImagePath = updatedProduct.ImagePath;
            productToUpdate.IsSold = updatedProduct.IsSold;
            productToUpdate.CategoryId = updatedProduct.CategoryId;
            productToUpdate.ProductType = updatedProduct.ProductType;

            _dbContext.Products.Update(productToUpdate);

            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);
            await _dbContext.SaveChangesAsync();

        }

        public async Task DeleteProduct(Product deletedProduct)
        {
            Product productToDelete = await GetProductById(deletedProduct.ID);

            if (productToDelete == null)
            {
                throw new ArgumentException("The restaurant to update cannot be found.");
            }

            _dbContext.Remove(productToDelete);
            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);
            await _dbContext.SaveChangesAsync();
        }

        public async Task MinusQuantityInStock(ObjectId id, int quantity)
        {
            Product productToUpdate = await GetProductById(id);

            if (productToUpdate == null)
            {
                throw new ArgumentException("The restaurant to update cannot be found.");
            }

            productToUpdate.QuantityInStock -= quantity;

            _dbContext.Products.Update(productToUpdate);
            _dbContext.ChangeTracker.DetectChanges();
            Console.WriteLine(_dbContext.ChangeTracker.DebugView.LongView);
            await _dbContext.SaveChangesAsync();
        }
    }
}

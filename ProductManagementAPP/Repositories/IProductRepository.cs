using ProductManagementAPP.Model;

namespace ProductManagementAPP.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        // Method khusus untuk fitur Search and Filtering sesuai requirement
        Task<IEnumerable<Product>> SearchAsync(string? name, decimal? minPrice, decimal? maxPrice);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ProductManagementAPP.Model;
using ProductManagementAPP.Repositories;

namespace ProductManagementAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductController> _logger;
        private const string CacheKeyAllProducts = "all_products";

        public ProductController(IProductRepository repository, IMemoryCache cache, ILogger<ProductController> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Attempting to fetch all products.");

            try
            {
                // Attempt to retrieve data from cache first
                if (!_cache.TryGetValue(CacheKeyAllProducts, out IEnumerable<Product>? products))
                {
                    _logger.LogInformation("Product data not found in cache. Fetching from database.");
                    products = await _repository.GetAllAsync();

                    // Set cache options: expires after 5 minutes
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set(CacheKeyAllProducts, products, cacheEntryOptions);
                    _logger.LogInformation($"Successfully retrieved and cached {products.Count()} products.");
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved product data from cache.");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all products.");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation($"Attempting to fetch product with ID: {id}");

            try
            {
                var product = await _repository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    return NotFound(new { Message = $"Product with ID {id} not found." });
                }

                _logger.LogInformation($"Successfully fetched product with ID: {id}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching product with ID: {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            _logger.LogInformation($"Search requested with parameters - Name: {name}, MinPrice: {minPrice}, MaxPrice: {maxPrice}");

            try
            {
                // Create a unique cache key based on search parameters
                string searchCacheKey = $"search_{name}_{minPrice}_{maxPrice}";

                if (!_cache.TryGetValue(searchCacheKey, out IEnumerable<Product>? products))
                {
                    _logger.LogInformation("Search results not found in cache. Executing database query.");
                    products = await _repository.SearchAsync(name, minPrice, maxPrice);

                    // Sliding expiration: resets the expiration time if accessed within the window
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    _cache.Set(searchCacheKey, products, cacheEntryOptions);
                }
                else
                {
                    _logger.LogInformation("Search results retrieved from cache.");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing search query.");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            _logger.LogInformation($"Attempting to create a new product: {product.Name}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product creation.");
                return BadRequest(ModelState);
            }

            try
            {
                var createdProduct = await _repository.AddAsync(product);

                // Invalidate main cache since new data is added
                _cache.Remove(CacheKeyAllProducts);
                _logger.LogInformation($"Successfully created product with ID: {createdProduct.Id}. Main cache invalidated.");

                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating product: {product.Name}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            _logger.LogInformation($"Attempting to update product with ID: {id}");

            if (id != product.Id)
            {
                _logger.LogWarning($"Update failed: ID mismatch. URL ID: {id}, Body ID: {product.Id}");
                return BadRequest(new { Message = "ID in URL and body must match." });
            }

            try
            {
                var existingProduct = await _repository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning($"Update failed: Product with ID {id} not found.");
                    return NotFound(new { Message = $"Product with ID {id} not found." });
                }

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;

                await _repository.UpdateAsync(existingProduct);

                // Invalidate main cache
                _cache.Remove(CacheKeyAllProducts);
                _logger.LogInformation($"Successfully updated product with ID: {id}. Main cache invalidated.");

                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating product with ID: {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Attempting to delete product with ID: {id}");

            try
            {
                var existingProduct = await _repository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    _logger.LogWarning($"Delete failed: Product with ID {id} not found.");
                    return NotFound(new { Message = $"Product with ID {id} not found." });
                }

                await _repository.DeleteAsync(id);

                // Invalidate main cache
                _cache.Remove(CacheKeyAllProducts);
                _logger.LogInformation($"Successfully deleted product with ID: {id}. Main cache invalidated.");

                return Ok(new { Message = $"Product with ID {id} has been successfully deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting product with ID: {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}
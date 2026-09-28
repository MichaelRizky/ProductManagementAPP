using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagementAPP.Controllers;
using ProductManagementAPP.Data;
using ProductManagementAPP.Model;
using ProductManagementAPP.Repositories;

namespace ProductManagementAPP.Tests
{
    public class ProductControllerTests
    {
        private readonly AppDbContext _context;
        private readonly ProductRepository _repository;
        private readonly ProductController _controller;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ProductController>> _mockLogger;

        public ProductControllerTests()
        {
            // 1. Setup In-Memory Database (RAM-based, fresh for each test run)
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new ProductRepository(_context);

            // 2. Mock Logger and Cache so they don't interfere with our data tests
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ProductController>>();

            // Setup mock cache to always act like it's empty (forces fetching from DB)
            object? expectedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue)).Returns(false);
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);

            // 3. Inject everything into the controller
            _controller = new ProductController(_repository, _mockCache.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange (Persiapan Data)
            _context.Products.Add(new Product { Name = "Test Product 1", Price = 100, Description = "Desc 1" });
            _context.Products.Add(new Product { Name = "Test Product 2", Price = 200, Description = "Desc 2" });
            await _context.SaveChangesAsync();

            // Act (Eksekusi Fungsi yang Dites)
            var result = await _controller.GetAll();

            // Assert (Validasi Hasil)
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(2, returnedProducts.Count());
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenProductIsValid()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "New Laptop",
                Price = 1500,
                Description = "Gaming Laptop"
            };

            // Act
            var result = await _controller.Create(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedProduct = Assert.IsType<Product>(createdResult.Value);

            Assert.Equal("New Laptop", returnedProduct.Name);
            Assert.Equal(1, await _context.Products.CountAsync()); // Pastikan masuk ke DB
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenProductExists()
        {
            // Arrange
            var product = new Product { Name = "Old Phone", Price = 500, Description = "Old model" };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(product.Id);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0, await _context.Products.CountAsync()); // Pastikan terhapus dari DB
        }
    }
}
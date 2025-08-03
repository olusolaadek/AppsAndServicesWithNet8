using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory; // To use IMemoryCache

using Northwind.DataContext;
using Northwind.EntityModels;

namespace Northwind.WebApi.Service.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly NorthwindContext _db;
    private int pageSize = 10;

    private readonly IMemoryCache _memoryCache;
    private const string OutOfStockProductsKey = "OOSP";

    public ProductsController(ILogger<ProductsController> logger, 
        NorthwindContext db, IMemoryCache memoryCache)
    {
        _logger = logger;
        _db = db;
        _memoryCache = memoryCache;
    }

    [HttpGet]
    [Produces(typeof(Product[]))]
    public IEnumerable<Product> Get(int? page)
    {
        _logger.LogInformation("Fetching all products");

        return _db.Products
            .Where(p => p.UnitsInStock > 0 && !p.Discontinued)
            .OrderBy(Product => Product.ProductId)
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize);
    }

    [HttpGet]
    [Route("outofstock")]
    [Produces(typeof(Product[]))]
    public IEnumerable<Product> GetOutOfStockProducts()
    {
        // Check if the out-of-stock products are already cached
        if (!_memoryCache.TryGetValue(OutOfStockProductsKey, out Product[]? cachedProducts))
        {
            //_logger.LogInformation("Returning cached out-of-stock products");
            //return cachedProducts;
            cachedProducts = _db.Products
                .Where(p => p.UnitsInStock == 0 && !p.Discontinued)
                .ToArray();

            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
            {
                // Set the cache expiration time to 5 minutes
                SlidingExpiration = TimeSpan.FromMinutes(5),
                // Set the size of the cache entry to 1 (for example, if you want to limit the number of products cached)
                Size = cachedProducts.Length
            };
            _memoryCache.Set(OutOfStockProductsKey, cachedProducts, cacheOptions);
        }

        MemoryCacheStatistics? stats = _memoryCache.GetCurrentStatistics();

        _logger.LogInformation($"Memory cache. Total hits: {stats?.TotalHits}. Estimated size: {stats?.CurrentEstimatedSize}");
        return cachedProducts ?? Enumerable.Empty<Product>();
    }

    [HttpGet]
    [Route("discontinued")]
    [Produces(typeof(Product[]))]
    public IEnumerable<Product> GetDiscontinuedProducts()
    {
        return _db.Products.Where(p =>p.Discontinued);
    }

    // GET api/products/5
    [HttpGet("{id:int}")]
    public async ValueTask<Product?> Get(int id)
    {
        return await _db.Products.FindAsync(id);   
    }

    // GET api/products/cha
    [HttpGet("{name}")]
    public IEnumerable<Product> Get(string name)
    {
        return _db.Products.Where(p => p.ProductName.Contains(name));
    }

    // POST api/products
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return Created($"api/products/{product.ProductId}",product);
    }

    // PUT api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Product product)
    {
        Product? existingProduct = await _db.Products.FindAsync(id);

        if (existingProduct is null)
        {
            return NotFound();
        }

        existingProduct.ProductName = product.ProductName;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.SupplierId = product.SupplierId;
        existingProduct.QuantityPerUnit = product.QuantityPerUnit;
        existingProduct.UnitsInStock = product.UnitsInStock;
        existingProduct.UnitsOnOrder = product.UnitsOnOrder;
        existingProduct.ReorderLevel = product.ReorderLevel;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.Discontinued = product.Discontinued;


        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _db.Products.FindAsync(id) is Product product)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        return NotFound();
    }
}

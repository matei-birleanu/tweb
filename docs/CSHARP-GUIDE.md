# C# and ASP.NET Core Development Guide

Complete guide for developing C# ASP.NET Core microservices in this project.

## Table of Contents

- [C# Best Practices](#c-best-practices)
- [ASP.NET Core Patterns](#aspnet-core-patterns)
- [Entity Framework Core](#entity-framework-core)
- [Async/Await Patterns](#asyncawait-patterns)
- [LINQ Queries](#linq-queries)
- [Dependency Injection](#dependency-injection)
- [Configuration Management](#configuration-management)
- [Logging](#logging)
- [Error Handling](#error-handling)
- [Testing](#testing)

## C# Best Practices

### Naming Conventions

```csharp
// Classes, Interfaces, Methods - PascalCase
public class ProductService { }
public interface IProductRepository { }
public async Task<Product> GetProductAsync(Guid id) { }

// Variables, Parameters - camelCase
private readonly ILogger _logger;
public async Task CreateProduct(CreateProductRequest request) { }

// Constants - PascalCase
public const int MaxPageSize = 100;

// Private fields - _camelCase with underscore
private readonly ApplicationDbContext _context;
```

### Nullable Reference Types

```csharp
#nullable enable

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // Required, never null
    public string? Description { get; set; } // Optional, can be null
    public Category Category { get; set; } = null!; // Will be set by EF Core
}
```

### Records for DTOs

```csharp
// Immutable DTOs with records (C# 9+)
public record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    string CategoryName
);

// Request DTOs with init-only properties
public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public Guid CategoryId { get; init; }
}
```

### Pattern Matching

```csharp
public string GetOrderStatus(Order order) => order.Status switch
{
    OrderStatus.Pending => "Your order is pending",
    OrderStatus.Confirmed => "Order confirmed",
    OrderStatus.Shipped => "Order shipped",
    OrderStatus.Delivered => "Order delivered",
    OrderStatus.Cancelled => "Order cancelled",
    _ => "Unknown status"
};
```

## ASP.NET Core Patterns

### Minimal APIs (Alternative to Controllers)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/products", async (IProductService service) =>
{
    var products = await service.GetAllProductsAsync();
    return Results.Ok(products);
});

app.MapPost("/api/products", async (CreateProductRequest request, IProductService service) =>
{
    var product = await service.CreateProductAsync(request);
    return Results.Created($"/api/products/{product.Id}", product);
})
.RequireAuthorization("admin");

app.Run();
```

### Controller Pattern (Recommended for this project)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService service,
        ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetProducts(
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        _logger.LogInformation("Getting products: page={Page}, size={Size}", page, size);
        var result = await _service.GetProductsAsync(page, size);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(Guid id)
    {
        var product = await _service.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _service.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

### Global Exception Handling

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }
}

// Register in Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

## Entity Framework Core

### DbContext Configuration

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Or apply specific configurations
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update timestamps
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                   (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

### Entity Configuration

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        builder.HasData(
            new Product
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Sample Product",
                Price = 99.99m,
                Sku = "SAMPLE-001",
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
```

### Repository Pattern (Optional)

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
```

## Async/Await Patterns

### Best Practices

```csharp
// ✅ Good: Async all the way
public async Task<List<Product>> GetProductsAsync()
{
    return await _context.Products.ToListAsync();
}

// ❌ Bad: Blocking on async code
public List<Product> GetProducts()
{
    return GetProductsAsync().Result; // Can cause deadlocks!
}

// ✅ Good: ConfigureAwait(false) in library code
public async Task<Product?> GetProductByIdAsync(Guid id)
{
    return await _context.Products
        .FirstOrDefaultAsync(p => p.Id == id)
        .ConfigureAwait(false);
}

// ✅ Good: Parallel execution
public async Task<(List<Product> products, List<Category> categories)> GetDataAsync()
{
    var productsTask = _context.Products.ToListAsync();
    var categoriesTask = _context.Categories.ToListAsync();

    await Task.WhenAll(productsTask, categoriesTask);

    return (await productsTask, await categoriesTask);
}

// ✅ Good: Using ValueTask for hot path
public async ValueTask<Product?> GetCachedProductAsync(Guid id)
{
    if (_cache.TryGetValue(id, out Product? product))
        return product; // Synchronous path

    product = await _context.Products.FindAsync(id);
    _cache.Set(id, product);
    return product;
}
```

## LINQ Queries

### Common Patterns

```csharp
// Basic filtering
var products = await _context.Products
    .Where(p => p.Status == ProductStatus.Active)
    .ToListAsync();

// Ordering
var products = await _context.Products
    .OrderBy(p => p.Name)
    .ThenByDescending(p => p.Price)
    .ToListAsync();

// Pagination
var products = await _context.Products
    .Skip(page * pageSize)
    .Take(pageSize)
    .ToListAsync();

// Projection (select specific fields)
var productDtos = await _context.Products
    .Select(p => new ProductResponse(
        p.Id,
        p.Name,
        p.Price,
        p.InventoryCount,
        p.Category.Name
    ))
    .ToListAsync();

// Include related entities
var product = await _context.Products
    .Include(p => p.Category)
    .ThenInclude(c => c.Parent)
    .FirstOrDefaultAsync(p => p.Id == id);

// AsNoTracking for read-only queries (better performance)
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.Status == ProductStatus.Active)
    .ToListAsync();

// Group by
var categoryStats = await _context.Products
    .GroupBy(p => p.CategoryId)
    .Select(g => new
    {
        CategoryId = g.Key,
        Count = g.Count(),
        AveragePrice = g.Average(p => p.Price)
    })
    .ToListAsync();

// Any/All
var hasActiveProducts = await _context.Products
    .AnyAsync(p => p.Status == ProductStatus.Active);

var allExpensive = await _context.Products
    .AllAsync(p => p.Price > 100);

// FirstOrDefault vs SingleOrDefault
var product = await _context.Products
    .FirstOrDefaultAsync(p => p.Sku == sku); // Returns first or null

var product = await _context.Products
    .SingleOrDefaultAsync(p => p.Id == id); // Throws if multiple matches
```

### Query Performance Tips

```csharp
// ✅ Good: Compiled query for frequently used queries
private static readonly Func<ApplicationDbContext, Guid, Task<Product?>>
    GetProductQuery = EF.CompileAsyncQuery(
        (ApplicationDbContext context, Guid id) =>
            context.Products.FirstOrDefault(p => p.Id == id)
    );

public async Task<Product?> GetProductAsync(Guid id)
{
    return await GetProductQuery(_context, id);
}

// ✅ Good: Split query for large includes
var products = await _context.Products
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .AsSplitQuery() // Generates separate SQL queries
    .ToListAsync();

// ✅ Good: Explicit loading
var product = await _context.Products.FindAsync(id);
if (product != null)
{
    await _context.Entry(product)
        .Reference(p => p.Category)
        .LoadAsync();

    await _context.Entry(product)
        .Collection(p => p.Reviews)
        .LoadAsync();
}
```

## Dependency Injection

### Service Lifetimes

```csharp
// Transient: New instance for each request
builder.Services.AddTransient<IEmailService, EmailService>();

// Scoped: One instance per HTTP request
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Singleton: One instance for app lifetime
builder.Services.AddSingleton<IConfigurationCache, ConfigurationCache>();

// DbContext is scoped by default
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// HttpClient with typed client
builder.Services.AddHttpClient<IStripeService, StripeService>(client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {stripeKey}");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
```

## Configuration Management

### Strongly-Typed Options

```csharp
// Options class
public class StripeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}

// appsettings.json
{
  "Stripe": {
    "ApiKey": "sk_test_...",
    "WebhookSecret": "whsec_...",
    "PublicKey": "pk_test_..."
  }
}

// Register in Program.cs
builder.Services.Configure<StripeOptions>(
    builder.Configuration.GetSection("Stripe"));

builder.Services.AddOptions<StripeOptions>()
    .Bind(builder.Configuration.GetSection("Stripe"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Inject in service
public class StripeService
{
    private readonly StripeOptions _options;

    public StripeService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }
}
```

## Logging

### Structured Logging with Serilog

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Usage in service
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation(
            "Creating product {ProductName} with SKU {Sku}",
            request.Name,
            request.Sku
        );

        try
        {
            var product = await SaveProductAsync(request);
            _logger.LogInformation(
                "Product created successfully: {ProductId}",
                product.Id
            );
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create product {ProductName}",
                request.Name
            );
            throw;
        }
    }
}
```

## Error Handling

### Custom Exceptions

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}

// Usage
if (product == null)
    throw new NotFoundException($"Product with ID {id} not found");
```

## Testing

### xUnit Tests

```csharp
public class ProductServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ProductService(_context, Mock.Of<ILogger<ProductService>>());
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 99.99m,
            Sku = "TEST-001",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = await _service.CreateProductAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Price, result.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateProduct_InvalidName_ThrowsException(string name)
    {
        // Arrange
        var request = new CreateProductRequest { Name = name };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateProductAsync(request));
    }
}
```

---

Last Updated: 2026-03-14

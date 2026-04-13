# Claude Code Instructions - C# ASP.NET Core E-Commerce Platform

This document provides specific instructions for Claude Code when working with this C# ASP.NET Core microservices e-commerce project.

## Project Overview

This is a **microservices-based e-commerce platform** built with:
- **Backend**: ASP.NET Core 8.0 + C# 12 (Product, Order, Payment services)
- **Frontend**: React 18 + TypeScript 5.0 + Vite
- **Gateway**: Yarp (Yet Another Reverse Proxy)
- **Auth**: Keycloak or IdentityServer4 (OAuth2/OIDC)
- **Databases**: PostgreSQL 15 (one per service)
- **ORM**: Entity Framework Core 8.0
- **Search**: Elasticsearch 8.x
- **Payments**: Stripe API integration

## Project Structure

```
csharptweb/
├── backend/
│   ├── ApiGateway/         # Yarp API Gateway (port 8080)
│   ├── ProductService/     # Product management (port 8081)
│   └── OrderService/       # Order processing (port 8082)
├── frontend/                # React app (port 3000)
│   ├── src/
│   │   ├── components/     # Reusable UI components
│   │   ├── pages/          # Page components
│   │   ├── services/       # API service clients
│   │   ├── types/          # TypeScript interfaces
│   │   └── utils/          # Utility functions
│   └── public/             # Static assets
├── database/               # Database scripts
│   ├── migrations/        # EF Core migrations
│   └── seeds/            # Seed data
├── docker/                # Dockerfiles
└── docs/                  # Documentation
```

## Common Commands

### Backend Services (.NET)

```bash
# Restore dependencies
dotnet restore

# Build a service
cd backend/ProductService
dotnet build

# Run a service
dotnet run

# Run with hot reload (watch mode)
dotnet watch run

# Run tests
dotnet test

# Run with specific profile
dotnet run --launch-profile Development

# Package as self-contained executable
dotnet publish -c Release -r linux-x64 --self-contained
```

### Entity Framework Core

```bash
# Add migration
dotnet ef migrations add InitialCreate --project backend/ProductService

# Update database
dotnet ef database update --project backend/ProductService

# List migrations
dotnet ef migrations list --project backend/ProductService

# Remove last migration
dotnet ef migrations remove --project backend/ProductService

# Generate SQL script
dotnet ef migrations script --project backend/ProductService

# Drop database (CAUTION!)
dotnet ef database drop --force --project backend/ProductService
```

### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Start dev server
npm run dev

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint

# Type check
npm run type-check
```

### Docker

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f service-name

# Rebuild and restart
docker-compose up -d --build

# Scale a service
docker-compose up -d --scale product-service=3
```

## Architecture Patterns

### Backend Service Structure (ASP.NET Core)

Each microservice follows this pattern:

```
backend/{ServiceName}/
├── Controllers/           # API controllers
│   └── ProductsController.cs
├── Models/               # Domain entities
│   └── Product.cs
├── DTOs/                 # Data transfer objects
│   ├── Requests/        # Request DTOs
│   │   └── CreateProductRequest.cs
│   └── Responses/       # Response DTOs
│       └── ProductResponse.cs
├── Services/            # Business logic
│   ├── IProductService.cs
│   └── ProductService.cs
├── Data/                # EF Core DbContext
│   ├── ApplicationDbContext.cs
│   └── Configurations/  # Entity configurations
├── Migrations/          # EF Core migrations
├── Extensions/          # Extension methods
├── Middleware/          # Custom middleware
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── {ServiceName}.csproj # Project file
```

### REST API Conventions

- **Base URL**: `/api/{resource}`
- **Authentication**: JWT Bearer token in `Authorization` header
- **Content-Type**: `application/json`

**Standard endpoints**:
- `GET /api/{resource}` - List all (with pagination)
- `GET /api/{resource}/{id}` - Get by ID
- `POST /api/{resource}` - Create new
- `PUT /api/{resource}/{id}` - Update existing
- `DELETE /api/{resource}/{id}` - Delete

**Response format**:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```

### Database Naming Conventions

- **Tables**: PascalCase plural (e.g., `Products`, `OrderItems`)
- **Columns**: PascalCase (e.g., `CreatedAt`, `UserId`)
- **Primary Keys**: `Id` (Guid type)
- **Foreign Keys**: `{Table}Id` (e.g., `ProductId`, `CategoryId`)
- **Timestamps**: `CreatedAt`, `UpdatedAt`

### C# Code Style

```csharp
// Entity example
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int InventoryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Service example
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ApplicationDbContext context,
        ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation("Creating product: {Name}", request.Name);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Sku = request.Sku,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }
}

// Controller example
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetProducts(
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        var products = await _productService.GetProductsAsync(page, size);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
```

### Dependency Injection

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Configuration Management

```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=productdb;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "http://localhost:8180/realms/csharptweb",
    "Audience": "csharptweb-client"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}

// Access configuration
public class ProductService
{
    private readonly IConfiguration _configuration;

    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void DoSomething()
    {
        var elasticUri = _configuration["Elasticsearch:Uri"];
    }
}

// Or use strongly-typed options
public class ElasticsearchOptions
{
    public string Uri { get; set; } = string.Empty;
}

// Register options
builder.Services.Configure<ElasticsearchOptions>(
    builder.Configuration.GetSection("Elasticsearch"));

// Inject options
public class ProductService
{
    private readonly ElasticsearchOptions _options;

    public ProductService(IOptions<ElasticsearchOptions> options)
    {
        _options = options.Value;
    }
}
```

### Async/Await Patterns

```csharp
// Good: Async all the way
public async Task<List<Product>> GetProductsAsync()
{
    return await _context.Products.ToListAsync();
}

// Good: ConfigureAwait(false) in libraries
public async Task<Product?> GetProductByIdAsync(Guid id)
{
    return await _context.Products
        .FirstOrDefaultAsync(p => p.Id == id)
        .ConfigureAwait(false);
}

// Avoid: Blocking on async code
public List<Product> GetProducts()
{
    return GetProductsAsync().Result; // BAD - can cause deadlocks
}

// Good: Use async Task instead
public async Task<List<Product>> GetProducts()
{
    return await GetProductsAsync();
}
```

### LINQ Queries

```csharp
// Basic query
var products = await _context.Products
    .Where(p => p.Status == ProductStatus.Active)
    .OrderBy(p => p.Name)
    .ToListAsync();

// Include related entities
var product = await _context.Products
    .Include(p => p.Category)
    .FirstOrDefaultAsync(p => p.Id == id);

// Projection
var productDtos = await _context.Products
    .Select(p => new ProductResponse
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToListAsync();

// AsNoTracking for read-only queries
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.Status == ProductStatus.Active)
    .ToListAsync();

// Pagination
var products = await _context.Products
    .Skip(page * size)
    .Take(size)
    .ToListAsync();
```

### Logging with ILogger

```csharp
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);

        try
        {
            // Business logic
            _logger.LogDebug("Product created with ID: {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            throw;
        }
    }
}
```

## Development Tips

### When Adding a New Feature

1. **Backend**: Create Migration → Entity → Repository (if needed) → Service → Controller → Tests
2. **Frontend**: Create type → Service → Component → Page
3. **Always write tests** for new code
4. **Update API documentation** in docs/API.md
5. **Follow existing patterns** in the codebase

### When Debugging

1. **Check logs**: `docker-compose logs -f service-name`
2. **Database state**: Use DBeaver or Azure Data Studio
3. **API testing**: Use Swagger UI at `http://localhost:8081/swagger`
4. **Network issues**: Check docker network with `docker network inspect csharptweb_default`
5. **Use Visual Studio debugger** for step-by-step debugging

### When Creating EF Core Migrations

```bash
# Add migration
dotnet ef migrations add AddProductReviews --project backend/ProductService

# Review migration file before applying
cat backend/ProductService/Migrations/*_AddProductReviews.cs

# Apply migration
dotnet ef database update --project backend/ProductService

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --project backend/ProductService
```

## Testing Guidelines

### Backend Tests (xUnit)

```csharp
public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsProduct()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new ApplicationDbContext(options);
        var service = new ProductService(context, Mock.Of<ILogger<ProductService>>());

        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 99.99m,
            Sku = "TEST-001"
        };

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(99.99m, result.Price);
    }
}
```

## Common Issues and Solutions

### Issue: Service can't connect to database
**Solution**: Check that PostgreSQL container is running and connection string is correct.
```bash
docker ps | grep postgres
docker logs csharp-postgres
```

### Issue: Port already in use
**Solution**: Find and kill the process using the port.
```bash
# Windows
netstat -ano | findstr :8080
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :8080
kill -9 <PID>
```

### Issue: EF Core migration fails
**Solution**: Check database connection and migration files.
```bash
dotnet ef database drop --force --project backend/ProductService
dotnet ef database update --project backend/ProductService
```

## Best Practices

1. **Always use absolute paths** when working with files
2. **Run tests before committing** code
3. **Keep services independent** - avoid tight coupling
4. **Use DTOs for API contracts** - don't expose entities directly
5. **Handle errors gracefully** - use global exception handling
6. **Log important operations** - use ILogger
7. **Document complex logic** - add XML comments
8. **Follow REST conventions** - use proper HTTP methods
9. **Validate input data** - use Data Annotations and FluentValidation
10. **Use async/await properly** - avoid blocking calls

## Quick Reference

### Port Mapping
- **3000** - React Frontend
- **8080** - Yarp API Gateway
- **8081** - Product Service
- **8082** - Order Service
- **8083** - Payment Service
- **8180** - Keycloak
- **5432** - PostgreSQL
- **9200** - Elasticsearch

### Key Technologies
- ASP.NET Core: 8.0
- C#: 12
- Entity Framework Core: 8.0
- React: 18.x
- TypeScript: 5.0.x
- PostgreSQL: 15.x
- Elasticsearch: 8.11.x

### Documentation Links
- Main README: `/README.md`
- Architecture: `/docs/ARCHITECTURE.md`
- API Docs: `/docs/API.md`
- Development: `/docs/DEVELOPMENT.md`
- C# Guide: `/docs/CSHARP-GUIDE.md`
- Deployment: `/docs/DEPLOYMENT.md`

---

When working with this project, always refer to existing code patterns and maintain consistency with the established architecture. If unsure about a pattern or convention, check similar implementations in the codebase or refer to the documentation.

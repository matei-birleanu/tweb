# Contributing to C# Shop Platform

Thank you for considering contributing to our ASP.NET Core project!

## Development Setup

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/csharptweb.git`
3. Install .NET 8.0 SDK
4. Create a branch: `git checkout -b feature/your-feature-name`
5. Make your changes
6. Run tests: `dotnet test`
7. Commit with descriptive messages
8. Push to your fork: `git push origin feature/your-feature-name`
9. Open a Pull Request

## Code Standards

### Backend (C# / ASP.NET Core)
- Follow C# coding conventions
- Use meaningful variable and method names
- Write XML documentation comments for public APIs
- Use async/await for all I/O operations
- Implement proper dependency injection
- Write unit tests for all service methods
- Use Entity Framework Core for database access
- Follow repository pattern
- Use DTOs for API responses

### Frontend (React/TypeScript)
- Follow React best practices
- Use TypeScript strict mode
- Write functional components with hooks
- Use proper typing for all props and state
- Follow Material-UI theming conventions
- Write unit tests for components

## Commit Messages

Format: `type(scope): description`

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

Example: `feat(product): add elasticsearch search functionality`

## Pull Request Process

1. Update README.md if needed
2. Add tests for new functionality
3. Ensure all tests pass: `dotnet test`
4. Ensure code builds: `dotnet build`
5. Update documentation
6. Request review from maintainers

## C# Specific Guidelines

### Naming Conventions
- PascalCase for class names, method names, properties
- camelCase for local variables and parameters
- Prefix interfaces with `I` (e.g., `IProductService`)
- Suffix async methods with `Async` (e.g., `GetProductAsync`)

### Code Structure
```csharp
// Good
public async Task<ProductDto> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    if (product == null)
    {
        throw new NotFoundException($"Product with ID {id} not found");
    }

    return _mapper.Map<ProductDto>(product);
}

// Bad
public ProductDto GetProduct(int id)
{
    var product = _repository.GetById(id).Result;
    return new ProductDto { Id = product.Id, Name = product.Name };
}
```

### Dependency Injection
Always use constructor injection:
```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
}
```

## Entity Framework Core

### Migrations
```bash
# Create migration
cd backend/ProductService
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Entity Configuration
Use Fluent API in `OnModelCreating`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        entity.HasIndex(e => e.Sku).IsUnique();
    });
}
```

## Testing

### Unit Tests (xUnit)
```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetProductAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var expected = new Product { Id = 1, Name = "Test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetProductAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Name, result.Name);
    }
}
```

## Code Review

All submissions require review. We use GitHub pull requests for this purpose.

## Questions?

Feel free to open an issue for questions or discussions.

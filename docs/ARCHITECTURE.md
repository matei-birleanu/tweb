# Architecture Documentation - C# ASP.NET Core

## Table of Contents

- [Overview](#overview)
- [Microservices Architecture](#microservices-architecture)
- [Service Descriptions](#service-descriptions)
- [Communication Patterns](#communication-patterns)
- [Data Flow](#data-flow)
- [Database Schema](#database-schema)
- [Security Architecture](#security-architecture)
- [Scalability & Performance](#scalability--performance)
- [Resilience Patterns](#resilience-patterns)

## Overview

This e-commerce platform follows a microservices architecture pattern where the application is composed of loosely coupled, independently deployable services built with ASP.NET Core 8.0 and C# 12. Each service is responsible for a specific business capability and maintains its own database using Entity Framework Core.

### Design Principles

1. **Single Responsibility**: Each service handles one business domain
2. **Database per Service**: Each service owns its data with EF Core
3. **API-First Design**: Well-defined REST APIs for all services
4. **Decentralized Data Management**: No shared databases
5. **Fault Isolation**: Failures in one service don't cascade (using Polly)
6. **Independent Deployment**: Services can be deployed independently
7. **Technology Consistency**: All services use ASP.NET Core 8.0

## Microservices Architecture

### Complete System Architecture

```
                                    ┌─────────────────────┐
                                    │   React Frontend    │
                                    │  (TypeScript/Vite)  │
                                    └──────────┬──────────┘
                                               │
                                               │ HTTPS/REST
                                               │
                    ┌──────────────────────────▼────────────────────────┐
                    │             Yarp API Gateway                      │
                    │              (ASP.NET Core)                       │
                    │  ┌──────────────────────────────────────────┐   │
                    │  │  • Route Management                      │   │
                    │  │  • Load Balancing                        │   │
                    │  │  • Rate Limiting                         │   │
                    │  │  • Authentication (JWT)                  │   │
                    │  │  • Request/Response Transformation       │   │
                    │  │  • Circuit Breaker (Polly)               │   │
                    │  └──────────────────────────────────────────┘   │
                    └─┬──────────────┬─────────────┬─────────────┬────┘
                      │              │             │             │
         ─────────────┼──────────────┼─────────────┼─────────────┼────────
                      │              │             │             │
        ┌─────────────▼─────┐  ┌────▼─────┐  ┌───▼─────┐  ┌────▼─────────┐
        │  Product Service  │  │  Order   │  │ Payment │  │User Service  │
        │   (ASP.NET Core)  │  │ Service  │  │ Service │  │  (Future)    │
        ├───────────────────┤  ├──────────┤  ├─────────┤  ├──────────────┤
        │ • Product CRUD    │  │ • Order  │  │ • Stripe│  │ • Profile    │
        │ • Inventory Mgmt  │  │   CRUD   │  │   API   │  │ • Addresses  │
        │ • Categories      │  │ • Status │  │ • Webhook│ │ • Wishlist   │
        │ • EF Core         │  │   Track  │  │ • Refund│  │              │
        │ • Search Index    │  │ • History│  │         │  │              │
        └─────────┬─────────┘  └────┬─────┘  └────┬────┘  └──────────────┘
                  │                 │             │
                  ▼                 ▼             │
        ┌─────────────────┐  ┌──────────────┐   │
        │   PostgreSQL    │  │  PostgreSQL  │   │
        │  (Products DB)  │  │  (Orders DB) │   │
        │   EF Core 8.0   │  │  EF Core 8.0 │   │
        └─────────────────┘  └──────────────┘   │
                  │                              │
                  │                              │
        ┌─────────▼──────────────────────────────▼─────┐
        │            Elasticsearch Cluster              │
        │  • Product Search & Filtering                 │
        │  • Full-text Search                           │
        │  • Aggregations & Analytics                   │
        └───────────────────────────────────────────────┘

        ┌───────────────────────────────────────────────┐
        │         Keycloak / IdentityServer             │
        │  • User Authentication (OAuth2/OIDC)          │
        │  • Authorization (RBAC)                       │
        │  • JWT Token Generation                       │
        │  • Single Sign-On (SSO)                       │
        └───────────────────────────────────────────────┘

        ┌───────────────────────────────────────────────┐
        │          Monitoring & Observability           │
        │  • Prometheus (Metrics)                       │
        │  • Grafana (Dashboards)                       │
        │  • Serilog (Structured Logging)               │
        │  • OpenTelemetry (Distributed Tracing)        │
        └───────────────────────────────────────────────┘
```

## Service Descriptions

### 1. Yarp API Gateway

**Port**: 8080
**Technology**: Yarp (Yet Another Reverse Proxy) on ASP.NET Core
**Purpose**: Single entry point for all client requests

**Responsibilities**:
- Route requests to appropriate microservices
- Load balancing across service instances
- Authentication and authorization (JWT)
- Rate limiting and throttling
- Request/response transformation
- Circuit breaker implementation with Polly
- CORS handling
- API versioning

**Key Configuration**:
```csharp
// appsettings.json
{
  "ReverseProxy": {
    "Routes": {
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "/api/products/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/products/{**catch-all}" }
        ]
      }
    },
    "Clusters": {
      "product-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://product-service:8081"
          }
        }
      }
    }
  }
}
```

### 2. Product Service

**Port**: 8081
**Technology**: ASP.NET Core 8.0 + C# 12 + EF Core + PostgreSQL + Elasticsearch
**Purpose**: Product catalog management

**Responsibilities**:
- CRUD operations for products
- Category management
- Inventory tracking
- Price management
- Product search indexing
- Image storage references
- SKU management
- Product variants

**Entity Framework Core Models**:
```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int InventoryCount { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string[]? Images { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ProductStatus
{
    Active,
    Inactive,
    OutOfStock
}
```

**Database Configuration**:
```csharp
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.ParentId);

            entity.HasOne(e => e.Parent)
                  .WithMany(e => e.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

### 3. Order Service

**Port**: 8082
**Technology**: ASP.NET Core 8.0 + C# 12 + EF Core + PostgreSQL
**Purpose**: Order processing and management

**Responsibilities**:
- Order creation and validation
- Order status tracking
- Order history
- Cart-to-order conversion
- Order cancellation
- Integration with payment service
- Order notifications

**Entity Framework Core Models**:
```csharp
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentId { get; set; }
    public Address ShippingAddress { get; set; } = null!;
    public Address BillingAddress { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}
```

**Order State Machine**:
```
PENDING → CONFIRMED → PROCESSING → SHIPPED → DELIVERED
   ↓                      ↓
CANCELLED ← ← ← ← ← ← ← ←
```

### 4. Payment Service

**Port**: 8083
**Technology**: ASP.NET Core 8.0 + C# 12 + Stripe API
**Purpose**: Payment processing integration

**Responsibilities**:
- Stripe payment processing
- Payment intent creation
- Webhook handling
- Payment confirmation
- Refund processing
- Payment history

**Payment Flow**:
```
1. Client requests payment intent
2. Service creates Stripe PaymentIntent
3. Client confirms payment with Stripe
4. Stripe sends webhook to service
5. Service validates and processes webhook
6. Update order payment status
7. Notify order service
```

### 5. Keycloak / IdentityServer (Authentication Service)

**Port**: 8180
**Technology**: Keycloak or IdentityServer4
**Purpose**: Identity and access management

**Responsibilities**:
- User authentication (OAuth2/OIDC)
- User registration
- Password management
- Role-based access control (RBAC)
- JWT token generation
- Single Sign-On (SSO)

**Roles**:
- `customer`: Regular users
- `admin`: Platform administrators
- `vendor`: Product vendors (future)

## Communication Patterns

### 1. Synchronous Communication (REST/HTTP)

Used for:
- Client-to-Gateway communication
- Gateway-to-Service communication
- Real-time data queries

**Example using HttpClient**:
```csharp
public class ProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Product?> GetProductAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>();
    }
}
```

### 2. Asynchronous Communication (Message Queue)

Planned for future implementation using RabbitMQ or Azure Service Bus:
- Order placed events
- Inventory updates
- Email notifications
- Payment confirmations

## Data Flow

### User Authentication Flow

```
┌────────┐     1. Login Request      ┌─────────────┐
│        │ ────────────────────────→ │             │
│        │                            │  Keycloak/  │
│ Client │     2. JWT Token          │  Identity   │
│        │ ←──────────────────────── │   Server    │
│        │                            └─────────────┘
└────┬───┘
     │
     │ 3. API Request + JWT
     │
     ▼
┌─────────────┐     4. Validate JWT   ┌─────────────┐
│             │ ────────────────────→ │             │
│ Yarp Gateway│                        │  Keycloak   │
│             │ ←──────────────────── │             │
└─────┬───────┘     5. Token Valid    └─────────────┘
      │
      │ 6. Forward Request
      │
      ▼
┌─────────────┐
│  Product    │
│  Service    │
└─────────────┘
```

### Order Creation Flow

```
1. Client submits order
        ↓
2. Yarp Gateway validates JWT
        ↓
3. Order Service receives request
        ↓
4. Validate product availability (HTTP call to Product Service)
        ↓
5. Create payment intent (HTTP call to Payment Service)
        ↓
6. Save order with PENDING status (EF Core)
        ↓
7. Return payment intent to client
        ↓
8. Client confirms payment with Stripe
        ↓
9. Stripe webhook → Payment Service
        ↓
10. Payment Service updates order status (HTTP call to Order Service)
        ↓
11. Order Service updates inventory (HTTP call to Product Service)
        ↓
12. Order confirmed
```

## Database Schema

### Product Service Database (Entity Framework Core)

```csharp
// Migration
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Categories_Categories_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                InventoryCount = table.Column<int>(type: "integer", nullable: false),
                Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Images = table.Column<string[]>(type: "text[]", nullable: true),
                Attributes = table.Column<string>(type: "jsonb", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryId",
            table: "Products",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Sku",
            table: "Products",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_Status",
            table: "Products",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ParentId",
            table: "Categories",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_Slug",
            table: "Categories",
            column: "Slug",
            unique: true);
    }
}
```

## Security Architecture

### Authentication & Authorization

1. **OAuth2 + OpenID Connect** via Keycloak or IdentityServer
2. **JWT tokens** for stateless authentication
3. **Role-based access control (RBAC)**
4. **Yarp Gateway enforces security** on all requests

**ASP.NET Core Authentication Configuration**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = false; // Development only
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("user", "admin"));
});
```

**Controller Authorization**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // Public endpoint
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Product>> CreateProduct(CreateProductDto dto)
    {
        // Admin only
    }
}
```

### Security Best Practices

- Passwords hashed with ASP.NET Core Identity (PBKDF2)
- Sensitive data encrypted at rest
- HTTPS enforced in production
- CORS properly configured
- Rate limiting on all endpoints (AspNetCoreRateLimit)
- SQL injection prevention (EF Core parameterized queries)
- XSS protection headers
- CSRF tokens for state-changing operations

## Scalability & Performance

### Horizontal Scaling

All services are stateless and can be scaled horizontally using Docker Compose or Kubernetes.

### Caching Strategy

- **IMemoryCache**: In-process caching for frequently accessed data
- **IDistributedCache with Redis**: Distributed caching across instances
- **Response Caching Middleware**: HTTP response caching
- **CDN**: Static assets, product images

**Example**:
```csharp
public class ProductService
{
    private readonly IMemoryCache _cache;
    private readonly ProductDbContext _context;

    public async Task<Product?> GetProductAsync(Guid id)
    {
        var cacheKey = $"product_{id}";

        if (!_cache.TryGetValue(cacheKey, out Product? product))
        {
            product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                _cache.Set(cacheKey, product, TimeSpan.FromMinutes(10));
            }
        }

        return product;
    }
}
```

### Database Optimization

- Proper indexing with EF Core
- Connection pooling (built-in with EF Core)
- Compiled queries for frequently used queries
- AsNoTracking for read-only queries

```csharp
// Compiled query example
private static readonly Func<ProductDbContext, Guid, Task<Product?>> GetProductQuery =
    EF.CompileAsyncQuery((ProductDbContext context, Guid id) =>
        context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id));
```

## Resilience Patterns

### 1. Circuit Breaker with Polly

```csharp
builder.Services.AddHttpClient<IProductService, ProductService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

### 2. Health Checks

Each service exposes health endpoints:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>()
    .AddElasticsearch(builder.Configuration["Elasticsearch:Uri"]!)
    .AddCheck("self", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

## Future Enhancements

1. **Message Queue** (RabbitMQ/Azure Service Bus) for asynchronous communication
2. **Service Mesh** (Istio/Linkerd) for advanced traffic management
3. **Distributed Tracing** (OpenTelemetry + Jaeger)
4. **Centralized Configuration** (Azure App Configuration)
5. **Service Discovery** (Consul)
6. **API Rate Limiting** (AspNetCoreRateLimit)
7. **Monitoring** (Prometheus + Grafana)
8. **Logging** (Serilog + Elasticsearch)
9. **Event Sourcing** for order history
10. **CQRS** pattern for complex queries

---

Last Updated: 2026-03-14

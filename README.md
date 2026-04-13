# Platformă E-Commerce — ASP.NET Core 8 + React

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![C#](https://img.shields.io/badge/C%23-12-blue)
![React](https://img.shields.io/badge/React-18-blue)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)
![Docker](https://img.shields.io/badge/Docker-Compose-blue)

Platformă de e-commerce completă cu arhitectură microservicii, construită cu **ASP.NET Core 8 (C#)** la backend, **React 18 + TypeScript** la frontend, **PostgreSQL** ca bază de date, **Elasticsearch** pentru căutare și **Stripe** pentru plăți. Totul rulează în **Docker**.

---

## Cuprins

1. [Arhitectura sistemului](#1-arhitectura-sistemului)
2. [Structura proiectului](#2-structura-proiectului)
3. [Backend — ASP.NET Core](#3-backend--aspnet-core)
   - [API Gateway](#31-api-gateway-port-8080)
   - [Product Service](#32-product-service-port-8081)
   - [Order Service](#33-order-service-port-8082)
   - [Entități și relații](#34-entități-și-relații)
   - [Pattern-uri folosite](#35-pattern-uri-folosite)
4. [Frontend — React + TypeScript](#4-frontend--react--typescript)
5. [Baza de date — PostgreSQL](#5-baza-de-date--postgresql)
6. [Autentificare — Keycloak + JWT](#6-autentificare--keycloak--jwt)
7. [Căutare — Elasticsearch](#7-căutare--elasticsearch)
8. [Plăți — Stripe](#8-plăți--stripe)
9. [Email — MailTrap](#9-email--mailtrap)
10. [Docker & Containerizare](#10-docker--containerizare)
11. [CI/CD — GitHub Actions](#11-cicd--github-actions)
12. [Cum rulezi proiectul](#12-cum-rulezi-proiectul)
13. [Întrebări frecvente pentru laborant](#13-întrebări-frecvente-pentru-laborant)

---

## 1. Arhitectura sistemului

```
                    ┌──────────────────────────────────┐
                    │         CLIENT (Browser)          │
                    │   React 18 + TypeScript           │
                    │   Material-UI · Axios · Formik    │
                    └─────────────────┬────────────────┘
                                      │ HTTP port 3000
                    ┌─────────────────▼────────────────┐
                    │        API GATEWAY (8080)         │
                    │         YARP Reverse Proxy        │
                    │  • Validare JWT                   │
                    │  • Rutare cereri                  │
                    │  • Middleware global erori        │
                    └──────────┬───────────────┬────────┘
                               │               │
            ┌──────────────────▼───┐   ┌───────▼──────────────────┐
            │  PRODUCT SERVICE     │   │     ORDER SERVICE         │
            │    (port 8081)       │   │      (port 8082)          │
            │                      │   │                           │
            │  • CRUD produse      │   │  • CRUD comenzi           │
            │  • Elasticsearch     │   │  • Plăți Stripe           │
            │  • Stoc              │   │  • Feedback utilizatori   │
            │  • PostgreSQL        │   │  • Înregistrare useri     │
            └──────────────────────┘   │  • Email MailTrap         │
                       │               │  • PostgreSQL             │
                       │               └───────────────────────────┘
                       │                           │
            ┌──────────▼───────────────────────────▼──────┐
            │              PostgreSQL 15 (port 5432)       │
            └──────────────────────────────────────────────┘
                       │
            ┌──────────▼────────────┐    ┌─────────────────────┐
            │  Elasticsearch 8      │    │   Keycloak 23        │
            │  (port 9200)          │    │   (port 8180)        │
            │  Motor căutare        │    │   Identity Provider  │
            └───────────────────────┘    └─────────────────────┘
```

**Principii arhitecturale:**
- **Microservicii** — fiecare serviciu are responsabilitate unică și baza de date proprie
- **API Gateway** — punct unic de intrare, izolează serviciile interne
- **Stateless** — nicio stare păstrată în backend; token JWT în client
- **Container-first** — fiecare serviciu rulează în propriul container Docker

---

## 2. Structura proiectului

```
csharptweb/
│
├── backend/                          ← Soluție .NET (ShopPlatform.sln)
│   ├── ApiGateway/                   ← YARP Gateway (port 8080)
│   │   ├── Controllers/
│   │   │   └── HealthController.cs
│   │   ├── Middleware/
│   │   │   └── JwtMiddleware.cs      ← Validare JWT pe fiecare cerere
│   │   └── Program.cs
│   │
│   ├── ProductService/               ← Microserviciu produse (port 8081)
│   │   ├── Controllers/
│   │   │   └── ProductsController.cs
│   │   ├── Models/
│   │   │   └── Product.cs            ← Entitate EF Core
│   │   ├── DTOs/
│   │   │   ├── CreateProductDto.cs
│   │   │   ├── UpdateProductDto.cs
│   │   │   ├── ProductDto.cs
│   │   │   └── SearchRequestDto.cs
│   │   ├── Services/
│   │   │   ├── IProductService.cs
│   │   │   ├── ProductService.cs
│   │   │   └── ElasticsearchService.cs
│   │   ├── Repositories/
│   │   │   ├── IProductRepository.cs
│   │   │   └── ProductRepository.cs
│   │   ├── Data/
│   │   │   └── ApplicationDbContext.cs
│   │   ├── Validators/
│   │   │   └── CreateProductValidator.cs
│   │   ├── Mappings/
│   │   │   └── AutoMapperProfile.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionMiddleware.cs
│   │   └── Program.cs
│   │
│   └── OrderService/                 ← Microserviciu comenzi (port 8082)
│       ├── Controllers/              ← (există în serviciu)
│       ├── Models/
│       │   ├── Order.cs
│       │   ├── User.cs
│       │   ├── Payment.cs
│       │   ├── Feedback.cs
│       │   └── Enums/
│       │       ├── OrderStatus.cs
│       │       ├── OrderType.cs
│       │       ├── PaymentStatus.cs
│       │       └── FeedbackCategory.cs
│       ├── DTOs/
│       │   ├── CreateOrderDto.cs
│       │   ├── OrderDto.cs
│       │   ├── CreateUserDto.cs
│       │   ├── UserDto.cs
│       │   ├── PaymentDto.cs
│       │   ├── CreateFeedbackDto.cs
│       │   └── FeedbackDto.cs
│       ├── Services/                 ← Logică business
│       ├── Repositories/
│       │   ├── IOrderRepository.cs + OrderRepository.cs
│       │   ├── IUserRepository.cs + UserRepository.cs
│       │   ├── IPaymentRepository.cs + PaymentRepository.cs
│       │   └── IFeedbackRepository.cs + FeedbackRepository.cs
│       ├── Data/
│       │   └── ApplicationDbContext.cs
│       └── Middleware/
│           └── ExceptionMiddleware.cs
│
├── frontend/                         ← React 18 + TypeScript
│   └── src/
│       ├── App.tsx                   ← Router principal + layout
│       ├── main.tsx                  ← Entry point React
│       ├── theme.ts                  ← Tema Material-UI
│       ├── components/
│       │   ├── Navbar.tsx
│       │   ├── ProtectedRoute.tsx
│       │   ├── LoadingSpinner.tsx
│       │   └── ErrorMessage.tsx
│       ├── pages/
│       │   ├── HomePage.tsx
│       │   ├── LoginPage.tsx
│       │   ├── RegisterPage.tsx
│       │   ├── ProductsPage.tsx
│       │   ├── ProductDetailPage.tsx
│       │   ├── OrdersPage.tsx
│       │   ├── AdminProductsPage.tsx
│       │   ├── FeedbackPage.tsx
│       │   └── NotFoundPage.tsx
│       ├── services/
│       │   ├── apiClient.ts          ← Axios instance cu interceptori JWT
│       │   ├── authService.ts
│       │   ├── productService.ts
│       │   ├── orderService.ts
│       │   ├── feedbackService.ts
│       │   └── userService.ts
│       ├── types/
│       │   ├── product.types.ts
│       │   ├── order.types.ts
│       │   ├── feedback.types.ts
│       │   ├── auth.types.ts
│       │   └── common.types.ts
│       └── utils/
│           ├── keycloak.ts           ← Client Keycloak JS
│           ├── storage.ts            ← Gestionare localStorage
│           └── validation.ts
│
├── database/
│   └── init.sql                      ← Schema inițială PostgreSQL
│
├── keycloak-config/
│   └── realm-export.json             ← Configurare realm Keycloak
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API.md
│   └── CSHARP-GUIDE.md
│
├── .github/workflows/                ← CI/CD GitHub Actions
│   ├── dotnet-ci.yml
│   ├── frontend-ci.yml
│   ├── docker-build.yml
│   ├── code-quality.yml
│   ├── deploy.yml
│   └── release.yml
│
├── docker-compose.yml                ← Orchestrare servicii
├── docker-compose.dev.yml
├── docker-compose.prod.yml
├── Makefile                          ← Comenzi utile
└── .env.example                      ← Variabile de mediu (template)
```

---

## 3. Backend — ASP.NET Core

### 3.1 API Gateway (port 8080)

**Tehnologie**: YARP (Yet Another Reverse Proxy) — bibliotecă Microsoft pentru reverse proxy în .NET.

| Fișier | Ce face |
|---|---|
| `Program.cs` | Configurează YARP, JWT Bearer, middleware-uri |
| `Middleware/JwtMiddleware.cs` | Interceptează fiecare cerere, validează token JWT, extrage claims |
| `Controllers/HealthController.cs` | Endpoint `/health` pentru verificarea stării serviciului |

**Cum funcționează rutarea YARP** (din `appsettings.json`):
```json
{
  "ReverseProxy": {
    "Routes": {
      "products-route": {
        "ClusterId": "product-cluster",
        "Match": { "Path": "/api/products/{**catch-all}" }
      },
      "orders-route": {
        "ClusterId": "order-cluster",
        "Match": { "Path": "/api/orders/{**catch-all}" }
      }
    },
    "Clusters": {
      "product-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://product-service:8081/" }
        }
      }
    }
  }
}
```
Gateway-ul citește config-ul și rutează automat — `/api/products/**` merge la ProductService, `/api/orders/**` merge la OrderService.

---

### 3.2 Product Service (port 8081)

**Responsabilitate**: gestionare completă a produselor — CRUD, stoc, căutare Elasticsearch.

#### Endpoint-uri REST

| Metodă | URL | Descriere | Rol necesar |
|---|---|---|---|
| `GET` | `/api/products` | Listare produse (paginat) | Public |
| `GET` | `/api/products/{id}` | Detalii produs | Public |
| `GET` | `/api/products/category/{cat}` | Filtrare după categorie | Public |
| `POST` | `/api/products/search` | Căutare full-text Elasticsearch | Public |
| `POST` | `/api/products` | Creare produs | Admin |
| `PUT` | `/api/products/{id}` | Actualizare produs | Admin |
| `DELETE` | `/api/products/{id}` | Ștergere produs | Admin |
| `POST` | `/admin/products/reindex` | Re-indexare Elasticsearch | Admin |

#### Fișiere principale

**`Models/Product.cs`** — entitatea bază de date:
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; }
    public string ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**`DTOs/CreateProductDto.cs`** — ce primim de la client (validat):
```csharp
public class CreateProductDto
{
    [Required] public string Name { get; set; }
    public string Description { get; set; }
    [Range(0.01, 999999)] public decimal Price { get; set; }
    [Range(0, int.MaxValue)] public int Stock { get; set; }
    public string Category { get; set; }
}
```

**`Validators/CreateProductValidator.cs`** — FluentValidation:
```csharp
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
```

**`Mappings/AutoMapperProfile.cs`** — mapare automată Model ↔ DTO:
```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
    }
}
```

**`Middleware/ExceptionMiddleware.cs`** — prinde excepțiile și returnează răspuns JSON structurat:
```csharp
// Dacă apare o excepție oriunde în aplicație:
// → 404 NotFound pentru resurse inexistente
// → 400 BadRequest pentru date invalide
// → 500 InternalServerError pentru erori neașteptate
```

---

### 3.3 Order Service (port 8082)

**Responsabilitate**: comenzi, plăți, feedback, înregistrare utilizatori, email-uri.

#### Endpoint-uri REST

| Metodă | URL | Descriere | Rol |
|---|---|---|---|
| `POST` | `/api/users` | Înregistrare utilizator | Public |
| `GET` | `/api/users/{id}` | Profil utilizator | User |
| `GET` | `/api/orders` | Comenzile userului curent | User |
| `GET` | `/api/orders/{id}` | Detalii comandă | User |
| `POST` | `/api/orders` | Plasare comandă | User |
| `PUT` | `/api/orders/{id}` | Update status | User/Admin |
| `DELETE` | `/api/orders/{id}` | Anulare comandă | User |
| `POST` | `/api/payments/create-intent` | Creare Payment Intent Stripe | User |
| `POST` | `/api/payments/webhook` | Webhook Stripe | Public (semnat) |
| `POST` | `/api/feedback` | Trimitere feedback | User |
| `GET` | `/api/feedback` | Listare feedback | Admin |

#### Modele / Entități

**`Models/Order.cs`**:
```csharp
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }          // Relație many-to-one
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderType Type { get; set; }     // Enum: Buy / Sell
    public OrderStatus Status { get; set; } // Enum: Pending/Completed/etc
    public Payment Payment { get; set; }    // Relație one-to-one
    public DateTime CreatedAt { get; set; }
}
```

**`Models/Enums/OrderStatus.cs`** — stări comandă:
```csharp
public enum OrderStatus
{
    Pending, PendingPayment, PaymentProcessing,
    Completed, Cancelled, Failed, Refunded
}
```

**`Models/Feedback.cs`**:
```csharp
public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public FeedbackCategory Category { get; set; } // Enum
    public string Title { get; set; }
    public string Message { get; set; }
    public int Rating { get; set; }  // 1-5
    public DateTime CreatedAt { get; set; }
}
```

---

### 3.4 Entități și relații

Proiectul implementează **5 entități** cu toate tipurile de relații cerute:

```
┌──────────┐         ┌──────────┐         ┌──────────┐
│   User   │──1..N──▶│  Order   │──1..1──▶│ Payment  │
│          │         │          │         │          │
│ id       │         │ id       │         │ id       │
│ username │         │ user_id  │         │ order_id │
│ email    │         │ product_id          │ amount   │
│ password │         │ quantity │         │ status   │
│ _hash    │         │ total    │         │ stripe_id│
└──────────┘         │ status   │         └──────────┘
     │               │ type     │
     │ 1..N          └──────────┘
     ▼
┌──────────┐         ┌──────────┐
│ Feedback │         │ Product  │
│          │         │          │
│ id       │         │ id       │
│ user_id  │         │ name     │
│ category │         │ price    │
│ message  │         │ stock    │
│ rating   │         │ category │
└──────────┘         └──────────┘

User ↔ Roles = many-to-many (un user are N roluri)
```

| Relație | Tip |
|---|---|
| User → Orders | **one-to-many** |
| Order → Payment | **one-to-one** |
| User → Feedback | **one-to-many** |
| User ↔ Roles | **many-to-many** |

**Configurare relații în EF Core** (FluentAPI în `ApplicationDbContext.cs`):
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // one-to-many: User → Orders
    modelBuilder.Entity<Order>()
        .HasOne(o => o.User)
        .WithMany(u => u.Orders)
        .HasForeignKey(o => o.UserId);

    // one-to-one: Order → Payment
    modelBuilder.Entity<Payment>()
        .HasOne(p => p.Order)
        .WithOne(o => o.Payment)
        .HasForeignKey<Payment>(p => p.OrderId);
}
```

---

### 3.5 Pattern-uri folosite

#### Repository Pattern

Separă logica de acces la date de logica business:

```csharp
// Interfața — contract abstract
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(int page, int size);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

// Implementarea — folosește EF Core concret
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
        => _context = context;

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products.FindAsync(id);

    public async Task<IEnumerable<Product>> GetAllAsync(int page, int size)
        => await _context.Products
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
}
```

#### DTO Pattern

Nu expune niciodată entitățile direct. Separă datele interne de cele expuse la API:

```csharp
// Entitate (intern, în baza de date)
public class User { public string PasswordHash { get; set; } ... }

// DTO Request (ce primim de la client — validat)
public class CreateUserDto {
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // parola plain-text, hashată în service
}

// DTO Response (ce trimitem la client — fără date sensibile)
public class UserDto {
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    // NU includem PasswordHash!
}
```

#### Dependency Injection

ASP.NET Core injectează automat dependențele:

```csharp
// Înregistrare în Program.cs
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IElasticsearchService, ElasticsearchService>();

// Injectat automat de framework în constructor
public class ProductsController
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;
}
```

#### Global Error Handling

`ExceptionMiddleware.cs` prinde toate excepțiile și returnează răspuns JSON consistent:
```json
{
  "statusCode": 404,
  "message": "Product with id 99 not found",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## 4. Frontend — React + TypeScript

**Tehnologii:**

| Librărie | Versiune | Scop |
|---|---|---|
| React | 18.2 | UI framework |
| TypeScript | 5.3 | Type safety |
| Vite | 5.0 | Build tool rapid |
| Material-UI | 5.14 | Componente UI |
| React Router | 6.20 | Navigare SPA |
| Axios | 1.6 | HTTP client |
| Formik | 2.4 | Gestionare formulare |
| Yup | 1.3 | Validare formulare |
| React Query | 5.14 | Server state management |
| Keycloak JS | 23.0 | Client OAuth2/OIDC |

### Componente

| Fișier | Ce face |
|---|---|
| `Navbar.tsx` | Bara de navigare cu link-uri și meniu utilizator (login/logout/rol) |
| `ProtectedRoute.tsx` | Wrapper pentru rute — redirecționează la login dacă nu ești autentificat sau nu ai rolul necesar |
| `LoadingSpinner.tsx` | Indicator de încărcare afișat în timpul fetch-urilor |
| `ErrorMessage.tsx` | Afișare erori API în format user-friendly |

### Pagini

| Pagină | Rută | Descriere |
|---|---|---|
| `HomePage.tsx` | `/` | Landing page cu produse recomandate |
| `LoginPage.tsx` | `/login` | Formular autentificare cu Formik + Yup |
| `RegisterPage.tsx` | `/register` | Înregistrare: nume, username, email, parolă, confirmare parolă |
| `ProductsPage.tsx` | `/products` | Catalog cu search, filtrare categorie, paginare — tabel 4+ coloane |
| `ProductDetailPage.tsx` | `/products/:id` | Detalii produs + buton cumpără |
| `OrdersPage.tsx` | `/orders` | Istoricul comenzilor — tabel paginat 4+ coloane + filtrare status |
| `AdminProductsPage.tsx` | `/admin/products` | CRUD produse (doar admin) — add/edit/delete cu modal de confirmare |
| `FeedbackPage.tsx` | `/feedback` | Formular cu `<select>`, `<radio>`, `<checkbox>`, `<textarea>` |
| `NotFoundPage.tsx` | `*` | Pagina 404 |

### Servicii API

**`services/apiClient.ts`** — instanță Axios centralizată:
```typescript
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

// Interceptor: adaugă automat JWT la fiecare cerere
apiClient.interceptors.request.use((config) => {
  const token = storage.getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Interceptor: redirect la login dacă token expirat
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) window.location.href = '/login';
    return Promise.reject(error);
  }
);
```

### TypeScript Types

Toate datele sunt tipizate strict:
```typescript
// types/product.types.ts
export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  imageUrl: string;
  isAvailable: boolean;
}

// types/order.types.ts
export type OrderStatus =
  | 'Pending' | 'PendingPayment' | 'PaymentProcessing'
  | 'Completed' | 'Cancelled' | 'Failed' | 'Refunded';

export interface Order {
  id: number;
  productId: number;
  quantity: number;
  totalPrice: number;
  status: OrderStatus;
  createdAt: string;
}
```

### Routing și protecție rute

```tsx
// App.tsx
<Routes>
  <Route path="/" element={<HomePage />} />
  <Route path="/login" element={<LoginPage />} />
  <Route path="/register" element={<RegisterPage />} />
  <Route path="/products" element={<ProductsPage />} />

  {/* Rute protejate — necesită autentificare */}
  <Route element={<ProtectedRoute />}>
    <Route path="/orders" element={<OrdersPage />} />
    <Route path="/feedback" element={<FeedbackPage />} />
  </Route>

  {/* Rute admin — necesită rol admin */}
  <Route element={<ProtectedRoute requiredRole="admin" />}>
    <Route path="/admin/products" element={<AdminProductsPage />} />
  </Route>

  <Route path="*" element={<NotFoundPage />} />
</Routes>
```

---

## 5. Baza de date — PostgreSQL

**ORM**: Entity Framework Core 8.0 — traduce C# în SQL automat.

### Schema principală

```sql
-- Produse (gestionat de ProductService)
CREATE TABLE products (
    id           SERIAL PRIMARY KEY,
    name         VARCHAR(200) NOT NULL,
    description  TEXT,
    price        DECIMAL(10,2) NOT NULL CHECK (price > 0),
    stock        INTEGER DEFAULT 0 CHECK (stock >= 0),
    category     VARCHAR(100),
    image_url    VARCHAR(500),
    is_available BOOLEAN DEFAULT true,
    created_at   TIMESTAMP DEFAULT NOW(),
    updated_at   TIMESTAMP DEFAULT NOW()
);

-- Utilizatori (gestionat de OrderService)
CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    username      VARCHAR(100) UNIQUE NOT NULL,
    email         VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMP DEFAULT NOW()
);

-- Comenzi
CREATE TABLE orders (
    id               SERIAL PRIMARY KEY,
    user_id          INTEGER NOT NULL REFERENCES users(id),
    product_id       INTEGER NOT NULL,
    quantity         INTEGER NOT NULL CHECK (quantity > 0),
    total_price      DECIMAL(10,2) NOT NULL,
    order_type       VARCHAR(20) CHECK (order_type IN ('Buy','Sell')),
    status           VARCHAR(30) DEFAULT 'Pending',
    shipping_address TEXT,
    created_at       TIMESTAMP DEFAULT NOW()
);

-- Plăți (one-to-one cu orders)
CREATE TABLE payments (
    id                        SERIAL PRIMARY KEY,
    order_id                  INTEGER UNIQUE REFERENCES orders(id),
    stripe_payment_intent_id  VARCHAR(255),
    amount                    DECIMAL(10,2),
    status                    VARCHAR(30),
    created_at                TIMESTAMP DEFAULT NOW()
);

-- Feedback
CREATE TABLE feedback (
    id         SERIAL PRIMARY KEY,
    user_id    INTEGER REFERENCES users(id),
    category   VARCHAR(50),
    title      VARCHAR(200),
    message    TEXT,
    rating     INTEGER CHECK (rating BETWEEN 1 AND 5),
    created_at TIMESTAMP DEFAULT NOW()
);
```

### Migrații EF Core

Versionează schema bazei de date:
```bash
# Creare migrație nouă (după modificarea unui Model)
dotnet ef migrations add AddProductCategory --project ProductService

# Aplicare migrații în DB
dotnet ef database update --project ProductService
```

Fiecare migrație generează un fișier C# cu `Up()` (aplicare) și `Down()` (rollback).

---

## 6. Autentificare — Keycloak + JWT

### Keycloak

**Keycloak** este un Identity Provider open-source (SSO):
- gestionează utilizatori, parole, roluri
- emite token-uri JWT prin **OAuth2 / OpenID Connect**
- configurat cu Realm: `shop-platform`, Client: `shop-client`
- configurația realm: `keycloak-config/realm-export.json`

**Roluri definite:**
- `visitor` — poate vedea produse (read-only)
- `user` — poate plasa comenzi, lăsa feedback
- `admin` — acces complet (CRUD produse, toate comenzile)

### Flux complet autentificare

```
1. User trimite username + parolă
   POST http://keycloak:8180/realms/shop-platform/protocol/openid-connect/token
   Body: grant_type=password&client_id=shop-client&username=X&password=Y

2. Keycloak returnează JWT:
   { "access_token": "eyJhbGciOiJSUzI1NiJ9...", "expires_in": 300 }

3. Frontend stochează token-ul (localStorage via utils/storage.ts)

4. Fiecare cerere API include:
   Authorization: Bearer eyJhbGciOiJSUzI1NiJ9...

5. JwtMiddleware (API Gateway) validează:
   → verifică semnătura cu cheia publică Keycloak
   → verifică că token-ul nu e expirat
   → extrage claims: user_id, roles

6. Controller verifică rolul:
   [Authorize(Roles = "admin")]
   [HttpDelete("{id}")]
   public async Task<IActionResult> DeleteProduct(int id) { ... }
```

### Structura JWT (payload decodat)

```json
{
  "sub": "user-uuid-123",
  "preferred_username": "john_doe",
  "email": "john@example.com",
  "realm_access": {
    "roles": ["user", "offline_access"]
  },
  "exp": 1717000000,
  "iat": 1716999700
}
```

---

## 7. Căutare — Elasticsearch

**Elasticsearch** este un motor de căutare full-text distribuit, bazat pe Lucene.

### Index `products`

Configurat cu analyzer custom pentru text în română/engleză:
```json
{
  "settings": {
    "analysis": {
      "analyzer": {
        "product_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "asciifolding", "stop"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name":        { "type": "text", "analyzer": "product_analyzer" },
      "description": { "type": "text", "analyzer": "product_analyzer" },
      "category":    { "type": "keyword" },
      "price":       { "type": "double" },
      "stock":       { "type": "integer" }
    }
  }
}
```

### Cum funcționează căutarea

Când user-ul caută "laptop gaming":
1. Elasticsearch tokenizează: `["laptop", "gaming"]`
2. Caută în `name` și `description` din toate produsele
3. Returnează rezultate ordonate după **scor relevanță** (BM25)
4. Suportă: typo-tolerance, plural/singular, diacritice

**Cerere căutare:**
```
POST /api/products/search
{
  "query": "laptop gaming",
  "category": "Electronics",
  "minPrice": 500,
  "maxPrice": 3000,
  "page": 1,
  "size": 10
}
```

**Sincronizare**: când se adaugă/modifică un produs în PostgreSQL → `ElasticsearchService` actualizează automat și indexul Elasticsearch.

**Re-indexare completă** (admin):
```
POST /admin/products/reindex
```

---

## 8. Plăți — Stripe

**Stripe** este o platformă de procesare plăți online (carduri, SEPA, etc.).

### Flux plată (Payment Intent)

```
1. User apasă "Cumpără"
   → Frontend POST /api/payments/create-intent
   → Body: { orderId: 123 }

2. Backend creează Payment Intent în Stripe:
   var options = new PaymentIntentCreateOptions {
     Amount = 29999,          // în cenți (299.99 RON)
     Currency = "ron",
     Metadata = { {"orderId", "123"} }
   };
   var intent = await _stripeClient.PaymentIntentsAsync.CreateAsync(options);
   // Returnează clientSecret

3. Frontend primește clientSecret
   → Stripe.js afișează formularul de card securizat (hosted by Stripe)
   → Userul introduce datele cardului

4. Stripe procesează plata și trimite webhook:
   POST /api/payments/webhook
   Header: Stripe-Signature: t=...,v1=...
   Body: { type: "payment_intent.succeeded", data: {...} }

5. Backend:
   → Verifică semnătura webhook (securitate)
   → Actualizează status comandă → "Completed"
   → Trimite email confirmare
```

**Carduri de test Stripe:**
- `4242 4242 4242 4242` → plată reușită
- `4000 0000 0000 0002` → card refuzat

---

## 9. Email — MailTrap

**MailTrap** este un serviciu SMTP pentru testare email — email-urile ajung într-un inbox sandbox, nu la utilizatori reali.

**Când se trimit email-uri:**
- La plasarea comenzii → confirmare
- La finalizarea plății → bon fiscal
- La înregistrare cont → bun venit
- La anulare → notificare

**Implementare cu MailKit** (`Services/EmailService.cs`):
```csharp
public async Task SendOrderConfirmationAsync(string toEmail, Order order)
{
    using var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Shop Platform", "noreply@shop.com"));
    message.To.Add(MailboxAddress.Parse(toEmail));
    message.Subject = $"Confirmare comandă #{order.Id}";
    message.Body = new TextPart("html") {
        Text = $"<h1>Comanda ta #{order.Id} a fost plasată!</h1><p>Total: {order.TotalPrice} RON</p>"
    };

    using var client = new SmtpClient();
    await client.ConnectAsync("smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}
```

---

## 10. Docker & Containerizare

### Servicii în `docker-compose.yml`

| Serviciu | Imagine | Port intern | Port extern |
|---|---|---|---|
| `postgres` | postgres:15-alpine | 5432 | 5432 |
| `elasticsearch` | elasticsearch:8.11.0 | 9200 | 9200 |
| `keycloak` | keycloak/keycloak:23.0 | 8080 | 8180 |
| `api-gateway` | build local | 8080 | 8080 |
| `product-service` | build local | 8081 | 8081 |
| `order-service` | build local | 8082 | 8082 |
| `frontend` | build local | 80 | 3000 |

### Dockerfile backend (.NET) — strategie multi-stage

```dockerfile
# Stage 1: Build — folosim SDK complet (~800MB)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore                    # descarcă dependențele NuGet
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime — folosim doar runtime (~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .      # copiem doar build-ul, nu SDK-ul
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductService.dll"]
```

**Avantaj**: imaginea finală e ~200MB în loc de ~1GB.

### Dockerfile frontend (React + Nginx)

```dockerfile
# Stage 1: Build React cu Node.js
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json .
RUN npm ci                            # instalare dependențe exacte
COPY . .
RUN npm run build                     # compilare TypeScript + bundle Vite

# Stage 2: Serve fișiere statice cu Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

**Nginx** gestionează:
- Routing SPA: `try_files $uri /index.html` (permite refresh pe orice rută)
- Cache pentru assets statice (JS, CSS, imagini)
- Compresie gzip

### Health checks

Fiecare serviciu are health check configurat:
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8081/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

---

## 11. CI/CD — GitHub Actions

**Director**: `.github/workflows/`

| Workflow | Fișier | Trigger | Ce face |
|---|---|---|---|
| .NET Build & Test | `dotnet-ci.yml` | Push/PR | `dotnet build` + `dotnet test` |
| Frontend Build | `frontend-ci.yml` | Push/PR | `npm ci` + `npm run build` + lint |
| Docker Build | `docker-build.yml` | Push pe main | Build imagini + push la registry |
| Code Quality | `code-quality.yml` | Push/PR | Analiză statică cod |
| Deploy | `deploy.yml` | Manual | Deploy în producție |
| Release | `release.yml` | Tag `v*` | Creare release GitHub |

**Exemplu `dotnet-ci.yml`:**
```yaml
name: .NET CI
on: [push, pull_request]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore backend/ShopPlatform.sln
      - run: dotnet build --no-restore backend/ShopPlatform.sln
      - run: dotnet test --no-build --verbosity normal
```

---

## 12. Cum rulezi proiectul

### Cerințe
- Docker Desktop 24+
- Git

### Pornire rapidă

```bash
# 1. Clonare repo
git clone https://github.com/USERNAME/tweb.git
cd tweb

# 2. Copiază variabilele de mediu
cp .env.example .env
# Editează .env cu valorile tale (Stripe, MailTrap)

# 3. Pornire toate serviciile
docker-compose up -d

# 4. Verifică că totul merge
docker-compose ps
```

### URL-uri după pornire

| Serviciu | URL |
|---|---|
| Frontend React | http://localhost:3000 |
| API Gateway | http://localhost:8080 |
| Swagger ProductService | http://localhost:8081/swagger |
| Swagger OrderService | http://localhost:8082/swagger |
| Keycloak Admin | http://localhost:8180/admin (admin/admin) |

### Conturi demo

| Username | Parolă | Rol |
|---|---|---|
| admin | admin123 | admin |
| user | user123 | user |

### Comenzi Makefile utile

```bash
make docker-up      # Pornire servicii
make docker-down    # Oprire servicii
make docker-logs    # Vizualizare loguri
make health         # Verificare stare servicii
make migrate        # Rulare migrații DB
make build          # Build proiecte .NET
make test           # Rulare teste
```

---

## 13. Întrebări frecvente pentru laborant

### Ce pattern-uri de design folosești?

**Repository Pattern** — separă accesul la date de logica business:
- Interfețe: `IProductRepository`, `IOrderRepository`, `IUserRepository`, `IPaymentRepository`, `IFeedbackRepository`
- Implementări: câte una pentru fiecare, folosind EF Core
- Injectate în servicii prin Dependency Injection

**DTO Pattern** — nu expunem niciodată entitățile direct la API:
- `CreateProductDto` — ce primim de la client (validat)
- `ProductDto` — ce trimitem la client (fără câmpuri sensibile)
- Mapare automată: `AutoMapper` cu profile configurate

**Middleware Pattern** — procesare cereri în pipeline:
- `JwtMiddleware` — validare token pe fiecare request
- `ExceptionMiddleware` — prinde excepțiile, returnează răspuns JSON structurat

---

### Câte entități și ce relații?

**5 entități**: `Product`, `User`, `Order`, `Payment`, `Feedback`

| Relație | Tip | Cum |
|---|---|---|
| User → Orders | **one-to-many** | `HasOne().WithMany()` FluentAPI |
| Order → Payment | **one-to-one** | `HasOne().WithOne()` FluentAPI |
| User → Feedback | **one-to-many** | `HasOne().WithMany()` FluentAPI |
| User ↔ Roles | **many-to-many** | tabel de joncțiune implicit EF Core |

---

### Cum funcționează JWT?

Un JWT are 3 secțiuni codificate Base64 separate prin `.`:
- **Header**: algoritmul de semnare (`RS256`)
- **Payload**: datele utilizatorului (sub, email, roles, exp)
- **Signature**: semnătura cu cheia privată Keycloak

Nimeni nu poate falsifica un JWT fără cheia privată. API Gateway-ul verifică semnătura cu cheia **publică** Keycloak (nu secret shared).

---

### Ce este Entity Framework Core?

ORM (Object-Relational Mapper) — traduce obiecte C# în interogări SQL:
```csharp
// C# cod:
var cheapProducts = await _context.Products
    .Where(p => p.Price < 100 && p.IsAvailable)
    .OrderBy(p => p.Name)
    .ToListAsync();

// EF Core generează SQL:
// SELECT * FROM products WHERE price < 100 AND is_available = true ORDER BY name
```

---

### Ce este YARP?

**YARP** = Yet Another Reverse Proxy, librărie Microsoft pentru .NET.
- Configurabil 100% din `appsettings.json`, fără cod extra
- Load balancing, health checks, transformări de cereri
- Alternativă .NET nativă la Nginx/Envoy

---

### De ce microservicii?

| Aspect | Microservicii | Monolith |
|---|---|---|
| Scalare | Independent per serviciu | Totul sau nimic |
| Fault isolation | Un serviciu pică, restul merg | Totul pică |
| Deployment | Servicii independente | Un singur deployment |
| Complexitate | Mai mare | Mai simplă |

---

### Cum funcționează paginarea?

**Backend** — `Skip` + `Take`:
```csharp
var items = await _context.Products
    .Skip((page - 1) * pageSize)   // sari primele N înregistrări
    .Take(pageSize)                 // ia maxim pageSize înregistrări
    .ToListAsync();
```

**Frontend** — componentă MUI Pagination legată de state.

---

### Cum funcționează Stripe webhooks?

Stripe trimite un HTTP POST la `/api/payments/webhook` când o plată e procesată. Backend-ul:
1. Verifică header-ul `Stripe-Signature` (securitate — nimeni altcineva nu poate trimite)
2. Identifică tipul evenimentului (`payment_intent.succeeded`)
3. Actualizează statusul comenzii în DB
4. Trimite email de confirmare

---

*Proiect pentru laboratorul de Tehnologii Web — Platformă E-Commerce cu ASP.NET Core 8 + React*

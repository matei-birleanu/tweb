# Shop Platform Backend - C# ASP.NET Core

Complete microservices-based backend for an e-commerce platform.

## Architecture

### Services Overview

1. **ApiGateway** (Port 5000/7000)
   - YARP Reverse Proxy
   - JWT Authentication
   - Routes requests to microservices

2. **ProductService** (Port 5001/7001)
   - Product management (CRUD)
   - Elasticsearch integration for search
   - PostgreSQL database

3. **OrderService** (Port 5002/7002)
   - User management & authentication
   - Order processing
   - Payment integration (Stripe)
   - Feedback system
   - Email notifications (MailKit)
   - PostgreSQL database

## Project Structure

```
backend/
├── ShopPlatform.sln
├── ApiGateway/
│   ├── Controllers/
│   ├── Middleware/
│   └── Properties/
├── ProductService/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Mappings/
│   ├── Middleware/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   └── Validators/
└── OrderService/
    ├── Controllers/
    ├── DTOs/
    ├── Data/
    ├── Mappings/
    ├── Middleware/
    ├── Models/
    ├── Repositories/
    ├── Services/
    └── Validators/
```

## Technologies

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **PostgreSQL** (Npgsql)
- **Elasticsearch** (NEST 7.17.5)
- **AutoMapper** (12.0.1)
- **FluentValidation** (11.3.0)
- **JWT Bearer Authentication**
- **Stripe.net** (43.0.0)
- **MailKit** (4.3.0)
- **YARP Reverse Proxy** (2.1.0)
- **Swagger/OpenAPI**
- **BCrypt.Net** (Password hashing)

## Database Setup

### PostgreSQL Databases

1. **productdb** - ProductService database
2. **orderdb** - OrderService database

Connection strings in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=productdb;Username=postgres;Password=postgres"
}
```

## Configuration

### JWT Settings (All Services)
```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456",
  "Issuer": "ShopPlatform",
  "Audience": "ShopPlatformUsers",
  "ExpirationMinutes": 60
}
```

### Stripe Configuration (OrderService)
```json
"Stripe": {
  "SecretKey": "sk_test_your_stripe_secret_key",
  "PublishableKey": "pk_test_your_stripe_publishable_key"
}
```

### Elasticsearch Configuration (ProductService)
```json
"Elasticsearch": {
  "Url": "http://localhost:9200",
  "IndexName": "products"
}
```

### Email Configuration (OrderService)
```json
"Email": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",
  "FromEmail": "noreply@shopplatform.com",
  "FromName": "Shop Platform"
}
```

## Building and Running

### Build All Projects
```bash
dotnet build ShopPlatform.sln
```

### Run Individual Services

#### API Gateway
```bash
cd ApiGateway
dotnet run
```
Access at: http://localhost:5000

#### Product Service
```bash
cd ProductService
dotnet run
```
Access at: http://localhost:5001

#### Order Service
```bash
cd OrderService
dotnet run
```
Access at: http://localhost:5002

## API Endpoints

### ProductService (/api/products)
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `POST /api/products/search` - Search products
- `POST /api/products` - Create product [Auth]
- `PUT /api/products/{id}` - Update product [Auth]
- `DELETE /api/products/{id}` - Delete product [Auth]
- `PATCH /api/products/{id}/stock` - Update stock [Auth]

### OrderService

#### Users (/api/users)
- `POST /api/users/register` - Register new user
- `POST /api/users/login` - Login and get JWT token
- `GET /api/users` - Get all users [Auth]
- `GET /api/users/{id}` - Get user by ID [Auth]
- `PUT /api/users/{id}` - Update user [Auth]
- `DELETE /api/users/{id}` - Delete user [Auth]

#### Orders (/api/orders)
- `GET /api/orders` - Get all orders [Auth]
- `GET /api/orders/{id}` - Get order by ID [Auth]
- `GET /api/orders/user/{userId}` - Get orders by user [Auth]
- `GET /api/orders/status/{status}` - Get orders by status [Auth]
- `POST /api/orders` - Create order [Auth]
- `PATCH /api/orders/{id}/status` - Update order status [Auth]
- `DELETE /api/orders/{id}` - Delete order [Auth]

#### Payments (/api/payments)
- `GET /api/payments` - Get all payments [Auth]
- `GET /api/payments/{id}` - Get payment by ID [Auth]
- `GET /api/payments/order/{orderId}` - Get payment by order [Auth]
- `POST /api/payments` - Create payment [Auth]
- `POST /api/payments/stripe` - Process Stripe payment [Auth]
- `PATCH /api/payments/{id}/status` - Update payment status [Auth]
- `POST /api/payments/{id}/refund` - Refund payment [Auth]

#### Feedback (/api/feedback)
- `GET /api/feedback` - Get all feedback [Auth]
- `GET /api/feedback/{id}` - Get feedback by ID [Auth]
- `GET /api/feedback/user/{userId}` - Get user feedback [Auth]
- `GET /api/feedback/category/{category}` - Get by category [Auth]
- `GET /api/feedback/unresolved` - Get unresolved [Auth]
- `POST /api/feedback` - Create feedback [Auth]
- `POST /api/feedback/{id}/respond` - Respond to feedback [Auth]
- `DELETE /api/feedback/{id}` - Delete feedback [Auth]

## Models

### ProductService
- **Product**: id, name, description, price, stock, category, imageUrl, isAvailable

### OrderService
- **User**: id, username, email, passwordHash, fullName, address, phone, isActive
- **Order**: id, userId, productId, quantity, totalPrice, orderType, status, shippingAddress, rentalDates
- **Payment**: id, orderId, amount, paymentMethod, status, transactionId, stripePaymentIntentId
- **Feedback**: id, userId, category, subject, message, rating, isResolved, adminResponse

## Enums

### OrderType
- Purchase = 1
- Rental = 2

### OrderStatus
- Pending = 1
- Processing = 2
- Shipped = 3
- Delivered = 4
- Cancelled = 5
- Returned = 6

### PaymentStatus
- Pending = 1
- Completed = 2
- Failed = 3
- Refunded = 4
- Cancelled = 5

### FeedbackCategory
- General = 1
- ProductQuality = 2
- DeliveryService = 3
- CustomerSupport = 4
- WebsiteUsability = 5
- PricingAndPayment = 6
- Other = 7

## Features

### Security
- JWT Bearer token authentication
- BCrypt password hashing
- Authorization filters on protected endpoints
- CORS configuration

### Database
- Entity Framework Core with Code-First approach
- PostgreSQL with optimized indexes
- Repository pattern implementation
- Automatic timestamps (CreatedAt, UpdatedAt)

### Validation
- FluentValidation for DTOs
- Data annotations on models
- Custom validation rules
- Model state validation in controllers

### Error Handling
- Global exception middleware
- Structured error responses
- Logging with ILogger
- HTTP status code mapping

### Integrations
- Elasticsearch for product search with fuzzy matching
- Stripe for payment processing
- MailKit for email notifications
- YARP for API Gateway routing

## Development Notes

All files were created manually without using `dotnet` CLI commands. The solution is ready to be built with:

```bash
dotnet restore
dotnet build
dotnet run --project ApiGateway
dotnet run --project ProductService
dotnet run --project OrderService
```

## Total Files Created: 75+

- 3 .csproj files
- 1 .sln file
- 70+ .cs files
- Configuration files (appsettings.json, launchSettings.json)

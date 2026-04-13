# Quick Start Guide - C# ASP.NET Core E-Commerce Platform

Get the platform up and running in minutes with Docker or .NET CLI.

## Prerequisites

- **Docker Desktop** installed and running (for Docker method)
- **.NET 8.0 SDK** (for manual method)
- **Node.js 18+** (for frontend)
- At least 8GB RAM allocated to Docker
- At least 20GB free disk space

## Method 1: Docker Compose (Recommended)

### Installation Steps

#### 1. Clone and Setup

```bash
# Navigate to project directory
cd csharptweb

# Create environment file
cp .env.example .env

# (Optional) Edit environment variables
code .env  # or nano .env
```

#### 2. Start All Services

```bash
# Using Docker Compose
docker-compose up -d
```

#### 3. Wait for Services to Start

```bash
# Check service status
docker-compose ps

# Watch logs
docker-compose logs -f
```

Services are ready when all show "Up (healthy)" status (typically 2-3 minutes).

#### 4. Run Database Migrations

```bash
# Apply EF Core migrations
docker-compose exec product-service dotnet ef database update
docker-compose exec order-service dotnet ef database update
```

#### 5. Access the Application

| Service | URL | Credentials |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | - |
| **API Gateway** | http://localhost:8080 | - |
| **Product Service Swagger** | http://localhost:8081/swagger | - |
| **Order Service Swagger** | http://localhost:8082/swagger | - |
| **Keycloak Admin** | http://localhost:8180/admin | admin / admin |
| **Elasticsearch** | http://localhost:9200 | - |

---

## Method 2: Manual .NET CLI (Development)

### 1. Start Infrastructure Services

```bash
# Start PostgreSQL
docker run -d \
  --name csharp-postgres \
  -e POSTGRES_USER=csharp_user \
  -e POSTGRES_PASSWORD=csharp123 \
  -e POSTGRES_DB=csharptweb \
  -p 5432:5432 \
  postgres:15

# Start Elasticsearch
docker run -d \
  --name csharp-elasticsearch \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -p 9200:9200 \
  elasticsearch:8.11.0

# Start Keycloak
docker run -d \
  --name csharp-keycloak \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  -p 8180:8080 \
  quay.io/keycloak/keycloak:23.0 \
  start-dev
```

### 2. Setup Backend Services

```bash
# Product Service
cd backend/ProductService
dotnet restore
dotnet ef database update
dotnet build
dotnet run

# In a new terminal - Order Service
cd backend/OrderService
dotnet restore
dotnet ef database update
dotnet build
dotnet run

# In a new terminal - API Gateway
cd backend/ApiGateway
dotnet restore
dotnet build
dotnet run
```

### 3. Setup Frontend

```bash
cd frontend
npm install
npm run dev
```

---

## Test User Accounts

Pre-configured test users (after Keycloak setup):

| Username | Password | Roles |
|----------|----------|-------|
| **admin** | admin123 | admin, user |
| **user** | user123 | user |
| **visitor** | visitor123 | visitor |

---

## Common Commands

### Docker Compose Commands

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Restart services
docker-compose restart

# View logs (all services)
docker-compose logs -f

# View logs (specific service)
docker-compose logs -f product-service

# Rebuild services
docker-compose build --no-cache
docker-compose up -d
```

### .NET CLI Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run service
dotnet run --project backend/ProductService

# Run tests
dotnet test

# Create migration
dotnet ef migrations add MigrationName --project backend/ProductService

# Apply migration
dotnet ef database update --project backend/ProductService

# Check .NET version
dotnet --version

# List installed SDKs
dotnet --list-sdks
```

### Frontend Commands

```bash
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

---

## Verify Everything Works

### Check Health Endpoints

```bash
# API Gateway
curl http://localhost:8080/health

# Product Service
curl http://localhost:8081/health

# Order Service
curl http://localhost:8082/health
```

### Test API Endpoints

```bash
# Get all products
curl http://localhost:8080/api/products

# Get product by ID
curl http://localhost:8080/api/products/{id}

# Create product (requires authentication)
curl -X POST http://localhost:8080/api/products \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","price":99.99,"stock":100}'
```

---

## Troubleshooting

### Services Won't Start

1. **Check Docker is running:**
   ```bash
   docker info
   ```

2. **Check available resources:**
   - Docker Desktop → Settings → Resources
   - Ensure at least 8GB RAM

3. **View logs:**
   ```bash
   docker-compose logs product-service
   ```

4. **Complete reset:**
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

### Port Already in Use

```bash
# Windows - Find process using port
netstat -ano | findstr :8080

# macOS/Linux - Find process using port
lsof -i :8080

# Kill process
kill -9 <PID>
```

### Database Connection Issues

```bash
# Check PostgreSQL is ready
docker-compose exec postgres pg_isready -U csharp_user

# Verify database exists
docker-compose exec postgres psql -U csharp_user -l

# Restart services
docker-compose restart product-service order-service
```

### Entity Framework Migration Issues

```bash
# Reset database (CAUTION: deletes all data)
dotnet ef database drop --force --project backend/ProductService
dotnet ef database update --project backend/ProductService

# List migrations
dotnet ef migrations list --project backend/ProductService

# Remove last migration
dotnet ef migrations remove --project backend/ProductService
```

### Frontend Not Loading

1. **Check API Gateway is running:**
   ```bash
   curl http://localhost:8080/health
   ```

2. **Check frontend logs:**
   ```bash
   docker-compose logs frontend
   ```

3. **Clear npm cache and rebuild:**
   ```bash
   cd frontend
   rm -rf node_modules package-lock.json
   npm install
   npm run dev
   ```

### .NET Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore --force
```

---

## Development Workflow

### 1. Run Backend in Watch Mode

```bash
# Run with hot reload
dotnet watch run --project backend/ProductService

# Or use Visual Studio/Rider debugger
```

### 2. Run Frontend in Dev Mode

```bash
cd frontend
npm run dev
```

### 3. Make Changes

- Backend: Edit `.cs` files - changes auto-reload with `dotnet watch`
- Frontend: Edit `.tsx` files - changes auto-reload with Vite HMR

### 4. Test Changes

```bash
# Backend tests
dotnet test

# Frontend tests
npm test
```

---

## What's Next?

- **Browse Products**: Visit http://localhost:3000
- **Explore API**: Check http://localhost:8081/swagger
- **Read Documentation**: See `/docs` folder
  - [ARCHITECTURE.md](docs/ARCHITECTURE.md) - System design
  - [API.md](docs/API.md) - API reference
  - [DEVELOPMENT.md](docs/DEVELOPMENT.md) - Development guide
  - [CSHARP-GUIDE.md](docs/CSHARP-GUIDE.md) - C# best practices
  - [DEPLOYMENT.md](docs/DEPLOYMENT.md) - Deployment guide

---

## Quick Reference

### Service Ports

| Service | Port | Purpose |
|---------|------|---------|
| Frontend | 3000 | React application |
| API Gateway | 8080 | Yarp reverse proxy |
| Product Service | 8081 | Product management |
| Order Service | 8082 | Order processing |
| Payment Service | 8083 | Payment integration |
| PostgreSQL | 5432 | Database |
| Elasticsearch | 9200 | Search engine |
| Keycloak | 8180 | Authentication |

### Environment Files

- `.env` - Environment variables
- `appsettings.json` - Service configuration
- `appsettings.Development.json` - Dev overrides
- `appsettings.Production.json` - Prod overrides

### Useful VS Code Extensions

- C# Dev Kit
- .NET Extension Pack
- REST Client
- Docker
- ESLint
- Prettier

### Useful Visual Studio Features

- Debug → Attach to Process
- Tools → NuGet Package Manager
- View → SQL Server Object Explorer
- Test → Test Explorer

---

## Need Help?

1. Check logs: `docker-compose logs [service-name]`
2. Review documentation in `/docs`
3. Check [CLAUDE.md](CLAUDE.md) for AI assistant tips
4. Search GitHub issues
5. Contact the development team

---

**That's it!** Your C# ASP.NET Core E-Commerce Platform should now be running. Visit http://localhost:3000 to get started.

For more detailed information, check out the complete documentation in the `/docs` folder.

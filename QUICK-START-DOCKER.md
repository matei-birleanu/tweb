# Quick Start - Docker Setup

This guide will get you up and running with the Shop Platform using Docker in 5 minutes.

## Prerequisites

- Docker Desktop installed and running
- 8GB+ RAM allocated to Docker
- Ports available: 3000, 8080, 8081, 8082, 8180, 5432, 9200

## Step 1: Clone and Configure

```bash
# Navigate to project
cd /Users/matei/Desktop/tweb/csharptweb

# Copy environment template
cp .env.example .env

# Optional: Edit .env if you want to customize settings
# nano .env
```

## Step 2: Make Scripts Executable

```bash
chmod +x scripts/*.sh
chmod +x docker/healthcheck.sh
```

## Step 3: Start Everything

```bash
# Option A: Using Make (recommended)
make docker-up

# Option B: Using docker-compose directly
docker-compose up -d
```

This will start all services:
- PostgreSQL (database)
- Elasticsearch (search)
- Keycloak (authentication)
- API Gateway
- Product Service
- Order Service
- Frontend

## Step 4: Wait for Services

The first startup takes 2-3 minutes as Docker downloads images and initializes databases.

```bash
# Check status
docker-compose ps

# Watch logs
docker-compose logs -f

# Or use the health check script
./scripts/health-check.sh
```

## Step 5: Access the Application

Once all services show as "healthy":

- **Frontend**: http://localhost:3000
- **API Gateway**: http://localhost:8080
- **Product Service**: http://localhost:8081/swagger
- **Order Service**: http://localhost:8082/swagger
- **Keycloak Admin**: http://localhost:8180 (admin/admin)
- **Elasticsearch**: http://localhost:9200

## Default Login Credentials

### Admin User
- Email: `admin@shop.com`
- Password: `admin123`

### Regular User
- Email: `user@shop.com`
- Password: `user123`

## Common Commands

### Viewing Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api-gateway
docker-compose logs -f product-service
```

### Restarting Services
```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart api-gateway
```

### Stopping Services
```bash
# Stop all (keeps data)
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

### Rebuilding After Code Changes
```bash
# Rebuild all
make docker-build

# Rebuild specific service
docker-compose build api-gateway

# Rebuild and restart
docker-compose up -d --build
```

## Development Workflow

### For Backend Changes
```bash
# Stop backend services
docker-compose stop api-gateway product-service order-service

# Run locally with hot reload
cd backend/ApiGateway
dotnet watch run

# In another terminal
cd backend/ProductService
dotnet watch run

# In another terminal
cd backend/OrderService
dotnet watch run
```

### For Frontend Changes
```bash
# Stop frontend container
docker-compose stop frontend

# Run locally
cd frontend
npm install
npm run dev
```

## Troubleshooting

### Services Won't Start

```bash
# Check Docker is running
docker info

# Check logs for errors
docker-compose logs

# Clean restart
docker-compose down -v
docker-compose up -d
```

### Port Already in Use

```bash
# Check what's using the port (macOS/Linux)
lsof -i :8080

# Kill the process
kill -9 <PID>

# Or change the port in docker-compose.yml
```

### Database Connection Issues

```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check PostgreSQL logs
docker-compose logs postgres

# Test connection
docker-compose exec postgres psql -U shop_user -d shop_db
```

### Services Show as Unhealthy

```bash
# Wait a bit longer (first startup is slow)
sleep 30
docker-compose ps

# Check specific service logs
docker-compose logs api-gateway

# Restart the service
docker-compose restart api-gateway
```

### Clean Slate

If everything is broken, start fresh:

```bash
# Stop and remove everything
docker-compose down -v

# Clean Docker system
docker system prune -f

# Start fresh
docker-compose up -d
```

## Useful Make Commands

```bash
make help              # Show all available commands
make docker-up         # Start all services
make docker-down       # Stop all services
make docker-logs       # View logs
make docker-build      # Rebuild images
make docker-clean      # Clean everything
make health            # Check service health
make status            # Show service status
```

## Testing the API

### Using curl
```bash
# Get products
curl http://localhost:8080/api/products

# Get product by ID (replace {id} with actual ID)
curl http://localhost:8080/api/products/{id}

# Health check
curl http://localhost:8080/health
```

### Using Swagger
1. Open http://localhost:8081/swagger (Product Service)
2. Click "Try it out" on any endpoint
3. Execute the request

## Next Steps

1. **Explore the API**
   - Try the Swagger UI for each service
   - Test authentication with Keycloak
   - Create products and orders

2. **Check the Documentation**
   - [DOCKER.md](./DOCKER.md) - Detailed Docker guide
   - [README.md](./README.md) - Project overview
   - [docs/API.md](./docs/API.md) - API documentation

3. **Start Development**
   - Make changes to the code
   - Run tests: `make test`
   - Follow the development guide in README.md

## Production Deployment

When ready for production:

```bash
# Use production configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Important: Update .env with production values!
# - Strong passwords
# - Real Stripe keys
# - Production URLs
# - SSL certificates
```

## Getting Help

- Check logs: `docker-compose logs -f [service-name]`
- View status: `docker-compose ps`
- Health check: `./scripts/health-check.sh`
- See full documentation: [DOCKER.md](./DOCKER.md)

---

**Ready to go!** Your Shop Platform is now running on Docker. 🚀

Visit http://localhost:3000 to see the application.

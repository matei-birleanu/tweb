# Docker Setup Guide

This document provides information about the Docker setup for the Shop Platform project.

## Architecture

The project uses a microservices architecture with the following components:

- **API Gateway** (Port 8080) - Entry point for all API requests
- **Product Service** (Port 8081) - Manages products and categories
- **Order Service** (Port 8082) - Handles orders and shopping carts
- **Frontend** (Port 3000) - React application
- **PostgreSQL** (Port 5432) - Database
- **Elasticsearch** (Port 9200) - Search engine
- **Keycloak** (Port 8180) - Authentication and authorization

## Quick Start

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- Make (optional, for convenience commands)

### Initial Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd csharptweb
   ```

2. **Create environment file**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start all services**
   ```bash
   docker-compose up -d
   # Or use: make docker-up
   ```

4. **Initialize the database**
   The database will be automatically initialized on first startup using the `database/init.sql` script.

5. **Access the services**
   - Frontend: http://localhost:3000
   - API Gateway: http://localhost:8080
   - Keycloak Admin: http://localhost:8180 (admin/admin)

## Docker Compose Files

### docker-compose.yml
The main configuration file for all services. Use this for basic setup.

### docker-compose.dev.yml
Development override with hot-reload support.

```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### docker-compose.prod.yml
Production configuration with security hardening and proper logging.

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Common Commands

### Using Make (Recommended)

```bash
# Start all services
make docker-up

# Stop all services
make docker-down

# View logs
make docker-logs

# Rebuild images
make docker-build

# Clean everything
make docker-clean

# Check service health
make health
```

### Using Docker Compose

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f [service-name]

# Rebuild a specific service
docker-compose build [service-name]

# Restart a service
docker-compose restart [service-name]

# Check service status
docker-compose ps
```

## Service Management

### Individual Services

```bash
# Start only database services
docker-compose up -d postgres elasticsearch keycloak

# Start only backend services
docker-compose up -d api-gateway product-service order-service

# Start only frontend
docker-compose up -d frontend
```

### Scaling Services

```bash
# Scale product service to 3 instances
docker-compose up -d --scale product-service=3
```

## Environment Variables

Key environment variables (see `.env.example` for complete list):

```env
# Database
POSTGRES_DB=shop_db
POSTGRES_USER=shop_user
POSTGRES_PASSWORD=shop_pass123

# Keycloak
Keycloak__Authority=http://localhost:8180/realms/shop-platform

# Stripe (optional)
Stripe__SecretKey=sk_test_...
Stripe__WebhookSecret=whsec_...

# Email (optional)
Email__Host=smtp.mailtrap.io
Email__Port=2525
```

## Volumes

Persistent data is stored in Docker volumes:

- `postgres_data` - Database data
- `elasticsearch_data` - Search indices

To backup volumes:
```bash
docker run --rm -v postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres_backup.tar.gz /data
```

## Networking

All services communicate through the `shop-network` bridge network.

Internal hostnames:
- `postgres` - Database
- `elasticsearch` - Search engine
- `keycloak` - Auth server
- `api-gateway` - API Gateway
- `product-service` - Product Service
- `order-service` - Order Service

## Health Checks

Each service includes health checks:

```bash
# Check all services
./scripts/health-check.sh

# Check individual service
curl http://localhost:8080/health
```

## Troubleshooting

### Services won't start

```bash
# Check logs
docker-compose logs

# Check specific service
docker-compose logs api-gateway

# Verify Docker is running
docker info
```

### Database connection issues

```bash
# Check PostgreSQL is ready
docker-compose exec postgres pg_isready -U shop_user

# Check database logs
docker-compose logs postgres
```

### Port conflicts

If ports are already in use, modify the port mappings in `docker-compose.yml`:

```yaml
services:
  api-gateway:
    ports:
      - "8081:8080"  # Changed from 8080:8080
```

### Permission issues

```bash
# Reset permissions
sudo chown -R $USER:$USER .

# Clean and restart
make docker-clean
make docker-up
```

## Development Workflow

### Hot Reload Setup

1. **Start in development mode**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
   ```

2. **Edit code locally** - Changes will be automatically reflected in running containers

### Debugging

1. **Attach to a service**
   ```bash
   docker-compose exec api-gateway sh
   ```

2. **View real-time logs**
   ```bash
   docker-compose logs -f api-gateway
   ```

## Production Deployment

### Pre-deployment Checklist

- [ ] Update `.env` with production values
- [ ] Generate SSL certificates
- [ ] Update Keycloak realm configuration
- [ ] Set strong passwords for all services
- [ ] Configure backup strategy
- [ ] Set up monitoring and alerting

### Deployment Steps

1. **Build production images**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
   ```

2. **Start production stack**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

3. **Verify deployment**
   ```bash
   ./scripts/health-check.sh
   ```

## Monitoring

### Container Stats

```bash
# View resource usage
docker stats

# View specific service
docker stats shop-api-gateway
```

### Logs

```bash
# All services
docker-compose logs -f

# Specific service with tail
docker-compose logs -f --tail=100 api-gateway
```

## Backup and Restore

### Database Backup

```bash
# Backup
docker-compose exec postgres pg_dump -U shop_user shop_db > backup.sql

# Restore
docker-compose exec -T postgres psql -U shop_user shop_db < backup.sql
```

### Volume Backup

```bash
# Backup all volumes
docker run --rm -v postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres.tar.gz /data
docker run --rm -v elasticsearch_data:/data -v $(pwd):/backup alpine tar czf /backup/elasticsearch.tar.gz /data
```

## Security Best Practices

1. **Never commit `.env` files**
2. **Use strong passwords** in production
3. **Enable SSL/TLS** for all external connections
4. **Regularly update** base images
5. **Scan images** for vulnerabilities:
   ```bash
   docker scan shop-api-gateway:latest
   ```
6. **Limit container resources**:
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '1'
         memory: 1G
   ```

## CI/CD Integration

The project includes GitHub Actions workflows:

- `.github/workflows/docker-build.yml` - Builds and publishes Docker images
- `.github/workflows/deploy.yml` - Deploys to production
- `.github/workflows/code-quality.yml` - Code quality checks

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Elasticsearch Docker](https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html)

## Support

For issues and questions:
- Check the logs: `docker-compose logs`
- Review this documentation
- Create an issue in the repository

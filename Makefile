.PHONY: help build clean restore test run-gateway run-product run-order run-frontend docker-up docker-down docker-build docker-logs docker-clean format lint migrate seed

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[0;33m
NC := \033[0m # No Color

# Default target
help:
	@echo "$(GREEN)Shop Platform - Available Commands$(NC)"
	@echo ""
	@echo "$(YELLOW)Backend Commands:$(NC)"
	@echo "  make build           - Build all .NET projects"
	@echo "  make clean           - Clean build artifacts"
	@echo "  make restore         - Restore NuGet packages"
	@echo "  make test            - Run all tests"
	@echo "  make format          - Format code with dotnet-format"
	@echo "  make lint            - Lint code"
	@echo ""
	@echo "$(YELLOW)Service Commands:$(NC)"
	@echo "  make run-gateway     - Run API Gateway"
	@echo "  make run-product     - Run Product Service"
	@echo "  make run-order       - Run Order Service"
	@echo "  make run-frontend    - Run Frontend"
	@echo "  make run-all         - Run all services"
	@echo ""
	@echo "$(YELLOW)Docker Commands:$(NC)"
	@echo "  make docker-up       - Start all Docker services"
	@echo "  make docker-down     - Stop all Docker services"
	@echo "  make docker-build    - Build Docker images"
	@echo "  make docker-logs     - Show Docker logs"
	@echo "  make docker-clean    - Clean Docker volumes and images"
	@echo "  make docker-restart  - Restart Docker services"
	@echo ""
	@echo "$(YELLOW)Database Commands:$(NC)"
	@echo "  make migrate         - Run database migrations"
	@echo "  make seed            - Seed database with sample data"
	@echo "  make db-reset        - Reset database (drop and recreate)"

# Backend commands
build:
	@echo "$(GREEN)Building .NET projects...$(NC)"
	cd backend && dotnet build ShopPlatform.sln --configuration Release

clean:
	@echo "$(GREEN)Cleaning build artifacts...$(NC)"
	cd backend && dotnet clean ShopPlatform.sln
	rm -rf backend/*/bin backend/*/obj

restore:
	@echo "$(GREEN)Restoring NuGet packages...$(NC)"
	cd backend && dotnet restore ShopPlatform.sln

test:
	@echo "$(GREEN)Running tests...$(NC)"
	cd backend && dotnet test ShopPlatform.sln --configuration Release --verbosity normal

format:
	@echo "$(GREEN)Formatting code...$(NC)"
	cd backend && dotnet format ShopPlatform.sln

lint:
	@echo "$(GREEN)Linting code...$(NC)"
	cd backend && dotnet format ShopPlatform.sln --verify-no-changes

# Service commands
run-gateway:
	@echo "$(GREEN)Starting API Gateway...$(NC)"
	cd backend/ApiGateway && dotnet run

run-product:
	@echo "$(GREEN)Starting Product Service...$(NC)"
	cd backend/ProductService && dotnet run

run-order:
	@echo "$(GREEN)Starting Order Service...$(NC)"
	cd backend/OrderService && dotnet run

run-frontend:
	@echo "$(GREEN)Starting Frontend...$(NC)"
	cd frontend && npm start

run-all:
	@echo "$(GREEN)Starting all services...$(NC)"
	@make -j3 run-gateway run-product run-order

# Docker commands
docker-up:
	@echo "$(GREEN)Starting Docker services...$(NC)"
	docker-compose up -d
	@echo "$(GREEN)Services started! Access:$(NC)"
	@echo "  Frontend:        http://localhost:3000"
	@echo "  API Gateway:     http://localhost:8080"
	@echo "  Product Service: http://localhost:8081"
	@echo "  Order Service:   http://localhost:8082"
	@echo "  Keycloak:        http://localhost:8180"
	@echo "  Elasticsearch:   http://localhost:9200"

docker-down:
	@echo "$(YELLOW)Stopping Docker services...$(NC)"
	docker-compose down

docker-build:
	@echo "$(GREEN)Building Docker images...$(NC)"
	docker-compose build --no-cache

docker-logs:
	@echo "$(GREEN)Showing Docker logs...$(NC)"
	docker-compose logs -f

docker-clean:
	@echo "$(RED)Cleaning Docker volumes and images...$(NC)"
	docker-compose down -v
	docker system prune -f

docker-restart:
	@echo "$(YELLOW)Restarting Docker services...$(NC)"
	docker-compose restart

# Database commands
migrate:
	@echo "$(GREEN)Running database migrations...$(NC)"
	cd backend/ApiGateway && dotnet ef database update

seed:
	@echo "$(GREEN)Seeding database...$(NC)"
	docker-compose exec postgres psql -U shop_user -d shop_db -f /docker-entrypoint-initdb.d/init.sql

db-reset:
	@echo "$(RED)Resetting database...$(NC)"
	docker-compose down postgres
	docker volume rm csharptweb_postgres_data || true
	docker-compose up -d postgres
	@sleep 5
	@make seed

# Development workflow
dev: docker-up
	@echo "$(GREEN)Development environment ready!$(NC)"

dev-down: docker-down
	@echo "$(YELLOW)Development environment stopped.$(NC)"

# Quick rebuild and restart
rebuild:
	@echo "$(GREEN)Rebuilding and restarting...$(NC)"
	@make docker-down
	@make docker-build
	@make docker-up

# Status check
status:
	@echo "$(GREEN)Checking service status...$(NC)"
	@docker-compose ps

# Health check
health:
	@echo "$(GREEN)Checking service health...$(NC)"
	@curl -f http://localhost:8080/health || echo "$(RED)API Gateway is down$(NC)"
	@curl -f http://localhost:8081/health || echo "$(RED)Product Service is down$(NC)"
	@curl -f http://localhost:8082/health || echo "$(RED)Order Service is down$(NC)"
	@curl -f http://localhost:9200/_cluster/health || echo "$(RED)Elasticsearch is down$(NC)"

# Install dependencies
install:
	@echo "$(GREEN)Installing dependencies...$(NC)"
	cd backend && dotnet restore ShopPlatform.sln
	cd frontend && npm install

# Watch for changes and rebuild
watch:
	@echo "$(GREEN)Watching for changes...$(NC)"
	cd backend/ApiGateway && dotnet watch run

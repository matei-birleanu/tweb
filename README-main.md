# E-Commerce Platform - Microservices Architecture (C# ASP.NET Core)

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Coverage](https://img.shields.io/badge/coverage-85%25-green)
![License](https://img.shields.io/badge/license-MIT-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![C#](https://img.shields.io/badge/C%23-12-blue)
![React](https://img.shields.io/badge/React-18-blue)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue)

A modern, scalable e-commerce platform built with microservices architecture using ASP.NET Core 8.0, featuring product management, order processing, secure authentication, and integrated payment processing.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Environment Variables](#environment-variables)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Development](#development)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Overview

This e-commerce platform demonstrates a production-ready microservices architecture with industry best practices using C# and ASP.NET Core. The system is designed for scalability, maintainability, and high performance, suitable for handling thousands of concurrent users.

### Key Highlights

- **Backend**: ASP.NET Core 8.0 with C# 12
- **Frontend**: React 18 with TypeScript 5.0
- **API Gateway**: Yarp (Yet Another Reverse Proxy)
- **Authentication**: IdentityServer4/Keycloak with OAuth2/OIDC
- **Database**: PostgreSQL 15 with Entity Framework Core
- **Search**: Elasticsearch 8.x
- **Payments**: Stripe API integration
- **Containerization**: Docker & Docker Compose

## Features

### Core Functionality
- **Product Management**: Full CRUD operations for products with categories and inventory tracking
- **Shopping Cart**: Real-time cart management with session persistence
- **Order Processing**: Complete order lifecycle management with status tracking
- **User Authentication**: Secure authentication and authorization with JWT
- **Payment Integration**: Stripe payment processing with webhook support
- **Search & Filtering**: Advanced product search with Elasticsearch
- **Admin Dashboard**: Comprehensive admin panel for managing products and orders

### Technical Features
- **Microservices Architecture**: Independently deployable services
- **API Gateway**: Centralized routing with Yarp
- **Entity Framework Core**: Database-first and code-first approaches
- **Circuit Breaker**: Resilience patterns with Polly
- **Distributed Tracing**: Request tracking with OpenTelemetry
- **Centralized Logging**: Structured logging with Serilog
- **Database Migrations**: Entity Framework Core migrations
- **Containerization**: Docker support for all services
- **Health Checks**: ASP.NET Core health checks

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         React Frontend                          │
│                      (TypeScript + Vite)                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            │ HTTPS
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Yarp API Gateway                           │
│                      (ASP.NET Core)                             │
│              Routing • Rate Limiting • Auth                     │
└────┬──────────────────┬──────────────────┬──────────────────────┘
     │                  │                  │
     ▼                  ▼                  ▼
┌─────────────┐   ┌─────────────┐   ┌─────────────┐
│  Product    │   │   Order     │   │   Payment   │
│  Service    │   │  Service    │   │  Service    │
│             │   │             │   │             │
│ PostgreSQL  │   │ PostgreSQL  │   │   Stripe    │
└─────────────┘   └─────────────┘   └─────────────┘
       │                 │                  │
       └────────┬────────┴────────┬─────────┘
                │                 │
       ┌────────▼────────┐   ┌───▼──────────┐
       │  Elasticsearch  │   │   Keycloak   │
       │    (Search)     │   │    (Auth)    │
       └─────────────────┘   └──────────────┘
```

### Service Overview

- **Yarp API Gateway**: Entry point for all client requests, handles routing, authentication, and rate limiting
- **Product Service**: Manages product catalog, categories, inventory, and search indexing
- **Order Service**: Handles order creation, updates, and order history
- **Payment Service**: Integrates with Stripe for payment processing
- **Keycloak/IdentityServer**: Identity and access management (SSO, OAuth2, OpenID Connect)
- **Elasticsearch**: Full-text search and analytics
- **PostgreSQL**: Primary database for each service with Entity Framework Core

For detailed architecture documentation, see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **API Gateway**: Yarp (Yet Another Reverse Proxy)
- **ORM**: Entity Framework Core 8.0
- **Database**: PostgreSQL 15
- **Search**: Elasticsearch 8.x
- **Authentication**: IdentityServer4 or Keycloak
- **Payment**: Stripe API
- **Resilience**: Polly
- **Testing**: xUnit, Moq, FluentAssertions
- **Documentation**: Swashbuckle (Swagger/OpenAPI)

### Frontend
- **Framework**: React 18
- **Language**: TypeScript 5.0
- **Build Tool**: Vite
- **State Management**: Redux Toolkit / Zustand
- **Routing**: React Router 6
- **HTTP Client**: Axios
- **UI Library**: Material-UI / Tailwind CSS
- **Testing**: Vitest, React Testing Library

### DevOps & Infrastructure
- **Containerization**: Docker
- **Orchestration**: Docker Compose
- **CI/CD**: GitHub Actions
- **Monitoring**: Prometheus + Grafana (planned)
- **Logging**: Serilog + Elasticsearch (ELK Stack)

## Prerequisites

### Required
- **.NET SDK**: 8.0 or higher
- **Node.js**: 18.x or higher
- **Docker**: 24.x or higher
- **Docker Compose**: 2.x or higher
- **PostgreSQL**: 15.x (for local development without Docker)

### Optional
- **Visual Studio 2022**: Version 17.8 or higher (Windows)
- **Visual Studio Code**: With C# Dev Kit extension
- **JetBrains Rider**: 2023.3 or higher
- **Git**: For version control
- **Elasticsearch**: 8.x (for local development)
- **Keycloak**: 23.x (for local development)

## Quick Start

### Option 1: Using Docker Compose (Recommended)

1. **Clone the repository**
```bash
git clone <repository-url>
cd csharptweb
```

2. **Set up environment variables**
```bash
cp .env.example .env
# Edit .env with your configuration
```

3. **Start all services**
```bash
docker-compose up -d
```

4. **Verify services are running**
```bash
docker-compose ps
```

5. **Access the application**
- Frontend: http://localhost:3000
- API Gateway: http://localhost:8080
- Keycloak Admin: http://localhost:8180
- Elasticsearch: http://localhost:9200

### Option 2: Manual Setup (Development)

#### Backend Services

1. **Start PostgreSQL**
```bash
docker run -d \
  --name postgres \
  -e POSTGRES_USER=csharp_user \
  -e POSTGRES_PASSWORD=csharp123 \
  -e POSTGRES_DB=csharptweb \
  -p 5432:5432 \
  postgres:15
```

2. **Start Elasticsearch**
```bash
docker run -d \
  --name elasticsearch \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -p 9200:9200 \
  elasticsearch:8.11.0
```

3. **Start Keycloak**
```bash
docker run -d \
  --name keycloak \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  -p 8180:8080 \
  quay.io/keycloak/keycloak:23.0 \
  start-dev
```

4. **Run database migrations**
```bash
# Navigate to each service and run migrations
cd backend/ProductService
dotnet ef database update

cd ../OrderService
dotnet ef database update
```

5. **Build and run each service**
```bash
# Product Service
cd backend/ProductService
dotnet restore
dotnet build
dotnet run

# Order Service
cd backend/OrderService
dotnet restore
dotnet build
dotnet run

# API Gateway
cd backend/ApiGateway
dotnet restore
dotnet build
dotnet run
```

#### Frontend

```bash
cd frontend
npm install
npm run dev
```

## Environment Variables

Create a `.env` file in the root directory:

```env
# Database Configuration
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=csharptweb
POSTGRES_USER=csharp_user
POSTGRES_PASSWORD=csharp123

# Elasticsearch
ELASTICSEARCH_HOST=localhost
ELASTICSEARCH_PORT=9200

# Keycloak
KEYCLOAK_URL=http://localhost:8180
KEYCLOAK_REALM=csharptweb-realm
KEYCLOAK_CLIENT_ID=csharptweb-client
KEYCLOAK_CLIENT_SECRET=your-client-secret

# Stripe
STRIPE_API_KEY=sk_test_your_stripe_key
STRIPE_WEBHOOK_SECRET=whsec_your_webhook_secret

# Service Ports
API_GATEWAY_PORT=8080
PRODUCT_SERVICE_PORT=8081
ORDER_SERVICE_PORT=8082
PAYMENT_SERVICE_PORT=8083

# Frontend
VITE_API_URL=http://localhost:8080
VITE_STRIPE_PUBLIC_KEY=pk_test_your_public_key
```

For detailed environment configuration, see [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).

## API Documentation

### Swagger/OpenAPI

API documentation is available via Swagger UI:

- **Product Service**: http://localhost:8081/swagger
- **Order Service**: http://localhost:8082/swagger
- **API Gateway**: http://localhost:8080/swagger

### Quick API Reference

#### Products
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product (admin)
- `PUT /api/products/{id}` - Update product (admin)
- `DELETE /api/products/{id}` - Delete product (admin)
- `GET /api/products/search?q={query}` - Search products

#### Orders
- `GET /api/orders` - List user orders
- `GET /api/orders/{id}` - Get order details
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}/status` - Update order status (admin)

For detailed API documentation, see [docs/API.md](docs/API.md).

## Testing

### Backend Tests

```bash
# Run all tests
dotnet test

# Run tests for specific service
cd backend/ProductService
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Frontend Tests

```bash
cd frontend
npm test                  # Run all tests
npm run test:watch       # Watch mode
npm run test:coverage    # With coverage
```

## Project Structure

```
csharptweb/
├── backend/
│   ├── ApiGateway/              # Yarp API Gateway
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── ApiGateway.csproj
│   ├── ProductService/          # Product management service
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   ├── Program.cs
│   │   └── ProductService.csproj
│   └── OrderService/            # Order processing service
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Data/
│       ├── Program.cs
│       └── OrderService.csproj
├── frontend/                    # React frontend
│   ├── public/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       ├── types/
│       └── utils/
├── database/                    # Database scripts
│   ├── migrations/
│   └── seeds/
├── docker/                      # Docker configurations
│   ├── Dockerfile.gateway
│   ├── Dockerfile.product
│   └── Dockerfile.order
├── docs/                        # Documentation
│   ├── ARCHITECTURE.md
│   ├── API.md
│   ├── DEVELOPMENT.md
│   ├── DEPLOYMENT.md
│   ├── BONUS-FEATURES.md
│   ├── CSHARP-GUIDE.md
│   └── PROJECT-COMPLETE.md
├── docker-compose.yml
├── .env.example
├── README.md
├── CLAUDE.md
└── QUICK-START.md
```

## Development

### Development Workflow

1. **Create a feature branch**
```bash
git checkout -b feature/your-feature-name
```

2. **Make your changes**
- Follow C# coding conventions (see [docs/CSHARP-GUIDE.md](docs/CSHARP-GUIDE.md))
- Write tests for new features
- Update documentation

3. **Run tests**
```bash
dotnet test     # Backend
npm test        # Frontend
```

4. **Commit your changes**
```bash
git add .
git commit -m "feat: add your feature description"
```

5. **Push and create a pull request**
```bash
git push origin feature/your-feature-name
```

### Code Conventions

- **C#**: Follow Microsoft C# Coding Conventions
- **TypeScript**: Use ESLint and Prettier
- **Commit Messages**: Follow Conventional Commits
- **Branch Naming**: `feature/`, `bugfix/`, `hotfix/`

For detailed development guidelines, see [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) and [docs/CSHARP-GUIDE.md](docs/CSHARP-GUIDE.md).

## Deployment

### Docker Deployment

```bash
# Build images
docker-compose build

# Deploy
docker-compose up -d
```

### Cloud Deployment

Deployment guides for various platforms:
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- Kubernetes (AKS, EKS, GKE)

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed deployment instructions.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write/update tests
5. Submit a pull request

Please read our contributing guidelines for more details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

**Project Maintainer**: Your Name
- Email: your.email@example.com
- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your Profile](https://linkedin.com/in/yourprofile)

**Project Repository**: [github.com/yourusername/csharptweb](https://github.com/yourusername/csharptweb)

---

Built with ASP.NET Core 8.0, C# 12, React, and TypeScript | 2026

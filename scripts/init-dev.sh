#!/bin/bash

# Development Environment Initialization Script
set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}==================================${NC}"
echo -e "${GREEN}Shop Platform - Dev Init${NC}"
echo -e "${GREEN}==================================${NC}"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running${NC}"
    exit 1
fi

# Check if .env exists
if [ ! -f .env ]; then
    echo -e "${YELLOW}Creating .env from .env.example...${NC}"
    cp .env.example .env
    echo -e "${GREEN}✓ .env file created${NC}"
else
    echo -e "${YELLOW}⚠ .env file already exists, skipping...${NC}"
fi

# Install backend dependencies
echo -e "${GREEN}Installing backend dependencies...${NC}"
cd backend
if [ -f "ShopPlatform.sln" ]; then
    dotnet restore ShopPlatform.sln
    echo -e "${GREEN}✓ Backend dependencies installed${NC}"
else
    echo -e "${YELLOW}⚠ No solution file found, skipping backend restore${NC}"
fi
cd ..

# Install frontend dependencies
echo -e "${GREEN}Installing frontend dependencies...${NC}"
cd frontend
if [ -f "package.json" ]; then
    npm install
    echo -e "${GREEN}✓ Frontend dependencies installed${NC}"
else
    echo -e "${YELLOW}⚠ No package.json found, skipping frontend install${NC}"
fi
cd ..

# Start Docker services
echo -e "${GREEN}Starting Docker services...${NC}"
docker-compose up -d postgres elasticsearch keycloak

# Wait for services to be ready
echo -e "${YELLOW}Waiting for services to be ready...${NC}"
sleep 10

# Check PostgreSQL
until docker-compose exec -T postgres pg_isready -U shop_user > /dev/null 2>&1; do
    echo -e "${YELLOW}Waiting for PostgreSQL...${NC}"
    sleep 2
done
echo -e "${GREEN}✓ PostgreSQL is ready${NC}"

# Check Elasticsearch
until curl -s http://localhost:9200/_cluster/health > /dev/null 2>&1; do
    echo -e "${YELLOW}Waiting for Elasticsearch...${NC}"
    sleep 2
done
echo -e "${GREEN}✓ Elasticsearch is ready${NC}"

# Check Keycloak
until curl -s http://localhost:8180/health/ready > /dev/null 2>&1; do
    echo -e "${YELLOW}Waiting for Keycloak...${NC}"
    sleep 2
done
echo -e "${GREEN}✓ Keycloak is ready${NC}"

echo -e "${GREEN}==================================${NC}"
echo -e "${GREEN}Development environment ready!${NC}"
echo -e "${GREEN}==================================${NC}"
echo ""
echo -e "${YELLOW}Services:${NC}"
echo "  PostgreSQL:    localhost:5432"
echo "  Elasticsearch: http://localhost:9200"
echo "  Keycloak:      http://localhost:8180"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "  1. Run 'make run-gateway' to start API Gateway"
echo "  2. Run 'make run-product' to start Product Service"
echo "  3. Run 'make run-order' to start Order Service"
echo "  4. Run 'make run-frontend' to start Frontend"
echo ""
echo "  Or run 'docker-compose up' to start all services with Docker"

#!/bin/bash

# Health Check Script for all services
set -e

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Checking service health...${NC}"
echo ""

# Function to check service health
check_service() {
    local name=$1
    local url=$2
    local status_code=$(curl -s -o /dev/null -w "%{http_code}" $url 2>/dev/null || echo "000")

    if [ "$status_code" = "200" ] || [ "$status_code" = "204" ]; then
        echo -e "${GREEN}✓ $name is healthy${NC} (HTTP $status_code)"
        return 0
    else
        echo -e "${RED}✗ $name is down${NC} (HTTP $status_code)"
        return 1
    fi
}

# Check services
all_healthy=true

check_service "API Gateway" "http://localhost:8080/health" || all_healthy=false
check_service "Product Service" "http://localhost:8081/health" || all_healthy=false
check_service "Order Service" "http://localhost:8082/health" || all_healthy=false
check_service "Frontend" "http://localhost:3000" || all_healthy=false
check_service "Elasticsearch" "http://localhost:9200/_cluster/health" || all_healthy=false
check_service "Keycloak" "http://localhost:8180/health/ready" || all_healthy=false

# Check PostgreSQL
if docker-compose exec -T postgres pg_isready -U shop_user > /dev/null 2>&1; then
    echo -e "${GREEN}✓ PostgreSQL is healthy${NC}"
else
    echo -e "${RED}✗ PostgreSQL is down${NC}"
    all_healthy=false
fi

echo ""
if [ "$all_healthy" = true ]; then
    echo -e "${GREEN}All services are healthy!${NC}"
    exit 0
else
    echo -e "${RED}Some services are down!${NC}"
    exit 1
fi

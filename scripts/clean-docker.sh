#!/bin/bash

# Clean Docker resources
set -e

RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo -e "${YELLOW}==================================${NC}"
echo -e "${YELLOW}Docker Cleanup${NC}"
echo -e "${YELLOW}==================================${NC}"

echo -e "${YELLOW}This will remove:${NC}"
echo "  - All stopped containers"
echo "  - All unused networks"
echo "  - All dangling images"
echo "  - All build cache"
echo ""

read -p "Are you sure? (y/N) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Cleanup cancelled${NC}"
    exit 0
fi

echo -e "${GREEN}Stopping all containers...${NC}"
docker-compose down

echo -e "${GREEN}Removing volumes...${NC}"
docker volume prune -f

echo -e "${GREEN}Removing unused networks...${NC}"
docker network prune -f

echo -e "${GREEN}Removing dangling images...${NC}"
docker image prune -f

echo -e "${GREEN}Removing build cache...${NC}"
docker builder prune -f

echo -e "${GREEN}==================================${NC}"
echo -e "${GREEN}Cleanup complete!${NC}"
echo -e "${GREEN}==================================${NC}"

# Show disk space saved
echo ""
echo -e "${YELLOW}Disk usage:${NC}"
docker system df

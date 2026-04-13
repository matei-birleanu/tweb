#!/bin/sh
# Docker healthcheck script for ASP.NET Core services

set -e

# Get the port from environment or use default
PORT=${ASPNETCORE_URLS##*:}
PORT=${PORT:-8080}

# Extract just the port number if it has a protocol
PORT=$(echo $PORT | sed 's/[^0-9]*//g')

# Try to curl the health endpoint
if command -v curl >/dev/null 2>&1; then
    curl -f http://localhost:${PORT}/health || exit 1
elif command -v wget >/dev/null 2>&1; then
    wget --no-verbose --tries=1 --spider http://localhost:${PORT}/health || exit 1
else
    echo "Neither curl nor wget found, cannot perform health check"
    exit 1
fi

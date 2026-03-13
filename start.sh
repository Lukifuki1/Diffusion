#!/bin/bash

# Stability Matrix Chat Interface - Startup Script
# This script starts OpenWebUI and the backend service

set -e

echo "🚀 Starting Stability Matrix Chat Interface..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "⚠️  Docker daemon not running. Starting it..."
    dockerd > /tmp/docker.log 2>&1 &
    sleep 3
fi

# Pull OpenWebUI image if not present
echo "📦 Checking for OpenWebUI image..."
docker pull ghcr.io/open-webui/open-webui:main 2>/dev/null || echo "   Image already exists or pull failed"

# Build the backend service
echo "🔨 Building StabilityMatrix.ChatInterface..."
cd /workspace/project/Diffusion
docker-compose build stabilitymatrix-backend

# Start all services
echo "🌐 Starting services..."
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 5

# Show status
echo ""
echo "✅ Services started!"
echo ""
echo "📍 OpenWebUI Interface: http://localhost:3000"
echo "📍 Backend API: http://localhost:5000"
echo ""
echo "To view logs:"
echo "  docker-compose logs -f openwebui"
echo "  docker-compose logs -f stabilitymatrix-backend"
echo ""
echo "To stop services:"
echo "  docker-compose down"
echo ""

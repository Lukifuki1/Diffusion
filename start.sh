#!/bin/bash

# AuraFlow Studio - Startup Script
# This script starts AuraFlow Studio and the backend service

set -e

echo "🚀 Starting AuraFlow Studio..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "⚠️  Docker daemon not running. Starting it..."
    dockerd > /tmp/docker.log 2>&1 &
    sleep 3
fi

# Build the backend service
echo "🔨 Building AuraFlow API..."
cd /workspace/project/Diffusion
docker-compose build auraflow-api

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
echo "📍 AuraFlow Studio: http://localhost:5000"
echo ""
echo "To view logs:"
echo "  docker-compose logs -f auraflow-api"
echo ""
echo "To stop services:"
echo "  docker-compose down"
echo ""

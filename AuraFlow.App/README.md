# AuraFlow Studio Chat Interface

A simple chat-style interface for generating images and videos using Blazor Server as the frontend and AuraFlow Studio as the backend.

## Features

- Simple prompt-based generation
- Support for multiple models (Flux, SDXL, Wan2GP, etc.)
- Real-time progress tracking
- WebSocket communication for live updates
- REST API for synchronous operations

## Quick Start

1. Build the project:
   ```bash
   dotnet build AuraFlow.App/AuraFlow.App.csproj
   ```

2. Run with Docker:
   ```bash
   docker-compose up -d
   ```

3. Access AuraFlow Studio at http://localhost:5000

## Configuration

Edit `config.json` to customize settings:

```json
{
  "ChatInterface": {
    "enabled": true,
    "defaultModel": "Flux Dev",
    "maxConcurrentGenerations": 3,
    "timeoutSeconds": 120
  }
}
```

## API Endpoints

- `POST /api/v1/generate` - Generate image/video from prompt
- `GET /api/v1/progress/{taskId}` - Get generation progress
- `POST /api/v1/cancel/{taskId}` - Cancel ongoing generation

# AuraFlow Studio

<p align="center">
  <a href="https://github.com/Lukifuki1/Diffusion">
    <img src="https://img.shields.io/github/v/release/Lukifuki1/Diffusion?include_prereleases&label=latest" alt="Latest Release">
  </a>
  <a href="https://github.com/Lukifuki1/Diffusion/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/Lukifuki1/Diffusion" alt="License">
  </a>
  <a href="https://github.com/Lukifuki1/Diffusion/actions">
    <img src="https://img.shields.io/github/actions/workflow/status/Lukifuki1/Diffusion/ci.yml" alt="CI Status">
  </a>
  <img src="https://img.shields.io/badge/.NET-9.0-blue" alt=".NET 9.0">
</p>

A professional-grade AI image and video generation platform with a clean, unified chat-style interface. Built on .NET 9 with ASP.NET Core and Blazor Server.

---

## 🚀 Features

| Feature | Description |
|---------|-------------|
| **Text-to-Image** | Generate stunning images using Flux, SDXL, Realistic Vision models |
| **Text-to-Video** | Create videos from text descriptions using Wan2GP or CogVideo |
| **Chat Interface** | Simple, intuitive prompt-based UI similar to ChatGPT |
| **Real-time Progress** | Live generation progress tracking with WebSocket updates |
| **Model Management** | Easy model selection and configuration |
| **Generation History** | Browse and manage all previous generations |
| **REST API** | Full-featured API for programmatic access |

---

## 🏗️ Architecture

```
┌─────────────────────┐     ┌──────────────────────────┐     ┌─────────────────┐
│   AuraFlow Studio   │◄───►│  AuraFlow API            │◄───►│  ComfyUI        │
│   (Blazor Server)   │     │  (.NET 9.0 ASP.NET Core)   │     │  (AI Engine)    │
│   Port: 5000        │     │  Port: 5000              │     │  Port: 8188     │
└─────────────────────┘     └──────────────────────────┘     └─────────────────┘
         │                           │                              │
         ▼                           ▼                              ▼
   Interactive UI              REST + WebSocket              Model Execution
   SignalR Hub                 JSON API                      Image/Video Output
```

### Technology Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Blazor Server, Bootstrap 5, SignalR |
| **Backend** | .NET 9.0, ASP.NET Core, Entity Framework Core |
| **AI Engine** | ComfyUI (Flux, SDXL, Wan2GP, CogVideo) |
| **Database** | LiteDB (embedded NoSQL) |
| **Caching** | In-memory distributed cache |
| **Logging** | NLog with structured logging |

---

## 📦 Project Structure

```
Diffusion/
├── src/
│   ├── AuraFlow/                      # Main web application
│   │   ├── Controllers/              # REST API endpoints
│   │   │   └── GenerationController.cs
│   │   ├── Services/                 # Business logic layer
│   │   │   ├── GenerationService.cs
│   │   │   ├── DownloadService.cs
│   │   │   └── SettingsManager.cs
│   │   ├── Infrastructure/           # Infrastructure layer
│   │   │   ├── Engines/             # ComfyUI integration
│   │   │   ├── Persistence/         # LiteDB storage
│   │   │   └── Messaging/           # Queue services
│   │   ├── Middlewares/              # HTTP middleware
│   │   │   ├── HealthCheckMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Pages/                    # Blazor pages
│   │   │   ├── Chat.razor
│   │   │   ├── Index.razor
│   │   │   └── SettingsPage.razor
│   │   ├── Program.cs               # Application entry point
│   │   └── AuraFlow.csproj
│   │
│   └── AuraFlow.Core/                # Core domain library
│       ├── Api/                      # ComfyUI REST client
│       ├── Services/                 # Domain services
│       ├── Models/                   # Domain models
│       └── Common/                   # Shared utilities
│
├── AuraFlow.sln                     # Solution file
├── Dockerfile                       # Multi-stage Docker build
├── docker-compose.yml               # Service orchestration
├── config.json                      # Application configuration
└── README.md                        # This file
```

---

## 🛠️ Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Docker & Docker Compose (optional)
- ComfyUI with Flux/Wan2GP workflows installed

### Local Development

```bash
# Clone the repository
git clone https://github.com/Lukifuki1/Diffusion.git
cd Diffusion

# Restore dependencies
dotnet restore AuraFlow.sln

# Configure settings
# Edit config.json to set preferred models

# Run the application
dotnet run --project src/AuraFlow/AuraFlow.csproj
```

The application will be available at `http://localhost:5000`

### Docker Deployment

```bash
# Build and start all services
docker-compose up -d

# View API logs
docker-compose logs -f auraflow-api

# View ComfyUI logs
docker-compose logs -f comfyui

# Stop services
docker-compose down
```

---

## ⚙️ Configuration

### config.json

```json
{
  "preferredModels": {
    "photos": "Flux Dev",
    "video": "Wan2GP"
  },
  "inferenceSettings": {
    "defaultWidth": 1024,
    "defaultHeight": 1024,
    "steps": 30,
    "cfgScale": 7.5
  },
  "ChatInterface": {
    "enabled": true,
    "defaultModel": "Flux Dev",
    "maxConcurrentGenerations": 3,
    "timeoutSeconds": 120,
    "apiBaseUrl": "http://localhost:5000"
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DOTNET_ENVIRONMENT` | Runtime environment | `Development` |
| `ChatInterface__Enabled` | Enable chat interface | `true` |
| `ChatInterface__DefaultModel` | Default AI model | `Flux Dev` |
| `ChatInterface__MaxConcurrentGenerations` | Max parallel tasks | `3` |
| `ChatInterface__TimeoutSeconds` | Generation timeout | `120` |

---

## 🔌 API Reference

### REST Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/v1/generation/generate` | POST | Generate image or video |
| `/api/v1/generation/progress/{taskId}` | GET | Get generation progress |
| `/api/v1/generation/models` | GET | List available models |
| `/api/v1/generation/history` | GET | Get generation history |
| `/health` | GET | Health check (always returns 200) |
| `/ready` | GET | Readiness check |

### Generate Content

```bash
curl -X POST http://localhost:5000/api/v1/generation/generate \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "A serene mountain landscape at sunset, 4K, photorealistic",
    "modelName": "Flux Dev",
    "width": 1024,
    "height": 1024,
    "steps": 30,
    "cfgScale": 7.5
  }'
```

### Response

```json
{
  "taskId": "abc123",
  "status": "completed",
  "imageUrl": "/output/abc123.png",
  "prompt": "A serene mountain landscape...",
  "modelName": "Flux Dev",
  "generationTime": 15.2
}
```

---

## 🎨 Supported Models

### Image Generation

| Model | Description | Best For |
|-------|-------------|----------|
| **Flux Dev** | State-of-the-art diffusion model | High-quality photorealistic images |
| **SDXL Turbo** | Fast generation (4-8 steps) | Quick iterations |
| **Realistic Vision** | Photorealistic portraits | Portraits, landscapes |
| **Pony Diffusion** | Anime/illustration style | Art, illustrations |

### Video Generation

| Model | Description | Best For |
|-------|-------------|----------|
| **Wan2GP** | Wan 2.1 video generation | High-quality videos |
| **CogVideo** | Text-to-video | Creative projects |
| **SVD** | Stable Video Diffusion | Short clips |

---

## 🔧 Development

### Build

```bash
# Debug build
dotnet build AuraFlow.sln

# Release build
dotnet build AuraFlow.sln -c Release
```

### Testing

```bash
# Run unit tests
dotnet test tests/AuraFlow.UnitTests/
```

### Code Style

This project follows:
- C# 13 coding conventions
- XML documentation for public APIs
- Async/await best practices
- Dependency injection throughout

---

## 📊 Performance

| Metric | Target |
|--------|--------|
| API Response Time | < 2s (p95) |
| Generation Queue | Max 3 concurrent |
| Uptime | 99.9% SLA |
| Error Rate | < 0.1% |

---

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [ComfyUI](https://github.com/comfyanonymous/ComfyUI) - For the powerful AI inference engine
- [Flux](https://blackforestlabs.ai/) - For the state-of-the-art image generation models
- [.NET Team](https://dotnet.microsoft.com/) - For the excellent development platform

---

**Built with ❤️ using .NET 9, ASP.NET Core, and Blazor Server**

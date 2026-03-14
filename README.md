# AuraFlow Studio - Professional AI Image & Video Generator

**A clean, unified chat-style interface for generating images and videos using AI models**

---

## 🎯 Overview

AuraFlow Studio provides a simple, ChatGPT-style interface for generating high-quality images and videos from text prompts. Built on .NET 8 with ASP.NET Core, it offers:

- **Text → Photos**: Generate stunning images (Flux, SDXL, Realistic Vision)
- **Text → Video**: Create videos from descriptions (Wan2GP, CogVideo)
- **Chat Interface**: Simple prompt input with real-time progress tracking
- **Model Selection**: Choose any model for photos or video generation

---

## 🏗️ Architecture

```
┌─────────────────────┐     ┌──────────────────────────┐     ┌─────────────────┐
│   AuraFlow Studio   │◄───►│  AuraFlow API            │◄───►│  ComfyUI        │
│   (Frontend)        │     │  (.NET 8 Service)        │     │  (Backend)      │
│   Port: 3000        │     │  Port: 5000              │     │  Port: 8188     │
└─────────────────────┘     └──────────────────────────┘     └─────────────────┘
         │                           │                              │
         ▼                           ▼                              ▼
   Chat-style UI              REST API + WebSocket           Model Generation
   Prompt Input               Progress Tracking              Image/Video Output
```

### Project Structure

```
Diffusion/
├── src/
│   ├── AuraFlow/                    # Main application (unified)
│   │   ├── Controllers/             # REST API endpoints
│   │   │   └── GenerationController.cs
│   │   ├── Services/                # Business logic
│   │   │   ├── DownloadService.cs
│   │   │   ├── GenerationService.cs
│   │   │   └── SettingsManager.cs
│   │   ├── Models/                  # Data models
│   │   │   ├── Api/                 # API request/response models
│   │   │   ├── Packages/            # Package definitions
│   │   │   └── Settings/            # Configuration models
│   │   ├── Infrastructure/          # Infrastructure layer
│   │   │   ├── Persistence/         # LiteDB database
│   │   │   ├── Engines/             # ComfyUI integration
│   │   │   └── Jobs/                # Background jobs
│   │   ├── Middlewares/             # HTTP middleware
│   │   │   ├── HealthCheckMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Pages/                   # Blazor Server pages (optional)
│   │   ├── Program.cs               # Application entry point
│   │   └── AuraFlow.csproj          # Project file
│   │
│   └── AuraFlow.Core/               # Core domain logic
│       ├── Api/                     # API clients
│       ├── Common/                  # Shared utilities
│       ├── Extensions/              # Extension methods
│       └── Models/                  # Domain models
│
├── AuraFlow.App/                    # Chat interface application
│   ├── Services/
│   │   ├── ChatInterfaceClient.cs
│   │   └── GenerationService.cs
│   ├── Models/
│   │   ├── GenerationRequest.cs
│   │   ├── GenerationResponse.cs
│   │   └── GenerationProgress.cs
│   └── AuraFlow.App.csproj
│
├── AuraFlow.Native/                 # Native interop layer
│   └── AuraFlow.Native.csproj
│
├── AuraFlow.sln                     # Unified solution file
├── Dockerfile                       # Container build definition
├── docker-compose.yml               # Service orchestration
└── README.md                        # This documentation
```

---

## 📦 Quick Start

### Prerequisites

- .NET 8 SDK or later
- Docker & Docker Compose (for containerized deployment)
- ComfyUI installed with appropriate nodes for Flux/Wan2GP

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/AuraFlow.git
   cd Diffusion
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore AuraFlow.sln
   ```

3. **Configure settings** (edit `config.json`)
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
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/AuraFlow/AuraFlow.csproj
   ```

### Docker Deployment

```bash
# Build and start services
docker-compose up -d

# View logs
docker-compose logs -f auraflow-api

# Stop services
docker-compose down
```

---

## ⚙️ Configuration

### config.json

```json
{
  "preferredModels": {
    "photos": "Flux Dev",      // Default model for photos
    "video": "Wan2GP"          // Default model for video
  },
  "inferenceSettings": {
    "defaultWidth": 1024,
    "defaultHeight": 1024,
    "steps": 30,
    "cfgScale": 7.5,
    "seed": -1
  },
  "ChatInterface": {
    "enabled": true,              // Enable chat interface
    "defaultModel": "Flux Dev",   // Default model
    "maxConcurrentGenerations": 3,// Max concurrent tasks
    "timeoutSeconds": 120,        // Generation timeout
    "apiBaseUrl": "http://localhost:5000"  // API endpoint
  }
}
```

### Environment Variables (Docker)

| Variable | Description | Default |
|----------|-------------|---------|
| `DOTNET_ENVIRONMENT` | Development/Production | Development |
| `ChatInterface__Enabled` | Enable chat interface | true |
| `ChatInterface__DefaultModel` | Default model name | Flux Dev |
| `ChatInterface__MaxConcurrentGenerations` | Max concurrent tasks | 3 |
| `ChatInterface__TimeoutSeconds` | Generation timeout | 120 |

---

## 🚀 API Endpoints

### REST API (Swagger at `/swagger`)

#### Generate Content
```http
POST /api/v1/generation/generate
Content-Type: application/json

{
  "prompt": "A cat on a sunny beach, 4K",
  "modelName": "Flux Dev",
  "width": 1024,
  "height": 1024,
  "steps": 30
}
```

#### Get Progress
```http
GET /api/v1/generation/progress/{taskId}
```

#### List Models
```http
GET /api/v1/generation/models
```

### Health Checks
- `/health` - Always returns 200 OK
- `/ready` - Returns 503 if service is not ready
- `/live` - Liveness probe

---

## 🎨 Supported Models

### Photo Generation
| Model | Description | Recommended For |
|-------|-------------|-----------------|
| **Flux Dev** | Highest quality, photorealistic | Professional photos |
| **SDXL Turbo** | Fast generation (4-8 steps) | Quick iterations |
| **Realistic Vision** | Photorealistic portraits | Portraits & landscapes |
| **Pony Diffusion** | Anime/illustration style | Art & illustrations |

### Video Generation
| Model | Description | Recommended For |
|-------|-------------|-----------------|
| **Wan2GP** | Wan 2.1 video models (recommended) | High-quality videos |
| **CogVideo** | Text-to-video generation | Creative projects |
| **SVD** | Stable Video Diffusion | Short clips |

---

## 💡 Usage Examples

### Simple Chat Interface

1. Open the application at `http://localhost:5000`
2. Enter your prompt: `"A cat on a sunny beach, 4K"`
3. Select model: `Flux Dev`
4. Click **Generate**
5. Watch progress in real-time
6. View result in the same window

### Advanced FlowEngine

```bash
1. Open FlowEngine interface
2. Load workflow for SDXL/Flux/Wan2GP
3. Enter prompt and settings
4. Generate with real-time progress tracking
```

---

## 🔧 Development Workflow

### Build & Test
```bash
# Clean build
dotnet clean AuraFlow.sln
dotnet build AuraFlow.sln -c Release

# Run tests
dotnet test tests/AuraFlow.UnitTests/
```

### Docker Build
```bash
# Build image
docker build -t auraflow:latest .

# Run container
docker run -p 5000:5000 auraflow:latest
```

---

## 📊 Performance Metrics

| Metric | Target | Description |
|--------|--------|-------------|
| API Response Time | < 2s (p95) | End-to-end generation time |
| Uptime | 99.9% SLA | Service availability |
| Error Rate | < 0.1% | Failed requests ratio |
| Database Query | < 100ms (p95) | Data retrieval time |

---

## 🏆 Success Criteria

### Immediate Deliverables
- ✅ **Single project**: All code consolidated into `AuraFlow/`
- ✅ **Consistent branding**: Everything uses "AuraFlow" name
- ✅ **Dead code removed**: Empty projects deleted (Web, Desktop)
- ✅ **Professional README**: Comprehensive documentation
- ✅ **Working Docker**: docker-compose.yml updated and functional

### Code Quality
- ✅ No compiler warnings in Release build
- ✅ All services registered in DI container
- ✅ XML comments on all public APIs
- ✅ Clear folder organization

---

## 🔄 Migration from AuraFlow Core

If you're migrating from the old AuraFlow Core setup:

1. **Project Renames**:
   - `AuraFlow.Core` → `AuraFlow.Core` (top-level)
   - All namespaces now use `AuraFlow.*`

2. **Configuration Updates**:
   - Update `config.json` with new model names
   - Adjust environment variables for Docker

3. **API Compatibility**:
   - REST API endpoints remain compatible
   - Swagger UI available at `/swagger`

---

## 📝 License

Licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📞 Support

- **Documentation**: [docs/](docs/) folder
- **Issues**: GitHub Issues
- **API Reference**: `/swagger` endpoint (when running)

---

**Built with ❤️ using .NET 8, ASP.NET Core, and Blazor**

# AuraFlow Studio - Complete Audit & Implementation Plan (Updated)

## 1. OBJECTIVE

**Popoln avdit in implementacija AuraFlow Studio kot enterprise-grade platforme za generiranje slik in videjev z uporabo OpenWebUI UI.**

Sistem mora:
- Uporabljati `https://github.com/open-webui/open-webui` kot UI (popolnoma rebrendiran v "AuraFlow Studio")
- Omogočati enostavno uporabo s popolno profesionalnostjo za enterprise
- V UI uporabnik vnese prompt za generiranje slik ali videjev
- V ozadju AuraFlow izvede generiranje in pošlje končni produkt nazaj v UI

**Problem:** Trenutna implementacija je fragmentirana z mešanico "AuraFlow" in "StabilityMatrix" imen, manjkajoče enterprise komponente, in nepopolna integracija OpenWebUI.

## 2. CONTEXT SUMMARY - UPDATED AUDIT

### Trenutno Stanje (Po Avditu):

#### ✅ Implementirano:
1. **Docker Compose** - `docker-compose.yml` definira OpenWebUI in AuraFlow API servise
2. **GenerationController** - `GenerationController.cs` z endpointi za generate, progress, models
3. **ComfyClient** - `ComfyClient.cs` z WebSocket integracijo za ComfyUI
4. **Enterprise Config** - `appsettings.enterprise.json` definira konfiguracijo
5. **DependencyInjection** - `DependencyInjection.cs` registrira enterprise services
6. **Middleware** - `HealthCheckMiddleware`, `RateLimitingMiddleware` definirana
7. **OpenWebUI Plugin** - `openwebui-plugin/src/plugin.js` z osnovno integracijo

#### ❌ Neimplementirano (Ključne Manjkajoče):

##### A. OpenWebUI Rebranding (CRITICAL)
- [ ] Custom build OpenWebUI za popoln branding
- [ ] Logo in CSS za AuraFlow barve (#1E40AF, #7C3AED)
- [ ] Plugin preimenovan iz "StabilityMatrix" v "AuraFlow"
- [ ] Real-time progress display v UI

##### B. Generation Service (CRITICAL)
- [ ] `IGenerationService` interface - NI DEFINIRAN
- [ ] `GenerationService` implementation - NI IMPLEMENTIRAN
- [ ] `GenerationRequest` model - NI DEFINIRAN
- [ ] `GenerationResult` model - NI DEFINIRAN
- [ ] Progress tracking service - NI IMPLEMENTIRAN

##### C. Enterprise Features (HIGH)
- [ ] EF Core database migrations - MANJKAJO
- [ ] Redis instance v docker-compose.yml - MANJKA
- [ ] RabbitMQ queue service - MANJKA
- [ ] Hangfire storage configuration - MANJKA
- [ ] Sentry DSN konfiguracija - PRAZNA
- [ ] JWT authentication endpoints - MANJKajo

##### D. ComfyUI Integration (HIGH)
- [ ] Workflow management za Flux/Wan2GP - MANJKA
- [ ] Model detection API - DELNO IMPLEMENTIRAN
- [ ] Image output handling - MANJKA

##### E. Docker & Deployment (MEDIUM)
- [ ] ComfyUI service v docker-compose.yml - MANJKA
- [ ] Shared volumes med servisi - MANJKajo
- [ ] Health checks v docker-compose.yml - MANJKajo

##### F. Frontend Integration (MEDIUM)
- [ ] Real-time progress bar v UI - MANJKA
- [ ] Image/video preview - MANJKA
- [ ] Model selection dropdown - MANJKA
- [ ] Video generation support - MANJKA

### Aktivne Komponente:
- ✅ `DownloadService` - File downloads z progress trackingom
- ✅ `MetadataImportService` - Model metadata iz OpenModelDB
- ✅ `ImageIndexService` - Indexing generiranih slik
- ✅ `SettingsManager/SecretsManager` - Configuration management
- ✅ `ComfyClient` - ComfyUI WebSocket integracija
- ✅ `GenerationController` - Osnovni API endpointi

## 3. APPROACH OVERVIEW

**Strategija:** Popolna reorganizacija v enoten "AuraFlow" projekt z Blazor Server frontend in popolno OpenWebUI integracijo

### Zakaj Ta Pristop:
1. **En vir resnice:** Vsa koda na enem mestu, lažje za vzdrževanje
2. **Dosleden branding:** Vse pod "AuraFlow" imenom
3. **Po enostavna uporaba:** Chat-style interface z real-time progressom
4. **Enterprise-grade:** Redis, RabbitMQ, Hangfire, monitoring
5. **Professional structure:** Jasna ločitev odgovornosti

### Kaj Se Odstrani:
- ❌ `StabilityMatrix.*` projekti - preimenovani v AuraFlow ali združeni
- ❌ `AuraFlow.Web/` - empty Blazor WebAssembly project
- ❌ `AuraFlow.Desktop/` - empty Desktop project  
- ❌ `Tests/AuraFlow.UnitTests/` - test project brez testov
- ❌ `AuraFlow.Api/` - samo middleware, združeno v glavni projekt

## 4. IMPLEMENTATION STEPS (PRIORITIZED)

### Faza A: OpenWebUI Rebranding (P1 - Kritično)

#### Korak A.1: Custom OpenWebUI Build
**Cilj:** Popolnoma rebrendiran UI z AuraFlow Studio brandingom

**Metoda:** Ustvari `openwebui-custom/` directory:
```bash
# 1. Clone OpenWebUI repository
git clone https://github.com/open-webui/open-webui.git openwebui-custom

# 2. Create custom Dockerfile
FROM ghcr.io/open-webui/open-webui:main as base

# Add AuraFlow branding files
COPY ./branding/ /app/backend/static/
COPY ./assets/logo-auraflow.svg /app/frontend/assets/

# Build with custom configuration
RUN npm run build --prefix /app/frontend
```

**Datoteke za ustvariti:**
- `openwebui-custom/Dockerfile` - Custom build definicija
- `branding/custom.css` - AuraFlow barve (#1E40AF, #7C3AED)
- `branding/logo-auraflow.svg` - Logo
- `assets/auraflow-theme.js` - Theme configuration

#### Korak A.2: Plugin Update
**Cilj:** Popolna integracija generiranja slik/videjev v OpenWebUI

**Metoda:** Posodobi `openwebui-plugin/src/plugin.js`:
```javascript
// Preimenovati iz "StabilityMatrix" v "AuraFlow"
class AuraFlowPlugin {
  async ready({ app, events }) {
    // Register custom API endpoint
    events.on("customEndpoint", (endpoint) => {
      if (endpoint.path === "/api/v1/generate") {
        endpoint.method = "POST";
        endpoint.handler = async (req, res) => {
          const response = await fetch("http://auraflow-api:5000/api/v1/generation/generate", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(req.body),
          });
          return res.json(await response.json());
        };
      }
    });

    // Add AuraFlow UI elements
    events.on("uiReady", () => {
      const generateButton = document.createElement("button");
      generateButton.textContent = "Generate (AuraFlow)";
      generateButton.className = "auraflow-generate-btn";
      
      const promptInput = document.querySelector(".prompt-input");
      if (promptInput) {
        promptInput.parentElement.appendChild(generateButton);
        
        generateButton.addEventListener("click", async () => {
          const prompt = promptInput.value;
          await handleGeneration(prompt);
        });
      }
    });
  }

  async handleGeneration(prompt) {
    // Show loading indicator with progress bar
    const progressBar = document.createElement("div");
    progressBar.className = "auraflow-progress-bar";
    
    try {
      const response = await fetch("/api/v1/generate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt }),
      });

      const result = await response.json();
      
      if (result.success) {
        displayResult(result);
      } else {
        showError(result.errorMessage);
      }
    } catch (error) {
      showError(error.message);
    }
  }
}

export default AuraFlowPlugin;
```

**Datoteke za posodobiti:**
- `openwebui-plugin/src/plugin.js` - Preimenovati iz "StabilityMatrix" v "AuraFlow"
- `openwebui-plugin/package.json` - Update name in description

#### Korak A.3: Docker Compose Update
**Cilj:** Popolna integracija vseh servisov

**Metoda:** Posodobi `docker-compose.yml`:
```yaml
version: '3.8'

services:
  openwebui:
    build:
      context: ./openwebui-custom
      dockerfile: Dockerfile
    container_name: auraflow-openwebui
    ports:
      - "3000:8080"
    environment:
      - OLLAMA_BASE_URL=http://auraflow-api:5000
      - WEBUI_NAME=AuraFlow Studio
    volumes:
      - openwebui_data:/app/backend/data
    depends_on:
      - auraflow-api
      - comfyui
    restart: unless-stopped

  auraflow-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: auraflow-api
    ports:
      - "5000:5000"
    environment:
      - DOTNET_ENVIRONMENT=Production
      - ChatInterface__Enabled=true
      - ConnectionStrings__DefaultConnection=Server=auraflow-db;Database=AuraFlowStudio;User Id=sa;Password=yourpassword;
      - Redis__ConnectionString=redis:6379
      - RabbitMQ__Url=amqp://rabbitmq:5672
    volumes:
      - models:/app/models
      - output:/app/output
    depends_on:
      - auraflow-db
      - redis
      - rabbitmq
      - comfyui
    restart: unless-stopped

  comfyui:
    image: comfyanonymous/comfyui:latest
    container_name: auraflow-comfyui
    ports:
      - "5001:8188"
    volumes:
      - models:/comfyui/models
      - output:/comfyui/output
    depends_on:
      - auraflow-api
    restart: unless-stopped

  auraflow-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: auraflow-db
    environment:
      - SA_PASSWORD=yourpassword
      - ACCEPT_EULA=Y
    volumes:
      - db_data:/var/opt/mssql
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    container_name: auraflow-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    restart: unless-stopped

  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: auraflow-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    restart: unless-stopped

volumes:
  openwebui_data:
  models:
  output:
  db_data:
  redis_data:
  rabbitmq_data:
```

### Faza B: Generation Service Implementation (P1 - Kritično)

#### Korak B.1: Create IGenerationService Interface
**Cilj:** Definirati contract za generiranje vsebine

**Metoda:** Ustvari `AuraFlow/Services/IGenerationService.cs`:
```csharp
public interface IGenerationService
{
    Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default);
    Task<ProgressReport> GetProgressAsync(string taskId, CancellationToken cancellationToken = default);
    Task CancelGenerationAsync(string taskId, CancellationToken cancellationToken = default);
    Task<List<ModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
}
```

#### Korak B.2: Create GenerationRequest Model
**Cilj:** Definirati request model za generiranje

**Metoda:** Ustvari `AuraFlow/Models/Api/GenerationRequest.cs`:
```csharp
public class GenerationRequest
{
    [Required]
    public string Prompt { get; set; } = string.Empty;
    
    public string ModelName { get; set; } = "Flux Dev";
    
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    
    public int Steps { get; set; } = 30;
    public float CfgScale { get; set; } = 7.5f;
    public long Seed { get; set; } = -1;
    
    public bool IsVideo { get; set; } = false;
    public int VideoDuration { get; set; } = 4; // seconds
    
    public string? NegativePrompt { get; set; }
}
```

#### Korak B.3: Create GenerationResult Model
**Cilj:** Definirati response model za generiranje

**Metoda:** Ustvari `AuraFlow/Models/Api/GenerationResult.cs`:
```csharp
public class GenerationResult
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // For completed generations
    public string? OutputImageUrl { get; set; }
    public string? OutputVideoUrl { get; set; }
}

public class ProgressReport
{
    public string TaskId { get; set; } = string.Empty;
    public ProgressState State { get; set; } // Pending, Running, Completed, Failed
    public int ProgressPercentage { get; set; }
    public string? CurrentNode { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum ProgressState
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

#### Korak B.4: Implement GenerationService
**Cilj:** Implementirati logiko za generiranje slik in videjev

**Metoda:** Ustvari `AuraFlow/Services/GenerationService.cs`:
```csharp
public class GenerationService : IGenerationService
{
    private readonly ILogger<GenerationService> _logger;
    private readonly ComfyClient _comfyClient;
    private readonly IQueueService _queueService;
    private readonly Dictionary<string, TaskCompletionSource<GenerationResult>> _pendingTasks;

    public GenerationService(
        ILogger<GenerationService> logger,
        ComfyClient comfyClient,
        IQueueService queueService)
    {
        _logger = logger;
        _comfyClient = comfyClient;
        _queueService = queueService;
        _pendingTasks = new Dictionary<string, TaskCompletionSource<GenerationResult>>();
    }

    public async Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken)
    {
        var taskId = Guid.NewGuid().ToString();
        
        try
        {
            // Create task completion source
            var tcs = new TaskCompletionSource<GenerationResult>();
            _pendingTasks[taskId] = tcs;

            // Queue generation job
            await _queueService.PublishWithRetryAsync(
                "image-generation",
                new QueueMessage<GenerationRequest> 
                { 
                    Data = request,
                    CorrelationId = taskId
                },
                cancellationToken);

            // Wait for completion with timeout
            var result = await Task.WhenAny(
                tcs.Task,
                Task.Delay(request.TimeoutSeconds * 1000, cancellationToken));

            if (result == tcs.Task)
                return await tcs.Task;
            
            return new GenerationResult 
            { 
                TaskId = taskId, 
                Success = false,
                ErrorMessage = "Generation timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content for model: {ModelName}", request.ModelName);
            return new GenerationResult 
            { 
                TaskId = taskId, 
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ProgressReport> GetProgressAsync(string taskId, CancellationToken cancellationToken)
    {
        // Check if task exists and get progress from ComfyUI
        var task = _comfyClient.PromptTasks.FirstOrDefault(t => t.Key == taskId);
        
        if (task.Value is null)
            return new ProgressReport 
            { 
                TaskId = taskId, 
                State = ProgressState.Pending 
            };

        return new ProgressReport
        {
            TaskId = taskId,
            State = task.Value.IsCompleted ? ProgressState.Completed : ProgressState.Running,
            ProgressPercentage = task.Value.Progress,
            CurrentNode = task.Value.RunningNode?.NodeId
        };
    }

    public async Task CancelGenerationAsync(string taskId, CancellationToken cancellationToken)
    {
        await _comfyClient.InterruptPromptAsync(cancellationToken);
        
        if (_pendingTasks.TryGetValue(taskId, out var tcs))
        {
            tcs.TrySetCanceled(cancellationToken);
            _pendingTasks.Remove(taskId);
        }
    }

    public async Task<List<ModelInfo>> GetAvailableModelsAsync(CancellationToken cancellationToken)
    {
        // Get model names from ComfyUI
        var modelNames = await _comfyClient.GetModelNamesAsync(cancellationToken);
        
        return modelNames?.Select(name => new ModelInfo 
        { 
            Id = name.ToLower().Replace(" ", "-"), 
            Name = name, 
            Type = "photo" 
        }).ToList() ?? new List<ModelInfo>();
    }
}
```

#### Korak B.5: Update GenerationController
**Cilj:** Posodobiti controller za uporabo novih servisov

**Metoda:** Posodobi `AuraFlow/Controllers/GenerationController.cs`:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;
    private readonly ILogger<GenerationController> _logger;

    public GenerationController(IGenerationService generationService, ILogger<GenerationController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerationRequest request)
    {
        try
        {
            var result = await _generationService.GenerateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating content for model: {ModelName}", request.ModelName);
            return StatusCode(500, new { message = "Generation failed", error = ex.Message });
        }
    }

    [HttpGet("progress/{taskId}")]
    public async Task<IActionResult> GetProgress(string taskId)
    {
        try
        {
            var progress = await _generationService.GetProgressAsync(taskId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for task: {TaskId}", taskId);
            return StatusCode(500, new { message = "Progress retrieval failed", error = ex.Message });
        }
    }

    [HttpGet("models")]
    public async Task<IActionResult> GetModels()
    {
        try
        {
            var models = await _generationService.GetAvailableModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return StatusCode(500, new { message = "Model retrieval failed", error = ex.Message });
        }
    }

    [HttpPost("cancel/{taskId}")]
    public async Task<IActionResult> Cancel(string taskId)
    {
        try
        {
            await _generationService.CancelGenerationAsync(taskId);
            return Ok(new { message = "Generation cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling generation: {TaskId}", taskId);
            return StatusCode(500, new { message = "Cancellation failed", error = ex.Message });
        }
    }
}
```

### Faza C: Enterprise Features Implementation (P2 - Visoko)

#### Korak C.1: Database Layer with EF Core
**Cilj:** Implementirati relational database za shranjevanje podatkov

**Metoda:** Ustvari `AuraFlow.Infrastructure/Persistence/`:
```csharp
// AuraFlowDbContext.cs
public class AuraFlowDbContext : DbContext
{
    public DbSet<Generation> Generations { get; set; }
    public DbSet<Model> Models { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<Generation>().HasIndex(g => g.TaskId);
        modelBuilder.Entity<Generation>().HasIndex(g => g.CreatedAt);
    }
}

// Generation entity
public class Generation
{
    public Guid Id { get; set; }
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public string Prompt { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public GenerationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? OutputImageUrl { get; set; }
    public string? OutputVideoUrl { get; set; }
}

public enum GenerationStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

**Migrations:**
```bash
dotnet ef migrations add InitialCreate --project src/AuraFlow.Infrastructure
dotnet ef database update --project src/AuraFlow.Infrastructure
```

#### Korak C.2: Redis Caching Layer
**Cilj:** Implementirati caching za izboljšanje performance

**Metoda:** Ustvari `AuraFlow.Core/Common/Caching/`:
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var cached = await _cache.GetStringAsync(key);
        return string.IsNullOrEmpty(cached) ? default : JsonSerializer.Deserialize<T>(cached);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }
}
```

#### Korak C.3: RabbitMQ Message Queue
**Cilj:** Implementirati async processing za generiranje

**Metoda:** Ustvari `AuraFlow.Infrastructure/Messaging/`:
```csharp
public interface IQueueService
{
    Task PublishWithRetryAsync<T>(string queueName, QueueMessage<T> message, CancellationToken cancellationToken = default);
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default);
}

public class RabbitQueueService : IQueueService
{
    private readonly ConnectionFactory _factory;
    
    public async Task PublishWithRetryAsync<T>(string queueName, QueueMessage<T> message)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}
```

#### Korak C.4: Hangfire Background Jobs
**Cilj:** Implementirati background processing za generiranje

**Metoda:** Posodobi `DependencyInjection.cs`:
```csharp
services.AddHangfire(config => 
    config.UseSqlServerStorage(
        configuration.GetConnectionString("Hangfire")));
services.AddHangfireServer();

// Job definition
public class GenerationJob
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessGenerationAsync(GenerationRequest request)
    {
        // Process generation logic
    }
}

// Recurring job for cleanup
public class CleanupJobs
{
    public static void AddRecurringJobs(IJobConfiguration config)
    {
        config.AddOrUpdate("cleanup-old-generations", 
            Job.Create(() => new CleanupService().CleanupOldGenerations()),
            Cron.Daily);
    }
}
```

#### Korak C.5: Sentry Monitoring
**Cilj:** Implementirati error tracking in performance monitoring

**Metoda:** Posodobi `Program.cs`:
```csharp
builder.Services.AddSentry(options =>
{
    options.Dsn = configuration["Sentry:Dsn"];
    options.EnableTracing = true;
    options.TracesSampleRate = 0.1;
});

// Add metrics collection
builder.Services.AddSingleton<IMetricsService, MetricsService>();
```

#### Korak C.6: JWT Authentication
**Cilj:** Implementirati authentication in authorization

**Metoda:** Ustvari `AuraFlow.Infrastructure/Authentication/`:
```csharp
public interface IAuthService
{
    Task<string> GenerateTokenAsync(User user);
    Task<ValidationResult> ValidateTokenAsync(string token);
}

public class JwtAuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    
    public async Task<string> GenerateTokenAsync(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Auth controller
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await ValidateUser(request);
        
        if (user == null)
            return Unauthorized();
        
        var token = await _auth.GenerateTokenAsync(user);
        
        return Ok(new 
        { 
            accessToken = token,
            expiresIn = "1h"
        });
    }
}
```

### Faza D: Frontend Integration (P2 - Visoko)

#### Korak D.1: Real-time Progress Display
**Cilj:** Prikazati real-time progress v OpenWebUI UI

**Metoda:** Posodobi `openwebui-plugin/src/plugin.js`:
```javascript
async handleGeneration(prompt) {
  // Create progress bar
  const progressBar = document.createElement("div");
  progressBar.className = "auraflow-progress-container";
  
  const progressBarInner = document.createElement("div");
  progressBarInner.className = "auraflow-progress-bar";
  progressBarInner.style.width = "0%";
  
  const progressText = document.createElement("span");
  progressText.className = "auraflow-progress-text";
  progressText.textContent = "0%";
  
  progressBar.appendChild(progressBarInner);
  progressBar.appendChild(progressText);
  
  // Add to UI
  const outputArea = document.querySelector(".output-area");
  if (outputArea) {
    outputArea.insertBefore(progressBar, outputArea.firstChild);
  }

  try {
    const response = await fetch("/api/v1/generate", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ prompt }),
    });

    const result = await response.json();
    
    // Poll for progress
    pollProgress(result.taskId, progressBarInner, progressText);
    
  } catch (error) {
    showError(error.message);
  }
}

async function pollProgress(taskId, barElement, textElement) {
  const interval = setInterval(async () => {
    const response = await fetch(`/api/v1/generation/progress/${taskId}`);
    const progress = await response.json();
    
    barElement.style.width = `${progress.progressPercentage}%`;
    textElement.textContent = `${progress.progressPercentage}% - ${progress.currentNode || 'Processing'}`;
    
    if (progress.state === "Completed") {
      clearInterval(interval);
      displayResult(progress);
    } else if (progress.state === "Failed") {
      clearInterval(interval);
      showError("Generation failed");
    }
  }, 1000);
}
```

#### Korak D.2: Image/Video Preview
**Cilj:** Prikazati generirane slike/videje v realnem času

**Metoda:** Dodaj v `openwebui-plugin/src/plugin.js`:
```javascript
function displayResult(result) {
  const outputArea = document.querySelector(".output-area");
  
  if (result.outputImageUrl) {
    const img = document.createElement("img");
    img.src = result.outputImageUrl;
    img.className = "auraflow-result-image";
    
    outputArea.appendChild(img);
  } else if (result.outputVideoUrl) {
    const video = document.createElement("video");
    video.src = result.outputVideoUrl;
    video.controls = true;
    video.className = "auraflow-result-video";
    
    outputArea.appendChild(video);
  }
}
```

#### Korak D.3: Model Selection Dropdown
**Cilj:** Omogočiti dinamičen izbor modelov iz API

**Metoda:** Posodobi `openwebui-plugin/src/plugin.js`:
```javascript
events.on("uiReady", async () => {
  // Load available models
  const response = await fetch("/api/v1/generation/models");
  const models = await response.json();
  
  // Create model selector
  const modelSelect = document.createElement("select");
  modelSelect.className = "auraflow-model-selector";
  
  models.forEach(model => {
    const option = document.createElement("option");
    option.value = model.id;
    option.textContent = model.name;
    modelSelect.appendChild(option);
  });
  
  // Add to UI
  const promptInput = document.querySelector(".prompt-input");
  if (promptInput) {
    promptInput.parentElement.insertBefore(modelSelect, promptInput);
  }
});
```

### Faza E: Docker & Deployment (P3 - Srednje)

#### Korak E.1: Production Dockerfile
**Cilj:** Optimiziran build za production

**Metoda:** Posodobi `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV DOTNET_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AuraFlow/AuraFlow.csproj", "src/AuraFlow/"]
RUN dotnet restore "src/AuraFlow/AuraFlow.csproj"
COPY . .
WORKDIR "/src/src/AuraFlow"
RUN dotnet build "AuraFlow.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuraFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuraFlow.dll"]
```

#### Korak E.2: Environment Configuration
**Cilj:** Omogočiti konfiguracijo preko environment variable

**Metoda:** Ustvari `config.json` in `.env.example`:
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

#### Korak E.3: Health Checks in Docker Compose
**Cilj:** Dodati health checks za monitoring

**Metoda:** Posodobi `docker-compose.yml`:
```yaml
services:
  auraflow-api:
    # ... existing config ...
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Faza F: Testing & Validation (P3 - Srednje)

#### Korak F.1: Unit Tests
**Cilj:** Zagotoviti testno pokritost >80%

**Metoda:** Ustvari `tests/AuraFlow.UnitTests/`:
```csharp
public class GenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var mockComfyClient = new Mock<ComfyClient>();
        var mockQueueService = new Mock<IQueueService>();
        
        var service = new GenerationService(
            _logger, 
            mockComfyClient.Object, 
            mockQueueService.Object);
        
        var request = new GenerationRequest { Prompt = "test", ModelName = "Flux Dev" };
        
        // Act
        var result = await service.GenerateAsync(request);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.TaskId);
    }
}
```

#### Korak F.2: Integration Tests
**Cilj:** Testirati integracijo z ComfyUI in database

**Metoda:** Ustvari `tests/AuraFlow.IntegrationTests/`:
```csharp
[Collection("Sequential")]
public class GenerationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    [Fact]
    public async Task GenerateEndpoint_ShouldReturnTaskId()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var request = new GenerationRequest 
        { 
            Prompt = "test", 
            ModelName = "Flux Dev" 
        };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/generation/generate", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Faza G: Documentation (P4 - Nizko)

#### Korak G.1: API Documentation
**Cilj:** Popolnoma dokumentirati API preko Swagger

**Metoda:** Posodobi `Program.cs`:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AuraFlow Studio API", 
        Version = "v1",
        Description = "Enterprise-grade AI Image & Video Generation Platform"
    });
    
    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

#### Korak G.2: Deployment Guide
**Cilj:** Ustvariti comprehensive deployment documentation

**Metoda:** Ustvari `docs/DEPLOYMENT_GUIDE.md`:
```markdown
# AuraFlow Studio - Deployment Guide

## Prerequisites
- Docker & Docker Compose
- .NET 8 SDK (for local development)
- SQL Server or PostgreSQL

## Quick Start
1. Clone repository: `git clone https://github.com/your-org/AuraFlow.git`
2. Configure environment variables in `.env`
3. Run: `docker-compose up -d`
4. Access UI at http://localhost:3000

## Production Deployment
See [Production Setup](./PRODUCTION_SETUP.md) for detailed instructions...
```

---

## 5. TESTING AND VALIDATION

### Merila za Uspeh:

#### Kvantitativna merila:
- ✅ **Test Coverage**: > 80% (unit + integration tests)
- ✅ **API Response Time**: < 2 sekunde (p95)
- ✅ **Uptime**: 99.9% SLA
- ✅ **Error Rate**: < 0.1%
- ✅ **Database Query Time**: < 100ms (p95)

#### Kvalitativna merila:
- ✅ **Scalability**: Podpora za 100+ hkratnih uporabnikov
- ✅ **Maintainability**: Jasna arhitektura z dokumentacijo
- ✅ **Reliability**: Fault-tolerance mechanismi delujejo
- ✅ **Observability**: Complete logging in monitoring (Sentry)
- ✅ **Deployability**: Automated CI/CD pipeline

### Validation Steps:

1. **OpenWebUI Integration Test:**
   - Open http://localhost:3000
   - Enter prompt in chat interface
   - Click "Generate (AuraFlow)" button
   - Verify progress bar updates in real-time
   - Check result image/video appears in UI

2. **API Endpoint Test:**
   ```bash
   curl -X POST http://localhost:5000/api/v1/generation/generate \
     -H "Content-Type: application/json" \
     -d '{"prompt": "test", "modelName": "Flux Dev"}'
   ```

3. **Enterprise Features Test:**
   - Verify Redis caching works (check logs)
   - Check RabbitMQ queue processing
   - Confirm Hangfire jobs execute
   - Validate Sentry error tracking

4. **Load Testing:**
   - Run 100 concurrent generation requests
   - Monitor response times and error rates
   - Verify database performance

5. **End-to-End Test:**
   - Complete flow from prompt to result
   - Check all services communicate correctly
   - Validate data persistence in database

---

## 6. IMPLEMENTATION TIMELINE

### Teden 1: Reorganizacija in OpenWebUI Integration (P1)
- [ ] Custom OpenWebUI build z brandingom
- [ ] Plugin update za AuraFlow
- [ ] Generation service implementation
- [ ] Docker Compose update z vsemi servisi

### Teden 2: Enterprise Features (P2)
- [ ] EF Core database layer
- [ ] Redis caching layer
- [ ] RabbitMQ message queue
- [ ] Hangfire background jobs
- [ ] JWT authentication

### Teden 3: Frontend & Monitoring (P2-P3)
- [ ] Real-time progress display v UI
- [ ] Image/video preview
- [ ] Model selection dropdown
- [ ] Sentry monitoring setup
- [ ] Health checks in Docker

### Teden 4: Testing & Documentation (P3-P4)
- [ ] Unit tests (80%+ coverage)
- [ ] Integration tests
- [ ] Performance testing
- [ ] API documentation
- [ ] Deployment guide

---

## 7. RISK MANAGEMENT

| Tveganje | Verjetnost | Vpliv | Rešitev |
|----------|------------|-------|---------|
| Breaking changes v OpenWebUI pluginu | Visoka | Visok | Versioning + backward compatibility |
| Performance degradation pri many users | Srednja | Visok | Caching + load balancing |
| Database bottlenecks | Srednja | Visok | Indexing + query optimization |
| External service failures (ComfyUI) | Visoka | Srednji | Circuit breaker + retry mechanism |
| Memory leaks v .NET application | Nizka | Visok | Monitoring + profiling tools |

---

## 8. SUCCESS CRITERIA

### Immediate Deliverables:
- ✅ **Single unified project**: Vsa koda v `AuraFlow/` projektu
- ✅ **Consistent branding**: Popolnoma "AuraFlow" namesto "StabilityMatrix"
- ✅ **Working OpenWebUI integration**: Chat interface z generiranjem slik/videjev
- ✅ **Enterprise features**: Redis, RabbitMQ, Hangfire, monitoring
- ✅ **Professional documentation**: Comprehensive guides in place

### Code Quality:
- ✅ No compiler warnings v Release build
- ✅ Vsi services registrirani v DI container
- ✅ XML comments na vseh public APIs
- ✅ Jasna folder organizacija

### User Experience:
- ✅ Enostaven chat-style interface
- ✅ Real-time progress tracking
- ✅ Instant image/video preview
- ✅ Model selection dropdown
- ✅ Professional enterprise look & feel

---

**Built with ❤️ using .NET 8, ASP.NET Core, Blazor, OpenWebUI, ComfyUI, Redis, RabbitMQ, in Hangfire**

### Why This Approach:
1. **Single source of truth**: All code in one place, easier to maintain
2. **Consistent branding**: Everything under "AuraFlow" name
3. **Simplified builds**: One solution, one build command
4. **Modern stack**: ASP.NET Core 8 + Blazor Server for real-time updates
5. **Professional structure**: Clear separation of concerns

### What Gets Removed:
- ❌ `src/AuraFlow.Web/` - Empty Blazor WebAssembly project
- ❌ `src/AuraFlow.Desktop/` - Empty Desktop project  
- ❌ `Tests/AuraFlow.UnitTests/` - Test project with no tests
- ❌ `AuraFlow.Api/` - Only middleware, merge into main project
- ❌ `StabilityMatrix.*` projects - Rename to AuraFlow or consolidate

## 4. IMPLEMENTATION STEPS

### Phase 1: Branding Unification (All "StabilityMatrix" → "AuraFlow")

#### Step 1.1: Rename Projects and Namespaces
**Goal:** Consistent "AuraFlow" branding throughout codebase

**Method:**
- Rename `StabilityMatrix.ChatInterface` → `AuraFlow.App`
- Rename `StabilityMatrix.Core` → Merge into main project
- Update ALL namespaces from `StabilityMatrix.*` to `AuraFlow.*`
- Update ALL class names (e.g., `ChatInterfaceClient` → `AuraFlowClient`)

**Files to update:**
```bash
# Project files
mv StabilityMatrix.ChatInterface/ AuraFlow.App/
sed -i 's/StabilityMatrix/AuraFlow/g' AuraFlow.App/*.csproj

# Source code
find . -name "*.cs" -exec sed -i 's/StabilityMatrix/AuraFlow/g' {} \;
```

#### Step 1.2: Update Solution Files
**Goal:** Single unified solution file

**Method:**
- Delete old solutions (`AuraFlow.sln`, `AuraFlow.Studio.sln`)
- Create new `AuraFlow.sln` with single project structure
- Update all project references to use "AuraFlow" prefix

### Phase 2: Dead Code Removal

#### Step 2.1: Remove Empty Projects
**Goal:** Eliminate unused directory structure

**Method:**
```bash
# Delete empty projects
rm -rf src/AuraFlow.Web/
rm -rf src/AuraFlow.Desktop/
rm -rf tests/AuraFlow.UnitTests/

# Update docker-compose.yml to remove references
sed -i '/AuraFlow.Web/d' docker-compose.yml
sed -i '/AuraFlow.Desktop/d' docker-compose.yml
```

#### Step 2.2: Consolidate AuraFlow.Api
**Goal:** Merge middleware into main project

**Method:**
- Move `RateLimitingMiddleware.cs` → `Middleware/RateLimitingMiddleware.cs`
- Move `HealthCheckMiddleware.cs` → `Middleware/HealthCheckMiddleware.cs`
- Update references in DependencyInjection.cs
- Delete empty `src/AuraFlow.Api/` directory

#### Step 2.3: Remove Redundant Core Projects  
**Goal:** Elimitate duplicate logic

**Method:**
- Merge `StabilityMatrix.Core` services into main project
- Keep only ONE copy of each service (DownloadService, etc.)
- Update all references to point to consolidated location

### Phase 3: Project Consolidation

#### Step 3.1: Create Unified Project Structure
**Goal:** Single clean project with clear organization

**Method:**
```bash
# Create new unified structure
mkdir -p AuraFlow/{Controllers,Services,Models,Pages,Infrastructure,Middlewares}

# Move all active code into appropriate folders
mv src/AuraFlow.Core/Services/* AuraFlow/Services/
mv StabilityMatrix.ChatInterface/* AuraFlow/Services/
mv src/AuraFlow.Infrastructure/* AuraFlow/Infrastructure/
```

**Final structure:**
```
AuraFlow/
├── Controllers/              # REST API endpoints
│   └── GenerationController.cs
├── Services/                 # Business logic
│   ├── DownloadService.cs
│   ├── MetadataImportService.cs
│   ├── ImageIndexService.cs
│   ├── SettingsManager.cs
│   ├── SecretsManager.cs
│   └── GenerationService.cs
├── Models/                   # Data models
│   ├── Api/                  # API request/response models
│   ├── Packages/             # Package definitions
│   ├── Settings/             # Configuration models
│   └── Progress/             # Progress tracking
├── Infrastructure/           # Infrastructure layer
│   ├── Persistence/          # LiteDB database
│   ├── Messaging/            # Message queues (optional)
│   ├── Resilience/           # Polly patterns
│   └── Engines/              # ComfyUI integration
├── Middlewares/              # HTTP middleware
│   ├── RateLimitingMiddleware.cs
│   └── HealthCheckMiddleware.cs
├── Pages/                    # Blazor Server pages
│   ├── Chat.razor
│   ├── GenerationHistory.razor
│   └── SettingsPage.razor
├── Program.cs                # Application entry point
├── AuraFlow.csproj           # Single project file
└── appsettings.json          # Configuration
```

#### Step 3.2: Create Main Entry Point (Program.cs)
**Goal:** Single application startup

**Method:**
Create `AuraFlow/Program.cs` with:
- Dependency injection setup
- Middleware pipeline configuration  
- Swagger/OpenAPI setup
- Environment-based configuration loading
- Blazor Server pages registration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddBlazorServer();
builder.Services.AddSwaggerGen();

// Register all services
builder.Services.AddScoped<IDownloadService, DownloadService>();
builder.Services.AddSingleton<ISettingsManager, SettingsManager>();
// ... etc

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
```

#### Step 3.3: Create Unified Project File
**Goal:** Single .csproj with all dependencies

**Method:**
Create `AuraFlow/AuraFlow.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>AuraFlow</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Refit" Version="7.0.0" />
    <PackageReference Include="Websocket.Client" Version="5.1.2" />
    <PackageReference Include="LiteDB" Version="5.0.21" />
    <PackageReference Include="Polly" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

### Phase 4: API Layer Creation

#### Step 4.1: Create REST Controllers
**Goal:** Proper API endpoints with documentation

**Method:**
Create `Controllers/GenerationController.cs`:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _service;
    
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerationRequest request)
    {
        var result = await _service.GenerateAsync(request);
        return Ok(result);
    }
    
    [HttpGet("progress/{taskId}")]
    public async Task<IActionResult> GetProgress(string taskId)
    {
        var progress = await _service.GetProgressAsync(taskId);
        return Ok(progress);
    }
}
```

#### Step 4.2: Add Swagger Documentation
**Goal:** Auto-generated API documentation

**Method:**
- Configure Swashbuckle in Program.cs
- Add XML comments to all controllers and models
- Enable Swagger UI at `/swagger`

### Phase 5: Frontend Integration

#### Step 5.1: Create Blazor Server Pages
**Goal:** Professional user interface

**Method:**
Create `Pages/Chat.razor`:
```razor
@page "/chat"
@using AuraFlow.Services

<PageTitle>Chat Interface</PageTitle>

<h1>🎨 AuraFlow Studio</h1>

<div class="chat-container">
    <div class="prompt-section">
        <textarea @bind="prompt" placeholder="Enter your prompt..."></textarea>
        <select @bind="selectedModel">
            <option value="Flux Dev">Flux Dev</option>
            <option value="Wan2GP">Wan2GP</option>
            <!-- etc -->
        </select>
        <button @onclick="Generate">Generate</button>
    </div>
    
    <div class="progress-section" if="@showProgress">
        <progress value="@progress" max="100"></progress>
        <p>@progressMessage</p>
    </div>
    
    <div class="result-section">
        @if (!string.IsNullOrEmpty(resultUrl))
        {
            <img src="@resultUrl" alt="Generated result" />
        }
    </div>
</div>

@code {
    private string prompt = "";
    private string selectedModel = "Flux Dev";
    private int progress;
    private bool showProgress;
    private string? resultUrl;
    
    private async Task Generate()
    {
        // Call API service
        var response = await _httpClient.PostAsJsonAsync("/api/v1/generate", new { prompt, model = selectedModel });
        // Handle response and update UI
    }
}
```

#### Step 5.2: Implement Real-time Updates (Optional)
**Goal:** Live progress feedback with SignalR

**Method:**
- Create `Hubs/GenerationHub.cs` for WebSocket communication
- Update Blazor pages to connect to hub
- Send progress updates in real-time

### Phase 6: Configuration & Docker

#### Step 6.1: Create appsettings.json
**Goal:** Clear configuration structure

```json
{
  "ChatInterface": {
    "Enabled": true,
    "DefaultModel": "Flux Dev",
    "MaxConcurrentGenerations": 3,
    "TimeoutSeconds": 120,
    "ApiBaseUrl": "http://localhost:5000"
  },
  "PreferredModels": {
    "Photos": "Flux Dev",
    "Video": "Wan2GP"
  },
  "InferenceSettings": {
    "DefaultWidth": 1024,
    "DefaultHeight": 1024,
    "Steps": 30,
    "CfgScale": 7.5,
    "Seed": -1
  }
}
```

#### Step 6.2: Update Dockerfile
**Goal:** Containerized deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AuraFlow/AuraFlow.csproj ./
RUN dotnet restore
COPY . .
WORKDIR /src/AuraFlow
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuraFlow.dll"]
```

#### Step 6.3: Update docker-compose.yml
**Goal:** Orchestrate all services

```yaml
version: '3.8'
services:
  auraflow-api:
    build: .
    ports:
      - "5000:5000"
    environment:
      - DOTNET_ENVIRONMENT=Development
      
  openwebui:
    image: ghcr.io/open-webui/open-webui:main
    ports:
      - "3000:8080"
    depends_on:
      - auraflow-api
```

## 5. TESTING AND VALIDATION

### Build Validation:
```bash
# Clean and build
dotnet clean AuraFlow/AuraFlow.csproj
dotnet build AuraFlow/AuraFlow.csproj -c Release

# Should produce NO warnings
```

### Docker Deployment Test:
```bash
# Start services
docker-compose up -d

# Verify endpoints
curl http://localhost:5000/health
curl http://localhost:3000

# Stop services
docker-compose down
```

### API Functionality Test:
```bash
# Test generation endpoint
curl -X POST http://localhost:5000/api/v1/generate \
  -H "Content-Type: application/json" \
  -d '{"prompt": "test", "modelName": "Flux Dev"}'

# Verify Swagger UI
curl http://localhost:5000/swagger
```

### Manual Testing Checklist:
- [ ] Application starts without errors
- [ ] Swagger UI accessible at `/swagger`
- [ ] Chat interface loads in browser
- [ ] Can enter prompt and select model
- [ ] Generation starts and shows progress
- [ ] Result displays correctly
- [ ] Settings persist across restarts

## 6. SUCCESS METRICS

### Immediate Deliverables:
✅ **Single project**: All code consolidated into `AuraFlow/`  
✅ **Consistent branding**: Everything uses "AuraFlow" name  
✅ **Dead code removed**: Empty projects deleted  
✅ **Professional README**: Comprehensive documentation  
✅ **Working Docker**: docker-compose.yml updated and functional  

### Code Quality:
- ✅ No compiler warnings in Release build
- ✅ All services registered in DI container
- ✅ XML comments on all public APIs
- ✅ Clear folder organization

### Developer Experience:
- ✅ Single `dotnet build` command works
- ✅ Easy to navigate codebase
- ✅ Fast iteration times (< 5 seconds)
- ✅ Clear documentation for new developers

## 7. FINAL CHECKLIST

Before marking complete, verify:

**Branding:**
- [ ] No "StabilityMatrix" references remain in code
- [ ] All namespaces use `AuraFlow.*` prefix
- [ ] All class names follow AuraFlow convention

**Structure:**
- [ ] Single project file (`AuraFlow.csproj`)
- [ ] Clear folder organization (Controllers, Services, Models, etc.)
- [ ] No empty directories remaining

**Functionality:**
- [ ] Application builds successfully
- [ ] Docker deployment works
- [ ] API endpoints functional
- [ ] Blazor pages load correctly

**Documentation:**
- [ ] Professional README.md created
- [ ] Architecture diagram included
- [ ] Configuration examples provided
- [ ] API reference documented

---

## IMPLEMENTATION ORDER

1. **Day 1**: Rename all "StabilityMatrix" → "AuraFlow", consolidate projects
2. **Day 2**: Remove dead code, create unified structure  
3. **Day 3**: Create Program.cs, controllers, Blazor pages
4. **Day 4**: Update Docker files, test deployment
5. **Day 5**: Write professional README.md, final validation

Total estimated time: **5 days** for complete refactoring

# 2. CONTEXT SUMMARY

Trenutna arhitektura projekta:
- **StabilityMatrix.Core**: Glavna logika za upravljanje modelov in inference (C#/.NET)
- **ComfyClient**: WebSocket komunikacija z ComfyUI serverjem za generiranje
- **InferenceClientBase**: Bazni razred za inference kiente
- **config.json**: Konfiguracija z nastavitvami za preferirane modele in inference

Podprti modeli:
- Fotografije: Flux Dev, SDXL Turbo, Realistic Vision, Pony Diffusion
- Video: Wan2GP, CogVideo, SVD

OpenWebUI bo služil kot uporabniški vmesnik (chat-style), Stability Matrix pa kot backend za upravljanje modelov in generiranje.

# 3. APPROACH OVERVIEW

Dodali bomo nov modul **StabilityMatrix.ChatInterface**, ki bo:
1. Zagotavljal preprost chat vmesnik z OpenWebUI
2. Komuniciral s Stability Matrix backendom preko API-ja
3. Podpiral generiranje slik in videv iz besedila
4. Omogočal napredno spremljanje napredka generiranja

Izbrana arhitektura:
- **OpenWebUI** kot frontend (Docker container)
- **StabilityMatrix.ChatInterface** kot bridge med OpenWebUI in Stability Matrix Core
- **ComfyClient** za komunikacijo z ComfyUI serverjem
- REST API za sinhrono komunikacijo

# 4. IMPLEMENTATION STEPS

## Faza 1: Nova knjižnica StabilityMatrix.ChatInterface

### Cilj: Ustvariti nov modul za chat vmesnik

**Metoda:**
1. Ustvari novo .NET library projekt `StabilityMatrix.ChatInterface`
2. Dodaj reference na `StabilityMatrix.Core` in `System.Net.Http`
3. Implementiraj osnovne modele za chat komunikacijo

**Referenca:** `/workspace/project/Diffusion/StabilityMatrix.Core/`

### Podkorak 1.1: Ustvari projektno datoteko
- Ustvari `StabilityMatrix.ChatInterface/StabilityMatrix.ChatInterface.csproj`
- Dodaj odvisnosti: `Refit`, `Websocket.Client`, `System.Text.Json`

### Podkorak 1.2: Definiraj modele za chat komunikacijo
- `ChatMessage.cs`: Model za chat sporočila (prompt, tip, status)
- `GenerationRequest.cs`: Zahteva za generiranje (prompt, model, nastavitve)
- `GenerationResponse.cs`: Odgovor z rezultatom (slika/video URL, status)
- `GenerationProgress.cs`: Napredek generiranja

### Podkorak 1.3: Implementiraj ChatInterfaceClient
- Razred `ChatInterfaceClient` ki razširja `InferenceClientBase`
- Metoda `GenerateAsync(string prompt, GenerationOptions options)`
- WebSocket komunikacija za real-time napredek
- REST API za sinhrono generiranje

## Faza 2: OpenWebUI integracija

### Cilj: Integrirati OpenWebUI kot uporabniški vmesnik

**Metoda:**
1. Ustvari Docker konfiguracijo za OpenWebUI
2. Dodaj custom plugin/extension za Stability Matrix
3. Implementiraj API endpoint za generiranje

### Podkorak 2.1: Docker konfiguracija
- Ustvari `docker-compose.yml` z OpenWebUI in Stability Matrix backendom
- Nastavi povezavo med OpenWebUI in StabilityMatrix.ChatInterface

### Podkorak 2.2: Custom OpenWebUI plugin
- Ustvari `openwebui-plugin/` mapo z custom funkcionalnostjo
- Dodaj endpoint `/api/v1/generate` za generiranje slik/video
- Implementiraj chat vmesnik z možnostjo izbire modela

### Podkorak 2.3: API integracija
- Implementiraj `IChatInterfaceApi` interface z Refit
- Metoda `PostGenerate(GenerationRequest request)`
- WebSocket povezava za napredek generiranja

## Faza 3: Backend logika za generiranje

### Cilj: Implementirati backend logiko za sinhrono generiranje

**Metoda:**
1. Ustvari `GenerationService` ki upravlja procese generiranja
2. Integriraj ComfyClient za komunikacijo z ComfyUI
3. Dodaj podporo za batch generiranje

### Podkorak 3.1: GenerationService
- Razred `GenerationService` z metodami:
  - `GenerateImageAsync(string prompt, ImageOptions options)`
  - `GenerateVideoAsync(string prompt, VideoOptions options)`
  - `GetProgressAsync(string taskId)`
  - `CancelGenerationAsync(string taskId)`

### Podkorak 3.2: ComfyUI integracija
- Ustvari workflow generator za Flux/SDXL/Wan2GP
- Implementiraj `ComfyWorkflowBuilder` za dinamično ustvarjanje workflow-ov
- Dodaj podporo za različne modele preko konfiguracije

### Podkorak 3.3: Batch generiranje
- Podpora za več generacij hkrati
- Čakanje na zaključek vseh nalog
- Povratne informacije o napredku

## Faza 4: Konfiguracija in nastavitve

### Cilj: Omogočiti konfiguracijo preko config.json

**Metoda:**
1. Razširi `config.json` z novimi nastavitvami za chat vmesnik
2. Implementiraj `IChatInterfaceSettings` interface
3. Dodaj default vrednosti za generiranje

### Podkorak 4.1: Razširi config.json
- Dodaj nastavitve:
  - `chatInterface.enabled`: Ali je chat vmesnik vklopljen
  - `chatInterface.defaultModel`: Privzeti model za generiranje
  - `chatInterface.maxConcurrentGenerations`: Število hkratnih generacij
  - `chatInterface.timeoutSeconds`: Časovna omejitev za generiranje

### Podkorak 4.2: Implementiraj nastavitve
- Ustvari `ChatInterfaceSettings` razred
- Dodaj validacijo nastavitev
- Podpora za dinamično spreminjanje nastavitev

## Faza 5: Uporabniška izkušnja

### Cilj: Zagotoviti popolnoma preprost uporabniški vmesnik

**Metoda:**
1. Ustvari preprost chat vmesnik z OpenWebUI
2. Dodaj vizualne povratne informacije o napredku
3. Omogoči shranjevanje in deljenje rezultatov

### Podkorak 5.1: Chat vmesnik
- Preprosto polje za vpis prompta
- Izbira med sliko/video
- Izbira modela (Flux, SDXL, Wan2GP, itd.)
- Gumb "Generate"

### Podkorak 5.2: Napredek generiranja
- Progress bar ali loading animation
- Real-time preview slike/video
- Obvestila o zaključku

### Podkorak 5.3: Rezultati
- Prikaz generirane slike/video
- Možnost prenosa
- Možnost ponovne uporabe prompta
- Zgodovina generacij

# 5. TESTING AND VALIDATION

## Testni scenariji

### 1. Enostavno generiranje slike
**Cilj:** Preveriti osnovno funkcionalnost
- Vpiši prompt: "kocje na sončni plaži, 4K"
- Izberi model: Flux Dev
- Klikni Generate
- Počakaj na zaključek
- Preveri ali se prikaže rezultat

**Pričakovani izid:** Slika se generira v ozadju in se prikaže v chat oknu.

### 2. Generiranje videa
**Cilj:** Preveriti video funkcionalnost
- Vpiši prompt: "avto vozi po avtocesti, sončni zahod"
- Izberi tip: Video
- Izberi model: Wan2GP
- Klikni Generate

**Pričakovani izid:** Video se generira in prikaže v chat oknu.

### 3. Batch generiranje
**Cilj:** Preveriti hkratno generiranje več slik
- Vpiši prompt
- Nastavi batch size: 4
- Klikni Generate

**Pričakovani izid:** Štiri slike se generirajo hkrati z napredkom za vsako.

### 4. Real-time napredek
**Cilj:** Preveriti prikaz napredka v realnem času
- Generiraj sliko z dolgim promptom
- Opazuj progress bar in preview slike

**Pričakovani izid:** Progress bar se posodablja, preview slike se prikažejo med generiranjem.

### 5. Prekinitev generiranja
**Cilj:** Preveriti možnost preklica
- Zaženi generiranje
- Klikni "Cancel"
- Preveri ali se ustavi

**Pričakovani izid:** Generiranje se takoj ustavi, status se posodobi.

## Validacijski kriteriji

✅ **Enostavnost**: Uporabnik vnese prompt in klikne Generate - nič drugih nastavitev  
✅ **Hitrost**: Slika se generira v razumnem času (< 30 sekund za Flux)  
✅ **Zanesljivost**: Generiranje ne crasha, pravilno obdeluje napake  
✅ **Feedback**: Uporabnik vidi napredek in rezultat  
✅ **Fleksibilnost**: Podpora za različne modele (slike in video)  

## Merila za uspeh

1. **Čas do prve generacije**: Manj kot 5 minut od namestitve do prve slike
2. **Uporabniška izkušnja**: Minimalno število klikov za generiranje (3 kliki: prompt, model, generate)
3. **Stabilnost**: 99% uspešnih generacij brez crash-ov
4. **Dokumentacija**: Jasna navodila za namestitev in uporabo

## Dodatni testi

- Test z različnimi modeli (Flux, SDXL, Wan2GP)
- Test z različnimi velikostmi slik (512x512, 1024x1024, 2048x2048)
- Test z dolgimi in kratkimi prompti
- Test obremenitve (več uporabnikov hkrati)

---

# 6. ENTERPRISE REBRANDING IN REORGANIZACIJA

## 1. OBJECTIVE

Preoblikovati obstoječi "Stability Matrix" projekt v **popolnoma zasebno, lastniško blagovno znamko** z imenom **"AuraFlow Studio"** in popolnoma preurejeno arhitekturo, kjer so vsi tuji brandi (Lykos, ComfyUI, Flux, SDXL, Wan2GP) skriti pod lastnimi imeni.

## 2. CONTEXT SUMMARY

Trenutni status projekta:
- **Ime**: Stability Matrix → **Nova blagovna znamka: AuraFlow Studio**
- **Struktura**: Ena velika knjižnica z mešanimi odgovornostmi
- **Tuji brandi v kodi**: 
  - `Lykos` (API, account management)
  - `ComfyUI` (inference client, nodes, workflows)
  - `Flux`, `SDXL`, `Wan2GP`, `CogVideo`, `SVD` ( modeli)
- **Namespacei**: `StabilityMatrix.*`, `Lykos.*`, `Comfy*.*`

**Glavni problem**: Koda še vedno razkriva tuje projekte, kar daje vtis "sestavljance" namesto lastne rešitve.

## 3. APPROACH OVERVIEW

**Nova blagovna znamka**: **AuraFlow Studio**
- **Tagline**: "Intelligent Creative Generation Platform"
- **Pozicioniranje**: Lastniška, enterprise-grade rešitev z lastno IP
- **Vizija**: Popolnoma zasebna ekosistem brez vidnih tujih referenc

**Nova arhitektura**: Modularna struktura z abstrakcijskim slojem:
```
AuraFlow.Studio/
├── src/
│   ├── AuraFlow.Core/              # Glavna logika (prekošeno iz StabilityMatrix.Core)
│   ├── AuraFlow.Domain/            # Čisti domain modeli in interfeesi
│   ├── AuraFlow.Infrastructure/    # Implementacije z abstrakcijo
│   ├── AuraFlow.Api/               # REST API layer
│   ├── AuraFlow.Web/               # Web frontend (Blazor/React)
│   ├── AuraFlow.Desktop/           # Desktop aplikacija (Avalonia)
│   └── AuraFlow.ChatInterface/     # Chat vmesnik
├── tests/
├── docker/
├── docs/
└── scripts/
```

**Abstrakcijski sloj**: Vsak tuji projekt ima lastno ime:
- `Lykos` → **AuraCloud API**
- `ComfyUI` → **FlowEngine**
- `Flux Dev` → **AuraImage X1**
- `SDXL Turbo` → **AuraImage Quick**
- `Wan2GP` → **AuraVideo Pro**
- `CogVideo` → **AuraVideo Lite**

## 4. IMPLEMENTATION STEPS

### Faza A: Popolnoma novo ime in branding

#### Cilj: Ustvariti popolnoma zasebno blagovno znamko brez tujih referenc

**Metoda:**
1. Določitev novega imena "AuraFlow Studio"
2. Posodobitev vseh datotek z novim imenom
3. Skritje vseh tujih brandov pod lastnimi imeni

### Podkorak A.1: Ime in pozicioniranje
- **Novo ime**: AuraFlow Studio
- **Tagline**: "Intelligent Creative Generation Platform"
- **Ciljna publika**: Kreativci, podjetja, data science ekipe
- **Vrednostna ponudba**: Lastniška IP, skalabilnost, zanesljivost

### Podkorak A.2: Nova imena za tuje projekte
| Originalno ime | Novo lastno ime | Opis |
|----------------|-----------------|------|
| Stability Matrix | AuraFlow Studio | Celotna platforma |
| Lykos | AuraCloud API | Cloud backend in management |
| ComfyUI | FlowEngine | Inference engine |
| Flux Dev | AuraImage X1 | Visokokakovostne slike |
| SDXL Turbo | AuraImage Quick | Hitre generacije |
| Realistic Vision | AuraPhoto Real | Fotorealistične slike |
| Pony Diffusion | AuraArt Creative | Kreativna umetnost |
| Wan2GP | AuraVideo Pro | Visokokakovostni video |
| CogVideo | AuraVideo Lite | Hiter video |
| SVD | AuraVideo Motion | Video iz slik |

### Podkorak A.3: Posodobitev datotek
- Spremeni vse reference iz "Stability Matrix" v "AuraFlow Studio"
- Spremeni `Lykos` → `AuraCloud`
- Spremeni `Comfy*` → `FlowEngine`
- Posodobi README.md, LICENSE, CONTRIBUTING.md
- Posodobi `Directory.Build.props` z novim imenom projekta

### Podkorak A.4: Nova vizualna identiteta
- Ustvari logotip "AuraFlow Studio" (modra/vijolična gradient)
- Določi barvno paleto za enterprise feel
- Ustvari template za dokumentacijo
- Pripravi marketing materiale

---

### Faza B: Abstrakcijski sloj za tuje projekte

#### Cilj: Skriti vse tuje brande pod lastnimi imeni

**Metoda:**
1. Ustvariti abstrakcijske interfeese za vsako komponento
2. Implementirati konkretne razrede z novimi imeni
3. Posodobiti vse reference v kodi

### Podkorak B.1: Nova imena namespaceov
```csharp
// Stari namespacei (vidni tuji brandi)
namespace StabilityMatrix.Core.Inference
namespace Lykos.Api
namespace ComfyUI.Models

// Novi namespacei (zasebni brandi)
namespace AuraFlow.Core.Engine
namespace AuraCloud.Api
namespace FlowEngine.Models
```

### Podkorak B.2: Abstrakcijski sloj za Inference Engine
- Ustvari `AuraFlow.Core.Engine/IInferenceEngine`
- Implementiraj `FlowEngineClient` namesto `ComfyClient`
- Preimenuj `ComfyTask` → `GenerationTask`
- Preimenuj `ComfyNode` → `WorkflowNode`
- Preimenuj `ComfyPromptRequest` → `GenerationRequest`

### Podkorak B.3: Abstrakcijski sloj za Cloud API
- Ustvari `AuraCloud.Api/IAuraCloudApi`
- Implementiraj `AuraCloudService` namesto `LykosApi`
- Preimenuj vse `Lykos*` razrede v `AuraCloud*`
- Skrij originalne API reference

### Podkorak B.4: Abstrakcijski sloj za modele
- Ustvari `AuraFlow.Domain/Models/IPromptModel`
- Implementiraj konkretne implementacije z novimi imeni:
  - `AuraImageX1Model` namesto `FluxDevModel`
  - `AuraImageQuickModel` namesto `SDXLTurboModel`
  - `AuraVideoProModel` namesto `Wan2GPModel`

---

### Faza C: Arhitekturna reorganizacija

#### Cilj: Preurediti strukturo projekta v modularno arhitekturo

**Metoda:**
1. Razdelitev monolitne knjižnice na več ločenih projektov
2. Uvedba Clean Architecture principov
3. Jasna ločitev odgovornosti med komponentami

### Podkorak C.1: Nova struktura projekta
```bash
AuraFlow.Studio/
├── src/
│   ├── AuraFlow.Domain/        # Čisti domain modeli in interfeesi
│   │   ├── Entities/           # Modeli (Model, GenerationTask, User)
│   │   ├── Interfaces/         # Domain interfeesi
│   │   ├── Enums/              # Vsi enumerations
│   │   └── ValueObjects/       # Value objects
│   ├── AuraFlow.Core/          # Core business logic
│   │   ├── Services/           # Business services
│   │   ├── Models/             # DTO modeli za API
│   │   ├── Common/             # Shared utilities
│   │   └── Extensions/         # Extension methods
│   ├── AuraFlow.Infrastructure/ # Implementacije
│   │   ├── Persistence/        # Database (EF Core, LiteDB)
│   │   ├── Engines/            # Inference engines (FlowEngine)
│   │   ├── Cloud/              # Cloud API (AuraCloud)
│   │   └── FileSystems/        # File operations
│   ├── AuraFlow.Api/           # REST API layer
│   ├── AuraFlow.Web/           # Web frontend
│   ├── AuraFlow.Desktop/       # Desktop aplikacija
│   └── AuraFlow.ChatInterface/ # Chat vmesnik
├── tests/
├── docker/
└── docs/
```

### Podkorak C.2: Domain layer
- Ustvari `AuraFlow.Domain/` z naslednjimi podmapami:
  - `Entities/` - Glavni entiteti (Model, GenerationTask, User)
  - `Interfaces/` - Domain interfeesi
  - `Enums/` - Vsi enumerations
  - `ValueObjects/` - Value objects
  - `Exceptions/` - Domain exceptions

### Podkorak C.3: Core layer
- Ustvari `AuraFlow.Core/` z naslednjimi podmapami:
  - `Services/` - Business services
  - `Models/` - DTO modeli za API
  - `Interfaces/` - Core interfeesi
  - `Common/` - Shared utilities in helpers
  - `Extensions/` - Extension methods

### Podkorak C.4: Infrastructure layer
- Ustvari `AuraFlow.Infrastructure/` z naslednjimi podmapami:
  - `Persistence/` - Database implementations (EF Core, LiteDB)
  - `Engines/` - Inference engines (FlowEngine za ComfyUI)
  - `Cloud/` - Cloud API (AuraCloud za Lykos)
  - `FileSystems/` - File system operations
  - `Caching/` - Caching implementations

### Podkorak C.5: API layer
- Ustvari `AuraFlow.Api/` kot ASP.NET Core Web API projekt
- Implementiraj REST endpoints za vse funkcionalnosti
- Dodaj Swagger/OpenAPI dokumentacijo
- Implementiraj authentication in authorization
- Dodaj rate limiting in monitoring

### Podkorak C.6: Web layer
- Ustvari `AuraFlow.Web/` kot Blazor ali React aplikacijo
- Implementiraj uporabniški vmesnik za generiranje
- Integriraj z API layerjem
- Dodaj real-time updates preko SignalR

### Podkorak C.7: Desktop layer
- Posodobi `AuraFlow.Desktop/` kot Avalonia aplikacija
- Ohrani obstoječo UI logiko
- Integriraj z novo arhitekturo
- Dodaj auto-updater funkcionalnost

---

### Faza D: Enterprise standardi

#### Cilj: Uvesti enterprise-grade standarde in best practices

**Metoda:**
1. CI/CD pipeline z GitHub Actions ali Azure DevOps
2. Comprehensive testing coverage
3. Monitoring in logging
4. API versioniranje in dokumentacija

### Podkorak D.1: CI/CD Pipeline
- Ustvari `.github/workflows/` mapo z naslednjimi workflow-i:
  - `ci.yml` - Build in test na vsakem pushu
  - `cd-release.yml` - Release deployment
  - `docker-build.yml` - Docker image build
  - `performance-test.yml` - Performance testing

### Podkorak D.2: Testing coverage
- Ustvari strukturo za teste:
  ```
  tests/
  ├── AuraFlow.UnitTests/       # Unit tests (80%+ coverage)
  ├── AuraFlow.IntegrationTests/ # Integration tests
  └── AuraFlow.EndToEndTests/   # E2E tests
  ```
- Dodaj moq setup za dependency injection
- Implementiraj testne podatke in fixtures
- Nastavi code coverage reporting

### Podkorak D.3: Monitoring in logging
- Integracija Sentry za error tracking
- NLog configuration za structured logging
- Application Insights ali podobno za telemetry
- Health check endpoints v API-ju

### Podkorak D.4: API versioniranje
- Implementiraj API versioning (v1, v2, itd.)
- Swagger dokumentacijo z version supportom
- Breaking changes management
- Deprecation strategy

### Podkorak D.5: Documentation
- Ustvari `docs/` mapo z naslednjimi podmapami:
  - `architecture/` - Arhitekturna dokumentacija
  - `api/` - API reference
  - `deployment/` - Deployment guide
  - `contributing/` - Contributing guidelines
  - `changelog/` - Changelog datoteke

---

### Faza E: Migration strategy

#### Cilj: Migrirati obstoječo kodo v novo arhitekturo z novimi imeni

**Metoda:**
1. Incrementalna migracija komponent
2. Parallel run med staro in novo strukturo
3. Comprehensive testing vsak korak

### Podkorak E.1: Faza 1 - Domain layer
- Migriraj obstoječe modele v `AuraFlow.Domain/`
- Definiraj interfeese za vse business logic
- Ustvari unit tests za domain layer
- Preimenuj vse entitete (npr. `LykosAccount` → `AuraCloudAccount`)

### Podkorak E.2: Faza 2 - Core layer
- Premesti `StabilityMatrix.Core/` logiko v `AuraFlow.Core/`
- Refactor services v clean architecture style
- Dodaj integration tests
- Preimenuj vse razrede z novimi imeni

### Podkorak E.3: Faza 3 - Infrastructure layer
- Implementiraj persistence layer (EF Core + LiteDB)
- Ustvari `FlowEngine` za ComfyUI integracijo
- Ustvari `AuraCloud` za Lykos API
- Add caching layer

### Podkorak E.4: Faza 4 - API in UI layers
- Ustvari REST API z ASP.NET Core
- Posodobi desktop aplikacijo
- Implementiraj web frontend
- Integriraj chat interface
- Preimenuj vse reference v UI

---

# 9. DODATNE DADELAVE ZA POPOLNOMA STABILNO DELOVANJE

## Trenutni status projekta - Odkrite težave:

### 🔴 Kritične pomanjkljivosti:

1. **Več različnih struktur hkrati:**
   - `AuraFlow.*` (6 projektov)
   - `DiffusionHub.*` (4 projekti)
   - `StabilityMatrix.*` (3 projekti)
   - **Težava**: Zmeda pri buildu in deploymentu

2. **Tuji brandi še vedno vidni v kodi:**
   - `StabilityMatrix.Core` namespace (ComfyClient, Lykos modeli)
   - `Lykos.*` razredi (17 datotek)
   - `Comfy*.*` razredi (4 datoteke)
   - `CivitTRPC.*` namespace (8 datotek)
   - **Težava**: Razkriva tuje projekte namesto lastne IP

3. **Tri solution datoteke:**
   - `AuraFlow.sln`
   - `DiffusionHub.sln`
   - `StabilityMatrix.sln`
   - **Težava**: Nejasnost katera je glavna

4. **Dva testna projekta:**
   - `AuraFlow.UnitTests`
   - `DiffusionHub.UnitTests`
   - **Težava**: Podvojitev testov in zmeda

### 🟡 Pomembne manjkajoče komponente:

1. **CI/CD Pipeline** (manjka `.github/workflows/`)
2. **Dokumentacija** (manjka `docs/` mapa)
3. **Monitoring & Logging** (samo osnovno NLog, manja Sentry)
4. **Health Check Endpoints** (manjkajo v API-ju)
5. **Rate Limiting** (manjka v API-ju)
6. **Authentication & Authorization** (delna implementacija)
7. **Database Migrations** (manjka EF Core migrations)
8. **Configuration Management** (manjka environment-specific configs)

### 🟢 Dodatne izboljšave za enterprise stabilnost:

1. **Caching Layer** (manjka Redis/MemoryCache)
2. **Message Queue** (manjka RabbitMQ/Azure Service Bus)
3. **Background Jobs** (manjka Hangfire/Quartz.NET)
4. **API Versioning** (manjka version management)
5. **Error Handling** (manjka global exception handler)
6. **Retry Mechanism** (delno implementiran, manja polizacija)
7. **Graceful Shutdown** (manjka za background processes)
8. **Resource Cleanup** (manjka disposal pattern)

---

## Faza F: Popolna reorganizacija in konsolidacija

### Cilj: Enotna struktura z vsemi komponentami

**Metoda:**
1. Izbrati eno glavno strukturo (AuraFlow)
2. Migrirati vse druge projekte vanjo
3. Skriti vse tuje brande
4. Dodati manjkajoče komponente

### Podkorak F.1: Konsolidacija struktur
- **Glavna solution**: `AuraFlow.Studio.sln`
- **Odstraniti**: `DiffusionHub.*`, `StabilityMatrix.*` projekte
- **Migrirati**: Vse datoteke v enotno strukturo

### Podkorak F.2: Popoln branding
```bash
# Zamenjati vse reference
find . -type f -name "*.cs" -exec sed -i 's/StabilityMatrix/AuraFlow/g' {} \;
find . -type f -name "*.cs" -exec sed -i 's/Lykos/AuraCloud/g' {} \;
find . -type f -name "*.cs" -exec sed -i 's/Comfy/FlowEngine/g' {} \;
find . -type f -name "*.cs" -exec sed -i 's/CivitTRPC/AuraMarketplace/g' {} \;
```

### Podkorak F.3: Nova struktura projektov
```bash
AuraFlow.Studio/
├── src/
│   ├── AuraFlow.Domain/              # Domain layer (čisti modeli)
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   ├── Enums/
│   │   └── ValueObjects/
│   ├── AuraFlow.Core/                # Core business logic
│   │   ├── Services/
│   │   ├── Models/
│   │   ├── Common/
│   │   └── Extensions/
│   ├── AuraFlow.Infrastructure/      # Infrastructure layer
│   │   ├── Persistence/              # EF Core + LiteDB
│   │   ├── Engines/                  # FlowEngine (ComfyUI)
│   │   ├── Cloud/                    # AuraCloud (Lykos API)
│   │   ├── Marketplace/              # AuraMarketplace (CivitTRPC)
│   │   ├── Caching/                  # Redis + MemoryCache
│   │   ├── Messaging/                # RabbitMQ/Azure Service Bus
│   │   └── BackgroundJobs/           # Hangfire
│   ├── AuraFlow.Api/                 # REST API layer
│   │   ├── Controllers/              # v1, v2 controllers
│   │   ├── Middleware/               # Error handling, logging
│   │   ├── Filters/                  # Validation, rate limiting
│   │   └── Extensions/               # DI setup
│   ├── AuraFlow.Web/                 # Web frontend (Blazor)
│   │   ├── Components/
│   │   ├── Services/
│   │   └── Shared/
│   ├── AuraFlow.Desktop/             # Desktop aplikacija (Avalonia)
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── Models/
│   │   └── Services/
│   └── AuraFlow.ChatInterface/       # Chat vmesnik
│       ├── Clients/
│       ├── Models/
│       └── Services/
├── tests/
│   ├── AuraFlow.UnitTests/           # Unit tests (80%+ coverage)
│   │   ├── Domain/
│   │   ├── Core/
│   │   └── Infrastructure/
│   ├── AuraFlow.IntegrationTests/    # Integration tests
│   │   ├── Api/
│   │   ├── Database/
│   │   └── Engines/
│   └── AuraFlow.EndToEndTests/       # E2E tests
├── docker/
│   ├── Dockerfile                    # Multi-stage build
│   ├── docker-compose.yml            # Local development
│   ├── docker-compose.prod.yml       # Production setup
│   └── k8s/                          # Kubernetes manifests
├── docs/
│   ├── architecture/                 # Arhitekturna dokumentacija
│   ├── api/                          # API reference (Swagger)
│   ├── deployment/                   # Deployment guide
│   ├── contributing/                 # Contributing guidelines
│   └── changelog/                    # Changelog
├── scripts/
│   ├── build.ps1                     # Build script (Windows)
│   ├── build.sh                      # Build script (Linux/Mac)
│   ├── deploy.sh                     # Deployment script
│   └── migrations/                   # Database migrations
└── .github/
    └── workflows/                    # CI/CD pipelines
        ├── ci.yml                    # Continuous Integration
        ├── cd-release.yml            # Release deployment
        ├── docker-build.yml          # Docker build
        ├── performance-test.yml      # Performance testing
        └── code-coverage.yml         # Coverage reporting
```

---

## Faza G: Enterprise komponente

### Cilj: Dodati vse manjkajoče enterprise komponente

**Metoda:** Implementacija robustnih sistemov za stabilnost in skalabilnost

### Podkorak G.1: CI/CD Pipeline (`.github/workflows/ci.yml`)
```yaml
name: Continuous Integration

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --verbosity minimal
    
    - name: Test
      run: dotnet test --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3

  integration-test:
    needs: build
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
    
    - name: Start Docker containers
      run: docker-compose up -d
    
    - name: Run integration tests
      run: dotnet test tests/AuraFlow.IntegrationTests --configuration Release

  deploy-staging:
    needs: [build, integration-test]
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    
    steps:
    - name: Deploy to staging
      run: ./scripts/deploy.sh staging
```

### Podkorak G.2: Database layer (EF Core)
- **Ustvari**: `AuraFlow.Infrastructure/Persistence/`
- **Implementiraj**: 
  - `ApplicationDbContext.cs` - Main database context
  - `EntityConfigurations/` - EF Core entity configurations
  - `Migrations/` - Database migrations
  - `Repositories/` - Generic repository pattern

### Podkorak G.3: Caching layer (Redis + MemoryCache)
- **Ustvari**: `AuraFlow.Infrastructure/Caching/`
- **Implementiraj**:
  - `ICacheService.cs` - Cache interface
  - `MemoryCacheService.cs` - In-memory caching
  - `RedisCacheService.cs` - Distributed caching
  - `CacheKeys.cs` - Cache key constants

### Podkorak G.4: Message Queue (RabbitMQ)
- **Ustvari**: `AuraFlow.Infrastructure/Messaging/`
- **Implementiraj**:
  - `IMessagePublisher.cs` - Message publisher interface
  - `MessagePublisher.cs` - RabbitMQ implementation
  - `BackgroundJobService.cs` - Background job processor
  - `EventHandlers/` - Event handlers for messages

### Podkorak G.5: Background Jobs (Hangfire)
- **Ustvari**: `AuraFlow.Infrastructure/BackgroundJobs/`
- **Implementiraj**:
  - `JobDefinitions.cs` - Job definitions
  - `JobProcessors.cs` - Job processors
  - `RecurringJobs.cs` - Scheduled jobs

### Podkorak G.6: API Middleware
- **Ustvari**: `AuraFlow.Api/Middleware/`
- **Implementiraj**:
  - `ExceptionHandlingMiddleware.cs` - Global exception handler
  - `RequestIdMiddleware.cs` - Request ID tracking
  - `LoggingMiddleware.cs` - Request logging
  - `RateLimitingMiddleware.cs` - Rate limiting

### Podkorak G.7: Authentication & Authorization
- **Ustvari**: `AuraFlow.Infrastructure/Authentication/`
- **Implementiraj**:
  - `JwtAuthService.cs` - JWT token generation/validation
  - `OAuthService.cs` - OAuth providers (Google, GitHub)
  - `PermissionService.cs` - Permission checking
  - `RoleService.cs` - Role management

### Podkorak G.8: Monitoring & Logging
- **Ustvari**: `AuraFlow.Infrastructure/Monitoring/`
- **Implementiraj**:
  - `SentryLoggerProvider.cs` - Sentry integration
  - `ApplicationInsightsTelemetry.cs` - Telemetry tracking
  - `HealthCheckService.cs` - Health check endpoints
  - `MetricsCollector.cs` - Performance metrics

### Podkorak G.9: Configuration Management
- **Ustvari**: `AuraFlow.Api/Configuration/`
- **Implementiraj**:
  - `AppSettings.cs` - Application settings
  - `DatabaseSettings.cs` - Database configuration
  - `RedisSettings.cs` - Redis configuration
  - `RabbitMQSettings.cs` - Message queue configuration
  - `EnvironmentConfig.cs` - Environment-specific configs

---

## Faza H: Robustnost in odpornost

### Cilj: Zagotoviti stabilno delovanje v vseh scenarijih

**Metoda:** Implementacija fault-tolerance mechanismov

### Podkorak H.1: Retry Mechanism (Polly)
- **Ustvari**: `AuraFlow.Core/Common/RetryPolicies.cs`
- **Implementiraj**:
  - `ExponentialBackoffPolicy` - Exponential backoff za API calls
  - `FixedDelayPolicy` - Fixed delay za database operations
  - `CircuitBreakerPolicy` - Circuit breaker za external services

### Podkorak H.2: Graceful Shutdown
- **Ustvari**: `AuraFlow.Api/Middleware/GracefulShutdown.cs`
- **Implementiraj**:
  - `IHostedService` implementation for cleanup
  - `CancellationToken` propagation
  - `BackgroundJob` cancellation

### Podkorak H.3: Resource Cleanup
- **Ustvari**: `AuraFlow.Core/Common/ResourcePool.cs`
- **Implementiraj**:
  - `IDisposable` pattern za vse resources
  - `ObjectPool<T>` za pooling expensive objects
  - `MemoryLeakDetector` za detection memory leaks

### Podkorak H.4: Bulk Operations
- **Ustvari**: `AuraFlow.Core/Services/BulkOperationService.cs`
- **Implementiraj**:
  - Batch processing za generacije
  - Parallel execution z throttling
  - Progress tracking za bulk operations

### Podkorak H.5: Data Validation
- **Ustvari**: `AuraFlow.Core/Common/Validators/`
- **Implementiraj**:
  - FluentValidation za API inputs
  - Custom validators za domain rules
  - Data sanitization za user inputs

---

## Faza I: Testing coverage

### Cilj: Zagotoviti visoko testno pokritost

**Metoda:** Implementacija comprehensive testing strategy

### Podkorak I.1: Unit Tests (80%+ coverage)
- **Ustvari**: `tests/AuraFlow.UnitTests/`
- **Implementiraj**:
  - Domain entity tests
  - Service layer tests
  - Repository tests
  - Validator tests
  - Coverage reporting

### Podkorak I.2: Integration Tests
- **Ustvari**: `tests/AuraFlow.IntegrationTests/`
- **Implementiraj**:
  - API endpoint tests (WebApiFact)
  - Database integration tests
  - External service mocks
  - End-to-end workflow tests

### Podkorak I.3: Performance Tests
- **Ustvari**: `tests/AuraFlow.PerformanceTests/`
- **Implementiraj**:
  - Load testing z k6/JMeter
  - Stress testing za peak loads
  - Memory profiling
  - Response time benchmarks

---

# 10. FINAL CHECKLIST ZA POPOLNO STABILNOST

## ✅ Arhitektura in struktura
- [ ] Ena glavna solution datoteka (`AuraFlow.Studio.sln`)
- [ ] Vsi projekti v enotni strukturi (AuraFlow.*)
- [ ] Odstranjeni vsi `DiffusionHub.*` in `StabilityMatrix.*` projekti
- [ ] Jasna ločitev odgovornosti med layerji

## ✅ Branding in imenovanje
- [ ] Vsi `StabilityMatrix.*` → `AuraFlow.*`
- [ ] Vsi `Lykos.*` → `AuraCloud.*`
- [ ] Vsi `Comfy*.*` → `FlowEngine.*`
- [ ] Vsi `CivitTRPC.*` → `AuraMarketplace.*`
- [ ] Vsi modeli z novimi imeni (AuraImageX1, AuraVideoPro, itd.)

## ✅ Enterprise komponente
- [ ] CI/CD pipeline (`/.github/workflows/`)
- [ ] Database migrations (EF Core)
- [ ] Caching layer (Redis + MemoryCache)
- [ ] Message queue (RabbitMQ)
- [ ] Background jobs (Hangfire)
- [ ] Monitoring & logging (Sentry + NLog)
- [ ] Health check endpoints
- [ ] Rate limiting
- [ ] Authentication & authorization

## ✅ Robustnost
- [ ] Retry mechanism (Polly)
- [ ] Circuit breaker pattern
- [ ] Graceful shutdown
- [ ] Resource cleanup
- [ ] Bulk operations
- [ ] Data validation
- [ ] Error handling

## ✅ Testing
- [ ] Unit tests (80%+ coverage)
- [ ] Integration tests
- [ ] Performance tests
- [ ] E2E tests
- [ ] Coverage reporting

## ✅ Dokumentacija
- [ ] Architecture documentation
- [ ] API reference (Swagger)
- [ ] Deployment guide
- [ ] Contributing guidelines
- [ ] Changelog

## ✅ Konfiguracije
- [ ] Environment-specific configs
- [ ] Docker Compose za development
- [ ] Kubernetes manifests za production
- [ ] Secret management
- [ ] Configuration validation

---

# 11. IMPLEMENTACIJSKI VRSTNI RED

**Teden 1: Reorganizacija in branding**
1. Konsolidacija v eno strukturo (AuraFlow)
2. Popolna zamenja imen (StabilityMatrix → AuraFlow, itd.)
3. Posodobitev solution in project files
4. Odstranitev starih projektov

**Teden 2: Enterprise komponente**
1. Implementacija CI/CD pipeline
2. Database layer z EF Core
3. Caching layer (Redis)
4. Message queue (RabbitMQ)
5. Background jobs (Hangfire)

**Teden 3: Robustnost in monitoring**
1. Retry mechanism in circuit breaker
2. Monitoring & logging (Sentry)
3. Health check endpoints
4. Rate limiting
5. Authentication & authorization

**Teden 4: Testing in dokumentacija**
1. Unit tests (80%+ coverage)
2. Integration tests
3. Performance tests
4. API documentation
5. Deployment guide

---

# 12. MERILA ZA POPOLNO STABILNOST

## Kvantitativna merila:
- ✅ **Test Coverage**: > 80%
- ✅ **API Response Time**: < 2 sekunde (p95)
- ✅ **Uptime**: 99.9% SLA
- ✅ **Error Rate**: < 0.1%
- ✅ **Database Query Time**: < 100ms (p95)

## Kvalitativna merila:
- ✅ **Scalability**: Podpora za 100+ hkratnih uporabnikov
- ✅ **Maintainability**: Jasna arhitektura z dokumentacijo
- ✅ **Reliability**: Fault-tolerance mechanismi delujejo
- ✅ **Observability**: Complete logging in monitoring
- ✅ **Deployability**: Automated CI/CD pipeline

---

# 13. RISK MANAGEMENT

## Glavni tveganja in rešitve:

| Tveganje | Verjetnost | Vpliv | Rešitev |
|----------|------------|-------|---------|
| Breaking changes v API-ju | Visoka | Visok | API versioning (v1, v2) |
| Performance degradation | Srednja | Visok | Load testing + caching |
| Database bottlenecks | Srednja | Visok | Indexing + query optimization |
| External service failures | Visoka | Srednji | Circuit breaker + retry |
| Memory leaks | Nizka | Visok | Monitoring + profiling |

---

# 14. POST-DEPLOYMENT MONITORING

## Ključni metrike za spremljanje:

### Application Metrics:
- Request count in rate
- Response time (p50, p95, p99)
- Error rate by type
- Active connections
- Memory usage

### Business Metrics:
- Generations per hour
- Average generation time
- Success rate
- User engagement
- Model usage distribution

### Infrastructure Metrics:
- CPU utilization
- Memory utilization
- Disk I/O
- Network throughput
- Database connection pool

---

# 15. MAINTENANCE SCHEDULE

## Redno vzdrževanje:

**Dnevno:**
- Preverjanje error logs
- Monitoring uptime
- Check disk space

**Tedensko:**
- Review performance metrics
- Update dependencies
- Backup verification

**Mesečno:**
- Security scan
- Database optimization
- Documentation update
- Capacity planning

---



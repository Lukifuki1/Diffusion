# AuraFlow Studio - ChatGPT Style Generator

**Popolnoma preprost vmesnik za generiranje slik in videov**

## 🎯 Kaj to dela?

- **Besedilo → Fotografije**: Generiraj slike iz opisov (Flux, SDXL)
- **Besedilo → Video**: Generiraj video iz opisov (Wan2GP, CogVideo)  
- **Chat vmesnik**: Preprosto kot ChatGPT - napiši prompt, počakaj, dobiš rezultat
- **Model po želji**: Izbereš katerikoli model za slike ali video

## 🏗️ Arhitektura

```
┌─────────────────────┐     ┌──────────────────────────┐     ┌─────────────────┐
│   Blazor Server     │◄───►│  AuraFlow API            │◄───►│  ComfyUI        │
│   (Frontend)        │     │  (.NET 8 Service)       │     │  (Backend)      │
│   Port: 5000        │     │  Port: 5000             │     │  Port: 8188     │
└─────────────────────┘     └──────────────────────────┘     └─────────────────┘
         │                           │                              │
         ▼                           ▼                              ▼
   Chat-style UI              REST API + WebSocket           Model Generation
   Prompt Input               Progress Tracking              Image/Video Output
```

## 📦 Struktura projekta

```
AuraFlow Studio/
├── AuraFlow.Core/      # Glavna logika (Inference, FlowEngine)
├── AuraFlow.Native/    # Native interop
├── AuraFlow.Native.Abstractions/  # Abstrakcije
├── config.json         # Konfiguracija modelov in nastavitev
└── README.md           # Ta datoteka
```

## 🚀 Hitri začetek

1. **Za fotografije**: Install → Stable AuraFlow Studio WebUI Forge ali FlowEngine
2. **Za video**: Install → Wan2GP ali CogVideo
3. **Skupni modeli**: Vsi paketi delijo isto mapo `./models`

## ⚙️ Konfiguracija (config.json)

```json
{
  "preferredModels": {
    "photos": "Flux Dev",      // Model za fotografije
    "video": "Wan2GP"          // Model za video
  },
  "inferenceSettings": {
    "defaultWidth": 1024,
    "defaultHeight": 1024,
    "steps": 30,
    "cfgScale": 7.5
  },
  "ChatInterface": {
    "enabled": true,              // Ali je chat vmesnik vklopljen
    "defaultModel": "Flux Dev",   // Privzeti model
    "maxConcurrentGenerations": 3,// Max hkratnih generacij
    "timeoutSeconds": 120,        // Časovna omejitev
    "apiBaseUrl": "http://localhost:5000"  // Backend URL
  }
}
```

## 💡 Uporaba

### Preprost način (ChatGPT style):

1. Odpreš AuraFlow Studio Inference UI
2. Vpišeš prompt: "kocje na sončni plaži, 4K"
3. Izbereš model: Flux Dev
4. Klikneš Generate
5. Počakaš - generiranje v ozadju z napredkom
6. Rezultat se prikaže v istem oknu

### Napreden način (FlowEngine):

```
1. Odpreš FlowEngine
2. Naloži workflow za SDXL/Flux/Wan2GP
3. Vpiši prompt in nastavitve
4. Generate z napredkom v realnem času
```

## 🎨 Podprti modeli

### Fotografije:
- **Flux Dev** - Najbolj kakovosten (priporočeno)
- **SDXL Turbo** - Najhitrejši
- **Realistic Vision** - Za fotorealizem
- **Pony AuraFlow Studio** - Anime/illustration stil

### Video:
- **Wan2GP** - Wan 2.1 video modeli (priporočeno)
- **CogVideo** - Generiranje videa iz besedila
- **SVD** - Stable Video AuraFlow Studio

# Build in testiranje

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build AuraFlow.Core/AuraFlow.Core.csproj

# Test
dotnet test AuraFlow.UnitTests
```

## 🧪 API Endpoints

### POST /api/v1/generate
Generiraj sliko ali video iz prompta.

**Request:**
```json
{
  "prompt": "kocje na sončni plaži",
  "modelName": "Flux Dev",
  "type": "Image",
  "options": {
    "width": 1024,
    "height": 1024,
    "steps": 30,
    "guidanceScale": 7.5,
    "seed": -1
  }
}
```

- [AuraFlow Studio GitHub](https://github.com/Luky Tech/AuraFlow)
- [FlowEngine](https://github.com/comfyanonymous/ComfyUI)
- [Wan2GP](https://github.com/deepbeepmeep/Wan2GP)

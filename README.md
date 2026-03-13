# DiffusionHub Enterprise - ChatGPT Style Generator ✨

**Popolnoma preprost vmesnik za generiranje slik in videov**

## 🎯 Kaj to dela?

- **Besedilo → Fotografije**: Generiraj slike iz opisov (Flux, SDXL)
- **Besedilo → Video**: Generiraj video iz opisov (Wan2GP, CogVideo)  
- **Chat vmesnik**: Preprosto kot ChatGPT - napiši prompt, počakaj, dobiš rezultat
- **Model po želji**: Izbereš katerikoli model za slike ali video

## 🏗️ Arhitektura

```
┌─────────────────────┐     ┌──────────────────────────┐     ┌─────────────────┐
│   OpenWebUI         │◄───►│  StabilityMatrix         │◄───►│  ComfyUI        │
│   (Frontend)        │     │  ChatInterface           │     │  (Backend)      │
│   Port: 3000        │     │  (.NET Service)          │     │  Port: 5000     │
└─────────────────────┘     └──────────────────────────┘     └─────────────────┘
         │                           │                              │
         ▼                           ▼                              ▼
   Chat-style UI              REST API + WebSocket           Model Generation
   Prompt Input               Progress Tracking              Image/Video Output
```

## 📦 Struktura projekta

```
Diffusion/
├── StabilityMatrix.Core/      # Glavna logika (Inference, ComfyClient)
├── StabilityMatrix.ChatInterface/  # NOVO: Chat vmesnik (.NET)
│   ├── Models/                # Modeli za komunikacijo
│   ├── Services/              # Storitve za generiranje
│   ├── Interfaces/            # API interfejsi
│   └── Options/               # Konfiguracija
├── StabilityMatrix.Native/    # Native interop
├── StabilityMatrix.Native.Abstractions/  # Abstrakcije
├── openwebui-plugin/          # Custom OpenWebUI plugin
├── docker-compose.yml         # Docker konfiguracija
├── config.json                # Konfiguracija modelov in nastavitev
└── README.md                  # Ta datoteka
```

## 🚀 Hitri začetek

### 1. Zaženi z Docker Compose

```bash
# Pojdi v mapo projekta
cd /workspace/project/Diffusion

# Zaženi vse storitve
./start.sh

# Ali ročno:
docker-compose up -d
```

### 2. Odpreš vmesnik

Odpravi brskalnik in pojdi na: **http://localhost:3000**

### 3. Generiraj sliko/video

1. Vpiši prompt (npr. "kocje na sončni plaži, 4K")
2. Izberi model (Flux Dev za slike, Wan2GP za video)
3. Klikni "Generate"
4. Počakaj na zaključek generiranja
5. Rezultat se prikaže v chat oknu

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

```
1. Odpreš OpenWebUI v brskalniku (http://localhost:3000)
2. Vpišeš prompt: "kocje na sončni plaži, 4K"
3. Izbereš model: Flux Dev
4. Klikneš Generate
5. Počakaš - generiranje v ozadju z napredkom
6. Rezultat se prikaže v istem oknu
```

### Napreden način (ComfyUI):

```
1. Odpreš ComfyUI preko DiffusionHub Enterprise
2. Naloži workflow za SDXL/Flux/Wan2GP
3. Vpiši prompt in nastavitve
4. Generate z napredkom v realnem času
```

## 🎨 Podprti modeli

### Fotografije:
- **Flux Dev** - Najbolj kakovosten (priporočeno)
- **SDXL Turbo** - Najhitrejši
- **Realistic Vision** - Za fotorealizem
- **Pony Diffusion** - Anime/illustration stil

### Video:
- **Wan2GP** - Wan 2.1 video modeli (priporočeno)
- **CogVideo** - Generiranje videa iz besedila
- **SVD** - Stable Video Diffusion

## 🔧 Build in testiranje

```bash
# Restore dependencies
dotnet restore

# Build ChatInterface projekt
cd /workspace/project/Diffusion
dotnet build StabilityMatrix.ChatInterface/StabilityMatrix.ChatInterface.csproj

# Test (če so testi dodani)
dotnet test StabilityMatrix.Tests
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

**Response:**
```json
{
  "taskId": "uuid",
  "success": true,
  "url": "/output/image.png"
}
```

### GET /api/v1/progress/{taskId}
Dobi napredek generiranja v realnem času.

### POST /api/v1/cancel/{taskId}
Prekini tekoče generiranje.

## 📊 Validacijski kriteriji

✅ **Enostavnost** - Uporabnik vnese prompt in klikne Generate  
✅ **Hitrost** - Slika se generira v < 30 sekundah za Flux  
✅ **Zanesljivost** - Generiranje ne crasha, pravilno obdeluje napake  
✅ **Feedback** - Uporabnik vidi napredek in rezultat  
✅ **Fleksibilnost** - Podpora za različne modele (slike in video)  

## 🎯 Merila za uspeh

1. **Čas do prve generacije**: Manj kot 5 minut od namestitve do prve slike
2. **Uporabniška izkušnja**: Minimalno število klikov (3: prompt, model, generate)
3. **Stabilnost**: 99% uspešnih generacij brez crash-ov
4. **Dokumentacija**: Jasna navodila za namestitev in uporabo

## 🛠️ Docker komande

```bash
# Zaženi vse storitve
docker-compose up -d

# Ustavi vse storitve
docker-compose down

# Ogled logov
docker-compose logs -f openwebui
docker-compose logs -f stabilitymatrix-backend

# Ponovni build
docker-compose build --no-cache
```

## 📝 Opombe

- **OpenWebUI** se uporablja kot frontend chat vmesnik
- **StabilityMatrix.ChatInterface** je .NET backend service z REST API in WebSocket
- **ComfyUI** služi za dejansko generiranje slik in videov
- **WebSocket** omogoča real-time napredek generiranja
- **REST API** omogoča sinhrono komunikacijo
- **Batch generiranje** podpira do 3 hkratnih generacij

## 📁 Datoteke, ki so bile ustvarjene

### Nova knjižnica StabilityMatrix.ChatInterface:
- `StabilityMatrix.ChatInterface.csproj` - Projektna datoteka
- `Models/ChatMessage.cs` - Model za chat sporočila
- `Models/GenerationRequest.cs` - Zahteva za generiranje
- `Models/GenerationResponse.cs` - Odgovor z rezultatom
- `Models/GenerationProgress.cs` - Napredek generiranja
- `Services/ChatInterfaceClient.cs` - Client za API komunikacijo
- `Services/GenerationService.cs` - Storitev za generiranje
- `Interfaces/IChatInterfaceApi.cs` - API interfejs (Refit)
- `Options/ChatInterfaceSettings.cs` - Nastavitve

### Docker in plugin:
- `docker-compose.yml` - Docker konfiguracija z dvema storitvama
- `Dockerfile` - Build datoteka za .NET service
- `openwebui-plugin/src/plugin.js` - Custom OpenWebUI plugin
- `start.sh` - Skripta za enostaven zagon

## 👥 Avtorji

Ustvarjeno kot del projekta DiffusionHub Enterprise.

## 📄 Licenca

MIT License

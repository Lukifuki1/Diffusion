# Stability Matrix - ChatGPT Style Generator

Preprost, močan sistem za generiranje slik in videov z vmesnikom kot ChatGPT.

## 🎯 Kaj to dela?

- **Text → Photos**: Generiraj slike iz opisov (Flux, SDXL)
- **Text → Video**: Generiraj video iz opisov (Wan2GP, CogVideo)
- **Chat vmesnik**: Preprosto kot ChatGPT - napiši prompt, počakaj, dobiš rezultat
- **Model po želji**: Izbereš katerikoli LLM ali diffusion model

## 📦 Minimalna struktura

```
Diffusion/
├── StabilityMatrix.Core/      # Glavna logika (Inference, ComfyClient)
├── StabilityMatrix.Native/    # Native interop
├── StabilityMatrix.Native.Abstractions/  # Abstrakcije
├── config.json               # Konfiguracija modelov in nastavitev
└── README.md                 # Ta datoteka
```

## 🚀 Namestitev paketov

1. **Za fotografije**: Install → Stable Diffusion WebUI Forge ali ComfyUI
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
  }
}
```

## 💡 Uporaba

### Preprost način (ChatGPT style):

```
1. Odpreš Stability Matrix Inference UI
2. Vpišeš prompt: "kocje na sončni plaži, 4K"
3. Izbereš model: Flux Dev
4. Klikneš Generate
5. Počakaš - generiranje v ozadju
6. Rezultat se prikaže v istem oknu
```

### Napreden način (ComfyUI):

```
1. Odpreš ComfyUI
2. Naloži workflow za SDXL/Flux/Wan2GP
3. Vpiši prompt in nastavitve
4. Generate z napredkom v realnem času
```

## 🎨 Podprti modeli

### Fotografije:
- Flux Dev (najbolj kakovosten)
- SDXL Turbo (najhitrejši)
- Realistic Vision (za fotorealizem)
- Pony Diffusion

### Video:
- Wan2GP (Wan 2.1 video modeli)
- CogVideo
- SVD (Stable Video Diffusion)

## 🔧 Build

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build StabilityMatrix.Core/StabilityMatrix.Core.csproj

# Test
dotnet test StabilityMatrix.Tests
```

## 📊 Prednosti

✅ **Enostavno** - ChatGPT style vmesnik  
✅ **Močno** - Flux, SDXL, Wan2GP podpora  
✅ **Fleksibilno** - izbereš katerikoli model  
✅ **Deljeno** - ena mapa za vse modele  
✅ **Batch** - več generacij hkrati  

## 🌐 Več informacij

- [Stability Matrix GitHub](https://github.com/LykosAI/StabilityMatrix)
- [ComfyUI](https://github.com/comfyanonymous/ComfyUI)
- [Wan2GP](https://github.com/deepbeepmeep/Wan2GP)

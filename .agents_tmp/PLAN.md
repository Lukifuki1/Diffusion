# 1. OBJECTIVE

Uvesti popolnoma preprost ChatGPT-style vmesnik za generiranje slik in videov, kjer uporabnik vnese besedilni opis (prompt) in se rezultat generira v ozadju z uporabo OpenWebUI kot vmesnika ter Stability Matrix kot backend motorja.

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



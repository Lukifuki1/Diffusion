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

# 5. TESTING AND VALIDATION (Enterprise)

## Enterprise Validacijski kriteriji

✅ **Scalability**: Podpora za 100+ hkratnih generacij  
✅ **Reliability**: 99.9% uptime SLA  
✅ **Performance**: < 2 sekunde response time za API  
✅ **Security**: OWASP Top 10 compliance  
✅ **Maintainability**: 80%+ test coverage  
✅ **Documentation**: Complete API in deployment docs  

## Merila za enterprise uspeh

1. **Time to Market**: Manj kot 4 tedne za popolno reorganizacijo
2. **Team Collaboration**: Podpora za več razvojnih ekip hkrati
3. **Deployment Flexibility**: Docker, Kubernetes, Cloud support
4. **Backward Compatibility**: API versioning z minimal breaking changes
5. **Monitoring**: Real-time dashboard za generacije in performance

## Dodatni enterprise testi

- Load testing (JMeter/Gatling) za 1000+ requestov/minuto
- Stress testing za long-running generacije
- Chaos engineering za fault tolerance
- Security penetration testing
- User acceptance testing z realnimi uporabniki

---

# 7. BRANDING CHECKLIST - Skriti tuje brande

## Obvezne spremembe za popolnoma zasebno blagovno znamko:

### ✅ Imena in namespacei
- [ ] Vsi `StabilityMatrix.*` → `AuraFlow.*`
- [ ] Vsi `Lykos.*` → `AuraCloud.*`
- [ ] Vsi `Comfy*.*` → `FlowEngine.*`
- [ ] Vsi `Flux*` → `AuraImageX1*`
- [ ] Vsi `SDXL*` → `AuraImageQuick*`
- [ ] Vsi `Wan2GP*` → `AuraVideoPro*`
- [ ] Vsi `CogVideo*` → `AuraVideoLite*`

### ✅ Datoteke in mape
- [ ] `StabilityMatrix.Core/` → `AuraFlow.Core/`
- [ ] `StabilityMatrix.Native/` → `AuraFlow.Native/`
- [ ] `Lykos/` mapo → `AuraCloud/`
- [ ] `ComfyUI/` reference → `FlowEngine/`

### ✅ Solution in project files
- [ ] `StabilityMatrix.sln` → `AuraFlow.Studio.sln`
- [ ] Vsi `.csproj` datoteke z novimi imeni
- [ ] Posodobljeni `Directory.Build.props`
- [ ] Posodobljeni `Directory.Packages.props`

### ✅ Dokumentacija
- [ ] README.md z novim imenom in brandingom
- [ ] LICENSE file z novo blagovno znamko
- [ ] CONTRIBUTING.md z novimi smernicami
- [ ] API documentation brez tujih referenc

### ✅ Koda in komentarji
- [ ] Vsi XML dokumentacijski komentarji posodobljeni
- [ ] Vsi error messages brez tujih imen
- [ ] Vsi log messages z novimi imeni
- [ ] Vsi configuration keys z novimi imeni

### ✅ Konfiguracija
- [ ] `config.json` z novimi imeni modelov
- [ ] Vsi environment variables z novimi imeni
- [ ] Docker compose datoteke z novimi imeni

---

# 8. FINAL CHECK - Ali je vse skrito?

Pred release preveri:

1. **Iskanje v kodi**: `grep -r "StabilityMatrix\|Lykos\|ComfyUI"` → Naj bo 0 rezultatov
2. **Iskanje v dokumentaciji**: Preveri README, docs, API reference
3. **Iskanje v UI**: Preveri vse uporabniške vmesnike
4. **Iskanje v logih**: Preveri error messages in logs
5. **Iskanje v konfiguracijah**: Preveri config datoteke

Če so še kakšne tuje reference, jih moraš skriti pod lastna imena!




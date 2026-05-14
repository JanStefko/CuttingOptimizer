# CUTOptimizer – Backend

⚠️ **Projekt je ve vývoji.** Funkční jádro existuje, ale aplikace zatím nepokrývá všechny zamýšlené případy užití. Některé části (autentizace, importy z dalších CAD softwarů, exporty pro různé pily a dodavatele) jsou v plánu, ne hotové.

REST API pro aplikaci **CUTOptimizer** – nástroj pro optimalizaci nářezových plánů z plošných materiálů (LTD desky, MDF, překližka apod.). Frontend je v samostatném repozitáři.

## K čemu to slouží

V truhlářské a nábytkářské výrobě se z velkých desek (typicky 2800 × 2070 mm) řeže menší dílce a olepuje hranami. 
Cílem optimalizace je rozložit dílce na desky tak, aby vzniklo co nejméně odpadua připravit náhledy k řezání. 
Dále aplikace má umožnit exporty dílců s hranami pro optimalizaci na více nářezových centrech a vytvořit konkunkurenční prostředí.
Vstupem je seznam panelů (rozměry, kusy, hrany), výstupem je rozpis desek s rozmístěním dílců a souhrn využití materiálu, popřípadě vstupní data

## Stack

- .NET 8, ASP.NET Core Web API
- Entity Framework Core 8, MS SQL Server
- Swagger (Swashbuckle) pro dokumentaci API

## Struktura

```
Controllers/        // REST endpointy
Services/           // business logika včetně optimalizace
Repositories/       // přístup k databázi
Models/
  Entities/         // databázové entity (materiály, hrany)
  DTOs/             // requesty a response
  Optimization/     // interní modely pro packing algoritmus
  Calculation/      // výstupní struktura nářezového plánu
  ImportExport/     // řádky pro budoucí import/export (CSV apod.)
Data/               // AppDbContext
Migrations/         // EF migrace
```

## Hlavní endpointy

| Endpoint                  | Metoda | Co dělá                                          |
| ------------------------- | ------ | ------------------------------------------------ |
| `/api/sheetmaterials`     | GET    | seznam dostupných desek (rozměry, tloušťka)      |
| `/api/edgebandings`       | GET    | seznam dostupných hran                           |
| `/api/optimize`           | POST   | spočítá nářezový plán pro zadané panely a desku  |

Swagger UI je dostupné na `/swagger` v development režimu.

## Spuštění lokálně

Předpoklady: .NET 8 SDK, MS SQL Server (nebo LocalDB).

```bash
# nastavit connection string v appsettings.Development.json
dotnet ef database update
dotnet run
```

API běží defaultně na `https://localhost:7xxx` (viz `launchSettings.json`).
CORS je nakonfigurován pro frontend na `http://localhost:5173`.

## Co je hotové

- CRUD pro materiály desek a hran
- Optimalizační endpoint – algoritmus pro skládání panelů na desky (First Fit s free-rectangle splitem), respektuje šířku řezu (kerf) a okrajový ořez (trim margin)
- Validace vstupů přes data anotace
- EF Core migrace, SQL Server backend

## Co je v plánu

- Refactor packing algoritmu do samostatné třídy za rozhraním (`IPackingAlgorithm`) – aby šly přidávat další strategie (MaxRects, Guillotine)
- Lepší algoritmus pro vyšší využití desky
- Globální exception handler + ProblemDetails
- Import panelů z CSV (a postupně z výstupů různých CAD softwarů – SketchUp, IMOS apod.)
- Export nářezového plánu do formátů pro konkrétní pily a dodavatele
- Autentizace a uživatelské účty
- Unit testy pro optimalizační službu

## Poznámka

Aplikace vzniká jako osobní projekt – propojení mé předchozí praxe v nábytkářské konstrukci s tím, co se učím ve vývoji webových aplikací. Kód není dokonalý a vědomě jsou v něm místa, která čekají na refactor (viz „Co je v plánu“).

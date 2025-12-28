# npm-in-blazor 📊🚀

Blazor WebAssembly playground that bundles the [`simple-statistics`](https://github.com/simple-statistics/simple-statistics) npm library with Vite and exposes it to Blazor via a small JS facade. The WASM app ships with a polished MainPage that demos every exported function from `src/index.js`.

## What’s inside
- 🧮 JS facade (`NpmInBlazor.SimpleStatistics/src/index.js`) exposing:
  - `analyzeNumbers(values: number[])`
  - `analyzeColumn(rows: object[], columnKey: string)`
  - `linearRegressionTable(rows: object[], xKey: string, yKey: string)`
- 🎨 Blazor WASM UI (`NpmInBlazor.Wasm/Pages/MainPage.razor`) with interactive cards for all the stats functions (number summary, column analysis, linear regression).
- ⚙️ Build chain: Vite builds an IIFE bundle (`window.simpleStats`), copied into the Razor Class Library static assets and served to the WASM app at `/_content/NpmInBlazor.SimpleStatistics/_generated/index.js`.
- 🧰 Tech: .NET 9, Blazor WASM, Vite, simple-statistics.

## Quick start
Prereqs: Node.js 18+ and .NET 9 SDK.

1) Build the JS bundle (happens automatically on `dotnet build`, but you can run it directly):
```bash
cd NpmInBlazor.SimpleStatistics
npm install
npm run build
```

2) Run the Blazor app:
```bash
cd ../NpmInBlazor.Wasm
dotnet restore
dotnet run
```
Open the served URL (shown in console) and head to `/` to play with the demos.

## How JS interop is wired
- The Vite build outputs `dist/index.js` named `simpleStats` (global IIFE).
- MSBuild copies the bundle to `wwwroot/_generated/index.js` inside the Razor Class Library so it is available to the WASM app as a static web asset.
- Components call JS via `IJSRuntime`, e.g.:
```csharp
await JS.InvokeAsync<StatRow[]>("simpleStats.analyzeNumbers", numbers);
```

## Project layout
- `NpmInBlazor.SimpleStatistics/` — JS wrapper + Vite build for simple-statistics.
- `NpmInBlazor.Wasm/` — Blazor WebAssembly app that consumes the bundle and shows the UI demos.

Enjoy experimenting with stats directly in Blazor! 🧑‍💻✨

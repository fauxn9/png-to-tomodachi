# Tomodachi Canvas Tool — Blazor WebAssembly

Interface web 100 % client-side pour importer/exporter les textures de
**Tomodachi Life : Living the Dream**.
Aucun serveur requis. Tout tourne dans le navigateur via WebAssembly.

---

## Structure du projet

```
TomodachiWeb/
├── TomodachiWeb.csproj          ← Projet Blazor WASM
├── Program.cs                   ← Point d'entrée Blazor
├── App.razor                    ← Router
├── _Imports.razor               ← Using globaux
├── TextureProcessorAdapter.cs   ← Pont byte[] ↔ système de fichiers virtuel
├── Pages/
│   └── Index.razor              ← Interface principale (import + export)
├── Shared/
│   └── MainLayout.razor
└── wwwroot/
    ├── index.html
    ├── css/app.css
    └── js/fileDownload.js
```

---

## Étapes d'intégration

### 1. Copie ton code source existant

Copie tous les fichiers `.cs` de ton projet `TomodachiCanvasExport`
dans ce dossier (ou un sous-dossier), **sauf son `Program.cs`**.

Puis décommente le bloc `<ItemGroup>` dans `TomodachiWeb.csproj` :

```xml
<ItemGroup>
  <Compile Include="../TomodachiCanvasExport/**/*.cs"
           Exclude="../TomodachiCanvasExport/Program.cs" />
</ItemGroup>
```

(adapte le chemin selon ta structure de dossiers)

### 2. Vérifie la compatibilité WASM des dépendances

Certaines bibliothèques natives ne fonctionnent pas dans WASM.
Vérifie les packages NuGet utilisés par `TextureProcessor` :

| Usage                | ✅ Compatible WASM       | ❌ Pas compatible WASM     |
|----------------------|--------------------------|---------------------------|
| Images               | SkiaSharp (wasm target)  | System.Drawing.Common      |
| Zstandard (.zs)      | ZstdSharp (pure .NET)    | Bindings natifs C          |
| Compression générale | System.IO.Compression    | libzstd natif              |

Si `TextureProcessor` utilise `SkiaSharp`, ajoute le package WASM :
```
dotnet add package SkiaSharp.Views.Blazor
```

### 3. Build & test en local

```bash
cd TomodachiWeb
dotnet run
# ou
dotnet watch
```

Ouvre http://localhost:5000

### 4. Build de production (déploiement statique)

```bash
dotnet publish -c Release
```

Le résultat est dans `bin/Release/net9.0/publish/wwwroot/`.
C'est un dossier de fichiers statiques — déployable sur :
- **GitHub Pages** (gratuit)
- **Netlify / Vercel** (gratuit, glisser-déposer)
- **Azure Static Web Apps**
- N'importe quel hébergeur de fichiers statiques

---

## Architecture : pourquoi ça marche sans refactoriser TextureProcessor

Blazor WASM s'exécute dans un filesystem virtuel en mémoire (`/tmp/`).
`TextureProcessorAdapter.cs` :
1. Écrit le fichier uploadé dans `/tmp/<guid>/`
2. Appelle `TextureProcessor` avec les chemins fichiers habituels
3. Lit les fichiers générés et les renvoie en `byte[]`
4. Nettoie le dossier temporaire

TextureProcessor n'a pas besoin d'être modifié.

---

## Notes

- Taille max de fichier : 20 Mo (modifiable dans `Index.razor`)
- Les fichiers ne quittent jamais le navigateur de l'utilisateur
- Compatible Chrome, Firefox, Edge, Safari (WebAssembly requis)

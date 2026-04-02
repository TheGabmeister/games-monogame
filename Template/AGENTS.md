# AGENTS.md

## Purpose

This repository is a minimal MonoGame DesktopGL template targeting `net9.0`.
Use this file as the working agreement for code agents making changes here.

## Project Layout

- `Program.cs`: application entry point; starts `Template.Game1`.
- `Game1.cs`: main game loop (`Initialize`, `LoadContent`, `Update`, `Draw`).
- `Template.csproj`: project configuration, package references, app icon, manifest.
- `Content/Content.mgcb`: MonoGame content pipeline manifest.
- `Icon.ico`, `Icon.bmp`, `app.manifest`: Windows app resources.
- `bin/`, `obj/`: generated output; do not hand-edit generated files.

## Environment

- Primary shell: PowerShell on Windows.
- Framework: `.NET 9`.
- Rendering stack: `MonoGame.Framework.DesktopGL`.
- Content pipeline tools are declared in `.config/dotnet-tools.json`.

## Working Rules

- Keep changes scoped to this repository only.
- Prefer small, targeted edits over broad refactors.
- Preserve the current project style unless the user asks for a cleanup pass.
- Do not modify generated files in `bin/` or `obj/`.
- When adding assets, register them in `Content/Content.mgcb` instead of loading loose files ad hoc.
- When gameplay code grows, prefer keeping `Game1` as the composition root and splitting larger systems into new classes.

## Common Commands

Restore local tools:

```powershell
dotnet tool restore
```

Build the project:

```powershell
dotnet build Template.csproj
```

Run the game:

```powershell
dotnet run --project Template.csproj
```

Open the content pipeline editor on Windows:

```powershell
dotnet mgcb-editor-windows Content/Content.mgcb
```

## Change Guidance

- For rendering changes, keep draw-time allocation low and avoid creating disposable graphics objects every frame.
- For input changes, preserve the existing Escape-to-exit behavior unless the user asks otherwise.
- For content changes, keep source assets under `Content/` and let the MGCB pipeline produce outputs.
- If a change needs platform-specific behavior, call it out clearly in comments or the final summary.

## Verification

After code changes, prefer this minimum verification:

1. `dotnet build Template.csproj`
2. If the change affects runtime behavior and a desktop session is available, `dotnet run --project Template.csproj`

If verification cannot be completed, state what was attempted and what blocked it.

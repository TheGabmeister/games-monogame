# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Minimal MonoGame DesktopGL template targeting .NET 9. This is a starting point for 2D games using the MonoGame framework (XNA-compatible). The namespace is `Template` and the main class is `Game1`.

## Build Commands

```bash
dotnet tool restore                    # Restore local tools (mgcb, mgcb-editor)
dotnet build Template.csproj           # Build the project
dotnet run --project Template.csproj   # Run the game
```

There are no tests. Verification is: build succeeds, and if a desktop session is available, the game runs.

## Architecture

MonoGame uses a fixed game loop with four lifecycle methods in `Game1`:

1. **Initialize()** - One-time setup before content loading
2. **LoadContent()** - Load assets (textures, fonts) via `Content.Load<T>()`
3. **Update(GameTime)** - Per-frame game logic, input handling
4. **Draw(GameTime)** - Per-frame rendering via `SpriteBatch`

`Game1` inherits from `Microsoft.Xna.Framework.Game` and acts as the composition root. As gameplay code grows, split larger systems into new classes but keep `Game1` as the orchestrator.

## Content Pipeline

Assets go in `Content/` and must be registered in `Content/Content.mgcb`. The MGCB pipeline compiles assets at build time. Do not load loose files — always use the pipeline. Edit the manifest with:

```bash
dotnet mgcb-editor-windows Content/Content.mgcb
```

## Scope

This project is the Template directory only. Do not read, reference, or modify files in sibling directories (e.g., `../Pong/`). Other projects in the parent repo are unrelated.

## Key Conventions

- Escape key exits the game — preserve this unless asked otherwise.
- Avoid per-frame allocations in Draw; reuse graphics objects.
- Do not hand-edit files in `bin/` or `obj/`.
- Platform-specific behavior should be called out in comments.
- Prefer small, targeted edits over broad refactors.

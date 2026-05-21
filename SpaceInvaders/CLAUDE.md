# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpaceInvaders is a MonoGame game project targeting .NET 9.0 with the DesktopGL backend. It is currently a fresh template — the game logic has not been implemented yet.

## Build and Run

```shell
dotnet build
dotnet run
```

Content pipeline tools (MGCB) are installed as local dotnet tools. Restore them before editing content:

```shell
dotnet tool restore
```

Edit the content pipeline with:

```shell
dotnet mgcb-editor Content/Content.mgcb
```

## Known Issue

The .csproj is named `Template.csproj` but `SpaceInvaders.slnx` and `.vscode/launch.json` reference `SpaceInvaders.csproj`. Rename the .csproj to `SpaceInvaders.csproj` before the mismatch causes build/debug failures.

## Architecture

- **Source/** — All C# game code. Entry point is `Program.cs`, main game class is `Game1.cs` (namespace `SpaceInvaders`).
- **Content/** — MonoGame content pipeline assets (configured in `Content.mgcb`, platform DesktopGL, profile Reach).
- **Assets/** — App-level build resources (icons, manifest).

## Dependencies

- `MonoGame.Framework.DesktopGL` 3.8.x
- `MonoGame.Content.Builder.Task` 3.8.x
- MGCB tools 3.8.4.1 (local dotnet tools)

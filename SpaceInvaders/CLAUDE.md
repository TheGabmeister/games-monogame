# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpaceInvaders is a modernized Space Invaders (1978) built with MonoGame and the Nez framework. Nez provides a component-based architecture (Scene → Entity → Component), built-in collision, sprite animation, particles, tweening, camera shake, and scene transitions. See SPEC.md for the full game design.

## Build and Run

```shell
dotnet build
dotnet run
```

Content pipeline tools (MGCB) are installed as local dotnet tools. Restore them before editing content:

```shell
dotnet tool restore
```

## Architecture

Nez component-based: Scenes contain Entities, Entities contain Components. Custom behavior lives in Components and SceneComponents. No ECS — components hold both data and behavior (like Unity MonoBehaviour).

- **Source/** — All C# game code. Entry point is `Program.cs`, main game class is `Game1.cs` (namespace `SpaceInvaders`).
- **Source/Scenes/** — Nez Scene subclasses (MainMenuScene, GameplayScene, GameOverScene).
- **Source/Components/** — Custom Nez Components attached to entities (FormationController, PlayerController, BulletController, etc.).
- **Source/SceneComponents/** — Scene-level managers (WaveManager, BassRhythm, GameState).
- **Content/** — MonoGame content pipeline assets (configured in `Content.mgcb`, platform DesktopGL, profile Reach).
- **Assets/** — App-level build resources (icons, manifest) and editable source assets (SVGs) under `Assets/source/`.

## Nez Framework

Nez source is at `D:\Nez`. It is referenced as a project dependency, not a NuGet package. Key patterns:

- Collision uses `BoxCollider` + physics layers + `ProjectileMover` + `ITriggerListener` callbacks.
- `SpriteRenderer` / `SpriteAnimator` for all drawing and animation.
- `CameraShake` component for screen shake.
- `ParticleEmitter` for particle effects.
- `SceneResolutionPolicy.ShowAll` for 960x720 virtual resolution with letterboxing.
- Scene transitions (fade) for moving between scenes.

## Dependencies

- `MonoGame.Framework.DesktopGL` 3.8.x
- `MonoGame.Content.Builder.Task` 3.8.x
- Nez framework (project reference from `D:\Nez`)
- MGCB tools 3.8.4.1 (local dotnet tools)

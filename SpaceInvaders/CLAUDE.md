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
- **Source/Scenes/** — Nez Scene subclasses (MainMenuScene, GameplayScene).
- **Source/Components/** — Custom Nez Components attached to entities (FormationController, PlayerController, BulletController, etc.).
- **Source/SceneComponents/** — Scene-level managers (WaveManager, BassRhythm, GameState). SceneComponents update before entities each frame.
- **Source/Assets.cs** — Centralized static asset loader. All textures and sounds are loaded once via `Assets.Load(content)` and accessed as static properties (e.g., `Assets.Cannon`, `Assets.Shoot`). Never scatter `LoadTexture`/`LoadSoundEffect` calls in other files.
- **Source/Constants.cs** — All tuning values, physics layer definitions, tag constants, and enums.
- **Content/** — Runtime assets (PNGs, WAVs, compiled effects). Copied to output via .csproj globs.
- **Assets/** — App-level build resources (icons, manifest) and editable source SVGs under `Assets/source/`.

## Nez Framework

Nez source is at `D:\Nez`. It is referenced as a project dependency, not a NuGet package. Key patterns:

- Collision uses `BoxCollider` + physics layers (bitmasks) + `ProjectileMover` + `ITriggerListener` callbacks.
- Manual overlap checks via `Physics.BoxcastBroadphase(bounds, layerMask)`.
- `SpriteRenderer` (in `Nez.Sprites` namespace) for drawing. `SpriteAnimator` for frame animation.
- `NezContentManager` (in `Nez.Systems` namespace) for loading textures/sounds.
- `SceneResolutionPolicy.ShowAll` for 960×720 virtual resolution with letterboxing.
- Scene transitions via `Core.StartSceneTransition(new FadeTransition(...))`.
- Formation hierarchy uses `Transform.SetParent` — moving the parent entity moves all child invaders.

## Gotchas

- Use `System.Random`, not `Nez.Random` (ambiguous namespace conflict).
- Use `(float)System.Math.Pow()` — `MathF.Pow` may not resolve in all target configs.
- Sprites are exported at 3× size from SVG for detail, then scaled down at runtime via `Entity.Transform.SetScale()`. Don't change PNG dimensions without adjusting scale constants.
- `LoadTexture` requires `premultiplyAlpha: true` for correct rendering.
- `LoadSoundEffect` loads WAV files directly (no content pipeline processing needed).

## Dependencies

- `MonoGame.Framework.DesktopGL` 3.8.x
- `MonoGame.Content.Builder.Task` 3.8.x
- Nez framework (project reference from `D:\Nez\Nez.Portable\Nez.MG38.csproj`)
- MGCB tools 3.8.4.1 (local dotnet tools)

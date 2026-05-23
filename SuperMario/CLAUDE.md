# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
dotnet build SuperMario.slnx -v minimal
dotnet run --project SuperMario.csproj
```

A clean build is **0 warnings, 0 errors**. Treat any warning as something to fix.

## Nez Dependency

Nez is a **local project reference**, not a NuGet package or Git submodule:

```xml
<ProjectReference Include="D:\Nez\Nez.Portable\Nez.MG38.csproj" />
```

This project uses MonoGame 3.8, so always use `Nez.MG38.csproj` (not the .NET 6 / MG39 variant). Nez source lives at `D:\Nez` — read it directly when investigating Nez behavior. Do not vendor Nez source into this repo.

## Architecture

The flow is **`Game1` → `GameManager` (GlobalManager) → `GameplayScene` → `EntityFactory`**.

- **`Game1`** boots Nez and registers `GameManager` as a Nez `GlobalManager`. It does nothing else — game orchestration lives in `GameManager`.
- **`GameManager`** owns the `GameState` instance and handles scene transitions. It subscribes to `GameplayScene.LevelCompleted` / `GameOver` and decides what to load next. Persists across scene swaps automatically because it's a `GlobalManager`.
- **`GameState`** holds data that survives scene transitions: `Score`, `Lives`, `PowerState`. Passed into each `GameplayScene` constructor. `PlayerController` writes back to it directly (e.g. on `GrowPlayer`) so progression survives the next scene load.
- **`GameplayScene`** is constructed with a level path and a `GameState`. It loads a `.tmx` map, iterates the `entities` object group, and asks `EntityFactory` to spawn each object. Player respawning and game-over detection live here; the actual player visual state restore is done in `PlayerController.SetGameState`.
- **`EntityFactory`** is a registry (`Dictionary<string, Action<Scene, TmxObject>>`) mapping Tiled object **Class** strings to spawn functions. Add a new entity type by writing a `CreateFoo(Scene, TmxObject)` method and calling `Register("Foo", CreateFoo)` in the constructor. **Do not add a switch statement in the scene** — the whole point of the registry is to avoid that.

### Level loading (Tiled)

Levels are authored in **Tiled** (`.tmx` files in `Content/`). Conventions:

- One object layer named `entities`. Object **Class** (PascalCase) maps to a factory registration. **Name** is optional and used for entity names.
- The csproj has a glob `<Content Include="Content\**\*.tmx" CopyToOutputDirectory="PreserveNewest" />` — new `.tmx` files copy automatically. Add the path as a constant in `Assets.Maps`.
- Use `Content.LoadTiledMap(Assets.Maps.X)` to load; do not hardcode paths in scenes.
- `PlayerStart` is spawned like any other object — it creates a marker entity tagged `Tags.PlayerStart`. `GameplayScene.SpawnPlayer` finds it via `FindEntitiesWithTag` and throws if missing. The player itself is created in code by `EntityFactory.CreatePlayer` so it can be respawned without re-parsing the map.
- Tiled rotation: objects rotate around their **top-left corner**, not center. `EntityFactory.GetCenter` handles the math.

### Physics layers

Defined in `Constants.cs` as bit positions (Player, Enemy, Item, Environment). Set on every collider — `PhysicsLayer = 1 << PhysicsLayers.X`, `CollidesWithLayers = (1 << ...) | (1 << ...)`. Triggers use the same layer system; they also need `collider.IsTrigger = true`.

## Critical Nez Patterns

- **Input:** Use `VirtualButton` / `VirtualIntegerAxis` fields in components. Call `Deregister()` in `OnRemovedFromEntity()`. Raw `Nez.Input` is for one-off demos only.
- **Triggers:** Implement `ITriggerListener.OnTriggerEnter/OnTriggerExit` rather than polling with `Physics.BoxcastBroadphase` in `Update`. Nez `Mover` invokes these automatically.
- **Do NOT remove and re-add a collider during a trigger callback.** Nez's `ColliderTriggerHelper` retains a reference to the old collider for exit tracking; after `RemoveComponent<BoxCollider>()` the collider's `.Entity` becomes null, and the next frame crashes with `NullReferenceException` in `ColliderTriggerHelper.NotifyTriggerListeners`. To resize, use `BoxCollider.SetSize(w, h)` and `SetLocalOffset(...)` in-place. Same applies to destroying entities — destroying the *other* collider's entity during `OnTriggerEnter` is fine (Nez null-checks it), but destroying *this* one or its collider is not.

## Events vs Direct References

The codebase mixes two patterns intentionally:

- **Nez `Emitter<T>`** (`Events.Emitter` with `GameEvents` enum) for cross-cutting game-wide events that anything can subscribe to. Currently unused — the enum is empty. Reach for it only when no direct reference exists.
- **C# `event Action`** on a specific component (e.g. `PlayerController.OnDied`) when one specific subscriber owns the reference. Preferred when the caller naturally has a reference to the source — don't route through the global emitter just because.

## Layout

- `Source/Game1.cs` — boots Nez, registers `GameManager`.
- `Source/GameManager.cs` — `GlobalManager` owning `GameState` and scene transitions.
- `Source/GameState.cs` — persistent data across scenes.
- `Source/Scenes/GameplayScene.cs` — gameplay scene; loads a level + spawns entities.
- `Source/EntityFactory.cs` — Tiled-object → entity spawn registry.
- `Source/Components/` — Nez components (PlayerController, Mushroom, KillVolume).
- `Source/Constants.cs` — `PhysicsLayers`, `Tags`, `Constants`, `PlayerState` enum.
- `Source/Assets.cs` — content path constants (`Maps`, `Sprites`, `Sfx`, `Music`).
- `Source/Events.cs` — `GameEvents` enum + global `Emitter`.
- `Content/` — `.tmx` map files and (eventually) `.mgcb` pipeline content.
- `Assets/` — app icon and Windows manifest.
- `NezGuide.md` — Nez usage notes; consult before reinventing patterns.

## Common Pitfalls

- Object order in a Tiled map is **not guaranteed** to match what you'd expect. Don't rely on iteration order for dependency wiring — spawn marker entities, then resolve dependencies in a second pass (`GameplayScene` does this for the player).
- Generic `Content.LoadTiledMap` doesn't go through the MGCB pipeline — it reads the raw `.tmx` from the output directory, which is why the csproj has the explicit `Content\**\*.tmx` copy glob.
- `BoxCollider(x, y, w, h)` takes a top-left offset and stores it as a center offset internally. To match a sprite drawn from the entity center, pass `(-w/2, -h/2, w, h)`.

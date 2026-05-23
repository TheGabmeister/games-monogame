# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Collaboration Rule

Do not change files, assets, environment variables, registry values, PATH, config, or project state unless the user explicitly asks for a change with words like "do it", "make the change", "implement it", "fix it", "add it", "update it", or "revert it". For questions such as "how do we", "why", "what should we", or "let's plan", answer only.

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

This project uses MonoGame 3.8, so always use `Nez.MG38.csproj` (not the .NET 6 / MG39 variant). Nez source lives at `D:\Nez`; read it directly when investigating Nez behavior. Do not vendor Nez source into this repo.

## Architecture

The flow is **`Game1` -> `GameManager` (GlobalManager) -> `GameplayScene` -> `EntityFactory`**.

- **`Game1`** boots Nez and registers `GameManager` as a Nez `GlobalManager`. It does nothing else; game orchestration lives in `GameManager`.
- **`GameManager`** owns the `GameState` instance and handles scene transitions. It starts with `Levels.Debug`, subscribes to `GameplayScene.LevelCompleted` / `GameOver`, and decides what to load next. It persists across scene swaps automatically because it is a `GlobalManager`.
- **`GameState`** holds data that survives scene transitions: `Score`, `Lives`, `PowerState`. It is passed into each `GameplayScene` constructor. `PlayerController` writes back to it directly, for example on `GrowPlayer`, so progression survives the next scene load.
- **`Levels` / `LevelDefinition`** define level metadata: `Name`, `MapPath`, `MusicPath`, and `TimerSeconds`. Level map/music paths come from `Assets`.
- **`GameplayScene`** is constructed with a `LevelDefinition` and a `GameState`. It loads the level's `.tmx` map, iterates the `entities` object group, and asks `EntityFactory` to spawn each object. Player respawning and game-over detection live here; the actual player visual state restore is done in `PlayerController.SetGameState`. It also creates the HUD with `HudController`.
- **`EntityFactory`** is a registry (`Dictionary<string, Action<Scene, TmxObject>>`) mapping Tiled object **Class** strings to spawn functions. Add a new entity type by writing a `CreateFoo(Scene, TmxObject)` method and calling `Register("Foo", CreateFoo)` in the constructor. **Do not add a switch statement in the scene**; the whole point of the registry is to avoid that.

### Level Loading (Tiled)

Levels are authored in **Tiled** (`.tmx` files in `Content/Levels/`). Conventions:

- One object layer named `entities`. Object **Class** (PascalCase) maps to a factory registration. **Name** is optional and used for entity names.
- The csproj has globs for raw TMX and OGG content:

```xml
<Content Include="Content\**\*.tmx" CopyToOutputDirectory="PreserveNewest" />
<Content Include="Content\**\*.ogg" CopyToOutputDirectory="PreserveNewest" />
```

- Add map paths in `Assets.Maps`, music paths in `Assets.Music`, and playable level metadata in `Levels.cs`.
- Use `Content.LoadTiledMap(_level.MapPath)`; do not hardcode paths in scenes.
- `PlayerStart` is spawned like any other object, but it creates a marker entity with a `PlayerStart` component. `GameplayScene.SpawnPlayer` uses `FindComponentOfType<PlayerStart>()` and throws if missing. The player itself is created in code by `EntityFactory.CreatePlayer` so it can be respawned without re-parsing the map.
- Tiled rotation: objects rotate around their **top-left corner**, not center. `EntityFactory.GetCenter` handles the math.

Current factory Class registrations:

- `PlayerStart`
- `Platform`
- `Mushroom`
- `FireFlower`
- `OneUp`
- `GoalTrigger`
- `KillVolume`

### Music

Music files are raw `.ogg` assets in `Content/Music/`:

- `level_bounce.ogg`
- `level_cavern.ogg`
- `level_sky.ogg`

They are referenced by `Assets.Music` and assigned per level in `Levels.cs`. `generate_level_music.ps1` and the `.mid`/`.wav` sources are regeneration artifacts; keep them beside the exported OGGs.

### Physics Layers

Defined in `Constants.cs` as bit positions (`Player`, `Enemy`, `Item`, `Environment`). Set on every collider: `PhysicsLayer = 1 << PhysicsLayers.X`, `CollidesWithLayers = (1 << ...) | (1 << ...)`. Triggers use the same layer system; they also need `collider.IsTrigger = true`.

### Render Layers

`RenderLayers.World` and `RenderLayers.Hud` are defined in `Constants.cs`. `GameplayScene` uses `RenderLayerExcludeRenderer` for world rendering and `ScreenSpaceRenderer` for HUD.

## Critical Nez Patterns

- **Input:** Use `VirtualButton` / `VirtualIntegerAxis` fields in components. Call `Deregister()` in `OnRemovedFromEntity()`. Raw `Nez.Input` is for one-off demos only.
- **Triggers:** Implement `ITriggerListener.OnTriggerEnter/OnTriggerExit` rather than polling with `Physics.BoxcastBroadphase` in `Update`. Nez `Mover` invokes these automatically.
- **Do NOT remove and re-add a collider during a trigger callback.** Nez's `ColliderTriggerHelper` retains a reference to the old collider for exit tracking; after `RemoveComponent<BoxCollider>()` the collider's `.Entity` becomes null, and the next frame crashes with `NullReferenceException` in `ColliderTriggerHelper.NotifyTriggerListeners`. To resize, use `BoxCollider.SetSize(w, h)` and `SetLocalOffset(...)` in-place. Same applies to destroying entities: destroying the *other* collider's entity during `OnTriggerEnter` is fine because Nez null-checks it, but destroying this one or its collider is not.
- **Queued entities:** Entities created in `Scene.OnStart()` are not in tag lookup lists until `Entities.UpdateLists()` runs. Do not use `FindEntitiesWithTag` for markers created earlier in the same `OnStart()`. Use marker components and `FindComponentOfType<T>()`, which checks queued entities too.
- **Tags:** Nez entities default to `Tag == 0`; project tags should start at `1`.

## Events vs Direct References

The codebase mixes two patterns intentionally:

- **Nez `Emitter<T>`** (`Events.Emitter` with `GameEvents` enum) for cross-cutting game-wide events that anything can subscribe to. Currently unused; the enum is empty. Reach for it only when no direct reference exists.
- **C# `event Action`** on a specific component, for example `PlayerController.OnDied`, when one specific subscriber owns the reference. Preferred when the caller naturally has a reference to the source; do not route through the global emitter just because.

## Layout

- `Source/Game1.cs` - boots Nez, registers `GameManager`.
- `Source/GameManager.cs` - `GlobalManager` owning `GameState` and scene transitions.
- `Source/GameState.cs` - persistent data across scenes.
- `Source/Levels.cs` - `LevelDefinition` metadata and level list.
- `Source/Scenes/GameplayScene.cs` - gameplay scene; loads a level, spawns entities, creates HUD, handles respawn/completion.
- `Source/EntityFactory.cs` - Tiled-object -> entity spawn registry.
- `Source/Components/` - Nez components (`PlayerController`, `GravityBody`, pickups, triggers, HUD, marker components).
- `Source/Constants.cs` - `PhysicsLayers`, `RenderLayers`, `Tags`, `Constants`, `PlayerState` enum.
- `Source/Assets.cs` - content path constants (`Maps`, `Sprites`, `Sfx`, `Music`).
- `Source/Events.cs` - `GameEvents` enum + global `Emitter`.
- `Content/Levels/` - `.tmx` map files.
- `Content/Music/` - generated `.ogg` loops plus regeneration sources.
- `Content/Content.mgcb` - MonoGame content pipeline file.
- `Assets/` - app icon and Windows manifest.
- `NezGuide.md` - Nez usage notes; consult before reinventing patterns.

## Common Pitfalls

- Object order in a Tiled map is **not guaranteed** to match what you would expect. Do not rely on iteration order for dependency wiring.
- Generic `Content.LoadTiledMap` does not go through the MGCB pipeline; it reads the raw `.tmx` from the output directory, which is why the csproj has the explicit `Content\**\*.tmx` copy glob.
- `BoxCollider(x, y, w, h)` takes a top-left offset and stores it as a center offset internally. To match a sprite drawn from the entity center, pass `(-w/2, -h/2, w, h)`.

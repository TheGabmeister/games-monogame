# AGENTS.md

Guidance for coding agents working in this repository.

## Project Overview

A bullet-hell shoot-'em-up inspired by *Strikers 1945*, built on MonoGame DesktopGL
(.NET 9) + MonoGame.Extended.ECS as an ECS learning project. See `PLAN.md` for the design,
locked decisions, phase roadmap (with per-phase sprite wiring), and the asset/SFX lists.

- Entry point: `Program.cs`
- Main game class: `Game1.cs` — composes the ECS world + wires non-ECS services
- Project file: `Extended.csproj`
- Content pipeline file: `Content/Content.mgcb`
- MonoGame.Extended pipeline DLLs: `pipeline-references/`

## Build and Run

Use:

```powershell
dotnet run
```

If the local MonoGame tools are missing, restore them first:

```powershell
dotnet tool restore
```

The project uses MonoGame's content builder task, so `dotnet run` also invokes `dotnet mgcb` against `Content/Content.mgcb`.

## Content Pipeline Notes

Keep this reference in `Content/Content.mgcb`:

```text
/reference:..\pipeline-references\MonoGame.Extended.Content.Pipeline.dll
```

Do not replace it with a deep relative path into the user's NuGet cache such as `..\..\..\..\Users\...\.nuget\packages\...`. `mgcb` can misread that form as content to build and fail with a vague `MSB3073` error.

When adding game assets, add them through `Content/Content.mgcb` so they are built into `.xnb` files and loaded through `Content.Load<T>()`.

## ECS (MonoGame.Extended.ECS)

Namespaces are `MonoGame.Extended.ECS` (`World`, `WorldBuilder`, `Entity`, `Aspect`) and `MonoGame.Extended.ECS.Systems` — **not** `MonoGame.Extended.Entities`. System base classes: `EntityProcessingSystem` (per-entity `Process`), `EntityUpdateSystem` (one `Update`, loop `ActiveEntities` yourself), `EntityDrawSystem`.

Local code layout: components (plain data classes) in `Components/`; systems in `Source/Systems/`; non-ECS helpers/services in `Source/`.

### Project conventions

- **Components are data only** — fields plus a constructor/simple init. No game logic, no calls into other systems.
- **`EntityFactory` (`Source/EntityFactory.cs`) builds all entities.** Systems call `factory.Create*(...)`; never `Attach` components ad hoc inside a system. It loads/caches textures from the `ContentManager`.
- **Factory ↔ system wiring gotcha:** `EntityFactory` needs the built `World`, which doesn't exist until `WorldBuilder.Build()`. Systems that spawn entities (e.g. `WeaponSystem`) therefore can't take the factory as a constructor arg — `Game1` builds the world, then sets the factory via a public field on those systems. Follow that pattern for new spawning systems.
- **Use ECS where it makes sense (PLAN.md §6).** Per-entity gameplay → components/systems. Global/singleton state → plain classes injected into systems: e.g. `AudioManager` (`Source/AudioManager.cs`) for one-shot SFX, future wave timeline / score. Don't force those into the ECS.
- **Frame-rate independence:** scale motion by `gameTime` delta seconds (`gameTime.GetElapsedSeconds()` from `MonoGame.Extended`). Never move by a fixed per-frame amount.
- **Virtual resolution:** all gameplay math is in the 600×800 portrait space defined by `VirtualResolution` (`Source/VirtualResolution.cs`) — use those constants for bounds/spawns, not `Viewport`.
- **Asset naming is lowercase snake_case** (files and folders), because `Content.Load<T>` paths are strings and DesktopGL can run on case-sensitive filesystems. Content is organized by type under `Content/` (`sprites/`, `audio/`, etc.); load paths mirror the folders without extension (e.g. `Content.Load<SoundEffect>("audio/sfx/sfx_player_shot")`).

Reference samples on this machine at `D:\MonoGame-Extended-Samples` (clone of the official samples repo):

- `Demos/Sandbox` — simplest; the rain demo. Best starting point.
- `Games/Platformer` — `EntityFactory` pattern, tile collisions.
- `Games/StarWarrior` — full game; 8 components / 10 systems. The reference architecture.

`Games/SpaceGame` has an `Entities/` folder but does **not** use the ECS framework — don't cite it as an ECS example.

## Coding Style

- Keep gameplay logic in ECS systems, not `Game1.cs`. `Game1` only sets up the world and wires services; new behavior is a system, new data is a component.
- Prefer MonoGame and MonoGame.Extended APIs already referenced by the project.
- Keep content paths relative to `Content.RootDirectory`, which is set to `Content`.
- Avoid unrelated formatting churn in generated or tool-managed files.

## Troubleshooting

If `dotnet run` fails with:

```text
MonoGame.Content.Builder.Task.targets(...): error MSB3073
```

run the `mgcb` command without `/quiet` to reveal the real content pipeline error. The MSBuild error is usually only a wrapper around the underlying `mgcb` failure.

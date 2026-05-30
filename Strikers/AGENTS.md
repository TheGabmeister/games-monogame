# AGENTS.md

Guidance for coding agents working in this repository.

## Project Overview

A bullet-hell shoot-'em-up inspired by *Strikers 1945*, built on MonoGame DesktopGL
(.NET 9) + MonoGame.Extended.ECS as an ECS learning project. See `PLAN.md` for the design,
locked decisions, phase roadmap (with per-phase sprite wiring), and the asset/SFX lists.

Phases 0–4 are done (foundation, player shooting, enemies/collision/damage, bullet-hell
emitters + respawn i-frames, and the full stage/arcade loop: scrolling background, wave
timeline → STAGE CLEAR, score/lives/bomb HUD, bomb, power-up drops, lives/game-over +
restart). **Phase 5 is next:** title/game-over/pause screens, explosion particles,
SFX/music hooks, difficulty tuning, banking frames (PLAN.md §5).

- Entry point: `Program.cs`
- Main game class: `Game1.cs` — composes the ECS world + wires non-ECS services
- Project file: `Strikers.csproj`
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

**Animation JSON is the exception.** Files under `Content/animations/*.json` are *raw-copied*, not built — register each with `/copy:animations/foo.json` in `Content.mgcb` (build action Copy), and load them by reading the file off disk (`AnimationLibrary` enumerates the folder under `AppContext.BaseDirectory` with `Directory`/`File` + `System.Text.Json`), **not** `Content.Load<T>`. The sprite sheets the JSON references are normal pipeline textures. Adding such a JSON as a regular `/build:` item makes `mgcb` try to process it and fail.

## ECS (MonoGame.Extended.ECS)

Namespaces are `MonoGame.Extended.ECS` (`World`, `WorldBuilder`, `Entity`, `Aspect`) and `MonoGame.Extended.ECS.Systems` — **not** `MonoGame.Extended.Entities`. System base classes: `EntityProcessingSystem` (per-entity `Process`), `EntityUpdateSystem` (one `Update`, loop `ActiveEntities` yourself), `EntityDrawSystem`.

Local code layout: components (plain data classes) in `Components/`; systems in `Source/Systems/`; non-ECS helpers/services in `Source/`.

### Project conventions

- **Components are data only** — fields plus a constructor/simple init. No game logic, no calls into other systems.
- **`EntityFactory` (`Source/EntityFactory.cs`) builds all entities.** Systems call `factory.Create*(...)`; never `Attach` components ad hoc inside a system. It loads/caches textures from the `ContentManager`.
- **Factory ↔ system wiring gotcha:** `EntityFactory` needs the built `World`, which doesn't exist until `WorldBuilder.Build()`. Systems that spawn or react (`WeaponSystem`, `EnemySpawnSystem`, `EmitterSystem`, `DamageSystem`) therefore can't take the factory/services as constructor args — `Game1` builds the world, then sets them via public fields. Follow that pattern for new spawning systems. (System update order is set by `AddSystem` order in `Game1`; see PLAN.md §3.)
- **Use ECS where it makes sense (PLAN.md §6).** Per-entity gameplay → components/systems. Global/singleton state → plain classes in `Source/`, injected into systems: `AudioManager` (one-shot SFX), `AnimationLibrary` (shared animation clips), `PlayerTracker` (the player's position, for systems that aim at it), `GameState` (arcade flow phase + global score) and `Stage` (the wave timeline `EnemySpawnSystem` plays back). Don't force those into the ECS. Gameplay systems gate on `GameState.Phase == GamePhase.Playing` so the world freezes on STAGE CLEAR / GAME OVER; `Game1.NewGame()` rebuilds the world to (re)start a run.
- **Collision = layer/mask (PLAN.md §4).** Colliders are `CircleCollider { Radius, Layer, Mask }` over the `CollisionLayer` `[Flags]` enum. `CollisionSystem` only tests a pair when their layers mask each other, and *applies* damage to `Health`; `DamageSystem` reaps anything at 0 HP (enemy → explosion + destroy; player → explosion + respawn + i-frames). Set a collider's layer/mask in `EntityFactory`, never ad hoc.
- **Animation + bullet patterns are data, not code.** Animations: a `Clip` is defined in `Content/animations/*.json`, loaded once by `AnimationLibrary`; an entity plays one via the `Animator` component and `AnimationSystem` writes the current frame into `Sprite.SourceRect`. Enemy fire: an `Emitter` component holds the pattern/speed/count/spin as fields and `EmitterSystem` reads it. Author a new animation or enemy by editing JSON / tuning numbers; only touch C# for a genuinely new shape.
- **Frame-rate independence:** scale motion by `gameTime` delta seconds (`gameTime.GetElapsedSeconds()` from `MonoGame.Extended`). Never move by a fixed per-frame amount.
- **Virtual resolution:** all gameplay math is in the 600×800 portrait space defined by `VirtualResolution` (`Source/VirtualResolution.cs`) — use those constants for bounds/spawns, not `Viewport`.
- **Asset naming is lowercase snake_case** (files and folders), because `Content.Load<T>` paths are strings and DesktopGL can run on case-sensitive filesystems. Content is organized by type under `Content/` (`sprites/`, `audio/`, etc.); load paths mirror the folders without extension. Don't hardcode those paths — they live as constants in `Source/Assets.cs` (`Assets.Sprites.*`, `Assets.Sfx.*`, `Assets.Fonts.*`, `Assets.Backgrounds.*`), mirroring `Content.mgcb`. Add a const there when you register a new asset, and reference it (e.g. `Content.Load<SoundEffect>(Assets.Sfx.PlayerShot)`).

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

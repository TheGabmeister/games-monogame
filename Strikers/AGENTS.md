# AGENTS.md

Guidance for coding agents working in this repository.

## Project Overview

A bullet-hell shoot-'em-up inspired by *Strikers 1945*, built on MonoGame DesktopGL
(.NET 9) + MonoGame.Extended.ECS as an ECS learning project. See `PLAN.md` for the design,
locked decisions, phase roadmap, content lists, and post-v1 candidates.

Phases 0-5 are done:

- Foundation and project setup.
- Player movement and shooting.
- Enemies, collision, health, damage, explosions, and respawn i-frames.
- Bullet-hell emitters and parameterized enemy fire patterns.
- Stage/arcade loop: scrolling background, wave timeline, STAGE CLEAR, HUD, bombs,
  power-ups, lives, game-over, and restart.
- Polish: title/gameplay screen split, pause overlay, title/stage/game-over music flow,
  menu/graze SFX, banking frames, explosion sparks, and difficulty tuning.

Current top-level architecture:

- Entry point: `Program.cs`
- Main game shell: `Game1.cs` - device setup, shared content/services, `ScreenManager`
- Screens: `Source/Screens/TitleScreen.cs`, `Source/Screens/GameplayScreen.cs`
- Gameplay world setup/restart: `GameplayScreen.NewGame()`
- Project file: `Strikers.csproj`
- Solution file: `Strikers.slnx`
- Content pipeline file: `Content/Content.mgcb`
- MonoGame.Extended pipeline DLLs: `pipeline-references/`

## Build and Run

Use:

```powershell
dotnet run
```

If local MonoGame tools are missing, restore them first:

```powershell
dotnet tool restore
```

The project uses MonoGame's content builder task, so `dotnet run` and `dotnet build` invoke
`dotnet mgcb` against `Content/Content.mgcb`.

## Content Pipeline Notes

Keep this reference in `Content/Content.mgcb`:

```text
/reference:..\pipeline-references\MonoGame.Extended.Content.Pipeline.dll
```

Do not replace it with a deep relative path into the user's NuGet cache such as
`..\..\..\..\Users\...\.nuget\packages\...`. `mgcb` can misread that form as content to
build and fail with a vague `MSB3073` error.

When adding game assets, register them in `Content/Content.mgcb` so they are built into
`.xnb` files and loaded through `Content.Load<T>()`.

Animation JSON is the exception. Files under `Content/animations/*.json` are raw-copied, not
built. Register each with `/copy:animations/foo.json` in `Content.mgcb`, and load them by
reading the file off disk. `AnimationLibrary` enumerates the animation folder under
`AppContext.BaseDirectory` and uses `System.Text.Json`. The sprite sheets referenced by the
JSON are normal pipeline textures. Adding animation JSON as a regular `/build:` item makes
`mgcb` try to process it and fail.

## ECS and Screens

MonoGame.Extended ECS namespaces are `MonoGame.Extended.ECS` (`World`, `WorldBuilder`,
`Entity`, `Aspect`) and `MonoGame.Extended.ECS.Systems`. Do not use
`MonoGame.Extended.Entities`.

System base classes:

- `EntityProcessingSystem` - per-entity `Process`
- `EntityUpdateSystem` - one `Update`, loop `ActiveEntities` yourself
- `EntityDrawSystem`

Local code layout:

- Components: `Components/`
- ECS systems: `Source/Systems/`
- Top-level screens: `Source/Screens/`
- Non-ECS helpers/services: `Source/`

Use `MonoGame.Extended.Screens` only for top-level title/gameplay flow. Pause, STAGE CLEAR,
and GAME OVER are in-run `GameState.Phase` overlays so the gameplay world remains visible
behind them.

## Project Conventions

- Components are data only: fields plus constructors/simple init. No game logic, no calls
  into other systems.
- `EntityFactory` (`Source/EntityFactory.cs`) builds all entities. Systems call
  `factory.Create*(...)`; do not attach components ad hoc inside systems.
- `EntityFactory` needs the built `World`, which does not exist until `WorldBuilder.Build()`.
  Spawning/reacting systems (`WeaponSystem`, `EnemySpawnSystem`, `EmitterSystem`,
  `DamageSystem`, etc.) receive factories/services through public fields after the world is
  built. Follow the wiring pattern in `GameplayScreen.NewGame()`.
- Keep gameplay logic in ECS systems. `Game1` should stay a shell; `GameplayScreen` owns world
  lifetime and wiring; systems own per-entity behavior.
- Global/singleton state stays in plain services: `GameState`, `Stage`, `SfxManager`,
  `MusicManager`, `AnimationLibrary`, `PlayerTracker`.
- Gameplay systems gate on `GameState.Phase == GamePhase.Playing` unless they are deliberately
  allowed to continue for overlay polish, such as timed explosion sparks.
- Gameplay systems may fire SFX via `SfxManager`; music is driven by screens/flow only.
- Collision uses `CircleCollider { Radius, Layer, Mask }` over the `[Flags]` `CollisionLayer`
  enum. Set collider layer/mask in `EntityFactory`.
- Animations and bullet patterns are data, not code. Animation clips live in
  `Content/animations/*.json`; enemy fire parameters live on the `Emitter` component.
- Scale movement by elapsed seconds. Do not move by fixed per-frame amounts.
- All gameplay math uses the 600x800 virtual resolution in `Source/VirtualResolution.cs`, not
  the current viewport.
- Asset filenames and folders are lowercase snake_case. Load paths live in `Source/Assets.cs`;
  add constants there when registering new content.

## Reference Samples

Reference samples on this machine are at `D:\MonoGame-Extended-Samples`:

- `Demos/Sandbox` - simplest sample.
- `Games/Platformer` - `EntityFactory` pattern, tile collisions.
- `Games/StarWarrior` - larger ECS game architecture.
- `Games/Pong` - `MonoGame.Extended.Screens` usage.

`Games/SpaceGame` has an `Entities/` folder but does not use MonoGame.Extended ECS; do not cite
it as an ECS example.

## Coding Style

- Prefer existing MonoGame and MonoGame.Extended APIs already referenced by the project.
- Keep content paths relative to `Content.RootDirectory`, which is set to `Content`.
- Avoid unrelated formatting churn in generated or tool-managed files.
- Keep new abstractions small and aligned with existing phase architecture.

## Troubleshooting

If `dotnet run` or `dotnet build` fails with:

```text
MonoGame.Content.Builder.Task.targets(...): error MSB3073
```

run the `mgcb` command without `/quiet` to reveal the real content pipeline error. The MSBuild
error is usually only a wrapper around the underlying `mgcb` failure.

If the build says `Run "dotnet tool restore" to make the "mgcb" command available`, run:

```powershell
dotnet tool restore
```

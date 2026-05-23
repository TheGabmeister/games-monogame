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

`Game1.Initialize` registers three `GlobalManager`s — `MusicManager`, `SfxManager`, `GameManager` — and does nothing else. Everything downstream flows from `GameManager`'s scene transitions.

### Scene flow

`MainMenuScene` → `GameplayScene` (looping per campaign level) → `GameOverScene` → `MainMenuScene`.

- **MainMenuScene**: black background, "Press Start" text. Fires `StartPressed`. `GameManager.StartGame` resets `GameState` and loads the first campaign level.
- **GameplayScene**: loads one level via `LevelDefinition`. Fires `LevelCompleted` (goal touched) and `PlayerDied` (player killed).
- **GameOverScene**: "Game Over" text. Fires `Continue`, which returns to `MainMenuScene`.

Scenes only *report what happened*; `GameManager` decides *what to do*. The scene never advances itself — keep this split intact.

### GlobalManagers

Three GlobalManagers persist across scene swaps:

- **`GameManager`** owns `GameState`, the `_campaign[]` array (which levels in what order), and scene transition policy. `OnPlayerDied` decrements lives — if `Lives <= 0`, load game over; else reload the same level. `OnLevelCompleted` advances the campaign index; past the last level, load game over.
- **`MusicManager`**: `Play(path)` loads/plays an OGG via `Song.FromUri`. **`Play` is a no-op if the requested path matches the currently-playing path** — this is what keeps music continuous across same-level reloads on player death. Per-scene music is set in each scene's `OnStart` (gameplay reads `_level.MusicPath`).
- **`SfxManager`**: `Play(path)` loads-and-caches a `SoundEffect` by path, then plays it. **WAV-only** — see Audio § below.

Anything can call `Core.GetGlobalManager<T>()`. The service-locator style is fine for cross-cutting concerns (audio, lives, score lookups). Prefer **constructor injection** for component-to-component data flow — see `OneUp(GameState)` and `Coin(GameState)`.

### GameState & lives/score flow

`GameState` holds `Score`, `Lives`, `PowerState`. Lifecycle:
- Created fresh in `GameManager.StartGame`.
- Passed into `GameplayScene` constructor → forwarded to `EntityFactory` constructor → injected into items that mutate it.
- `Lives` is an **event-firing property** (`LivesChanged`). `HudController` subscribes in `OnAddedToEntity` and unsubscribes in `OnRemovedFromEntity`. The `-=` is required because `GameState` (publisher) outlives the scene's HUD (subscriber). See Events § below.

**Two valid item-mutation patterns**:
- *Affects player component state* (size, visuals) → go through a `PlayerController` method (e.g. Mushroom → `player.ApplyMushroom()`, Fire Flower → `player.ApplyFireFlower()`). The two are distinct: only Fire Flower can reach `Fire` state — a second Mushroom on Big Mario is a no-op. There is no generic "grow" entry point.
- *Affects pure `GameState` data* (lives, score) → take `GameState` via constructor and mutate directly (e.g. OneUp, Coin).

### Damage model & combat

Two distinct ways the player can be hurt — keep them separate:

- **`DamagePlayerTrigger`** (`Source/Components/DamagePlayerTrigger.cs`) — trigger-listener that calls `PlayerController.TakeDamage()`. Big/Fire → revert to Small with a ~2s invulnerability window; Small → die. Used by enemies (attached via the walking-enemy helper).
- **`KillVolume`** (`Source/Components/KillVolume.cs`) — trigger-listener that calls `PlayerController.KillPlayer()` directly. Instant death regardless of power state. Used for pits, lava — anything always-lethal per SPEC §4.3.

**Invulnerability window**: when `TakeDamage` reduces state to Small, `PlayerController` sets `_invulnTimer` and calls `_blinker.Blink(InvulnDuration)`. Subsequent `TakeDamage` calls no-op until the timer expires. `Blinker` is a generic renderer-toggler (`Source/Components/Blinker.cs`) — constructor-injected with the `RenderableComponent` it should flicker. Both `_renderer` and `_blinker` are constructor-injected into `PlayerController` by the factory — no `GetComponent` lookups on the hot path.

**Fireballs use centralized dispatch via `IFireballHittable`**. `Fireball` owns the trigger collider on the `Projectile` layer that collides with `Enemy`. On overlap, it looks up `IFireballHittable` on the other entity and dispatches based on the returned `FireballReaction`:

- `Defeated` — enemy handled its own destruction; fireball pops with hit-enemy SFX.
- `Blocked` — enemy unaffected; fireball pops with hit-block SFX (Buzzy Beetle / Bullet Bill territory).

Add the interface to a new enemy type to make it fireball-reactive. The enemy decides what happens to *itself*; the Fireball decides what happens to *itself* based on the returned reaction. Don't put fireball-pop logic inside the enemy.

### Levels / LevelDefinition

`LevelDefinition` has `Name`, `MapPath`, `MusicPath`, `TimerSeconds`. Level entries live in `Source/Levels.cs`. The active campaign sequence lives in `GameManager._campaign[]` — adding a new level means adding to `Levels.cs` **and** inserting it into `_campaign[]` at the right index. Map and music paths reference constants in `Assets` (auto-generated, see below).

### EntityFactory

Tiled-object-Class → spawn-function registry (`Dictionary<string, Action<Scene, TmxObject>>`). Add a new entity type by writing `CreateFoo(Scene, TmxObject)` and calling `Register("Foo", CreateFoo)` in the constructor. **Do not add a switch statement in the scene** — the whole point of the registry is to avoid that.

The constructor takes `GameState` and stores it; pass it into item components that mutate state.

Current Tiled-mapped registrations: `PlayerStart`, `Platform`, `Mushroom`, `FireFlower`, `OneUp`, `Coin`, `Goomba`, `GreenKoopaTroopa`, `RedKoopaTroopa`, `GreenKoopaParatroopa`, `RedKoopaParatroopa`, `BuzzyBeetle`, `PiranhaPlant`, `HammerBro`, `GoalTrigger`, `KillVolume`.

`Fireball` and `Hammer` are **runtime-spawned**, not registered with Tiled. `EntityFactory.CreateFireball(scene, position, facing, owner)` is called by `PlayerController` when X is pressed in Fire state. `EntityFactory.CreateHammer(scene, position, facing)` is called by `HammerBro` on its throw timer. Same shape as `CreatePlayer` — public method, not via the registry.

Walking ground enemies (Goomba, Koopa Troopa variants, Buzzy Beetle) share a `CreateWalkingEnemy(scene, obj, name, color, walkSpeed)` helper that wires up sprite + `Mover` + `GravityBody` + the standard solid/trigger collider pair + `EnemyWalker`. Per-enemy factory methods just call the helper, then attach the species-specific component and a `DamagePlayerTrigger`.

### GravityBody + two-collider pattern

Falling items/enemies (Mushroom, OneUp, Goomba) follow this shape:
- `Mover` + `GravityBody` (gravity integration + collision response)
- **Solid `BoxCollider`** on `Item`/`Enemy` layer, collides with `Environment` — so it lands on platforms
- **Trigger `BoxCollider`** on the same layer, collides with `Player` — fires `OnTriggerEnter` for pickup/damage

Order matters: `Mover.CalculateMovement` iterates `GetComponents<Collider>()` and uses the first non-trigger collider for swept collision. **Add the solid one first.**

`Coin` skips the solid collider (doesn't fall) and has only the trigger.

The Player's `CollidesWithLayers` is **Environment-only** — items don't physically block the player. Item triggers still fire because the item's own `Mover.ApplyMovement` runs `triggerHelper.Update` each frame.

`GravityBody` exposes `Velocity`, `IsGrounded`, `LastCollision`. **Don't use it for the player** — Mario's movement is bespoke (variable jump, etc.) and shouldn't share gravity code with passive falling items.

### HUD rendering

HUDs render screen-locked via `ScreenSpaceRenderer`. Two renderers per gameplay scene:

```csharp
AddRenderer(new RenderLayerExcludeRenderer(0, RenderLayers.Hud));   // world
AddRenderer(new ScreenSpaceRenderer(1, RenderLayers.Hud));          // HUD
```

**Positioning gotcha**: `ScreenSpaceRenderer`'s camera is lazily created on first `OnSceneBackBufferSizeChanged`; before then the renderer falls back to an **identity transform**. Position HUD entities in raw backbuffer pixels with `(0,0)` at the **upper-left** — not the centered (0,0)-is-screen-center convention you'd expect from a normal camera. See `GameplayScene.SpawnHud` (top-left at `(16,16)`) and `MainMenuScene` (centered at `(ScreenWidth/2, ScreenHeight/2)`).

## Level Loading (Tiled)

Levels are authored in **Tiled** (`.tmx` files in `Content/Levels/`). Conventions:

- One object layer named `entities`. Object **Class** (PascalCase) maps to a factory registration. **Name** is optional and used for entity names.
- The csproj copies content globs for raw runtime files:

```xml
<Content Include="Content\**\*.tmx" CopyToOutputDirectory="PreserveNewest" />
<Content Include="Content\**\*.ogg" CopyToOutputDirectory="PreserveNewest" />
<Content Include="Content\**\*.wav" CopyToOutputDirectory="PreserveNewest" />
```

Adding a new content file extension requires adding to this glob list.

- Map paths in `Assets.Maps`, music in `Assets.Music`, SFX in `Assets.Sfx`, level metadata in `Levels.cs`.
- Use `Content.LoadTiledMap(_level.MapPath)`; do not hardcode paths in scenes.
- `PlayerStart` is a marker entity with a `PlayerStart` component. `GameplayScene.SpawnPlayer` uses `FindComponentOfType<PlayerStart>()` and throws if missing.
- Tiled rotation: objects rotate around their **top-left corner**, not center. `EntityFactory.GetCenter` handles the math.

## Audio

### Music

- OGG files in `Content/Music/`: `main_menu.ogg`, `game_over.ogg`, `level_bounce.ogg`, `level_cavern.ogg`, `level_sky.ogg`.
- Played via `Song.FromUri` — DesktopGL handles OGG natively.
- Each scene calls `Core.GetGlobalManager<MusicManager>().Play(...)` in `OnStart`. `Play` no-ops if the requested track is already playing — this is what prevents level music restarting on player-death reload.

### SFX

- WAV files in `Content/Sfx/`, all mono 16-bit 44.1kHz.
- Played via `SoundEffect.FromStream` (cached in `SfxManager` after first load).

**Critical pitfall — OGG vs WAV asymmetry**: MonoGame DesktopGL 3.8's `SoundEffect.FromStream` is **WAV-only**. OGG SFX cannot be loaded at runtime; the first `Play` would crash with `ArgumentException: Specified stream is not a wave file.` The repo keeps source OGGs alongside the converted WAVs for reference. When adding a new SFX:

1. Drop the OGG in `Content/Sfx/`.
2. Convert with ffmpeg: `ffmpeg -i input.ogg -ar 44100 -ac 1 -sample_fmt s16 output.wav -y`
3. Regenerate `Assets.cs` with `Tools/generate_assets.py`.
4. Reference `Assets.Sfx.X` in code.

Music uses OGG directly because `Song` *does* support OGG. The asymmetry is annoying but real.

## `Assets.cs` is auto-generated

`Tools/generate_assets.py` is the source of truth — it scans selected `Content/` folders and emits `Source/Assets.cs`. **Manual edits to `Assets.cs` will be overwritten** the next time the generator runs.

Generated sections:

- `Content/Levels/*.tmx` → `Assets.Maps`
- `Content/Sfx/*.wav` → `Assets.Sfx`
- `Content/Music/*.ogg` → `Assets.Music`
- `Content/Sprites/*.png|*.jpg|*.jpeg` → `Assets.Sprites`

To add a new asset constant: drop the file in the correct `Content/<subdir>/`, then run:

```powershell
& 'C:\Users\Admin\AppData\Local\Python\pythoncore-3.14-64\python.exe' Tools\generate_assets.py
```

In VS Code, use `Terminal > Run Task > Generate Assets.cs`.

## Physics Layers

Defined in `Constants.cs` as bit positions (`Player`, `Enemy`, `Item`, `Environment`, `Projectile`). Set on every collider: `PhysicsLayer = 1 << PhysicsLayers.X`, `CollidesWithLayers = (1 << ...) | (1 << ...)`. Triggers use the same layer system; they also need `collider.IsTrigger = true`.

Player's `CollidesWithLayers` is **Environment-only** — items/enemies don't block the player, they overlap and trigger.

## Render Layers

`RenderLayers.World` (0) and `RenderLayers.Hud` (1) defined in `Constants.cs`. `GameplayScene` uses `RenderLayerExcludeRenderer` for world rendering and `ScreenSpaceRenderer` for HUD. See HUD rendering § for the positioning gotcha.

## Critical Nez Patterns

- **Input:** Use `VirtualButton` / `VirtualIntegerAxis` fields in components. Call `Deregister()` in `OnRemovedFromEntity()`. Raw `Nez.Input` is for one-off demos only.
- **Triggers:** Implement `ITriggerListener.OnTriggerEnter/OnTriggerExit` rather than polling with `Physics.BoxcastBroadphase` in `Update`. Nez `Mover` invokes these automatically.
- **Do NOT remove and re-add a collider during a trigger callback.** Nez's `ColliderTriggerHelper` retains a reference to the old collider for exit tracking; after `RemoveComponent<BoxCollider>()` the collider's `.Entity` becomes null, and the next frame crashes with `NullReferenceException` in `ColliderTriggerHelper.NotifyTriggerListeners`. To resize, use `BoxCollider.SetSize(w, h)` and `SetLocalOffset(...)` in-place. Same applies to destroying entities: destroying the *other* collider's entity during `OnTriggerEnter` is fine because Nez null-checks it, but destroying this one or its collider is not.
- **Queued entities:** Entities created in `Scene.OnStart()` are not in tag lookup lists until `Entities.UpdateLists()` runs. Do not use `FindEntitiesWithTag` for markers created earlier in the same `OnStart()`. Use marker components and `FindComponentOfType<T>()`, which checks queued entities too.
- **Tags:** Nez entities default to `Tag == 0`; project tags should start at `1`.

## Events vs Direct References

Two patterns coexist intentionally:

- **C# `event Action`** on a specific component or scene — `PlayerController.OnDied`, `GameState.LivesChanged`, `GameplayScene.PlayerDied`/`LevelCompleted`, `MainMenuScene.StartPressed`, `GameOverScene.Continue`. Preferred when one specific subscriber owns a reference to the source.
- **Nez `Emitter<T>`** (`Events.Emitter` with `GameEvents` enum) for game-wide events that anything could subscribe to. Currently unused; the enum is empty. Reach for it only when no direct reference exists.

**Event-leak rule** (subtle, important): a leak happens when **publisher outlives subscriber**.
- `GameState` outlives the scene's `HudController` → `HudController` **must** `-=` in `OnRemovedFromEntity`. Same for any scene component subscribing to a GlobalManager event.
- `PlayerController` dies before/with the scene that subscribed to its `OnDied` → no `-=` needed.

## Layout

- `Source/Game1.cs` — boots Nez, registers GlobalManagers.
- `Source/GameManager.cs` — campaign progression, scene transitions, owns `GameState`.
- `Source/MusicManager.cs`, `Source/SfxManager.cs` — audio GlobalManagers.
- `Source/GameState.cs` — persistent data across scenes; `Lives` fires `LivesChanged`.
- `Source/Levels.cs` — `LevelDefinition` and the `Levels` static registry.
- `Source/Scenes/MainMenuScene.cs`, `GameplayScene.cs`, `GameOverScene.cs` — the three scenes.
- `Source/EntityFactory.cs` — Tiled-object → entity spawn registry.
- `Source/Components/` — Nez components, grouped roughly by role:
  - **Player & combat**: `PlayerController`, `Fireball`, `Hammer`, `IFireballHittable` (interface + `FireballReaction` enum), `DamagePlayerTrigger` (damage), `KillVolume` (instant kill), `Blinker` (renderer flicker — used for invuln window, reusable).
  - **Pickups**: `Mushroom`, `FireFlower`, `OneUp`, `Coin`.
  - **Enemies**: `Goomba`, `GreenKoopaTroopa`, `RedKoopaTroopa`, `GreenKoopaParatroopa`, `RedKoopaParatroopa`, `BuzzyBeetle`, `PiranhaPlant`, `HammerBro`. Ground-walking enemies share `EnemyWalker` for the patrol behavior; `HammerBro` has its own bespoke movement/throw logic.
  - **Triggers / level objects**: `GoalTrigger`.
  - **Physics base**: `GravityBody`.
  - **Lifecycle**: `Lifetime(duration)` — attach to any entity to auto-destroy after N seconds. Used by `Fireball` and `Hammer`; reusable for particles/effects.
  - **HUD & UI**: `HudController`, `MainMenuController`, `GameOverController`.
  - **Markers**: `PlayerStart`.
- `Source/Constants.cs` — `PhysicsLayers`, `RenderLayers`, `Tags`, `Constants`, `PlayerState` enum.
- `Source/Assets.cs` — content path constants. **Auto-generated — see `Assets.cs` § above.**
- `Tools/generate_assets.py` — Python generator that updates `Source/Assets.cs` from `Content/`.
- `.vscode/tasks.json` — includes the `Generate Assets.cs` VS Code task.
- `Source/Events.cs` — `GameEvents` enum + global `Emitter` (currently unused).
- `Content/Levels/` — `.tmx` map files.
- `Content/Music/` — OGG music tracks.
- `Content/Sfx/` — WAV sound effects (with OGG sources kept alongside).
- `Content/Content.mgcb` — MonoGame content pipeline file (currently unused; raw-file content copy is used instead).
- `Assets/` — app icon and Windows manifest.
- `NezGuide.md` — Nez usage notes; consult before reinventing patterns.
- `SPEC.md` — gameplay-systems spec for the original Super Mario Bros. (1985). Source-of-truth for original-game mechanics when implementing new features.

## Common Pitfalls

- Object order in a Tiled map is **not guaranteed** to match what you'd expect. Do not rely on iteration order for dependency wiring.
- Generic `Content.LoadTiledMap` does not go through the MGCB pipeline; it reads the raw `.tmx` from the output directory, which is why the csproj has the explicit `Content\**\*.tmx` copy glob.
- `BoxCollider(x, y, w, h)` takes a top-left offset and stores it as a center offset internally. To match a sprite drawn from the entity center, pass `(-w/2, -h/2, w, h)`.
- New audio file extension → add to csproj content globs.
- New SFX must be WAV (see Audio § above).
- New entry in `Levels.cs` is invisible until added to `GameManager._campaign[]`.
- Editing `Assets.cs` directly is futile — regenerate with `Tools/generate_assets.py`.
- Player-death reload re-parses the `.tmx` and re-spawns the HUD; intentional clean-slate. `MusicManager`'s idempotent `Play` is what keeps the music from restarting on each death.

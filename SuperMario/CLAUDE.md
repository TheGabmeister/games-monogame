# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Collaboration Rule

Do not change files, assets, environment variables, registry values, PATH, config, or project state unless the user explicitly asks for a change with words like "do it", "make the change", "implement it", "fix it", "add it", "update it", or "revert it". For questions such as "how do we", "why", "what should we", or "let's plan", answer only.

## Build & Run

```powershell
dotnet build SuperMario.slnx -v minimal
dotnet run --project SuperMario.csproj
```

Clean build is **0 warnings, 0 errors**. Treat any warning as something to fix.

## Nez

Nez is a **local project reference** at `D:\Nez\Nez.Portable\Nez.MG38.csproj` (MG38, not MG39). Read Nez source directly when investigating engine behavior. Do not vendor it into this repo.

**Read `NezGuide.md`** before working with triggers, colliders, tweens, virtual input, `ScreenSpaceRenderer`, queued entities, or `GlobalManager`. Most Nez gotchas live there, not here.

## Architecture

`Game1.Initialize` registers three `GlobalManager`s (`MusicManager`, `SfxManager`, `GameManager`) and nothing else. Everything else flows from `GameManager`'s scene transitions.

### Scenes report, `GameManager` decides

Scenes fire events (`MainMenuScene.StartPressed`, `GameplayScene.LevelCompleted`/`PlayerDied`, `GameOverScene.Continue`). `GameManager` listens and decides what scene to load next. **The scene never advances itself.** Keep this split intact.

`MusicManager.Play(path)` is a no-op if the requested track is already playing — this is what keeps level music continuous across same-level reloads on player death.

### `GameState` and item mutation

`GameState` (Score, Lives, PowerState) is created in `GameManager.StartGame` and threaded into the gameplay scene + factory. Two valid mutation patterns:

- **Affects player component state** (size, visuals) → call a `PlayerController` method (`ApplyMushroom`, `ApplyFireFlower`). Mushroom and Fire Flower are distinct: only Fire Flower reaches `Fire` state.
- **Affects pure `GameState` data** (lives, score) → take `GameState` via constructor and mutate directly (see `Coin`, `OneUp`).

### Damage & combat dispatch

Two ways the player can be hurt:

- `DamagePlayerTrigger` calls `PlayerController.TakeDamage()` (Big/Fire → Small with ~2s invuln window; Small → die). If the entity also implements `IStompable`, the trigger tries `TryStomp` first and only damages on a non-stomp.
- `KillVolume` calls `PlayerController.KillPlayer()` — instant death regardless of power state (pits, lava).

**Three combat-dispatch interfaces** — `IFireballHittable`, `IStompable`, `IStarHittable`. Same pattern: **the enemy decides what happens to itself, the attacker decides what happens to itself.** Don't put attacker-cleanup logic inside the enemy. `IFireballHittable` returns a `FireballReaction` (`Defeated` or `Blocked`); the other two are `void`. Implement whichever apply.

### `EntityFactory` & the colocated-`Spawn` convention

`EntityFactory` is a thin Tiled-Class → spawn-function registry (~70 lines, don't grow it). Each entity owns its own `public static void Spawn(Scene, TmxObject [, GameState])` on its component file; the factory just wires `Register("Foo", Foo.Spawn)`. Pickups that need `GameState` get it wrapped in a closure in the factory constructor.

**Adding an entity:** write `Foo.Spawn` on the component, add one `Register` line. **Do not add creation logic to `EntityFactory`.** Do not add a switch statement in the scene.

Exception: `Platform` has no behavior component, so its `Spawn` lives on `public static class Platform` in `Platform.cs`. All other Spawns live on `Component` subclasses.

`EntityFactory.GetCenter(TmxObject)` is the shared "top-left → rotated center" helper; call it from every Spawn.

**Runtime spawning** (not via the registry, called directly): `PlayerController.Spawn`, `Fireball.Spawn`, `Hammer.Spawn`, `BulletBill.Spawn`, `KoopaTroopa.Spawn(scene, pos, w, h, color)`. No `EntityFactory` reference is plumbed through to spawner components — they call the static directly.

### Physics layers

Layer bit positions in [Source/Constants.cs](Source/Constants.cs). The non-obvious split is `PickupBody` vs `PickupTrigger`: a pickup's solid collider sits on `PickupBody` (collides with `Environment` only — for landing on platforms); its trigger sits on `PickupTrigger` (collides with `Player`). The player's mask includes `PickupTrigger` and `LevelTrigger` but **not** `PickupBody`, so Mario walks *through* pickups while still triggering them. `EnemyProjectile` is for enemy-fired projectiles (hammers) so they don't clobber player-fireball logic.

### Events: leak rule

A leak happens when **publisher outlives subscriber**. `GameState` (long-lived) → any scene component subscribing to `LivesChanged` **must** `-=` in `OnRemovedFromEntity`. A subscriber that dies with the publisher (e.g. a scene component subscribing to `PlayerController.OnDied`) doesn't need to unsubscribe.

## Audio

### Music
OGG via `Song.FromUri`. Each scene calls `Core.GetGlobalManager<MusicManager>().Play(...)` in `OnStart`.

### SFX — the OGG/WAV asymmetry
MonoGame DesktopGL 3.8's `SoundEffect.FromStream` is **WAV-only**. OGG SFX cannot be loaded at runtime; the first `Play` would crash with `ArgumentException: Specified stream is not a wave file.` The repo keeps source OGGs alongside the converted WAVs for reference. When adding a new SFX:

1. Drop the OGG in `Content/Sfx/`.
2. Convert: `ffmpeg -i input.ogg -ar 44100 -ac 1 -sample_fmt s16 output.wav -y`
3. Regenerate `Assets.cs` (see below). Reference `Assets.Sfx.X`.

Music uses OGG directly because `Song` *does* support OGG. The asymmetry is annoying but real.

## `Assets.cs` is auto-generated

`Tools/generate_assets.py` is the source of truth. **Manual edits to [Source/Assets.cs](Source/Assets.cs) will be overwritten.** It scans `Content/Levels`, `Content/Sfx`, `Content/Music`, `Content/Sprites` and emits the constants.

To regenerate:

```powershell
& 'C:\Users\Admin\AppData\Local\Python\pythoncore-3.14-64\python.exe' Tools\generate_assets.py
```

In VS Code: `Terminal > Run Task > Generate Assets.cs`.

## Tiled level loading

- One object layer named `entities`. Object **Class** (PascalCase) maps to a factory registration.
- `Content.LoadTiledMap(...)` reads the raw `.tmx` from the output dir — not via MGCB. The csproj has explicit copy globs for `.tmx`, `.ogg`, `.wav`. New content file extension → add a glob.
- `PlayerStart` is a marker component. `GameplayScene.SpawnPlayer` uses `FindComponentOfType<PlayerStart>()` and throws if missing.
- See `NezGuide.md` for Tiled-rotation math and `Nez.Tiled` quirks.

## Common Pitfalls

- New entry in `Levels.cs` is invisible until added to `GameManager._campaign[]`.
- Player-death reload re-parses the `.tmx` and re-spawns the HUD — intentional clean-slate. `MusicManager`'s idempotent `Play` is what keeps music from restarting on each death.
- Editing `Assets.cs` directly is futile — regenerate it.
- Object iteration order in `.tmx` is not guaranteed; don't rely on it for dependency wiring.

## Where things live

- [Source/](Source/) — game code. Top-level files are GlobalManagers (`GameManager.cs`, `MusicManager.cs`, `SfxManager.cs`), state (`GameState.cs`), constants (`Constants.cs`), the auto-generated `Assets.cs`, the entity registry (`EntityFactory.cs`), level definitions (`Levels.cs`), and bootstrap (`Game1.cs`).
- [Source/Scenes/](Source/Scenes/) — the three scenes.
- [Source/Components/](Source/Components/) — Nez components, one file per concept. Combat interfaces (`IFireballHittable`, `IStompable`, `IStarHittable`) live here too.
- [Content/](Content/) — levels (`.tmx`), music (`.ogg`), SFX (`.wav` + source `.ogg`).
- [NezGuide.md](NezGuide.md) — Nez engine quirks.
- [SPEC.md](SPEC.md) — original Super Mario Bros. (1985) gameplay reference.

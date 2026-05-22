# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpaceInvaders is a modernized Space Invaders-style game built with MonoGame and the Nez framework. Nez provides the scene/entity/component model, collision helpers, sprite rendering, virtual input, timers, and scene transitions. See `SPEC.md` for the full game design.

The project targets `net9.0`, uses MonoGame DesktopGL 3.8.x, and references Nez from local source at `D:\Nez\Nez.Portable\Nez.MG38.csproj`.

## Build and Run

```shell
dotnet build
dotnet run
```

Content pipeline tools are configured as local dotnet tools. Restore them before editing MGCB content:

```shell
dotnet tool restore
```

## Architecture

Nez is component-based: scenes contain entities, and entities contain components. Custom behavior lives in Nez `Component` and `SceneComponent` classes. This is not data-oriented ECS; components hold both data and behavior, similar to Unity `MonoBehaviour`.

- `Source/` - all C# game code. Entry point is `Program.cs`; main game class is `Game1.cs`.
- `Source/Scenes/` - Nez `Scene` subclasses: `MainMenuScene`, `GameplayScene`, and `GameOverScene`.
- `Source/Components/` - entity components such as `PlayerController`, `FormationController`, `BulletController`, `InvaderData`, `ShieldChunk`, `UFOController`, `Blinker`, and `ShakeListener`. Also static helpers: `ExplosionHelper` and `ScorePopup`.
- `Source/SceneComponents/` - scene-level managers: `GameState`, `HudController`, `WaveManager`, `BassRhythm`, and `EventBus`.
- `Source/Assets.cs` - nested `const string` paths for runtime content. Update this when adding, renaming, or removing content.
- `Source/Constants.cs` - tuning values, physics layers, tags, and enums.
- `Content/` - runtime PNG/WAV/effect assets copied to output by `.csproj` globs.
- `Assets/` - app resources and editable source SVGs under `Assets/source/`.

## Current Gameplay Scene Shape

`GameplayScene` composes the level:

- Adds `EventBus`, `GameState`, `WaveManager`, `HudController`, and `BassRhythm` scene components.
- Adds `CameraShake` and `ShakeListener` to the camera entity.
- Creates the player entity and shield entities.
- Starts the first invader formation through `WaveManager.SpawnFormation()`.
- Polls only pause input in `Update()`. Subscribes to `GameState.GameOver` to transition to `GameOverScene` after a short delay.

Avoid putting HUD refresh logic or repeated state checks in `GameplayScene.Update()`. Prefer event callbacks, Nez timers, or focused scene components.

## Event-Driven State and HUD

`GameState` is the source of truth for score, high score, lives, wave, and game-over state. It exposes events:

- `ScoreChanged`
- `HighScoreChanged`
- `LivesChanged`
- `WaveChanged`
- `GameOver`

Use `GameState.AddScore()`, `LoseLife()`, `AdvanceWave()`, and `TriggerGameOver()` instead of mutating state fields directly. `HighScore` and `Wave` have private setters by design.

`HudController` owns HUD creation and display updates. It creates the HUD text entities, subscribes to `GameState` events, refreshes labels only when state changes, and exposes `ShowPause()` / `HidePause()` for `GameplayScene`.

## Scene Event Bus

`EventBus` is a SceneComponent wrapping Nez's `Emitter<GameEvents>`. It provides a scene-wide publish/subscribe mechanism for gameplay moments that multiple unrelated systems need to react to.

Events: `PlayerDied`, `UfoDestroyed`, `WaveCleared`. These are signals with no data — consumers that need context (position, score) get it from the scene or handle it themselves.

Use the event bus for decoupled reactions (camera shake listening to deaths, GameplayScene handling respawn). Use direct spawning for effects that need context from the dying component (particles, score popups are spawned by the component that dies).

`GameState` C# events (`ScoreChanged`, `LivesChanged`, etc.) remain separate — those are UI data bindings, not gameplay events.

## Player Death and Respawn

The player entity is destroyed on death, not hidden. `PlayerController.Die()` sets its dead flag, plays the death sound, spawns a particle explosion, emits `GameEvents.PlayerDied` on the event bus, then destroys the entity.

`GameplayScene.OnPlayerDied()` handles life loss. If the game is not over, it uses Nez's global timer system:

```csharp
_playerRespawnTimer = Core.Schedule(Constants.DeathDelay, timer =>
{
    CreatePlayer(startInvulnerable: true);
});
```

Store scheduled timers that belong to a scene and stop them in `Unload()`:

```csharp
_playerRespawnTimer?.Stop();
_playerRespawnTimer = null;
```

`Core.Schedule` uses `Time.DeltaTime`, so scheduled respawn timing respects `Time.TimeScale`. Pausing with `Time.TimeScale = 0` pauses these timers too.

## Entity Construction Boundaries

Current player construction still lives in `GameplayScene.CreatePlayer()`: it creates the entity, attaches `SpriteRenderer`, `BoxCollider`, `Blinker`, and `PlayerController`. Death is handled via the event bus, not direct subscription.

Do not move renderer/collider setup into `PlayerController`. Keep `PlayerController` focused on behavior: movement, firing, death, and invulnerability. If player construction grows, prefer extracting a small factory or spawner component rather than turning `PlayerController` into a builder.

`BulletController.CreateBullet()` is currently a static factory for bullet entities. It creates the renderer, collider, `ProjectileMover`, and controller in one place.

## Asset Loading

Assets are loaded with the scene-scoped content manager:

- Scenes use `Content.LoadTexture(..., true)` or `Content.LoadSoundEffect(...)`.
- Components use `Entity.Scene.Content.LoadTexture(...)` / `LoadSoundEffect(...)`.
- SceneComponents use `Scene.Content.LoadTexture(...)` / `LoadSoundEffect(...)`.

Scene-scoped content is disposed on scene transition. Use `Core.Content` only for assets that must persist across scenes; currently none do.

## Input

Use Nez virtual input (`VirtualButton`, `VirtualIntegerAxis`), not raw MonoGame keyboard/gamepad polling.

- Scene-level virtual inputs are created in `Scene.Initialize()` and deregistered in `Unload()`.
- Component-level virtual inputs are created in `OnAddedToEntity()` and deregistered in `OnRemovedFromEntity()`.

`GameplayScene.Update()` polls virtual button edge states for pause only. That is acceptable; avoid adding unrelated gameplay counters or HUD refreshes there.

## Nez Framework Notes

- Collision uses `BoxCollider`, physics layer bitmasks, `ProjectileMover`, and `ITriggerListener`.
- Use Nez `Flags` helpers for physics layers: `Flags.SetFlagExclusive` to assign a single layer, `Flags.SetFlag` to add a layer to a mask, `IsUnshiftedFlagSet` to check layer membership. Avoid raw `1 <<` bit-shifting.
- Manual broadphase checks can use `Physics.BoxcastBroadphase(bounds, layerMask)`.
- `SpriteRenderer` is in `Nez.Sprites`. `SpriteAnimator` extends it for frame animation.
- `CameraShake` is a Component added to the camera entity (`Scene.Camera.Entity`).
- `ParticleEmitter` with `ParticleEmitterConfig` for one-shot burst effects. Use `OnAllParticlesExpired` to auto-destroy.
- Nez `Emitter<T>` provides a simple event bus keyed by enum. `AddObserver`/`RemoveObserver`/`Emit`.
- Entity tweens: `entity.TweenPositionTo()`, `entity.TweenScaleTo()`, `renderableComponent.TweenColorTo()`. Call `.Start()` to begin.
- `TextComponent`, `VirtualButton`, `VirtualIntegerAxis`, and content helpers are in `Nez.Systems`.
- Scene resolution uses `SceneResolutionPolicy.ShowAll` at `960x720`.
- Scene transitions use `Core.StartSceneTransition(new FadeTransition(...))`.
- Formation hierarchy uses `Transform.SetParent`; moving the parent moves all child invaders.
- Nez timers are global through `Core.Schedule`; store and stop scene-owned timers.
- `Time.DeltaTime` is scaled by `Time.TimeScale`; `Time.UnscaledDeltaTime` ignores it.

## Gotchas

- Use `System.Random`, not `Nez.Random`, to avoid namespace ambiguity.
- Use `(float)System.Math.Pow()` if needed; `MathF.Pow` may not resolve in all target configs.
- Sprites are exported large from SVGs, then scaled down at runtime with `Entity.Transform.SetScale()`.
- `LoadTexture` should pass `premultiplyAlpha: true`.
- WAV files are loaded directly with `LoadSoundEffect`; no content pipeline processing is needed for them.
- Nez bitmap fonts cannot be resized directly; scale the text entity transform instead.
- Virtual inputs must be deregistered, or they leak.
- Debug collider rendering is currently disabled in `Game1.Initialize()` with `DebugRenderEnabled = false`. Set it to `true` temporarily when debugging physics.

## Dependencies

- `MonoGame.Framework.DesktopGL` 3.8.x
- `MonoGame.Content.Builder.Task` 3.8.x
- Nez framework project reference: `D:\Nez\Nez.Portable\Nez.MG38.csproj`
- Target framework: `net9.0`

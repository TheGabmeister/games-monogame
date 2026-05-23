# AGENTS.md

## Project

This is a MonoGame + Nez Super Mario-style prototype built from a reusable template.

The app entry point is `Source/Program.cs`, which creates `SuperMario.Game1`.
`Game1` inherits from `Nez.Core` and registers `GameManager` as a Nez `GlobalManager`.
`GameManager` owns persistent `GameState` and loads `GameplayScene` instances from `Levels`.

## Build

Use:

```powershell
dotnet build SuperMario.slnx -v minimal
```

The expected result is a clean build with 0 warnings and 0 errors.

## Nez Dependency

Nez is referenced as a local project dependency, not a Git submodule.

Current reference:

```xml
<ProjectReference Include="D:\Nez\Nez.Portable\Nez.MG38.csproj" />
```

This project uses MonoGame 3.8 packages, so use `Nez.MG38.csproj` for Nez.

Do not add Nez source under this template unless explicitly asked.

## Layout

- `Source/Game1.cs` boots Nez and registers `GameManager`.
- `Source/GameManager.cs` owns persistent `GameState` and scene flow.
- `Source/GameState.cs` stores score, lives, and player power state across scene reloads. Use `GameState.AddScore(...)` for score changes so `ScoreChanged` updates the HUD.
- `Source/Levels.cs` defines `LevelDefinition` entries (`Debug`, `World1_1`, `World1_2`, `World1_3`).
- `Source/Scenes/GameplayScene.cs` loads TMX levels, spawns entities, spawns the HUD and bottom-of-level cleanup volume, handles player respawn, completion, and game over.
- `Source/EntityFactory.cs` maps Tiled object Class strings to component `Spawn(...)` methods.
- `Source/Components/` contains gameplay components such as `PlayerController`, `GravityBody`, `EnemyWalker`, `Goomba`, `KoopaTroopa`, `KoopaParatroopa`, `BuzzyBeetle`, `Spiny`, `PiranhaPlant`, `HammerBro`, `Blooper`, `Podoboo`, `BulletBill`, `BulletBillCannon`, `Mushroom`, `FireFlower`, `OneUp`, `Coin`, `Starman`, `Fireball`, `Hammer`, `Platform`, `MovingPlatform`, `QuestionBlock`, `BrickBlock`, `UsedBlock`, `GoalTrigger`, `KillVolume`, `CleanupVolume`, `HudController`, `ScorePopup`, and `PlayerStart`.
- `Source/Audio.cs` is a small facade for `Audio.PlaySfx(...)` and `Audio.PlayMusic(...)`; prefer it over direct `Core.GetGlobalManager<SfxManager>()` / `MusicManager` calls.
- `Source/Constants.cs` holds shared constants, physics layer bit positions, render layers, tags, and player state enum.
- `Source/Assets.cs` holds generated content path constants for maps, sprites, SFX, and music.
- `Tools/generate_assets.py` regenerates `Source/Assets.cs` from files under `Content/`.
- `Content/Levels/` contains Tiled `.tmx` level files.
- `Content/Music/` contains generated `.ogg` music loops and regeneration sources.
- `Content/Sfx/` contains runtime `.wav` sound effects.
- `Content/Content.mgcb` is the MonoGame content pipeline file.
- `Assets/` contains project icon and Windows manifest files.

## Nez Patterns

Prefer Nez scenes, entities, and components over raw MonoGame `Update`/`Draw` code.

For player or entity controls, follow the Nez samples' pattern: create `VirtualButton`,
`VirtualIntegerAxis`, or `VirtualJoystick` fields inside the controlling component and
call `Deregister()` when the component is removed.

Use raw `Nez.Input` only for very small one-off sample/demo behavior.

Tiled object Class names are factory keys. Current registrations are:

- `PlayerStart`
- `Platform`
- `LeftRightLift`
- `UpDownLift`
- `QuestionBlock`
- `BrickBlock`
- `UsedBlock`
- `Mushroom`
- `FireFlower`
- `OneUp`
- `Coin`
- `Starman`
- `Goomba`
- `GreenKoopaTroopa`
- `RedKoopaTroopa`
- `GreenKoopaParatroopa`
- `RedKoopaParatroopa`
- `BuzzyBeetle`
- `Spiny`
- `PiranhaPlant`
- `HammerBro`
- `Blooper`
- `BulletBillCannon`
- `Podoboo`
- `GoalTrigger`
- `KillVolume`

Prefer putting object creation next to the component as a static `Spawn(...)` method and registering that method in `EntityFactory`. For game-state-dependent pickups or blocks, register a small lambda that passes `_gameState`.

`GreenKoopaTroopa` and `RedKoopaTroopa` both use the shared `KoopaTroopa` component with a `KoopaColor` constructor value. `GreenKoopaParatroopa` and `RedKoopaParatroopa` both use the shared `KoopaParatroopa` component. Keep the Tiled Class names explicit, but avoid duplicating behavior components unless the behavior truly diverges.

`LeftRightLift` and `UpDownLift` both use `MovingPlatform` with a `MovingPlatformAxis`. Keep the Tiled Class names explicit and share the behavior component.

`QuestionBlock`, `BrickBlock`, and `UsedBlock` are solid `Environment` entities. `QuestionBlock` and `BrickBlock` implement `IBumpable`; `PlayerController` calls `IBumpable.OnBumped(...)` when it collides upward into a block. `BlockBump` owns the small bump animation. Do not use Tiled custom properties for block contents yet unless explicitly adding that design.

`KillVolume` is a gameplay hazard for the player death flow. `GameplayScene` also spawns a wide `CleanupVolume` below the map to destroy collider entities that fall out of the level; do not model that cleanup volume in Tiled unless explicitly changing the scene cleanup design.

For audio, use `Audio.PlaySfx(...)` and `Audio.PlayMusic(...)`. Keep direct global-manager lookups centralized in `Source/Audio.cs`.

For scoring, use `GameState.AddScore(...)` and spawn `ScorePopup` for visible point feedback when appropriate. Do not mutate `GameState.Score` directly.

Physics layers are deliberately split by collider job:

- `Player`: the player body collider.
- `Enemy`: enemy bodies and enemy damage triggers.
- `PickupBody`: solid body colliders for moving pickups such as mushrooms, fire flowers, one-ups, and Starman; these collide with `Environment`.
- `Environment`: level geometry, solid platforms, blocks, cannons, and other world solids.
- `Projectile`: player projectiles such as fireballs.
- `EnemyProjectile`: enemy projectiles such as hammers.
- `PickupTrigger`: collection trigger colliders for coins and pickup overlap areas; the player scans this layer.
- `LevelTrigger`: goal, kill, and other player-entered level triggers; the player scans this layer.

Do not collapse `PickupBody` and `PickupTrigger`: moving pickups need a solid body for world physics and a separate trigger for collection, while coins only need the trigger. Trigger layers should use `IsTrigger = true`; they should not be used for solid movement bodies.

Use small interaction interfaces for cross-component gameplay:

- `IBumpable` for blocks hit from below by the player.
- `IStompable` for entities the player can stomp.
- `IFireballHittable` for fireball reactions.
- `IStarHittable` for entities defeated by star-invincible player contact.

`DamagePlayerTrigger` is the central enemy-contact path. It checks star invincibility first, then stomp handling, then normal player damage.

`Starman` is a pickup on `PickupBody`/`PickupTrigger`. It bounces under `GravityBody`, grants a timed `PlayerController` star-invincibility state, and uses `IStarHittable` through `DamagePlayerTrigger` to defeat enemies on contact. Star mode does not protect from `KillVolume`, pits, lava, or other level-death flows.

Do not use `FindEntitiesWithTag` for entities created earlier in the same `Scene.OnStart()` call.
Nez queues new entities until `Entities.UpdateLists()`. Use marker components such as `PlayerStart`
with `FindComponentOfType<PlayerStart>()` when startup code must find newly spawned markers.

## Content

If Nez effects or post-processors are used, add or link Nez default content:

- `D:\Nez\DefaultContent\effects` -> `Content\nez\effects`
- `D:\Nez\DefaultContent\textures` -> `Content\nez\textures`

Keep content paths in sync with `Content/Content.mgcb`.

Raw `.tmx`, `.ogg`, and `.wav` files are copied by `SuperMario.csproj`:

```xml
<Content Include="Content\**\*.tmx" CopyToOutputDirectory="PreserveNewest" />
<Content Include="Content\**\*.ogg" CopyToOutputDirectory="PreserveNewest" />
<Content Include="Content\**\*.wav" CopyToOutputDirectory="PreserveNewest" />
```

After adding, removing, or renaming files under `Content/Levels`, `Content/Sfx`, `Content/Music`,
or `Content/Sprites`, regenerate `Source/Assets.cs`:

```powershell
& 'C:\Users\Admin\AppData\Local\Python\pythoncore-3.14-64\python.exe' Tools\generate_assets.py
```

In VS Code, the same command is available as the `Generate Assets.cs` task.

Map paths live in `Assets.Maps`, SFX paths in `Assets.Sfx`, and music paths in `Assets.Music`;
level-to-music assignment lives in `Levels.cs`.

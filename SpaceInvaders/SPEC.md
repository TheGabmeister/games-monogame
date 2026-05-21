# Space Invaders Modernized - Specification

## Vision

A modernized Space Invaders (1978) built with MonoGame and Nez. The gameplay stays faithful to the original — formation march, single-shot cannon, destructible shields, the tension ramp as invaders speed up — but the presentation is modern: neon-vector sprites, particles, screen shake, synthesized SFX, and an adaptive bass rhythm that accelerates with the invaders.

No powerups, weapon upgrades, roguelite progression, or campaign until the core loop is proven.

## Technical Stack

MonoGame (DesktopGL 3.8.x) on .NET 9.0 with Nez as the game framework. Nez sits on top of MonoGame and provides a component-based architecture (Scene → Entity → Component), built-in collision with `SpatialHash`, sprite animation, particles, tweening, camera shake, and scene transitions.

The game renders to a `960x720` virtual resolution (4:3 landscape) using Nez's `SceneResolutionPolicy.ShowAll` for automatic letterboxing. Default window size matches the virtual resolution.

Nez replaces MonoGame.Extended entirely — there is no need for both.

## The Formation

55 invaders arranged in an 11x5 grid. Three types by row:

| Rows | Type | Points |
|------|------|--------|
| 5 (top) | Squid | 30 |
| 3-4 | Crab | 20 |
| 1-2 | Octopus | 10 |

The formation is modeled as a parent Entity with a `FormationController` component. Each invader is a child Entity — moving the parent's Transform moves the entire grid. The `FormationController` handles the march pattern: left until the outermost living invader hits the boundary, drop one row, right, repeat. Movement is smooth and interpolated.

Each invader type has two animation frames that toggle as the formation advances, handled by Nez's built-in `SpriteAnimator`.

### The Speed-Up

This is the heart of Space Invaders. As invaders are destroyed, the survivors move faster:

```
speed = baseSpeed * (55 / remainingInvaders)
```

Capped at a maximum so the last invader is fast but still hittable. The bass rhythm loop (see Audio) is tied to this same curve — the whole game accelerates together.

### Enemy Fire

Only the bottom-most invader in each column can shoot. There's a single enemy bullet type, with a maximum of 3 enemy bullets on screen at once. The fire interval is randomized within a range (roughly 0.8-1.5s at wave 1) and tightens as waves progress (~5% per wave). The `FormationController` owns the firing logic since it knows which invaders are alive and in which columns.

## The Player

The cannon moves horizontally at a constant speed along the bottom of the playfield. The defining constraint: **one bullet on screen at a time**. You can't fire again until your shot connects or leaves the screen. This forces deliberate aim and is core to why Space Invaders works.

A `PlayerController` component handles input, movement, firing, death/respawn timing, and invulnerability flicker.

Starting tuning values:

- Cannon: ~48x32 px sprite, 280 px/s movement
- Player bullet: 4x12 px, 600 px/s
- Enemy bullet: 4x12 px, 320 px/s

### Controls

**Keyboard:** Left/A and Right/D to move, Space to fire, Escape to pause, Enter to confirm.

**Gamepad:** Left stick or D-pad to move, A to fire, Start to pause, B for back/cancel.

### Lives and Death

3 lives to start. One extra life awarded at 1500 points (once per game). When hit, the cannon explodes and respawns after ~1.5s with a brief invulnerability flicker. Game over when lives hit 0 or the formation reaches the player's row.

## Shields

4 shields spaced evenly across the playfield, positioned about 100 px above the cannon. Each shield is a group of 16 `ShieldChunk` entities arranged in a 4x4 grid. Each chunk has a `SpriteRenderer`, a `BoxCollider` on the Shield physics layer, and a `ShieldChunk` component that implements `ITriggerListener` — when hit by any bullet, the chunk's entity is destroyed. Invaders marching through shields also destroy chunks on contact.

Shield sprite is roughly 60x48 px total, making each chunk about 15x12 px.

## Mystery UFO

A bonus ship that flies across the top of the screen at 160 px/s. Spawned by the `WaveManager` at random intervals (every 20-30 seconds). A `UFOController` component moves it across the screen and awards a random score drawn from {50, 100, 150, 200, 300} on hit. If it reaches the far side, it destroys itself.

## Collision

Nez's built-in `SpatialHash` broadphase with `BoxCollider` on every game object. Collision is filtered by physics layer bitmasks — each collider declares which layers it collides with, and `ProjectileMover.Move()` checks overlaps and fires `ITriggerListener.OnTriggerEnter` callbacks on both sides.

Physics layers:

```
Layer 0: Player
Layer 1: Invader
Layer 2: PlayerBullet   → collides with: Invader, Shield, EnemyBullet
Layer 3: EnemyBullet    → collides with: Player, Shield
Layer 4: Shield
```

No custom collision code needed. Each entity's `ITriggerListener` component decides what happens on contact (destroy self, decrement lives, add score, spawn particles, etc.).

For invaders marching through shields, the `FormationController` checks for overlaps between invader colliders and shield chunks after each formation move and destroys any overlapping chunks.

## Waves

Each wave starts with a fresh 11x5 formation. After clearing all invaders, a brief "Wave N" transition plays (using Nez's tweening for the text animation) before the next formation spawns. Difficulty ramps across waves in three ways:

1. **Lower starting position** — the formation begins one row closer to the player each wave, down to a minimum safe distance above the shields.
2. **Faster base speed** — roughly +10% per wave.
3. **Faster enemy fire** — the fire interval tightens by ~5% per wave.

## Scenes

Three Nez Scenes with built-in transitions (fade) between them:

**MainMenuScene** — Menu entities for Start Game, Options, Quit.

**GameplayScene** — The main game scene. Contains the formation, player, shields, HUD, and all gameplay entities. Pause is handled as an overlay entity within this scene (enable/disable) rather than a separate scene, so pausing feels instant with no transition.

**GameOverScene** — Final score, high score (with a callout if it's a new record), Restart, Main Menu.

The gameplay HUD (score top-left, high score top-center, wave number top-right, lives bottom-left) is drawn by a `HudRenderer` component on a dedicated HUD entity.

## Visual Style

Neon-glow vector aesthetic on a dark background. Bright outlined sprites with color-coding per invader type, subtle glow on bullets and the cannon, sharp silhouettes that prioritize readability. Sprites are generated as SVGs and exported to PNG — no pixel art.

### Effects

Restrained juice — effects accent key moments without cluttering the screen:

- **Player death:** Strong camera shake (Nez `CameraShake` component), large `ParticleEmitter` explosion.
- **Invader destroyed:** Tiny shake, particle burst, brief hit flash.
- **Shield chunk destroyed:** Small particle puff, no shake.
- **UFO destroyed:** Medium shake, glowing score popup (tweened).
- **Bullets:** Subtle glow trails.
- **Cannon:** Faint pulsing glow, muzzle flash on fire.
- **Normal gameplay:** No camera wobble or ambient shake.

Readability always wins over spectacle.

## Audio

SFX-first, with one critical musical element: the adaptive bass rhythm.

### Bass Rhythm

The iconic four-note heartbeat that defines Space Invaders' soundscape. It starts at ~60 BPM with a full formation and accelerates alongside the invaders, reaching ~300 BPM with the last few survivors. This is tied directly to the speed-up formula. Managed by a `BassRhythm` SceneComponent on the GameplayScene.

### Sound Effects

| Sound | Trigger |
|-------|---------|
| shoot.wav | Player fires |
| invader_death.wav | Invader destroyed |
| player_death.wav | Cannon explosion |
| ufo_hum.wav | UFO flyover (looping) |
| ufo_score.wav | UFO hit |
| shield_hit.wav | Shield chunk destroyed |
| menu_move.wav | Menu cursor |
| menu_select.wav | Menu confirm |
| wave_start.wav | New wave begins |
| extra_life.wav | Extra life awarded |

No background music beyond the bass rhythm for v1.

## Persistence

High score and settings are saved as JSON files in `%AppData%/SpaceInvaders/`. Settings cover master volume, SFX volume, fullscreen toggle, and screen shake toggle. No cloud saves, no online leaderboards.

## Assets

Sprites and audio are generated via the `$gen-assets` workflow (requires blender, ffmpeg, fluidsynth, inkscape, and python on PATH).

**Source files** (SVGs, MIDI, scripts) live in `Assets/source/`. **Runtime assets** (PNGs, WAVs) go into `Content/` where MonoGame's content pipeline picks them up. Both directories are organized by category: `sprites/player/`, `sprites/invaders/`, `audio/sfx/`, etc.

The initial sprite set:

- `cannon.png` — player ship
- `squid_01.png`, `squid_02.png` — top-row invader frames
- `crab_01.png`, `crab_02.png` — middle-row invader frames
- `octopus_01.png`, `octopus_02.png` — bottom-row invader frames
- `ufo.png` — mystery ship
- `shield_chunk.png` — single shield segment
- `bullet_player.png`, `bullet_enemy.png`
- `explosion_particle.png`, `muzzle_flash.png`

## Code Organization

Nez's component-based architecture: Scenes contain Entities, Entities contain Components. Custom behavior lives in Components (attached to entities) and SceneComponents (scene-level managers). No separate "systems" layer — logic lives where it naturally belongs.

### SceneComponents (scene-level managers on GameplayScene)

- **WaveManager** — tracks wave number, counts living invaders, spawns formations and UFOs, triggers wave transitions.
- **BassRhythm** — plays the four-note heartbeat, tempo derived from WaveManager's alive count.
- **GameState** — score, lives, extra life tracking, game-over detection.

### Custom Components (attached to entities)

- **FormationController** — march pattern, speed-up curve, edge detection, enemy firing logic.
- **InvaderData** — invader type (squid/crab/octopus), point value, alive flag.
- **PlayerController** — input handling, horizontal movement, firing, death/respawn, invulnerability.
- **BulletController** — implements `ITriggerListener`, moves via `ProjectileMover`, destroys on hit or off-screen, notifies hit targets.
- **ShieldChunk** — implements `ITriggerListener`, destroys its entity on any bullet hit.
- **UFOController** — horizontal movement, random score award on death, self-destruct at screen edge.
- **HudRenderer** — draws score, high score, lives, wave number.

### Nez Built-ins Used (no custom code needed)

- `SpriteRenderer` / `SpriteAnimator` — all sprite drawing and invader frame animation.
- `BoxCollider` + `ProjectileMover` — collision detection and bullet movement.
- `CameraShake` — screen shake on kills and death.
- `ParticleEmitter` — explosion and trail effects.
- Tweening — score popups, wave text, menu transitions.
- `SceneResolutionPolicy.ShowAll` — 960x720 virtual resolution with letterboxing.
- Scene transitions — fades between scenes.

### File Structure

```
Source/
  Program.cs
  Game1.cs

  Scenes/
    MainMenuScene.cs
    GameplayScene.cs
    GameOverScene.cs

  Components/
    FormationController.cs
    InvaderData.cs
    PlayerController.cs
    BulletController.cs
    ShieldChunk.cs
    UFOController.cs
    HudRenderer.cs
    PauseOverlay.cs

  SceneComponents/
    WaveManager.cs
    BassRhythm.cs
    GameState.cs

  Persistence/
    HighScoreStore.cs
    SettingsStore.cs
```

Add files only as needed by the current milestone.

## Milestones

### Milestone 1: Core Playable

The game works with placeholder shapes. Nez project setup with `SceneResolutionPolicy.ShowAll` at 960x720. GameplayScene with formation entity (parent + 55 invader children), player cannon, 4 shields (64 chunk entities), and bullets. `FormationController` handles march, speed-up, and enemy fire. `PlayerController` handles movement and single-shot firing. Collision via Nez physics layers and `ITriggerListener`. `GameState` tracks score, lives, extra life at 1500. `WaveManager` handles wave progression. Game over on death or invasion.

### Milestone 2: Presentation and Flow

Generated neon-vector sprites and SFX replace placeholders. `SpriteAnimator` for invader two-frame animation. `BassRhythm` SceneComponent for adaptive heartbeat. MainMenuScene and GameOverScene with scene transitions. `PauseOverlay` on GameplayScene. `HudRenderer` for score, high score, lives, wave. `CameraShake` on kills and death. `ParticleEmitter` for explosions and bullet trails. Tweened wave transition text and score popups.

### Milestone 3: Polish and Extras

Mystery UFO with `UFOController` and random scoring. High score and settings persistence. Options menu (volume, fullscreen, screen shake). Respawn invulnerability flicker. Fullscreen with proper scaling. Difficulty tuning pass.

## Out of Scope

Multiplayer, online leaderboards, powerups, weapon upgrades, boss enemies, story/campaign, level editor, twin-stick controls, pixel-perfect collision.

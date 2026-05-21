# Asteroids Modernized - Specification

## Vision

Build a 2D modernized version of `Asteroids` (1979) in MonoGame. The game should preserve the arcade core while using modern presentation: generated high-resolution sprites, crisp effects, better input support, readable UI, satisfying audio, and polished screen flow.

The first version should stay arcade-focused. Do not add shops, upgrades, roguelite progression, campaign levels, story, or complex meta-progression until the core loop is proven.

## Core Direction

- Arcade-faithful movement, shooting, screen wrapping, asteroid splitting, scoring, lives, waves, and later saucers.
- Modernized look and feel through neon-vector-inspired sprites, particles, flashes, screen shake, sound effects, and UI polish.
- Use a fixed `960x720` virtual resolution as the full game playfield.
- Use keyboard and classic gamepad controls.
- Use MonoGame.Extended selectively as helper tooling, not as the main gameplay architecture.

## Technical Baseline

- Engine: MonoGame.
- Language: C#.
- Target resolution: `960x720` virtual resolution.
- Default window: `960x720`.
- Aspect ratio: `4:3`.
- Screen wrapping bounds: virtual `960x720` playfield.
- Project structure:

```text
Asteroids/
  Source/
    Program.cs
    Game1.cs

  Content/
    Content.mgcb

  Assets/
    Icon.ico
    Icon.bmp
    app.manifest
```

The structure will expand as features are implemented.

## Gameplay

### Player Ship

- The ship rotates independently of velocity.
- Thrust accelerates the ship in its facing direction.
- The ship coasts when thrust is released.
- Ship movement should feel classic and inertial.
- Use a modern maximum speed cap for fairness and tuning.
- Very light damping may be added later only if playtesting requires it.
- Screen wrapping preserves velocity.

Recommended starting size:

```text
ship sprite: ~42x42 px
ship collision radius: 16 px
```

### Controls

Keyboard:

```text
Left / A       rotate counterclockwise
Right / D      rotate clockwise
Up / W         thrust
Space          fire
Left Shift     hyperspace, later milestone
Escape         pause / back
Enter          confirm menu
```

Gamepad:

```text
Left stick X / D-pad left-right   rotate ship
X / Left trigger                  thrust
A / Right trigger                 fire
Y                                hyperspace, later milestone
Start                            pause
B                                back / cancel
```

Gamepad should use classic rotate/thrust/fire controls. Do not use twin-stick aiming in the main mode.

### Shooting

- Shots fire forward from the ship nose.
- Bullets travel in the ship's facing direction.
- Bullets inherit a small amount of ship velocity.
- Bullets expire after a short lifetime.
- Enforce a strict active bullet limit.

Recommended starting values:

```text
bullet speed: 520 px/s
bullet lifetime: 1.1 s
fire cooldown: 0.16 s
max active player bullets: 4
bullet collision radius: 3 px
bullet sprite size: 6-8 px
```

### Asteroids

Use three asteroid sizes with classic split behavior:

```text
large asteroid destroyed  -> 2 medium asteroids
medium asteroid destroyed -> 2 small asteroids
small asteroid destroyed  -> gone
```

Child asteroids should:

- Spawn near the destroyed parent.
- Receive a new random-ish direction.
- Move slightly faster than the parent tier.
- Use alternate sprite variants when available.
- Avoid spawning directly on top of the player where practical.

Recommended starting values:

```text
large asteroid sprite: ~88 px
medium asteroid sprite: ~52 px
small asteroid sprite: ~30 px

large collision radius: 42 px
medium collision radius: 25 px
small collision radius: 14 px

large score: 20
medium score: 50
small score: 100
```

### Collision

Use circle-based gameplay collision. Sprite visuals may be irregular, but gameplay collision should be predictable and forgiving.

Initial collision pairs:

```text
player bullet vs asteroid
player ship vs asteroid
player bullet vs saucer, milestone 3
saucer bullet vs player, milestone 3
saucer vs asteroid, milestone 3 if useful
```

Use a custom lightweight `CollisionSystem` first:

```text
distanceSquared <= (radiusA + radiusB)^2
```

MonoGame.Extended collision may be considered later if object counts or collision layers become complex.

### Death And Respawn

- Player starts with `3` lives.
- On collision, the ship explodes and becomes inactive.
- Respawn after a short delay.
- Respawn at screen center.
- Use temporary invulnerability after respawn.
- Only respawn when the center area is reasonably clear.
- Game over when lives reach `0`.

Recommended starting values:

```text
starting lives: 3
death delay: 1.25 s
respawn invulnerability: 2.0 s
safe respawn radius: 140 px around center
```

### Waves

- The game progresses through classic asteroid waves.
- A wave begins with several large asteroids.
- When all asteroids are cleared, the next wave begins.
- New waves increase asteroid count and/or speed.
- Asteroids should spawn near screen edges and away from the player.
- Show a brief `Wave N` transition before spawning the next wave.

Recommended curve:

```text
wave 1: 4 large asteroids
wave 2: 5 large asteroids
wave 3: 6 large asteroids
wave 4+: cap count around 8, then increase speed slightly
```

### Hyperspace

Include hyperspace in the full design, but defer it until after the core loop works.

Intended behavior:

- Instantly relocate the ship to a random safe-ish position.
- Add a short invulnerability flicker after arrival.
- Add a cooldown between uses.
- Optional later: small chance of dangerous arrival or failure.

### Saucer Enemy

Include saucers in the full design, but defer until milestone 3.

Intended behavior:

- Saucer occasionally enters from a screen edge.
- Moves horizontally or diagonally across the playfield.
- Fires at the player with imperfect aim.
- Can be destroyed for bonus points.
- Can interact with asteroids if useful.

## Screens And Flow

The game starts at the main menu. There is no separate user-facing boot/loading screen in the initial version.

Screen flow:

```text
Main Menu -> Playing -> Paused -> Playing
Main Menu -> Playing -> Game Over -> Restart / Main Menu
```

Main Menu:

```text
Start Game
Options
Quit
```

Playing HUD:

```text
score
high score
lives
wave
```

Paused:

```text
Resume
Restart
Main Menu
Quit
```

Game Over:

```text
final score
high score, including new high score indication
restart
main menu
```

## Options And Persistence

Initial options:

```text
Master volume
SFX volume
Fullscreen on/off
Screen shake on/off
```

Later options:

```text
Music/ambience volume
Keyboard remapping
Controller remapping
Colorblind/readability modes
Particle intensity
```

Persistence:

- Save high score locally.
- Save options locally when options exist.
- Use small JSON files.
- Store data outside the repository/game install folder.
- No cloud save.
- No online leaderboard.

Suggested Windows storage:

```text
%AppData%/Asteroids/highscore.json
%AppData%/Asteroids/settings.json
```

Example high score JSON:

```json
{
  "highScore": 12345
}
```

## Visual Style

Use a clean neon-vector arcade style rendered as modern 2D sprites.

Visual direction:

- Dark space background.
- Bright outlined ship and asteroids.
- Subtle glow.
- Sharp readable silhouettes.
- Minimal color accents.
- Particle trails for thrust, bullets, explosions.
- Irregular asteroid sprite shapes with simple circular collision.

Do not use pixel art for the main direction. The goal is crisp vector-like sprites exported to PNG.

## Visual Effects

Use restrained arcade juice with readability prioritized.

Effects:

- Small screen shake on asteroid break.
- Stronger shake on player death.
- Brief hit flashes.
- Thruster particles while accelerating.
- Short explosion particles.
- Subtle glow on bullets and ship.
- No camera wobble during normal movement.

Intensity guide:

```text
bullet fire: no shake or tiny recoil flash
small asteroid break: tiny shake
large asteroid break: medium shake
ship death: strongest shake
```

Effects must never obscure gameplay readability.

## Audio

Use SFX-first modern arcade audio. Do not require full gameplay music in the first version.

Initial SFX:

```text
shoot.wav
thrust_loop.wav
asteroid_hit.wav
ship_explosion.wav
menu_move.wav
menu_select.wav
```

Later SFX:

```text
ufo_spawn.wav
ufo_shoot.wav
hyperspace.wav
wave_start.wav
```

Music or ambience is optional later. If added, it should be subtle and not reduce the crisp arcade feel.

## Asset Workflow

Sprites, SFX, and music may be generated using the local `$gen-assets` workflow.

`$gen-assets` requirements:

- Verify these tools are available on `PATH` before generating assets:

```text
blender
ffmpeg
fluidsynth
inkscape
python
```

- Keep source files alongside exported assets.
- Run each CLI tool as a standalone command.
- For sprites, write SVG source files, then export PNGs using Inkscape CLI.
- For SFX, synthesize WAV files using ffmpeg.
- For music, create MIDI, render with fluidsynth, then optionally convert to OGG.

Use this project asset split:

```text
Assets/
  Icon.ico
  Icon.bmp
  app.manifest

  source/
    sprites/
      player/
      asteroids/
      enemies/
      effects/
      ui/

    audio/
      sfx/
      music/

Content/
  Content.mgcb

  sprites/
    player/
    asteroids/
    enemies/
    effects/
    ui/

  audio/
    sfx/
    music/

  fonts/
```

Meaning:

```text
Assets/source/ = editable/generated source assets such as SVG, MIDI, scripts
Content/       = runtime-ready assets loaded by MonoGame
Assets/        = app-level build resources plus generated source files
```

Generated PNGs in `Content/` should start as individual sprites. Use sprite sheets only where animation/effects need them.

Example:

```text
Assets/source/sprites/player/ship.svg
Content/sprites/player/ship.png
```

Initial sprite list:

```text
Content/sprites/player/ship.png
Content/sprites/asteroids/large_01.png
Content/sprites/asteroids/large_02.png
Content/sprites/asteroids/medium_01.png
Content/sprites/asteroids/medium_02.png
Content/sprites/asteroids/small_01.png
Content/sprites/asteroids/small_02.png
Content/sprites/effects/thruster_particle.png
Content/sprites/effects/explosion_particle.png
Content/sprites/projectiles/bullet.png
Content/sprites/enemies/saucer.png, milestone 3
```

## Code Organization

Use plain C# classes plus focused systems. Do not start with ECS.

Planned structure:

```text
Source/
  Program.cs
  Game1.cs

  Core/
    GameConstants.cs
    GameState.cs
    RandomService.cs

  Input/
    InputState.cs
    InputBindings.cs

  Entities/
    PlayerShip.cs
    Asteroid.cs
    Bullet.cs
    Saucer.cs

  Systems/
    AsteroidSpawner.cs
    CollisionSystem.cs
    ScoreSystem.cs
    ScreenWrapSystem.cs
    ParticleSystem.cs

  Screens/
    MainMenuScreen.cs
    GameplayScreen.cs
    PauseScreen.cs
    GameOverScreen.cs

  Rendering/
    SpriteRenderer.cs
    VirtualResolutionRenderer.cs
    HudRenderer.cs

  Audio/
    AudioManager.cs

  Persistence/
    HighScoreStore.cs
```

Only add folders/classes as needed by the milestone being implemented.

Use a simple custom `GameState` flow for the initial implementation rather than MonoGame.Extended screen management. The first version should keep transitions explicit and easy to debug.

## MonoGame.Extended Usage

Use MonoGame.Extended selectively as helper tooling.

Good candidates:

- Input helpers such as `KeyboardExtended`.
- Camera or viewport helpers if they simplify `960x720` virtual scaling, letterboxing, or screen shake.
- Particles if the Extended particle API feels lighter than custom particles.
- Tweening for menu transitions, score popups, wave text, and fades.
- Shape helpers where useful.

Avoid using MonoGame.Extended for:

- The main gameplay architecture.
- Screen management for the initial implementation.
- ECS/entity framework as the default architecture.
- Tiled maps.
- Complex collision pipeline unless the simple custom collision system becomes insufficient.

## Milestones

### Milestone 1: Core Playable

- `960x720` virtual resolution.
- Player ship movement.
- Keyboard and classic gamepad input.
- Screen wrapping.
- Bullets with active limit.
- Asteroids with three sizes and splitting.
- Circular collisions.
- Score.
- Lives.
- Game over.
- Wave progression.

This milestone may use placeholder geometric sprites if generated assets are not ready.

### Milestone 2: Presentation And Flow

- Generated sprites.
- Generated SFX.
- Main menu.
- Pause screen.
- Game over screen.
- Local high score persistence.
- HUD polish.
- Screen shake.
- Particles.
- Hit flashes.
- Options menu with volume, fullscreen, and screen shake.

### Milestone 3: Classic Extras

- Saucer enemy.
- Hyperspace.
- More asteroid variants.
- Additional tuning.
- Optional ambience/music.
- Controller polish.
- Additional accessibility/readability options if needed.

## Non-Goals For Initial Version

- Online leaderboard.
- Multiplayer.
- Shops/upgrades.
- Roguelite progression.
- Campaign/story mode.
- Level editor.
- Tiled map support.
- Twin-stick aiming in the main mode.
- Complex polygon collision.
- Full ECS architecture.

## Design Principles

- Preserve the classic Asteroids loop first.
- Prefer readable gameplay over visual noise.
- Keep implementation simple until complexity is earned.
- Use generated assets, but keep editable source files tracked.
- Build the game in playable milestones.
- Do not let polish delay the first playable loop.

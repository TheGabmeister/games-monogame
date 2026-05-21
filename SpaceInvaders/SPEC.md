# Space Invaders Modernized - Specification

## Vision

Build a 2D modernized version of Space Invaders (1978) in MonoGame. The game preserves the arcade core — formation march, single-shot cannon, destructible shields, speed-up tension — while using modern presentation: crisp neon-vector sprites, particles, screen shake, adaptive audio, and polished screen flow.

The first version stays arcade-focused. Do not add powerups, weapon upgrades, roguelite progression, campaign levels, or complex meta-progression until the core loop is proven.

## Core Direction

- Arcade-faithful formation march, shooting, shields, UFO, scoring, lives, waves, and the iconic speed-up.
- Modernized look and feel through neon-vector-inspired sprites, particles, screen shake, synthesized SFX, and an adaptive bass rhythm loop.
- Use a fixed `960x720` virtual resolution as the full game playfield.
- Use keyboard and classic gamepad controls.
- Use MonoGame.Extended selectively for screen management, input helpers, tweening, particles, and camera — not as the main gameplay architecture.

## Technical Baseline

- Engine: MonoGame with MonoGame.Extended.
- Language: C#.
- Target resolution: `960x720` virtual resolution.
- Default window: `960x720`.
- Aspect ratio: `4:3`.
- Project structure:

```text
SpaceInvaders/
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

### Invader Formation

The formation is an 11x5 grid (55 invaders) with 3 invader types:

```text
Row 5:      squid-type    (top row, 30 pts)
Rows 3-4:   crab-type     (middle rows, 20 pts)
Rows 1-2:   octopus-type  (bottom rows, 10 pts)
```

The formation marches left until the edge invader hits the boundary, drops one row, then marches right — repeating this pattern down the screen.

Movement is smooth and interpolated, not the original's discrete stepping. The formation moves as a single unit.

### Speed-Up

Speed scales inversely with remaining invaders:

```text
speed = baseSpeed * (55 / remainingInvaders)
```

Cap at a maximum speed so the last invader is fast but hittable. This recreates the original's iconic tension ramp where the final invaders are screaming across the screen.

### Player Cannon

- The player moves horizontally along the bottom of the screen at constant speed.
- One active bullet on screen at a time — cannot fire again until the shot hits or leaves the screen.
- This single-shot constraint is fundamental to Space Invaders' strategic aiming.

Recommended starting values:

```text
cannon sprite: ~48x32 px
cannon collision box: 44x28 px
cannon speed: 280 px/s
bullet speed: 600 px/s
bullet sprite: 4x12 px
```

### Controls

Keyboard:

```text
Left / A       move left
Right / D      move right
Space          fire
Escape         pause / back
Enter          confirm menu
```

Gamepad:

```text
Left stick X / D-pad left-right   move
A                                 fire
Start                             pause
B                                 back / cancel
```

### Enemy Shooting

- Only the bottom-most invader in each column can fire.
- A single enemy bullet type.
- Maximum 3 active enemy bullets on screen at a time.
- Fire rate is semi-random per interval and scales up with wave number.

Recommended starting values:

```text
enemy bullet speed: 320 px/s
base fire interval: 0.8-1.5 s (random within range)
fire interval reduction per wave: ~5%
max active enemy bullets: 3
enemy bullet sprite: 4x12 px
```

### Shields

4 destructible shields positioned between the player and the formation.

Each shield is divided into 12-16 breakable chunks (segments). When a bullet (player or enemy) hits a chunk, that chunk is destroyed. Invaders also destroy chunks by marching through them.

Recommended layout:

```text
shield sprite: ~60x48 px
chunks per shield: 16 (4x4 grid)
chunk size: ~15x12 px
shield positions: evenly spaced across the playfield
shield Y position: ~100 px above the player
```

### Mystery UFO

A mystery ship periodically flies across the top of the screen.

- Spawns at a random interval (every 20-30 seconds).
- Flies horizontally at constant speed from one edge to the other.
- Awards a random score from: 50, 100, 150, 200, 300 points.
- Despawns if it reaches the far edge without being hit.

Recommended starting values:

```text
UFO sprite: ~48x20 px
UFO speed: 160 px/s
spawn interval: 20-30 s (random)
```

### Collision

Use axis-aligned bounding box (AABB) collision. All game objects are roughly rectangular, making AABB a natural fit.

Collision pairs:

```text
player bullet vs invader
player bullet vs enemy bullet (optional, cancel both)
player bullet vs shield chunk
player bullet vs UFO
enemy bullet vs player
enemy bullet vs shield chunk
invader vs shield chunk (march-through destruction)
invader vs bottom line (game over trigger)
```

### Death and Respawn

- Player starts with 3 lives.
- Extra life awarded at 1500 points (one-time).
- On death, the cannon explodes and becomes inactive.
- Respawn after a short delay at the same horizontal position.
- Brief invulnerability flicker after respawn.
- Game over when lives reach 0 or invaders reach the player's row.

Recommended starting values:

```text
starting lives: 3
extra life at: 1500 pts
death delay: 1.5 s
respawn invulnerability: 1.5 s
```

### Waves

- A wave begins with a full 11x5 formation.
- When all invaders are destroyed, the next wave begins after a brief transition.
- Each subsequent wave:
  - Formation starts one row lower (to a minimum safe distance above shields).
  - Base movement speed increases slightly.
  - Enemy fire rate increases slightly.
- Show a brief `Wave N` indicator before spawning the next formation.

Recommended curve:

```text
wave 1: default start height, base speed, base fire rate
wave 2: 1 row lower, speed +10%, fire rate +5%
wave 3: 2 rows lower, speed +20%, fire rate +10%
wave 4+: cap start height, continue speed/fire scaling with diminishing returns
```

## Screens and Flow

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
score (top left)
high score (top center)
lives (bottom left)
wave number (top right)
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
high score (with new high score indication)
restart
main menu
```

## Options and Persistence

Options:

```text
Master volume
SFX volume
Fullscreen on/off
Screen shake on/off
```

Persistence:

- Save high score and options locally as JSON.
- Store data outside the repository/game install folder.
- No cloud save or online leaderboard.

Suggested Windows storage:

```text
%AppData%/SpaceInvaders/highscore.json
%AppData%/SpaceInvaders/settings.json
```

## Visual Style

Use a clean neon-vector arcade style rendered as modern 2D sprites.

Visual direction:

- Dark space background.
- Bright outlined invaders and cannon with color-coding per invader type.
- Subtle glow on sprites and bullets.
- Sharp readable silhouettes.
- Particle effects for explosions, bullet trails, and cannon thrust.
- Shield chunks glow when intact, dim when adjacent chunks are destroyed.

Do not use pixel art. The goal is crisp vector-like sprites exported to PNG via SVG generation.

## Visual Effects

Restrained arcade juice with gameplay readability prioritized.

Effects:

- Screen shake on player death (strongest).
- Medium screen shake when the last invader in a row is destroyed.
- Brief hit flash on invader destruction.
- Particle burst on invader death and shield chunk destruction.
- Bullet glow trails.
- Subtle pulsing glow on the player cannon.
- No camera wobble during normal gameplay.

Intensity guide:

```text
bullet fire: no shake, brief muzzle flash
invader destroyed: tiny shake, particle burst
shield chunk destroyed: no shake, small particle puff
player death: strong shake, large particle explosion
UFO destroyed: medium shake, score popup with glow
```

Effects must never obscure gameplay readability.

## Audio

SFX-first approach with an adaptive bass rhythm loop.

### Bass Rhythm

The iconic four-note bass heartbeat that speeds up as invaders are destroyed. Tempo is tied to the same speed curve as invader movement:

```text
base BPM: ~60 (one note per second at full formation)
max BPM: ~300 (last few invaders)
```

### SFX

Initial sound set:

```text
shoot.wav           player fires
invader_death.wav   invader destroyed
player_death.wav    cannon explosion
ufo_hum.wav         UFO flyover (looping)
ufo_score.wav       UFO destroyed
shield_hit.wav      shield chunk destroyed
menu_move.wav       menu cursor movement
menu_select.wav     menu selection confirm
wave_start.wav      new wave beginning
extra_life.wav      extra life awarded
```

Music beyond the bass rhythm is optional for v1. If added later, it should be subtle and not reduce the crisp arcade feel.

## Asset Workflow

Sprites, SFX, and music are generated using the local `$gen-assets` workflow.

`$gen-assets` requirements — verify these tools are available on PATH before generating:

```text
blender
ffmpeg
fluidsynth
inkscape
python
```

Asset layout:

```text
Assets/
  Icon.ico
  Icon.bmp
  app.manifest

  source/
    sprites/
      player/
      invaders/
      shields/
      effects/
      ui/

    audio/
      sfx/
      music/

Content/
  Content.mgcb

  sprites/
    player/
    invaders/
    shields/
    effects/
    ui/

  audio/
    sfx/
    music/

  fonts/
```

Meaning:

```text
Assets/source/ = editable/generated source assets (SVG, MIDI, scripts)
Content/       = runtime-ready assets loaded by MonoGame
Assets/        = app-level build resources plus generated source files
```

Initial sprite list:

```text
Content/sprites/player/cannon.png
Content/sprites/invaders/squid_01.png
Content/sprites/invaders/squid_02.png     (animation frame)
Content/sprites/invaders/crab_01.png
Content/sprites/invaders/crab_02.png
Content/sprites/invaders/octopus_01.png
Content/sprites/invaders/octopus_02.png
Content/sprites/invaders/ufo.png
Content/sprites/shields/shield_chunk.png
Content/sprites/effects/bullet_player.png
Content/sprites/effects/bullet_enemy.png
Content/sprites/effects/explosion_particle.png
Content/sprites/effects/muzzle_flash.png
```

## Code Organization

Use plain C# classes plus focused systems. Do not use ECS.

Planned structure:

```text
Source/
  Program.cs
  Game1.cs

  Core/
    GameConstants.cs
    GameState.cs

  Input/
    InputState.cs

  Entities/
    PlayerCannon.cs
    Invader.cs
    Bullet.cs
    Shield.cs
    ShieldChunk.cs
    MysteryUFO.cs

  Systems/
    FormationSystem.cs
    CollisionSystem.cs
    WaveSystem.cs
    ScoreSystem.cs
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
    BassRhythm.cs

  Persistence/
    HighScoreStore.cs
    SettingsStore.cs
```

Only add folders/classes as needed by the milestone being implemented.

## MonoGame.Extended Usage

Use MonoGame.Extended selectively as helper tooling.

Use for:

- Screen management for Main Menu, Gameplay, Pause, and Game Over.
- Input helpers such as `KeyboardExtended`.
- Camera for viewport and screen shake.
- Particles for explosions and bullet trails.
- Tweening for menu transitions, score popups, wave text, and fades.

Avoid using for:

- The main gameplay architecture or entity management.
- ECS/entity framework.
- Complex collision pipeline (custom AABB is sufficient).

## Milestones

### Milestone 1: Core Playable

- `960x720` virtual resolution with letterboxing.
- Player cannon movement and single-shot firing.
- Keyboard and gamepad input.
- 11x5 invader formation with smooth march and edge-drop behavior.
- Inverse proportional speed-up as invaders are destroyed.
- Enemy shooting (bottom-row, max 3 bullets).
- 4 shields with chunk-based destruction.
- AABB collision for all pairs.
- Score, lives, extra life at 1500.
- Wave progression (lower start, faster speed, faster fire).
- Game over on death or invasion.

This milestone may use placeholder geometric shapes if generated assets are not ready.

### Milestone 2: Presentation and Flow

- Generated neon-vector sprites for all entities.
- Generated SFX for all actions.
- Adaptive bass rhythm loop tied to invader count.
- Main menu, pause, and game over screens.
- HUD with score, high score, lives, wave number.
- Screen shake on kills and death.
- Particle effects on destruction events.
- Bullet glow trails.
- Invader animation frames (two-frame toggle).
- Wave transition text.

### Milestone 3: Polish and Extras

- Mystery UFO with random score.
- Local high score persistence.
- Settings persistence.
- Options menu (volume, fullscreen, screen shake).
- Death/respawn invulnerability flicker.
- Score popups on UFO kill.
- Additional tuning and difficulty balancing.
- Fullscreen support with proper scaling.

## Non-Goals for Initial Version

- Online leaderboard.
- Multiplayer.
- Powerups or weapon upgrades.
- Multiple game modes.
- Boss enemies.
- Story or campaign.
- Level editor.
- Twin-stick controls.
- Pixel-perfect collision.
- Full ECS architecture.

## Design Principles

- Preserve the classic Space Invaders loop first.
- Prefer readable gameplay over visual noise.
- Keep implementation simple until complexity is earned.
- Use generated assets, but keep editable source files tracked.
- Build the game in playable milestones.
- Do not let polish delay the first playable loop.

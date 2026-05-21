# Space Invaders Modernized - Specification

## Vision

A modernized Space Invaders (1978) built with MonoGame and MonoGame.Extended. The gameplay stays faithful to the original — formation march, single-shot cannon, destructible shields, the tension ramp as invaders speed up — but the presentation is modern: neon-vector sprites, particles, screen shake, synthesized SFX, and an adaptive bass rhythm that accelerates with the invaders.

No powerups, weapon upgrades, roguelite progression, or campaign until the core loop is proven.

## Technical Stack

MonoGame (DesktopGL 3.8.x) on .NET 9.0 with MonoGame.Extended 6.0.0. The game renders to a `960x720` virtual resolution (4:3 landscape), letterboxed in the window. Default window size matches the virtual resolution.

MonoGame.Extended handles screen management, input helpers (`KeyboardExtended`), camera/viewport (for screen shake), particles, and tweening. Gameplay logic itself is plain C# — no ECS, no Extended entity framework.

## The Formation

55 invaders arranged in an 11x5 grid. Three types by row:

| Rows | Type | Points |
|------|------|--------|
| 5 (top) | Squid | 30 |
| 3-4 | Crab | 20 |
| 1-2 | Octopus | 10 |

The formation moves as a single unit — march left until the outermost invader reaches the boundary, drop one row, march right, repeat. Movement is smooth and interpolated rather than the original's discrete cell-by-cell stepping.

Each invader type has two animation frames that toggle as the formation advances.

### The Speed-Up

This is the heart of Space Invaders. As invaders are destroyed, the survivors move faster:

```
speed = baseSpeed * (55 / remainingInvaders)
```

Capped at a maximum so the last invader is fast but still hittable. The bass rhythm loop (see Audio) is tied to this same curve — the whole game accelerates together.

### Enemy Fire

Only the bottom-most invader in each column can shoot. There's a single enemy bullet type, with a maximum of 3 enemy bullets on screen at once. The fire interval is randomized within a range (roughly 0.8-1.5s at wave 1) and tightens as waves progress (~5% per wave).

## The Player

The cannon moves horizontally at a constant speed along the bottom of the playfield. The defining constraint: **one bullet on screen at a time**. You can't fire again until your shot connects or leaves the screen. This forces deliberate aim and is core to why Space Invaders works.

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

4 shields spaced evenly across the playfield, positioned about 100 px above the cannon. Each shield is divided into a 4x4 grid of breakable chunks (16 per shield). Any bullet — player or enemy — destroys the chunk it hits. Invaders marching through shields also destroy chunks on contact.

Shield sprite is roughly 60x48 px total, making each chunk about 15x12 px.

## Mystery UFO

A bonus ship that flies across the top of the screen at 160 px/s. It spawns at random intervals (every 20-30 seconds), entering from one edge and exiting the other. Shooting it awards a random score drawn from {50, 100, 150, 200, 300}. If it reaches the far side, it simply disappears.

## Collision

AABB (axis-aligned bounding box) for everything. The game's objects are all roughly rectangular, so this is a natural fit.

The collision pairs:

- Player bullet vs. invader, shield chunk, UFO, and (optionally) enemy bullet
- Enemy bullet vs. player and shield chunk
- Invader body vs. shield chunk (march-through) and the bottom line (game over trigger)

## Waves

Each wave starts with a fresh 11x5 formation. After clearing all invaders, a brief "Wave N" transition plays before the next formation spawns. Difficulty ramps across waves in three ways:

1. **Lower starting position** — the formation begins one row closer to the player each wave, down to a minimum safe distance above the shields.
2. **Faster base speed** — roughly +10% per wave.
3. **Faster enemy fire** — the fire interval tightens by ~5% per wave.

## Screens

Four screens connected by MonoGame.Extended's screen manager:

**Main Menu** — Start Game, Options, Quit.

**Playing** — The gameplay screen with a HUD showing score (top-left), high score (top-center), wave number (top-right), and remaining lives (bottom-left).

**Paused** — Overlay with Resume, Restart, Main Menu, Quit.

**Game Over** — Final score, high score (with a callout if it's a new record), Restart, Main Menu.

## Visual Style

Neon-glow vector aesthetic on a dark background. Bright outlined sprites with color-coding per invader type, subtle glow on bullets and the cannon, sharp silhouettes that prioritize readability. Sprites are generated as SVGs and exported to PNG — no pixel art.

### Effects

Restrained juice — effects accent key moments without cluttering the screen:

- **Player death:** Strong screen shake, large particle explosion.
- **Invader destroyed:** Tiny shake, particle burst, brief hit flash.
- **Shield chunk destroyed:** Small particle puff, no shake.
- **UFO destroyed:** Medium shake, glowing score popup.
- **Bullets:** Subtle glow trails.
- **Cannon:** Faint pulsing glow, muzzle flash on fire.
- **Normal gameplay:** No camera wobble or ambient shake.

Readability always wins over spectacle.

## Audio

SFX-first, with one critical musical element: the adaptive bass rhythm.

### Bass Rhythm

The iconic four-note heartbeat that defines Space Invaders' soundscape. It starts at ~60 BPM with a full formation and accelerates alongside the invaders, reaching ~300 BPM with the last few survivors. This is tied directly to the speed-up formula.

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

Plain C# classes with focused systems — no ECS. The rough shape:

- **Entities** (`PlayerCannon`, `Invader`, `Bullet`, `Shield`/`ShieldChunk`, `MysteryUFO`) — data and per-entity behavior.
- **Systems** (`FormationSystem`, `CollisionSystem`, `WaveSystem`, `ScoreSystem`, `ParticleSystem`) — cross-cutting logic that operates on groups of entities.
- **Screens** (`MainMenuScreen`, `GameplayScreen`, `PauseScreen`, `GameOverScreen`) — managed by MonoGame.Extended's screen system.
- **Rendering** (`SpriteRenderer`, `VirtualResolutionRenderer`, `HudRenderer`) — drawing concerns separated from logic.
- **Audio** (`AudioManager`, `BassRhythm`) — sound playback and the adaptive rhythm.
- **Persistence** (`HighScoreStore`, `SettingsStore`) — JSON read/write for saved data.
- **Core** (`GameConstants`, `GameState`) and **Input** (`InputState`) round it out.

Add folders and classes only as needed by the current milestone.

## Milestones

### Milestone 1: Core Playable

The game works with placeholder shapes. Player cannon moves and shoots (one bullet at a time). The 11x5 formation marches, drops, and speeds up. Bottom-row invaders fire back (max 3 bullets). 4 shields with chunk destruction. AABB collision for all pairs. Score, lives, extra life at 1500, wave progression, and game over on death or invasion.

### Milestone 2: Presentation and Flow

Generated neon-vector sprites and SFX replace placeholders. The adaptive bass rhythm loop is in. All four screens are implemented (main menu, gameplay, pause, game over). HUD shows score, high score, lives, and wave. Screen shake, particles, bullet trails, hit flashes, and invader two-frame animation are added. Wave transitions show a "Wave N" indicator.

### Milestone 3: Polish and Extras

Mystery UFO with random scoring. High score and settings persistence. Options menu (volume, fullscreen, screen shake). Respawn invulnerability flicker. Score popups. Fullscreen with proper scaling. Difficulty tuning pass.

## Out of Scope

Multiplayer, online leaderboards, powerups, weapon upgrades, boss enemies, story/campaign, level editor, twin-stick controls, pixel-perfect collision, ECS architecture.

# Strikers 1945 — ECS Bullet Hell (Plan)

A vertically-scrolling shoot-'em-up inspired by Psikyo's *Strikers 1945*, built on
MonoGame.Extended.ECS. The point of the project is to **learn ECS by building a real
game** — but we use ECS *where it earns its keep*, not dogmatically (see §6).

Scope and decisions are locked in §6. This doc is the working spec.

---

## 1. Design Pillars

1. **Top-down vertical scroller.** Background scrolls down; the player flies "up" the map.
2. **Bullet hell.** Lots of enemy bullets, readable patterns, a *tiny* player hitbox so
   weaving through dense fire feels fair.
3. **Score-attack arcade loop.** Lives, bombs, power-ups.
4. **Small but complete.** **One playable ship, one stage** (a timed sequence of enemy
   waves ending in a "STAGE CLEAR"). That is the definition of "done" for v1.

### What we are *not* doing in v1
- **No boss.** Too complex for now; the stage ends after a final wave. The architecture
  (`Health`, `Emitter`, multi-phase patterns) leaves room to add one later.
- Multiple selectable aircraft with unique charge/special attacks (Strikers' signature) —
  deferred. The `Weapon` component is designed so it can grow into this.
- Charge shot, two-player co-op, stage 2+.

---

## 2. Reference: what *Strikers 1945* actually does

So we agree on the inspiration before trimming to scope:

- Pick 1 of several WWII-era planes. Each has a **main shot** (auto-fires, levels up via
  power-ups), a **charge shot** (hold to release), and a **bomb** (screen-clearing special).
- Power-up items: weapon level (P), bombs (B), points/medals.
- Enemies fly scripted paths and fire **patterns** (aimed shots, spreads, rings, spirals).
- Bosses have multiple phases. Player death costs a life and drops weapon level.

**v1 takes:** one ship's main shot + bomb + power-ups + scripted enemy waves & patterns.
**v1 drops:** charge shot, multi-ship, bosses.

---

## 3. ECS Architecture

We extend the existing setup (`Components/` = plain data, `Source/Systems/` = behavior).
The current `Player` component bundles position+speed; we split that into small reusable
components so systems compose. (On where we *don't* force ECS, see §6.)

### Components (data only)
| Component | Fields | Used by |
|---|---|---|
| `Transform` | `Vector2 Position`, `float Rotation`, `float Scale` | everything visible |
| `Velocity` | `Vector2 Value` | MovementSystem |
| `Sprite` | `Texture2D`, source rect, `Vector2 Origin`, `Color`, `float LayerDepth` | RenderSystem |
| `Player` | input state, weapon level, bomb count, lives, invuln timer | PlayerControlSystem |
| `Enemy` | type id, score value, path id | EnemyAISystem |
| `Weapon` | fire interval, cooldown timer, pattern id, level | WeaponSystem |
| `Bullet` | damage | CollisionSystem |
| `Faction` | `enum { Player, Enemy }` | CollisionSystem (who hits whom) |
| `Health` | `int Current/Max` | DamageSystem |
| `CircleCollider` | `float Radius` | CollisionSystem |
| `Lifetime` | seconds remaining, or "despawn when offscreen" flag | LifetimeSystem |
| `MovementPattern` | path/script id + elapsed time | MovementPatternSystem |
| `Emitter` | bullet-pattern descriptor + timers | EmitterSystem |
| `PowerUp` | kind (`Weapon/Bomb/Score`) | CollisionSystem |

Tag-style components (`Player`, `Bullet`) double as filters for `Aspect.All(...)`.

### Systems (update order matters)
1. `InputSystem` — read keyboard/gamepad → player intent.
2. `PlayerControlSystem` — apply intent: move (clamped to screen), trigger fire/bomb.
3. `WeaponSystem` — count down cooldowns, spawn player bullets per weapon level.
4. `EnemySpawnSystem` — wave/timeline scheduler creates enemies.
5. `EnemyAISystem` / `MovementPatternSystem` — drive enemy + scripted-bullet motion.
6. `EmitterSystem` — enemies emit bullet patterns (aimed/spread/ring/spiral).
7. `MovementSystem` — `Position += Velocity * dt` for everything plain-moving.
8. `CollisionSystem` — circle checks across factions (see §4).
9. `DamageSystem` — apply damage, kill at 0 HP, spawn pickups/explosions.
10. `LifetimeSystem` — despawn expired / offscreen entities (bullet cleanup).
11. `BackgroundScrollSystem` — scroll the stage.
12. `RenderSystem` — draw sprites by layer (generalized from the current square-drawer).
13. `HudSystem` — score, lives, bombs.

### EntityFactory
A single `EntityFactory` (per the StarWarrior sample referenced in AGENTS.md) builds
configured entities: `CreatePlayer()`, `CreateEnemy(type, pos)`, `CreateBullet(...)`,
`CreatePowerUp(...)`. Systems call the factory rather than `new`-ing components inline.

---

## 4. Key Technical Decisions

- **Resolution.** Fixed **600×800 portrait** virtual canvas, scaled to the window. All
  spawn/hitbox math is in virtual pixels so it's window-size independent.
- **Input.** Keyboard **and** gamepad from the start. `InputSystem` normalizes both into a
  single intent (move vector, fire, bomb) so no other system cares which device is used.
- **Collision = circles.** Bullet hell standard. The player's hitbox is much smaller than
  its sprite (~3–4px). Pairs we actually test:
  - enemy bullets → player (1 player, so O(bullets) — cheap)
  - player bullets → enemies (few enemies — cheap)
  - player → power-ups / enemies (graze/ram)
  Brute force is fine at v1 scale; revisit spatial hashing only if profiling demands it.
- **Bullets: naive create/destroy first.** Create and destroy bullet entities each frame
  as needed — no pooling yet. If GC/perf hurts under dense patterns, add object pooling
  then. Flagged as the #1 thing to watch, deliberately deferred.
- **Bullet patterns as data.** Patterns (spread count, angle, speed, spiral rate) live in
  the `Emitter`/`MovementPattern` component as parameters, not hardcoded per enemy — so we
  author enemies by tuning numbers, not writing code.
- **Frame-rate independence.** Scale all motion by `gameTime` dt. The current
  `PlayerSystem` moves by a fixed `+= 5` per frame — Phase 0 fixes this.
- **Assets.** Placeholder sprites only for now (see §7). Real art/SFX is a later pass.

---

## 5. Phased Roadmap (vertical slices)

Each phase ends in something runnable.

- **Phase 0 — Foundation.** Set 600×800 portrait canvas. Split `Player` into
  `Transform`/`Velocity`/`Sprite`; add the generalized `RenderSystem` + `MovementSystem` +
  `EntityFactory`; add `InputSystem` (keyboard+gamepad); actually spawn the player; clamp
  movement to screen bounds. *(Today no player entity is created, so nothing renders — this
  phase fixes that.)*
- **Phase 1 — Player shooting.** `Weapon` + `WeaponSystem`; player bullets travel up and
  despawn offscreen via `LifetimeSystem`.
- **Phase 2 — Enemies & combat.** `EnemySpawnSystem`, basic moving enemies, `CircleCollider`
  + `CollisionSystem` + `Health`/`DamageSystem`; killing enemies and dying.
- **Phase 3 — Bullet hell.** `EmitterSystem` with parameterized patterns (aimed, spread,
  ring, spiral); tiny player hitbox; invulnerability frames on respawn.
- **Phase 4 — Stage & arcade loop.** Scrolling background, a full wave timeline ending in
  STAGE CLEAR, score/lives/bomb HUD, bomb that clears bullets, power-up drops that level
  the weapon, game-over.
- **Phase 5 — Polish.** Title/game-over/pause screens, explosion particles, SFX/music
  hooks, difficulty tuning. (Real art swaps in for placeholders here.)

Post-v1 candidates: **boss fights**, charge shot, multiple ships, stage 2, medal chains.

---

## 6. Decisions (locked)

1. **Scope:** one ship, one stage, **no boss**.
2. **Display:** portrait, **600×800** virtual pixels.
3. **Input:** keyboard **and** gamepad.
4. **ECS philosophy:** use ECS where it makes sense, not everywhere. Per-entity gameplay
   data and behavior (ships, bullets, enemies, pickups → movement, collision, damage,
   rendering) lives in components/systems. Singletons and flow control that aren't
   "many similar things" — the wave timeline, scene/screen state, global score — can be
   plain classes the systems read from, rather than contorted into the ECS.
5. **Performance:** start with naive create/destroy; add pooling only if measured perf
   demands it.
6. **Assets:** placeholder sprites only for now (see §7).

---

## 7. Placeholder Sprite List (for the artist)

All sizes are in the 600×800 virtual canvas. Top-down view (aircraft point "up" the
screen). Simple, readable, single-frame PNGs with transparency unless noted. Hitboxes are
*gameplay* circles and are intentionally smaller than the art.

### Player
| Asset | Size (px) | Notes |
|---|---|---|
| `player_ship` | ~48×48 | Player aircraft, nose pointing up. ~3–4px hitbox at center. |
| `player_ship_left` / `player_ship_right` | ~48×48 | Optional banking frames when strafing. Skip if too much. |

### Enemies (a few archetypes is enough for v1)
| Asset | Size (px) | Notes |
|---|---|---|
| `enemy_popcorn` | ~32×32 | Cheap, weak fodder enemy. Flies in formations. |
| `enemy_fighter` | ~40×40 | Mid enemy that fires aimed shots. |
| `enemy_gunship` | ~64×64 | Larger, more HP, emits spread/ring patterns. |

### Bullets (small, high-contrast, must read against background)
| Asset | Size (px) | Notes |
|---|---|---|
| `bullet_player` | ~8×16 | Player main shot. Bright (e.g. cyan/white). |
| `bullet_enemy_round` | ~12×12 | Round enemy bullet (rings/spirals). Warm color. |
| `bullet_enemy_needle` | ~6×18 | Fast aimed "needle" shot. |

### Power-ups / pickups (~24×24, distinct shapes + letters)
| Asset | Size (px) | Notes |
|---|---|---|
| `powerup_weapon` | ~24×24 | "P" icon — raises weapon level. |
| `powerup_bomb` | ~24×24 | "B" icon — adds a bomb. |
| `powerup_score` | ~24×24 | Medal/coin — points only. |

### FX & background
| Asset | Size (px) | Notes |
|---|---|---|
| `explosion` | ~64×64 | Small sprite sheet (e.g. 4–6 frames) or a single puff for now. |
| `bg_tile` | 600×800 (or tileable strip) | Vertically-scrolling background (terrain/sea). Seamless top↔bottom. |

### HUD
| Asset | Size (px) | Notes |
|---|---|---|
| `hud_life_icon` | ~24×24 | Small ship icon for remaining lives. |
| `hud_bomb_icon` | ~24×24 | Small bomb icon for bomb stock. |
| Font | — | A bitmap/SpriteFont for score & "STAGE CLEAR"/"GAME OVER". |

Until these arrive, every entity renders as a solid colored quad (the current 1×1-pixel
approach), so development is never blocked on art.

---

## 8. Placeholder Sound List (for the SFX person)

Short, punchy arcade SFX. `.wav` for short one-shots (lowest-latency through the content
pipeline), `.ogg` for music/long loops. Mono is fine for one-shots. Keep them quiet enough
to layer — many can play at once during dense fire.

| Asset | Type | Notes |
|---|---|---|
| `sfx_player_shot` | one-shot | Player main shot. Fires often — keep it short & soft. |
| `sfx_enemy_shot` | one-shot | Enemy bullet spawn. Distinct from player shot. |
| `sfx_enemy_hit` | one-shot | Bullet connects with an enemy (chip damage tick). |
| `sfx_enemy_explode` | one-shot | Enemy destroyed. |
| `sfx_player_explode` | one-shot | Player death — bigger, more dramatic. |
| `sfx_powerup` | one-shot | Picking up any power-up. |
| `sfx_powerup_weapon` | one-shot | Optional distinct "weapon level up" sting. |
| `sfx_bomb` | one-shot | Bomb activation / screen clear — big whoosh/boom. |
| `sfx_graze` | one-shot | Optional: bullet narrowly misses the player hitbox. |
| `sfx_menu_move` | one-shot | Title/menu cursor move. |
| `sfx_menu_select` | one-shot | Confirm on a menu. |
| `sfx_stage_clear` | one-shot | "STAGE CLEAR" jingle. |
| `music_stage` | loop (.ogg) | Stage background music, seamless loop. |
| `music_title` | loop (.ogg) | Title screen music. |
| `music_gameover` | one-shot/short | Game-over sting. |

Until these arrive, the SFX/music systems are wired as no-ops, so audio is never a blocker.

---

## 9. Asset Folder Structure

**Yes — everything loaded via `Content.Load<T>()` must live under `Content/` and be
registered in `Content/Content.mgcb`.** The content pipeline builds those into `.xnb` files
at `bin/.../Content/`; `Content.RootDirectory` is `"Content"`, so load paths are relative to
it *without* extensions (e.g. `Content.Load<Texture2D>("sprites/player_ship")`).

We organize *inside* `Content/` by type so the `.mgcb` list stays readable:

```
Content/
  Content.mgcb            # pipeline manifest — every asset below is registered here
  sprites/
    player/               # player_ship, banking frames
    enemies/              # enemy_popcorn, enemy_fighter, enemy_gunship
    bullets/              # bullet_player, bullet_enemy_round, bullet_enemy_needle
    powerups/             # powerup_weapon/bomb/score
    fx/                   # explosion sheet
    hud/                  # life/bomb icons
  backgrounds/            # bg_tile
  fonts/                  # SpriteFont (.spritefont) for score / messages
  audio/
    sfx/                  # sfx_*.wav
    music/                # music_*.ogg
```

Load-path convention (mirrors the folders): `sprites/enemies/enemy_fighter`,
`audio/sfx/sfx_bomb`, `fonts/main`, etc.

### What goes where
- **`Content/`** — anything the game loads at runtime (sprites, audio, fonts). Pipeline-built.
- **`Assets/`** (already exists) — build-time/app resources that are *not* loaded through the
  content pipeline: the app icon (`Icon.ico`/`Icon.bmp`) and `app.manifest`. Leave as-is.
- **Source art** (artist's layered `.aseprite`/`.psd`, raw audio project files) does **not**
  belong in `Content/` — only the exported `.png`/`.wav`/`.ogg` do. If we want to version the
  source files, a top-level `art-src/` (git-tracked, not built) keeps them out of the pipeline.

> Pipeline reminder (from AGENTS.md): add each new asset through `Content/Content.mgcb` and
> keep the `/reference:..\pipeline-references\MonoGame.Extended.Content.Pipeline.dll` line —
> don't point it at a deep NuGet-cache path, or `mgcb` fails with a vague `MSB3073`.

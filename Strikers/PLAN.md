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
Phase 0 already split the original position+speed `Player` into small reusable components
(`Transform`/`Velocity`/`Sprite`); `Player` is now just a tag + control-state component, so
systems compose. (On where we *don't* force ECS, see §6.)

### Components (data only)
| Component | Fields | Used by |
|---|---|---|
| `Transform` | `Vector2 Position`, `float Rotation`, `float Scale` | everything visible |
| `Velocity` | `Vector2 Value` | MovementSystem |
| `Sprite` | `Texture2D`, `Vector2 Size`, `Vector2 Origin`, `Color`, `float LayerDepth`, `Rectangle? SourceRect` (null = whole texture) | RenderSystem |
| `Animator` | clip id, elapsed, speed, queued clip id | AnimationSystem (writes current frame's rect → `Sprite.SourceRect`) |
| `Player` | speed, input intent (move/fire); + weapon level, bombs, lives, invuln timer (later phases) | PlayerControlSystem |
| `Enemy` | type id, score value, path id | EnemyAISystem |
| `Weapon` | fire interval, cooldown, bullet speed, damage (+ pattern id/level later) | WeaponSystem |
| `Bullet` | damage | CollisionSystem |
| `Health` | `int Current/Max` | DamageSystem |
| `CircleCollider` | `float Radius`, `CollisionLayer Layer`, `CollisionLayer Mask` | CollisionSystem |
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
8. `CollisionSystem` — circle checks filtered by layer/mask (see §4).
9. `DamageSystem` — apply damage, kill at 0 HP, spawn pickups/explosions.
10. `LifetimeSystem` — despawn expired / offscreen entities (bullet cleanup).
11. `BackgroundScrollSystem` — scroll the stage.
12. `AnimationSystem` — advance `Animator` clips; write the current frame's source-rect into `Sprite`.
13. `RenderSystem` — draw all `Transform`+`Sprite` entities by `LayerDepth` (back-to-front, PointClamp); honors `Sprite.SourceRect` once animation lands.
14. `HudSystem` — score, lives, bombs.

### EntityFactory
A single `EntityFactory` (per the StarWarrior sample referenced in AGENTS.md) builds
configured entities: `CreatePlayer()`, `CreateEnemy(type, pos)`, `CreateBullet(...)`,
`CreatePowerUp(...)`. Systems call the factory rather than `new`-ing components inline.

---

## 4. Key Technical Decisions

- **Resolution.** Fixed **600×800 portrait** virtual canvas (`VirtualResolution`). All
  spawn/hitbox math is in virtual pixels so it's window-size independent. Phase 0 sizes the
  back buffer to the canvas; render-target scaling to an arbitrary window is a Phase 5 polish step.
- **Input.** Keyboard **and** gamepad from the start. `InputSystem` normalizes both into a
  single intent (move vector, fire, bomb) so no other system cares which device is used.
- **Collision = circles + layer masks.** Bullet hell standard; the player's hitbox is much
  smaller than its sprite (~3–4px). Each `CircleCollider` carries a `Layer` (what it *is*)
  and a `Mask` (what it collides with), both a `[Flags] enum CollisionLayer { Player,
  PlayerBullet, Enemy, EnemyBullet, PowerUp }`. `CollisionSystem` reports a hit for a pair
  when `(a.Layer & b.Mask) != 0` in either ordering, so passive entities (enemies, power-ups)
  carry no mask and are detected by whoever masks them. Default layer/mask assignments live
  in `EntityFactory`:

  | Entity | Layer | Mask (collides with) |
  |---|---|---|
  | Player ship | `Player` | `Enemy, EnemyBullet, PowerUp` |
  | Player bullet | `PlayerBullet` | `Enemy` |
  | Enemy | `Enemy` | *(none — others mask it)* |
  | Enemy bullet | `EnemyBullet` | `Player` |
  | Power-up | `PowerUp` | *(none — player masks it)* |

  Brute force is fine at v1 scale; revisit spatial hashing only if profiling demands it.
- **Bullets: naive create/destroy first.** Create and destroy bullet entities each frame
  as needed — no pooling yet. If GC/perf hurts under dense patterns, add object pooling
  then. Flagged as the #1 thing to watch, deliberately deferred.
- **Bullet patterns as data.** Patterns (spread count, angle, speed, spiral rate) live in
  the `Emitter`/`MovementPattern` component as parameters, not hardcoded per enemy — so we
  author enemies by tuning numbers, not writing code.
- **Frame-rate independence.** Scale all motion by frame delta seconds — done in Phase 0;
  `MovementSystem` integrates `Position += Velocity * dt`. Never move by a fixed per-frame amount.
- **Animation = data-driven, ECS-owned playback.** Three-way split so it scales to v2 (bosses,
  banking, charge) without refactor. Clip *definitions* (frames, per-frame duration, loop mode
  `Once/Loop/PingPong/HoldLast`, optional frame triggers) live in an `AnimationLibrary`
  singleton — loaded once, shared across all instances (100 enemies share one clip). Per-entity
  state is the `Animator` component (pure data). `AnimationSystem` advances it and writes the
  current frame's source-rect into `Sprite`, so `RenderSystem` stays animation-agnostic.
  Gameplay systems only set `Animator.ClipId`; the system plays whatever is set, so new states
  need no system changes. We own the frame-stepping (not Extended's `AnimatedSprite`) to get
  frame triggers (muzzle flash, SFX-on-frame, active-frame hitboxes). Use Extended's
  `Texture2DAtlas`/`Texture2DRegion` for slicing only. `Clip` is plain data deserialized from
  JSON under `Content/animations/` at startup — **JSON from v1**, no code-defined clips, so the
  authoring path never moves. Each clip JSON names its sprite-sheet load path, grid size, frame
  indices, loop mode, fps/per-frame duration, and triggers; `AnimationLibrary` loads the texture
  via `Content.Load`, slices it, and registers the clip by id.
- **Assets.** Real placeholder sprites exist under `Content/sprites/` and placeholder audio
  under `Content/audio/`, wired in per phase as entities are introduced (see §5 / §7 / §8).
  `AudioManager` already plays one-shot SFX (starting with the player shot); broader SFX/music
  hookup lands as the relevant systems do.

---

## 5. Phased Roadmap (vertical slices)

Each phase ends in something runnable. Real sprites already exist under `Content/sprites/`
(see §7), so each phase wires the relevant art into its `EntityFactory.Create*` methods as
the entities are introduced — rather than deferring all art to the end.

- **Phase 0 — Foundation.** ✅ *Done.* Set 600×800 portrait canvas. Split `Player` into
  `Transform`/`Velocity`/`Sprite`; add the generalized `RenderSystem` + `MovementSystem` +
  `EntityFactory`; add `InputSystem` (keyboard+gamepad); spawn the player; clamp movement
  to screen bounds.
  *Sprites:* `sprites/player/player_ship`.
- **Phase 1 — Player shooting.** ✅ *Done.* `Weapon` + `WeaponSystem`; player bullets travel
  up and despawn offscreen via `LifetimeSystem`.
  *Sprites:* `sprites/bullets/bullet_player`.
- **Phase 2 — Enemies & combat.** ✅ *Done.* `EnemySpawnSystem`, basic moving enemies, `CircleCollider`
  (with `CollisionLayer` layer/mask) + `CollisionSystem` + `Health`/`DamageSystem`; killing
  enemies and dying. Introduces `AnimationLibrary` + `Animator` + `AnimationSystem` (and adds
  `Sprite.SourceRect`, which `RenderSystem` starts honoring), scoped to a single `explosion`
  clip defined in `animations/explosion.json` (`Once`, self-despawns via `Lifetime`) — this
  stands up the JSON clip loader so later clips are pure data, no new code.
  *Sprites:* `sprites/enemies/enemy_popcorn`, `enemy_fighter`, `enemy_gunship`;
  `sprites/fx/explosion` on death.
- **Phase 3 — Bullet hell.** ✅ *Done.* `EmitterSystem` with parameterized patterns (aimed, spread,
  ring, spiral); tiny player hitbox; invulnerability frames on respawn. (Fighter fires aimed
  needles, gunship a round spiral; spread/ring are implemented but not yet assigned to an enemy.)
  *Sprites:* `sprites/bullets/bullet_enemy_round`, `bullet_enemy_needle`.
- **Phase 4 — Stage & arcade loop.** ✅ *Done.* Scrolling background (`BackgroundScrollSystem`,
  two wrapping tiles); a full wave timeline (plain `Stage` class played back by
  `EnemySpawnSystem`) ending in STAGE CLEAR; `HudSystem` showing score/power/lives/bombs +
  STAGE CLEAR / GAME OVER banner; `BombSystem` (clears enemy bullets, blows up on-screen
  enemies, grants i-frames); `PowerUp` drops (weapon level / bomb / score) picked up via
  `CollisionSystem`; weapon-level fan in `WeaponSystem`; lives/respawn/game-over in
  `DamageSystem`. Flow + score live in the plain `GameState` service (PLAN.md §6); gameplay
  systems gate on `GameState.Phase == Playing`. Enter / Start restarts after the run ends.
  *Sprites:* `backgrounds/bg_tile`; `sprites/powerups/powerup_weapon`, `powerup_bomb`,
  `powerup_score`; `sprites/hud/hud_life_icon`, `hud_bomb_icon`; `fonts/main`.
- **Phase 5 — Polish.** Title/game-over/pause screens, explosion particles, SFX/music
  hooks, difficulty tuning, banking frames (`player_ship_left`/`_right`).

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
6. **Assets:** placeholder sprites and SFX/music only for now (see §7–§8).

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

These now exist and are wired in per phase. The `RenderSystem` scales any texture to the
requested `Size`, so a 1×1 placeholder pixel still stands in for anything not yet authored —
development is never blocked on art.

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

These now exist. `AudioManager` (`Source/AudioManager.cs`) loads one-shot SFX on demand and
silently ignores any sound not yet authored, so audio is never a blocker. Music playback
wiring comes later.

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
  animations/             # *.json clip definitions (raw-copied, NOT pipeline-built)
  backgrounds/            # bg_tile
  fonts/                  # SpriteFont (.spritefont) for score / messages
  audio/
    sfx/                  # sfx_*.wav
    music/                # music_*.ogg
```

Load-path convention (mirrors the folders): `sprites/enemies/enemy_fighter`,
`audio/sfx/sfx_bomb`, `fonts/main`, etc.

### What goes where
- **`Content/`** — anything the game loads at runtime (sprites, audio, fonts). Pipeline-built
  into `.xnb`, except the animation JSON below.
- **`Content/animations/*.json`** — clip definitions, the one exception: **raw-copied, not
  pipeline-built** (no `.xnb`). Read with `TitleContainer.OpenStream` + `System.Text.Json` at
  startup, not `Content.Load<T>`. In `Content.mgcb` add them with `/copy:` (build action Copy)
  so they land in `bin/.../Content/animations/`. The *sprite sheets* they reference are normal
  pipeline-built textures loaded via `Content.Load<Texture2D>`.
- **`Assets/`** (already exists) — build-time/app resources that are *not* loaded through the
  content pipeline: the app icon (`Icon.ico`/`Icon.bmp`) and `app.manifest`. Leave as-is.
- **Source art** (artist's layered `.aseprite`/`.psd`, raw audio project files) does **not**
  belong in `Content/` — only the exported `.png`/`.wav`/`.ogg` do. If we want to version the
  source files, a top-level `art-src/` (git-tracked, not built) keeps them out of the pipeline.

> Pipeline reminder (from AGENTS.md): add each new asset through `Content/Content.mgcb` and
> keep the `/reference:..\pipeline-references\MonoGame.Extended.Content.Pipeline.dll` line —
> don't point it at a deep NuGet-cache path, or `mgcb` fails with a vague `MSB3073`.

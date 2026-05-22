# Milestone 2 Remaining — Presentation and Flow

Six phases completing Milestone 2. Each phase delivers one system on top of the existing playable core. Phases build in order — the event bus (Phase 2) is a dependency for later presentation phases.

Already complete (not covered here): neon-vector sprites/SFX, BassRhythm, MainMenuScene, scene transitions, pause overlay.

---

## Phase 1 — Invader Two-Frame Animation

Invaders animate between their two sprite frames in sync with the formation's march rhythm. Each invader type (squid, crab, octopus) has two frames that toggle on a looping timer that accelerates alongside formation movement.

- Replace static sprite rendering on invaders with animated sprite rendering using both frames per type
- Animation auto-plays on a looping timer via SpriteAnimator, with playback speed tied to the same speed curve as formation movement (baseSpeed * totalInvaders / aliveCount)
- As invaders are destroyed, animation FPS increases alongside movement speed — the visual rhythm and movement rhythm accelerate together naturally
- All invaders animate independently but at the same rate, so they appear synchronized

### Assets

All assets already exist — squid_01/02, crab_01/02, octopus_01/02 PNGs in Content/sprites/invaders/.

---

## Phase 2 — Scene Event Bus

Introduce a scene-wide event bus using Nez's built-in Emitter, replacing direct event subscriptions between components. This decouples producers (PlayerController, UFOController, InvaderData) from consumers (GameplayScene, camera shake, particles, score popups) and provides the communication backbone for all remaining phases.

- Define a GameEvents enum covering game-significant moments: PlayerDied, InvaderKilled, UfoDestroyed, ShieldHit, WaveCleared
- Create the Emitter as a SceneComponent so any component can access it via Scene.GetSceneComponent
- Migrate GameplayScene's player death handling: instead of subscribing directly to PlayerController.Died, subscribe to the PlayerDied event on the bus. GameplayScene no longer needs a reference to PlayerController for death handling.
- Components emit events at the appropriate moments:
  - PlayerController emits PlayerDied on death
  - InvaderData emits InvaderKilled on kill (with position and point value data for later use by score popups and particles)
  - UFOController emits UfoDestroyed on hit (with position and score)
  - ShieldChunk emits ShieldHit on destruction (with position)
  - WaveManager emits WaveCleared when the last invader dies
- Existing direct C# events on GameState (ScoreChanged, LivesChanged, etc.) remain unchanged — those are UI data bindings, not gameplay events. The bus is for gameplay moments that multiple unrelated systems need to react to.

### Assets

No new assets.

---

## Phase 3 — Camera Shake

Screen shake accents key destruction moments. No shake during normal gameplay — only on player death and UFO hits. Extends Phase 2 — subscribes to event bus instead of referencing dying components.

- **Player death:** Strong shake (intensity ~15, degradation ~0.9)
- **UFO destroyed:** Medium shake (intensity ~8)
- Invader destruction and shield chunk hits: no shake
- CameraShake component is added to the scene's camera entity (Scene.Camera.Entity)
- A listener subscribes to PlayerDied and UfoDestroyed on the event bus and calls Shake with the appropriate intensity
- Shake stacks correctly — if a new shake is triggered while one is active, only the stronger intensity applies (built-in behavior)

### Assets

No new assets.

---

## Phase 4 — Particle Effects

Particle emitters for explosions and muzzle flash using the existing sprites. Extends Phase 2 — subscribes to event bus for death events to spawn particles at the right positions.

- **Invader death explosion:** Burst of 8-12 particles at invader position on kill. Particles spread outward, fade from invader's color (type-dependent: green for octopus, blue for crab, purple for squid) to transparent, shrink over ~0.3s lifetime. Emitter fires once and self-destructs when particles expire.
- **Player death explosion:** Larger burst of 15-20 particles, longer lifetime (~0.5s), white/yellow color. Triggered by PlayerDied event.
- **UFO death explosion:** Medium burst of 10-15 particles, glowing red/orange.
- **Shield chunk hit:** Small puff of 3-5 particles, green, very short lifetime (~0.15s). Triggered by ShieldHit event.
- **Muzzle flash:** Brief 1-2 frame flash sprite at cannon position on fire. Uses existing muzzle_flash.png.
- Particle emitters are created at the death position as standalone entities, not attached to the dying entity (which gets destroyed). Each emitter entity auto-destroys after all particles expire.
- All particles simulate in world space so they don't follow destroyed entities.

### Assets

**2D Sprites**
- explosion_particle.png — already exists
- muzzle_flash.png — already exists

---

## Phase 5 — Tweened Wave Transition Text and Score Popups

Visual feedback for wave progression and scoring through tweened text animations. Extends Phase 2 — subscribes to WaveCleared, InvaderKilled, and UfoDestroyed events on the bus.

**Wave transition text:**
- When all invaders are cleared, a large "WAVE N" text entity appears at screen center
- Text scales up from 0 to full size with an elastic ease over ~0.5s
- Holds for ~1.0s
- Fades out (alpha tween) over ~0.3s before the next formation spawns
- Total transition timing fits within the existing WaveTransitionDelay (2.0s)
- wave_start.wav plays when the text appears (already wired)

**Score popups:**
- When an invader is killed, a small text showing the point value ("+10", "+20", "+30") spawns at the invader's position
- Text tweens upward ~40px over ~0.8s while fading from full opacity to transparent
- Entity self-destructs after the tween completes
- UFO score popup uses the same pattern but with the random score value and a larger/brighter style
- Popups are standalone entities, not parented to the dying invader

### Assets

No new assets — uses the existing bitmap font.

---

**Vertical slice checkpoint — All Milestone 2 presentation systems are complete. The game has animated invaders, a scene event bus, screen shake on death, particle explosions, tweened wave transitions, and score popups. Missing from the full game: GameOverScene (Phase 6), persistence, options menu, and difficulty tuning (Milestone 3).**

---

## Phase 6 — Game Over Scene

A dedicated scene for the game-over state, replacing the current in-scene text overlay.

- New GameOverScene receives final score, high score, and whether a new high score was set as parameters from GameplayScene
- Display layout: "GAME OVER" title, final score prominently centered, high score below it, "NEW HIGH SCORE!" callout if applicable
- Two menu options: "Restart" (transitions to new GameplayScene) and "Main Menu" (transitions to MainMenuScene)
- Menu navigation reuses the same input pattern as MainMenuScene (up/down axis, confirm button, menu_move/menu_select SFX)
- GameplayScene triggers transition to GameOverScene on game over instead of showing the in-scene overlay
- Remove game-over text and restart handling from HudController and GameplayScene — those responsibilities move to GameOverScene
- Scene transition uses FadeTransition (consistent with existing transitions)

### Assets

**Audio**
- Reuses menu_move.wav, menu_select.wav (already exist)

**UI**
- Game Over screen layout: title, score display, high score display, new record callout, menu options

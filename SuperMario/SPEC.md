# Super Mario Bros. — Gameplay Systems Spec

Super Mario Bros., Nintendo Entertainment System / Famicom, 1985. Designed by Shigeru Miyamoto and Takashi Tezuka. Music by Koji Kondo. Single-screen, left-to-right side-scrolling platformer for 1–2 alternating players.

Canonical reference is the NES/Famicom original. Where the Famicom Disk System, *Super Mario All-Stars* (SNES), or *Super Mario Bros. Deluxe* (GBC) differ materially, the difference is noted.

---

## 1. Core Gameplay Systems

### 1.1 Primary Loop

1. Player begins a level at a `PlayerStart` position on the left of a horizontally-scrolling stage.
2. Player traverses the stage rightward, avoiding/defeating enemies, collecting Coins and power-ups, and managing a continuously decreasing Timer.
3. Standard stages end with a **Flagpole** that the player grabs to clear the stage. Castle stages (`X-4`) end with an **Axe** that drops a bridge into lava, defeating Bowser or revealing a Mushroom Retainer.
4. After 32 levels (8 worlds × 4 stages), the player rescues Princess Toadstool. The game then offers a **Hard Mode / second quest** (see §11.5).

### 1.2 Win Conditions

| Scope | Condition |
|-------|-----------|
| Level (non-castle) | Touch the Flagpole. |
| Level (castle, worlds 1–7) | Cross the bridge and touch the Axe behind the impostor Bowser; Mushroom Retainer says "Thank you Mario! But our princess is in another castle!" |
| Level (castle, 8-4) | Touch the Axe behind the true Bowser; Princess Toadstool is rescued. |
| World | Clear `X-1` through `X-4`. |
| Game | Clear World 8-4. |

### 1.3 Player Resources

| Resource | Range | Notes |
|----------|-------|-------|
| Lives | starts at 3 | Loss of all lives → Game Over. Display caps visually past ~19 (becomes a crown / glyph soup); exceeding 128 lives triggers Game Over on next death. |
| Coins | 0–99 (rolls to 00 at 100) | Each 100 collected = +1 Life. |
| Timer | 400 → 0 (or 300 in `X-3` levels) | Decrements by 1 every 24 NTSC frames (≈0.4 real seconds). Reaching 0 = death regardless of power state. See §3.6. |
| Score | 0 to 999,999 (6 digits) | No extra-life threshold from Score (unlike most Nintendo platformers). |
| Power State | Small / Super / Fire | Persists across stages within a life. See §4.2. |

### 1.4 Sources of Extra Lives

| Source | Value |
|--------|-------|
| 1-Up Mushroom (green) | +1 Life |
| 100 Coins | +1 Life |
| Reaching top of Flagpole (5000 pts band) | +0 Life directly, but the 5000-point grab is the headline reward — see §8.2 |
| Stomp/shell chain reaching the cap | After the sequence ends in "1-Up" (see §7.4), every further hit on that chain also yields 1-Up |
| **Infinite 1-Up shell trick** | Position a Koopa shell against a step so it bounces back into Mario's descending feet repeatedly. Iconic locations: the descending staircase near the end of **World 3-1** (Green Koopa walking down stairs) and a similar setup in **World 8-3**. |

---

## 2. Controls & Input

### 2.1 NES Controller Mapping

| Button | Action |
|--------|--------|
| D-Pad Left/Right | Walk |
| D-Pad Down | Crouch (Super/Fire only); enter downward Pipe when standing on its mouth |
| D-Pad Up | Climb Vine/Beanstalk; required to exit water in some sources (debated — see §12) |
| **A** | Jump (variable height, see §5.3); underwater "swim stroke" |
| **B** | Run when held during movement; shoot Fireball (Fire Mario) |
| Start | Begin game; Pause / Unpause |
| Select | Toggle 1-Player / 2-Player on title screen; mostly unused in gameplay |

### 2.2 Context-Sensitive Inputs

- **Pipe entry (down):** Hold Down while standing on a pipe whose mouth faces up and which is flagged as a warp pipe.
- **Pipe entry (right):** Walk into the mouth of a horizontally-oriented warp pipe.
- **Vine/Beanstalk climb:** Press Up while overlapping a Vine to enter climb state; Up/Down to climb, Left/Right to move around the vine, A to jump off.
- **Variable jump:** Holding A continues to add upward force for a limited window after takeoff. See §5.3.
- **Underwater A taps:** Each press of A applies one upward impulse. See §5.7.

---

## 3. World Structure

### 3.1 Layout

The game contains **8 Worlds × 4 Stages = 32 nominal levels**. Naming is `World X-Y`.

Stage role within a world is conventional, not strict:

| Stage | Typical Role |
|-------|--------------|
| `X-1` | Overworld introduction; usually hides a 1-Up Mushroom (often in an invisible block). |
| `X-2` | Underground, underwater, or athletic. Several `X-2` stages host the **Warp Zones**. |
| `X-3` | Athletic / sky / tree-tops, **300-second timer** instead of 400. |
| `X-4` | Castle with lava, Firebars, an impostor or true Bowser, the Axe, and a Mushroom Retainer / Princess. |

### 3.2 Stage Types (Tilesets / Themes)

- **Overworld** (blue sky, green hills, bushes)
- **Underground** (black background, brick ceiling and walls; spawns Coins generously)
- **Underwater** (dark blue, swim physics; e.g. 2-2, 7-2, 8-4 final sections)
- **Castle** (gray brick, lava pits, Firebars, Bowser, Axe at the end)
- **Athletic / Sky** ("Coin Heaven" above clouds, reached via Beanstalk; trees, mushroom-shaped platforms)
- **Bridge / Cheep-Cheep flying** (e.g. 2-3, 7-3; gray Cheep-Cheeps leap from below)

### 3.3 Recycled Levels in Worlds 5–8

To fit the 8-world game on 32 KB of cartridge ROM, several later stages reuse earlier layouts with harder enemy placements / shortened lifts:

| Recycled As | Source |
|-------------|--------|
| 5-3 | 1-3 |
| 6-4 | 1-4 |
| 7-2 | 2-2 |
| 7-3 | 2-3 |
| 5-4 | 2-4 |

### 3.4 Castle Contents (§7 for Bowser, §9 for Axe)

| World | True identity of "Bowser" | Attack |
|-------|--------------------------|--------|
| 1 | Little Goomba | Fire breath |
| 2 | Green Koopa Troopa | Fire breath |
| 3 | Buzzy Beetle | Fire breath |
| 4 | Spiny | Fire breath |
| 5 | Lakitu | Fire breath |
| 6 | Blooper | Hammers (no fire in battle, only at long range before) |
| 7 | Hammer Brother | Hammers (no fire in battle) |
| 8 | **Bowser** (true) | Fire breath + Hammers |

Both methods of defeat work: reach the Axe behind Bowser (drops bridge into lava, scores 5000), or hit Bowser with 5 fireballs (only Fire Mario; scores 5000 and reveals the true identity in worlds 1–7).

### 3.5 Warp Zones

| Source Stage | Pipe destinations | How to access |
|--------------|-------------------|---------------|
| **1-2** | World 2 / World 3 / World 4 | Near the end, ride the rising elevator lifts up to the ceiling, then run right along the ceiling past the normal exit pipe. |
| **4-2** (vine route) | World 6 / World 7 / World 8 | Hit a specific Brick to grow a Vine, climb to Coin Heaven, walk right to a Warp Zone. |
| **4-2** (ceiling route) | World 5 only | Take the lifts up to the ceiling near the end, similar to 1-2. |

### 3.6 Timer

- All standard stages start at **400** (units, not real seconds).
- All `X-3` (athletic) stages start at **300**.
- 1 timer unit = 24 NTSC frames ≈ 0.4 real seconds, so 400 units ≈ 160 real seconds. (PAL: 1 unit = 20 frames at 50 Hz.)
- When the displayed Timer drops below **100**, music speeds up to a "hurry-up" variant (a brief jingle plays, then the current track resumes at a faster tempo).
- Timer = 0 → instant death regardless of Power State.
- Castle (`X-4`) levels grant **no time bonus** on completion; non-castle levels grant **50 points per remaining timer unit**.

### 3.7 Secret Areas

- **Coin Heaven** (sky bonus): reached by climbing a Beanstalk grown from a specific brick. Auto-scrolls right with Sky Lifts and rows of Coins.
- **Coin rooms** (underground bonus): entered via a downward pipe; a small room with several rows of coins, then a horizontal warp pipe back to the main stage further right.
- **Hidden 1-Up Mushroom blocks**: Every `X-1` hides one. In several worlds it spawns in an invisible block whose position is gated on having collected all coins in the previous world's `X-3` athletic stage.
- **Hidden Coin Blocks**: many invisible blocks scattered through the game (e.g. famously above World 1-1 just after the first Goomba).

### 3.8 Minus World (Glitch)

Performing a wall-clip at the end of **World 1-2** (jump backward off the upper pipe while crouching as Super/Fire Mario, before the Warp Zone has scrolled fully on screen) lands the player in **"World -1"**. Internally this is **World 36-1**; the tile for "36" renders blank, so the HUD reads "WORLD -1". The level itself is a structural copy of World 7-2 (underwater). The exit pipe is broken: its warp destination is not updated and loops the player back to the level start, making it **inescapable** except by losing all lives or resetting. It is a side effect of the warp-zone data being read with an out-of-range index; not a designed feature, but part of the game's cultural identity.

### 3.9 Hard Mode / Second Quest

After clearing 8-4, the player can begin a second loop (see §11.5).

---

## 4. Playable Character (Mario / Luigi)

### 4.1 Mario / Luigi

In 1-Player mode the player controls Mario. In 2-Player mode players alternate after each death; Player 2 controls Luigi (palette swap). **No mechanical differences** exist between Mario and Luigi in the NES original (unlike *Super Mario Bros. 2 USA* and later titles).

### 4.2 Power States

| State | Sprite | Capabilities | Damage Behavior |
|-------|--------|--------------|-----------------|
| **Small** | 16×16 px | Bump Bricks (don't break them); no crouch | Any damage = death |
| **Super** | 16×32 px | Break Bricks; crouch (down); collision box covers 2 tiles vertically | Any damage = revert to Small, ~2-second invulnerability (flashing) |
| **Fire** | 16×32 px (red/white palette) | Same as Super + Fireball (B button; up to 2 fireballs on screen) | Any damage = revert to Small directly (skips Super); ~2-second invulnerability |

Transitions are driven by power-up pickups (§6) and damage (§4.3).

Power state **persists across stage transitions within a single life** (saved by the game when a stage is cleared, restored on the next stage's load).

### 4.3 Death Conditions

- Damage from any enemy while Small.
- Falling into a pit (off the bottom of the screen) at any size.
- Falling into lava (castle stages) at any size.
- Timer reaches 0.
- Touching Bowser's body, his fire breath, or his hammers (Small only takes damage); Small Mario at any point dies as above.
- Touching certain always-lethal entities regardless of state: Bowser's Flames, Firebars, Podoboo, Spinies (touching the spikes), Spiny Eggs while they fall, Piranha Plant when emerged.

### 4.4 Death Sequence

On death (non-pit): Mario's sprite freezes, a death jingle plays, the sprite "pops up" in a fixed arc (small upward velocity, then gravity), and falls through the floor off the bottom of the screen. On pit/lava: Mario simply falls off the bottom with no popup. Control is locked throughout.

---

## 5. Movement Physics

This is what makes *Super Mario Bros.* distinctive. All values below are documented from disassembly / speedrun community measurements; sub-pixel precision is **16 subpixels = 1 pixel** (see §12 for any contested constants).

### 5.1 Walk vs Run

- **B held** = run; **B released** = walk.
- **Max walk speed** is exactly **3/5 of max run speed**.
- **Underwater max walk speed** is **2/3 of max swim speed**.
- Acceleration is **higher when reversing facing** ("turn-around boost") than when continuing in the current facing.
- Acceleration occurs in subpixel units per frame; the visible result is that Mario sometimes moves 1 pixel and sometimes 2 pixels per frame at max walk, due to subpixel accumulation.

### 5.2 Friction / Deceleration

- On release of direction, ground friction decelerates Mario.
- Skidding (holding the opposite direction at running speed) plays a distinct sprite (back foot raised) and decelerates faster than passive friction.
- In the air, friction is much weaker; Mario keeps near-full horizontal speed unless input pushes against him.

### 5.3 Jump Height as a Function of Horizontal Speed

A defining mechanic: **Mario jumps higher (and the gravity used during the jump is lighter) when moving faster horizontally.** The game indexes into a small table of `(initial-Y-velocity, ascending-gravity, falling-gravity)` triples keyed on Mario's horizontal speed at the moment A is pressed. There are three speed buckets:

| Horizontal speed band | Effect on jump arc |
|-----------------------|--------------------|
| Slow / standing | Lowest peak; heaviest ascending gravity. |
| Walking | Mid peak. |
| Running (max) | **Highest peak**; lightest ascending gravity (so holding A pushes Mario much higher); longest air time. |

Approximate documented maximums:

| Jump type | Horizontal distance | Apex height (Small Mario) |
|-----------|---------------------|---------------------------|
| Standing jump | ~0 | ≈4 blocks (64 px) |
| Walking jump | ~5 blocks | ≈4⅜ blocks |
| Running jump | ≈8½ blocks | ≈5 blocks |

(Block = 16 px.)

### 5.4 Variable Jump Height

- A tap of A (1 frame) produces a minimum jump (≈1 block).
- Holding A keeps gravity in its lighter "ascending" mode until either A is released or a maximum hold window elapses; once released, gravity switches to the heavier "falling" value immediately, even mid-rise.
- Therefore: **release A early to fall sooner; hold A until apex for max height.**
- A retrigger in mid-air does nothing (no double-jump).

### 5.5 Gravity & Terminal Velocity

- Two separate gravity values are in use per jump: a small **ascending** value while A is held and rising, and a larger **falling** value used once apex is passed or A is released.
- Both ascending and falling gravity values are reduced when Mario is at running horizontal speed (this is what extends running jumps so dramatically).
- **Terminal vertical speed** is clamped. Notably, **once Mario falls below max-walk speed during a jump, he cannot accelerate horizontally back above it until landing**. This is the source of the famous "if you barely tap A and let go, you keep your speed; if you fall hard you lose it" feel.

### 5.6 Air Control

- Horizontal direction can be changed in the air, but slowly. Air acceleration is similar in magnitude to ground acceleration but Mario cannot decelerate / brake in the air using the opposite direction as effectively as on the ground.
- **No coyote time / no buffered jump** in the strict sense — A must be pressed while the ground-flag is true.

### 5.7 Underwater Swim

Distinct physics block:

- Gravity is much weaker (Mario sinks slowly).
- **A button = one upward impulse per press**. Holding A is not equivalent to multiple presses; the player must tap.
- Each tap adds a fixed upward velocity (capped to a maximum upward velocity).
- There is a hard ceiling on how high Mario can rise per stroke; near the water surface he cannot leap out of the water (he caps at the surface).
- Horizontal max swim speed > max underwater walk speed (similar 3/5–2/3 ratio relationship).
- **No air meter** — Mario can stay underwater indefinitely.
- Touching the floor underwater allows "walking" with full collision against floor tiles.
- Enemies underwater (Cheep-Cheep, Blooper) cannot be stomped; only fireballs from Fire Mario defeat them.

### 5.8 Pipe Entry / Exit

- **Down-pipe entry:** Mario stands centered on a warp pipe's mouth; holding Down begins a fixed-duration descent animation (the sprite is masked by the pipe), then the scene transitions.
- **Side-pipe entry:** Walk fully into a horizontal pipe mouth; same fixed-duration transit animation.
- **Up-pipe exit:** Mario rises out of the destination pipe automatically with a fixed-duration animation. Control returns once fully out.
- During pipe transit, input is locked and gravity does not apply.

### 5.9 Vine / Beanstalk Climbing

- Touching a Vine while pressing **Up** (or simply standing in its column) attaches Mario to the Vine in climb state.
- Up/Down moves vertically; Left/Right rotates Mario around the Vine without falling.
- Pressing A jumps off horizontally with a small kick.
- Climbing past the top of the Beanstalk transitions to a Coin Heaven bonus area.

### 5.10 Screen Scrolling Rules

- **Camera does not scroll left.** Once a region has scrolled off the left edge, it is gone for the rest of the stage — any enemy or item that scrolled off is despawned and walls block leftward motion as if invisible boundaries.
- Camera scrolls right when Mario crosses roughly the horizontal **midpoint** of the screen moving right; before that, Mario moves but the camera is fixed.
- Camera locks at predefined points: when entering a Bowser room (camera stops at the room edge), and at the **Flagpole** (no further right scroll).
- Vertical: the camera is generally fixed; only specific overworld + sky sections have vertical scroll regions, but most stages are 1-screen tall in the play area.

---

## 6. Power-ups & Items

All power-ups (and Coins) emerge from Question Blocks or Bricks. The item that emerges from a `?` Block depends on the block's data, not on Mario's current power state — **except** that a Mushroom block becomes a Fire Flower block when Mario is Super or Fire (the same block ID produces a Fire Flower if Mario is already powered up; otherwise it produces a Super Mushroom). Coins and Starmen do not change.

| Item | Appearance | Effect | Movement | Points |
|------|------------|--------|----------|--------|
| **Super Mushroom** (Magic Mushroom) | Red cap, white spots | Small → Super | Emerges upward from block, walks right (+left on wall contact) at ~Goomba speed, gravity-affected, falls off platforms. Mushrooms cannot be defeated. | 1000 |
| **Fire Flower** | Orange/red flower with eyes | Super → Fire (or Small → Super if hit while Small) | Static after emerging; sits on top of block until touched. | 1000 |
| **Starman (Super Star)** | Yellow star with eyes | Invincibility (~10 game seconds; the duration is *frame-rule dependent*, can vary by up to 20 frames per pickup). Touching enemies defeats them. Still vulnerable to pits, lava, and timer-out. | Emerges upward, then bounces along the ground at running speed in one direction. Affected by gravity; bounces off walls. | 1000 |
| **1-Up Mushroom** | Green cap, white spots | +1 Life | Moves like a Super Mushroom. | 0 (the life is the reward) |
| **Coin** | Spinning yellow coin | +1 to coin counter (×200 score) | Static (in air) or popping out of a Brick / Question Block | 200 (also contributes to 100-coin 1-Up) |

Notes:
- "Magic Mushroom" was the SMB1 instruction-manual name for the Super Mushroom. The "Poison Mushroom" is a *Super Mario Bros. 2 (Japan / The Lost Levels)* item — **not present** in SMB1.
- **Starman music** replaces the current track for the invincibility window; on expiry the previous track resumes (rewinds to start, not pauses).
- A "?" Block over a pit can leave a Mushroom unobtainable if it walks off the edge; Mushrooms cannot fly back up.

---

## 7. Enemies

### 7.1 Roster

| Enemy | Behavior | Stomp | Fireball | Star/Shell | Stomp pts | Notes |
|-------|----------|-------|----------|------------|-----------|-------|
| **Little Goomba** | Walks forward at constant slow speed; reverses on wall contact; walks off edges. | Defeats (squashes flat for 1 frame, then despawns) | Defeats | Defeats | 100 | Most common enemy. Replaced by Buzzy Beetles in Hard Mode (§11.5). |
| **Green Koopa Troopa** | Walks forward; **walks off edges**. | Retracts into shell (stationary). Second stomp on shell kicks it; third stomp on a moving shell stops it. | Defeats (does not leave shell). | Defeats | 100 (kick of shell = 400; chain values follow §7.4) | Shell respawns the Koopa after ~13 seconds if undisturbed. |
| **Red Koopa Troopa** | Walks forward; **stops at edges** (turns around). | Same as Green. | Same | Same | 100 | Smarter platform behavior. |
| **Green / Red Koopa Paratroopa** | Winged Koopas. Green Paratroopas hop along the ground; Red Paratroopas patrol up-and-down vertically. | First stomp removes wings (becomes a regular Koopa Troopa of same color); second stomp = retract to shell. | Defeats outright. | Defeats | 400 (Paratroopa); subsequent = 100 (Koopa) | |
| **Buzzy Beetle** | Walks like a Koopa; **immune to fireballs**. Walks off edges. | Same shell behavior as Koopa. | **No effect** | Defeats | 100 | Replaces Goombas in Hard Mode. First appears in underground stages. |
| **Spiny** | Walks; **cannot be stomped** (spikes hurt Mario). | Hurts Mario | Defeats | Defeats | 200 (fireball) | Spawned by Lakitu. |
| **Spiny's Egg** | Falls from Lakitu's cloud. Lakitu spawns the egg; *intended* behavior was for the egg to bounce off walls and follow Lakitu's velocity, but **a bug causes eggs to fall straight down** (no horizontal velocity). Eggs hatch into Spinies on landing. | Cannot be stomped (lethal to touch) | Defeats | Defeats | (no points until it becomes a Spiny) | Bug remained unpatched in original SMB1; fixed in later remakes. |
| **Lakitu** | Floats in a cloud at the top of the screen, scrolling with Mario; periodically drops Spiny Eggs. Stays one-on-screen at a time. Respawns ~40 seconds after defeat if the player remains in its zone. | Defeats | Defeats | Defeats | Stomp 800 / Fireball 200 | First appears World 4-1. |
| **Piranha Plant (Green)** | Emerges from a pipe periodically, sits at full extension, retracts. **Won't emerge if Mario is standing on (or roughly within ½ block of) the pipe**, which makes it possible to safely walk over the pipe. | Cannot stomp | Defeats | Defeats | 200 (fireball) | |
| **Piranha Plant (Red)** | Same as Green but appears in later worlds and emerges even when Mario is near (no "stand on pipe" safe behavior — depending on version). | Cannot stomp | Defeats | Defeats | 200 | |
| **Cheep-Cheep (Red, flying)** | Leaps from below the screen in arcs across the play area on certain bridge stages (e.g. 2-3, 7-3). | Defeats (must land on it mid-arc) | Defeats | Defeats | 200 | |
| **Cheep-Cheep (Gray, swimming)** | Swims horizontally across underwater stages; some are faster. | Cannot stomp (underwater) | Defeats | Defeats | 200 | |
| **Blooper (Bloober)** | White squid; follows Mario underwater using a "dart-and-pause" pattern (rising then sinking). | Cannot stomp (underwater) | Defeats | Defeats | 200 | |
| **Hammer Bro** | Walks short distances on a platform, throwing a steady arc of hammers up-and-out at the player. After ~110 seconds in their zone, leaves the platform and walks aggressively toward Mario. | Defeats (tricky — pause between hammer arcs) | Defeats | Defeats | 1000 | Always spawned in pairs, typically on stacked Brick platforms. |
| **Bullet Bill** | Black bullet fired horizontally from off-screen Bullet Bill Cannons (or out of nowhere on certain stages). Moves at constant speed across the screen. | Defeats | **No effect** (passes through) | Defeats | 200 | |
| **Bowser** (true, 8-4 only) | Patrols a platform across a lava pit on a bridge. Breathes fire (Bowser's Flame) horizontally toward Mario, and throws hammers as Mario draws near. Takes 5 fireball hits to defeat directly, or instantly via touching the Axe behind him. | Cannot stomp (lethal) | Defeats after 5 hits | — | 5000 | |
| **Impostor Bowser** (worlds 1-7) | Same patrol / attack pattern as Bowser; 5 fireballs reveal underlying enemy (§3.4). | Cannot stomp | Defeats after 5 hits | — | 5000 (the impostor's "Bowser" form) | |
| **Bowser's Flame** | Horizontal fireball traveling in a slight wave from Bowser's mouth; passes through brick walls. | — | — | — | (no points) | Lethal to any state. |
| **Podoboo** | Lava bubble that leaps in a fixed arc from a lava pit and falls back. Completely invincible. | Cannot stomp (lethal) | No effect | No effect | (no points) | Castle stages. |
| **Firebar** | A row of 6 (sometimes more) fireballs rotating around a fixed Used Block as a pivot. | — | — | — | (no points) | Completely invincible. Castle stages. |

### 7.2 Enemy-vs-Enemy

- A kicked Koopa/Buzzy Shell traveling along the ground will defeat enemies on contact (chain — see §7.4).
- A shell can rebound off a wall and hit Mario. Shells also defeat Hammer Bros, Goombas, Koopas, etc.
- Bowser's Flame does not interact with other enemies.

### 7.3 Goomba's Appearance Per World

Goombas appear in worlds 1, 2, 3, 4, 5, 6, 7, 8 (overworld and underground stages especially). In Hard Mode they are entirely replaced by Buzzy Beetles.

### 7.4 Stomp & Shell Chains

While Mario is airborne (no ground contact between hits), defeats escalate in score:

**Stomp sequence (bouncing on consecutive enemies):**
100 → 200 → 400 → 500 → 800 → 1000 → 2000 → 4000 → 5000 → 8000 → **1-Up** (then 1-Up for every further hit on this chain)

**Shell-kick sequence (one moving shell defeating multiple enemies):**
500 → 800 → 1000 → 2000 → 4000 → 5000 → 8000 → **1-Up** (then 1-Up for every further hit on this chain)

Touching the ground resets the chain.

The Goomba-stair / Koopa-shell setup in 3-1 and 8-3 exploits the shell-kick chain by trapping a single shell in a re-strike loop (see §1.4).

---

## 8. Scoring

### 8.1 Score Sources

| Source | Points |
|--------|--------|
| Coin pickup | 200 |
| Brick break (Super/Fire only) | 50 |
| Stomp enemy | per §7.1 / §7.4 |
| Fireball defeat | per §7.1 |
| Power-up pickup (Mushroom, Fire Flower, Star) | 1000 |
| 1-Up Mushroom | 0 (life is the reward) |
| Flagpole touch (height-banded) | see §8.2 |
| Time bonus at level end (non-castle) | 50 × remaining Timer units |
| Fireworks | 500 × number of fireworks |
| Defeating Bowser / Impostor Bowser via fireball or axe | 5000 |
| Scale Lift collapse (causing one side to drop) | 1000 |

### 8.2 Flagpole Touch

The flagpole is divided into height bands. Touching at each band awards:

| Band (top → bottom) | Points |
|---------------------|--------|
| Very top (touch the ball) | **5000** |
| Upper band | 2000 |
| Middle band | 800 |
| Lower band | 400 |
| Bottom (touch at base) | 100 |

After the touch animation, Mario slides down the pole, walks into the castle, and the time bonus tallies (50 per remaining unit).

### 8.3 Fireworks at Level End

If the **last digit of the Timer** at flagpole touch is **1, 3, or 6**, fireworks erupt over the castle after Mario enters. **Number of fireworks = that last digit**, and **each firework = 500 points**.

| Timer last digit | Fireworks | Bonus |
|------------------|-----------|-------|
| 1 | 1 | 500 |
| 3 | 3 | 1500 |
| 6 | 6 | 3000 |
| any other | 0 | 0 |

Fireworks **do not trigger on castle (`X-4`) stages**, since those end with the Axe rather than a Flagpole.

### 8.4 Extra Lives from Score

**None.** SMB1 does not award lives based on score thresholds (a notable departure from many other arcade-era platformers). The only sources are §1.4.

---

## 9. Level Design Elements

### 9.1 Blocks

| Block | Behavior |
|-------|----------|
| **Question Block (?)** | Hit from below to dispense its contents (Coin / Super Mushroom / Fire Flower / Starman / 1-Up). Becomes a Used Block (empty) after dispensing. |
| **Brick Block** | Small Mario bumps it (block jiggles, no destruction). Super/Fire Mario breaks it from below (50 points, plus 4 brick-shard sprites). May contain a Coin, Power-up, Vine, or Starman — if it does, it acts like a `?` Block on first hit. |
| **Hidden Block / Invisible Block** | Invisible until hit from below. Same behaviors as `?` or Brick. Often contains 1-Up Mushrooms or Coins. Several iconic ones in 1-1. |
| **Used Block** | Solid, sprite of a dim brick. Cannot be re-hit. Used as Firebar pivots in castle stages. |
| **Multi-Coin Block** | A Brick whose `?`-like data dispenses Coins on repeated hits, time-windowed: the block can be re-hit during an 11-tick window of the frame-rule counter immediately after the first hit (≈230 frames / 3.8 seconds maximum). Hits resolve every ~16 frames; **maximum coins from one block ≈ 15** (TAS-verified 15–16). After the window closes, it becomes a Used Block. |

### 9.2 Pipes

| Variant | Behavior |
|---------|----------|
| Decorative / Solid | Acts as a wall and floor; cannot be entered. Often hosts Piranha Plants. |
| Warp Pipe (Down) | Player holds Down on the mouth to descend into a bonus area or warp zone. |
| Warp Pipe (Side) | Walking into the pipe from the correct side triggers warp transit. |
| Exit Pipe (Up) | Mario emerges upward at the destination automatically. |

### 9.3 Lifts / Platforms

| Lift Type | Behavior |
|-----------|----------|
| Up/Down (or Left/Right) Lift | Patrols between two endpoints continuously. |
| Elevator Lifts | An endless stream of upward- or downward-moving short platforms in a vertical shaft. Stepping off the top/bottom kills the platform. Some are joined by a visible white line ("pulley pair"). |
| Scale (Pulley / Balance) Lifts | Two platforms on a rope over a pulley. Standing on one pulls it down and raises the other. If one side reaches the top, the rope snaps and both fall. |
| Falling / Drop Lift | Stationary until Mario steps on, then falls. |
| Sky Lift | Found in Coin Heaven sections; begins moving rightward once touched. |
| Jumping Board (Springboard) | Mario can use it as a giant trampoline. Holding A during the rebound provides an extra-high launch. |

### 9.4 Flagpole

End of every non-castle stage. Touching it ends the stage; height of touch determines bonus (§8.2). Mario slides down, walks right into the castle silhouette, and the level-complete fanfare plays.

### 9.5 Axe

End of every castle stage. Touching the Axe drops the suspended bridge into the lava, sending Bowser (real or impostor) down with it. In worlds 1-7 the Mushroom Retainer ("Princess in another castle") room appears; in 8-4 Princess Toadstool's room.

### 9.6 Lava

Bottomless death plane in castle stages. Any contact = death regardless of state.

### 9.7 Bullet Bill Cannons

Black off-platform turret structures (sometimes stacked) that fire Bullet Bills horizontally at intervals. The cannon itself is a non-interactive part of the level geometry.

---

## 10. UI & HUD

### 10.1 HUD Layout (top of screen, always visible during play)

```
MARIO                  WORLD     TIME
000000  [coin] x00      1-1       400
```

| Field | Description |
|-------|-------------|
| `MARIO` / `LUIGI` | Active player label |
| Score | 6-digit decimal |
| Coin counter | Yellow coin icon + 2-digit count (00–99) |
| World | `World X-Y` |
| Time | 3-digit timer (counts down) |

### 10.2 Screens

- **Title Screen.** Logo, copyright, "1 PLAYER GAME / 2 PLAYER GAME" selector, demo loop of TAS-style attract gameplay.
- **World Intro ("WORLD 1-1") screen.** A black screen with `WORLD X-Y`, a small Mario sprite, and `× n` showing remaining lives. Brief pause, then the stage loads.
- **Game Over screen.** Black screen with "GAME OVER" text.
- **No file-select / save-game screen** — there is no save.

### 10.3 Pause

Pressing **Start** during play pauses the game. The pause is signaled by an audio cue (a short "ding" jingle) and music silence; no overlaid "PAUSE" text. Press Start again to unpause.

### 10.4 Continue (Game Over)

On the **Game Over** screen, **holding A and pressing Start** restarts at **World X-1** (the first stage of the world where the player died) instead of World 1-1. Score resets to 0 either way. Documented in the official Nintendo player's guide but obscure to many players.

---

## 11. Engine & Presentation Systems

### 11.1 Save System

**None.** There is no save, no password, no battery. Continues only via the A+Start trick (§10.4). Power off = full reset to title.

### 11.2 Music Tracks (Koji Kondo)

| Track | Played on |
|-------|-----------|
| **Ground Theme** ("Running About") | All overworld stages (1-1, 1-3, 2-1, 3-1, …) |
| **Underground BGM** | Underground stages (1-2, 4-2, etc.) |
| **Underwater BGM** ("Swimming Around") | Underwater stages |
| **Castle BGM** | Castle (`X-4`) stages |
| **Starman BGM** | Plays for the Starman invincibility window, overrides the level track |
| **Hurry-Up Variant** | When Timer < 100, a short "hurry up" jingle plays, then the current track restarts at a sped-up tempo |
| **Level Clear** | Plays during the post-flagpole castle walk |
| **Castle Clear** ("Princess in another castle") | Plays in the Mushroom Retainer / Princess room |
| **Game Over** | Plays on the Game Over screen |
| **Death Jingle** | Plays when Mario dies (non-pit) |
| **World Clear (8-4 end)** | Princess Toadstool rescue theme |

### 11.3 Sound Effects

Jump (Small), Jump (Super/Fire — slightly lower pitch), Coin pickup, 1-Up jingle, Power-up appearance, Power-up pickup ("growing"), Fireball, Stomp, Kick (shell), Block bump, Block break (Brick), Pipe entry, Vine grow, Bowser's flame, Bowser fall (bridge collapse), Axe touch, Flagpole touch (rising arpeggio), Fireworks ("twinkle"), Pause ding, Hurry-up alert, Death, Game Over.

### 11.4 Camera

- Locks at the left edge of the world (never scrolls leftward).
- Scrolls right when Mario passes roughly the horizontal midpoint of the screen.
- Locks at predefined room boundaries (Bowser arenas, the Flagpole area).
- Mostly fixed vertically; specific sky / Coin Heaven areas have a different vertical band.

### 11.5 Hard Mode (Second Quest)

Triggered automatically by clearing 8-4. The title screen lets the player pick a starting world for the new quest. Differences:

- **All Goombas are replaced by Buzzy Beetles** (which are immune to fireballs).
- Ground enemies (Goombas / Koopa Troopas / Buzzy Beetles) **move faster**.
- Koopas / Buzzy Beetles **emerge from their shells faster** (shorter retreat duration).
- **Hammer Bros wait less time** before leaving their platform to chase Mario.
- **Elevator-style lifts use the shorter platform variant**, even on stages that originally used the longer ones (e.g. 1-2, 3-3, 4-2, 4-3).
- Bonus / hidden 1-Up positions may shift.
- Otherwise the level layouts are unchanged.

### 11.6 Frame Rule (Internal Timing Mechanism)

Multiple long-duration timers (Star invincibility, screen transitions, hurry-up triggers, etc.) decrement only when an internal counter loops. The counter resets every **21 frames** (≈0.35 s NTSC), so long-timer events resolve on 21-frame boundaries. The level-clear handoff is the most famous case — Mario can "finish" a level anywhere inside a 21-frame window without losing any wall-clock time, which is why speedruns are organized around frame rules. Other framerule-dependent durations include Starman invincibility (varies by up to 20 frames per pickup) and the inter-stage status screen.

The displayed game-Timer ticks down by 1 unit every **24 NTSC frames** (≈0.4 s), independent of the 21-frame framerule.

---

## 12. Open Questions / Unverified

- **Exact subpixel constants** for max walk speed, max run speed, walk acceleration, run acceleration, skid deceleration, ascending gravity, falling gravity, running-jump gravity, and terminal Y velocity. Documentation across sources (TASVideos game-resources page, 6502 disassembly project, SDA knowledge base, various blog write-ups) describes the *ratios* and *qualitative behavior* but rarely quotes the literal subpixel constants in one place. The authoritative source is the SMB disassembly (e.g. `JumpMForceData`, `FallMForceData`, `VerticalForceData`, `MaximumLeftSpeed`, `MaximumRightSpeed`); a re-implementer should consult those tables directly.
- **Starman invincibility duration**: described as "~10 game seconds" / "around 20–30 seconds" depending on source. The duration is frame-rule dependent and varies by up to 20 frames depending on the frame the Star was collected on.
- **Hammer Bro "leave platform" timer**: cited as ≈110 seconds in some references; Hard Mode shortens it but exact deltas are not consistently documented.
- **Lakitu respawn delay**: ≈40 seconds is cited but not all sources agree.
- **Multi-Coin Block max yield**: community-documented as 15 coins under normal play, 16 under TAS-perfect input; the precise threshold depends on the per-hit frame budget.
- **Exact Hard Mode enemy speed multipliers**: documented qualitatively as "faster" without a confirmed numeric factor in the sources surveyed.
- **Underwater "Up to exit" claim**: some FAQs assert holding Up helps Mario exit water; the original SMB1 simply caps Mario at the water surface with normal swim strokes — there is no special water-exit input. (Possibly conflated with later games.)
- **Piranha Plant "stand on pipe to suppress" exact radius**: stated as "about half a block away" but no precise pixel range is documented.

---

## 13. References

- Super Mario Wiki — Point page (enemy point values, stomp/shell chains): https://www.mariowiki.com/Point
- Super Mario Wiki — Fireworks: https://www.mariowiki.com/Fireworks
- Super Mario Wiki — Fake Bowser (impostor identity per world): https://www.mariowiki.com/Fake_Bowser
- Super Mario Wiki — Super Star, Swim, Invincible Mario, Time Limit, Hard Mode, Minus World, Game Over, Continue, Pause (general references).
- Super Mario Wiki — Spiny Egg (bugged horizontal behavior): https://www.mariowiki.com/Spiny_Egg
- The Mushroom Kingdom — SMB Complete Breakdown (level structure, enemy roster, lift types, hidden areas, recycled levels, fake-Bowser table): https://themushroomkingdom.net/smb_breakdown.shtml
- TASVideos — Game Resources / NES / Super Mario Bros (subpixel system, 21-frame rule, wall-jump thresholds): https://tasvideos.org/GameResources/NES/SuperMarioBros
- SDA Knowledge Base — Super Mario Bros (walk/run/swim speed ratios): https://kb.speeddemosarchive.com/Super_Mario_Bros.
- Speedrun.com — General Game Mechanics & 21-frame-rule discussion: https://www.speedrun.com/smb1/guides/pbl9d
- Brandon's Thoughts — "Super Mario Bros. framerules" (long-timer / framerule mechanic deep-dive): https://brskari.wordpress.com/2021/08/01/super-mario-bros-framerules/
- 6502 Disassembly — Super Mario Bros Disassembly project (canonical source for physics constants): https://6502disassembly.com/nes-smb/
- Comprehensive SMB Disassembly (1wErt3r): https://gist.github.com/1wErt3r/4048722
- NESDev Forum — SMB Meta-Disassembly thread: https://forums.nesdev.org/viewtopic.php?t=25016
- Thonky — List of Warp Zones: https://www.thonky.com/super-mario-bros-1/list-of-warp-zones
- StrategyWiki — Super Mario Bros. World 3 (3-1 infinite 1-Up trick): https://strategywiki.org/wiki/Super_Mario_Bros./World_3
- The Cutting Room Floor — Super Mario Bros (unused data, bugs): https://tcrf.net/Super_Mario_Bros.
- Kotaku / Techspot — Multi-Coin Block 15-coin limit research.
- NESDev Forum — "Timer in Super Mario Bros. not in seconds" (24-frames-per-unit timing): https://forums.nesdev.org/viewtopic.php?t=14000
- Nintendo Life — Super Mario Bros. A+Start continue trick: https://www.nintendolife.com/news/2020/05/random_do_you_know_about_super_mario_bros_secret_game_over_continue_trick

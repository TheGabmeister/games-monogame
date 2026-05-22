# Metroid — Gameplay Systems Spec

NES (Famicom Disk System in Japan), 1986. Developed by Nintendo R&D1. Directed by Satoru Okada; designed by Yoshio Sakamoto. This spec covers the NTSC NES cartridge release (PRG-ROM version).

---

## 1. Core Gameplay Systems

### 1.1 Primary Gameplay Loop

Metroid is a 2D action-platformer with non-linear exploration. The core loop is:

1. **Explore** the interconnected world of Planet Zebes
2. **Find upgrades** (weapons, suit abilities, capacity expansions) that gate access to new areas
3. **Defeat enemies** to collect health and missile refills
4. **Defeat the two mini-bosses** (Kraid and Ridley) to unlock the final area
5. **Destroy Mother Brain** and escape within the time limit

The game is one of the earliest examples of non-linear design on a home console. Most items can be collected in any order, and the player is free to explore the world with minimal guidance.

### 1.2 Combat System

Combat is real-time action. Samus fires her arm cannon in the direction she faces (left or right) or upward while standing. She cannot fire downward or diagonally.

- **Normal Beam**: Samus's default weapon. Fires a single projectile horizontally (or vertically when aiming up). Short range until the Long Beam is acquired
- **Missiles**: Limited ammunition, stronger than beams. Selected via the Select button. Only one missile can be on-screen at a time (unless exploiting a glitch; see §1.2.1)
- **Bombs**: Usable only in Morph Ball form (§1.3). Detonate after a short delay. Damage nearby enemies and destroy certain blocks
- **Screw Attack**: Passive damage on contact during a spinning jump (§6)

**Fire Rate**: Samus can fire a beam every other frame under normal conditions (button must be released for 1 frame between shots). Advanced input allows firing every frame by alternating directional input while holding the fire button.

**On-screen Projectile Limit**: Up to 3 beam shots can be on screen simultaneously. Missiles are limited to 1.

#### 1.2.1 Missile-Beam Trick

Normally, Samus cannot fire a missile while a beam shot is active on-screen. However, firing a beam or bomb first, then quickly switching to missiles, allows both projectile types to coexist on-screen.

### 1.3 Movement & Traversal

| Action | Description |
|---|---|
| Walk | D-pad left/right. Constant speed, no acceleration curve |
| Jump | A button. Variable height based on hold duration |
| Crouch | D-pad down (standing only). Cannot move while crouching |
| Aim Up | D-pad up while standing. Fires upward |
| Morph Ball | D-pad down twice (or once when already crouching). Samus transforms into a ball to fit through 1-tile-high passages |
| Unmorph | D-pad up or A button while morphed |

**Jump Physics**: Samus loses all forward momentum upon landing — there is no ground slide or momentum carry. Midair direction can be changed freely (full air control).

**Morph Ball**: Essential traversal ability. Samus becomes a sphere exactly 1 tile tall. She can roll left/right and deploy bombs. No jump ability while morphed (without bomb jumping).

**Bomb Jumping**: Bombs provide upward thrust to Morph Ball Samus. A single bomb boost provides modest height. A double bomb boost (placing a second bomb at the apex of the first boost) provides greater height. These techniques allow vertical traversal in Morph Ball form.

**Edge Jump / Fall Jump**: At a platform's edge, releasing directional input on the correct frame allows Samus to fall while retaining her standing state, enabling a jump during the fall. This is a frame-precise technique.

### 1.4 Resource Management

| Resource | Starting Value | Maximum | Refill Method |
|---|---|---|---|
| Energy (HP) | 30 (of 99 base) | 99 + (100 × Energy Tanks held, max 6) = 699 | Enemy drops (5 or 20 units), Energy Tanks |
| Missiles | 0 | 255 (5 per Missile Tank + boss rewards) | Enemy drops (+2 per pickup) |

**Respawn**: Upon death, Samus respawns at the starting elevator of the current area with only 30 energy, regardless of her maximum. All items and upgrades are retained.

**Energy Display**: Health is shown as a number 00–99 representing the current tank, with filled squares indicating remaining full tanks. When the current tank depletes, the next tank begins draining.

### 1.5 Damage & Defense

**Varia Suit**: Reduces all damage taken (enemies and lava) by 50%.

**Invincibility Frames**: When Samus takes damage, she gains brief invulnerability (several frames), during which she can pass through enemies without taking additional hits. Knockback pushes her in the opposite direction of the hit.

**Lava/Acid**: Contact with lava in Norfair and other areas drains energy continuously. The Varia Suit halves this drain rate.

### 1.6 Item Drops

Defeated enemies may drop pickups. Drop type is determined by the frame on which the enemy is killed (frame-based RNG), not by enemy type.

| Drop | Effect |
|---|---|
| Small Energy | Restores 5 energy |
| Large Energy | Restores 20 energy |
| Missile Ammo | Restores 2 missiles |

Before Samus acquires her first Missile Tank, all drops are energy. After acquiring missiles, missile ammo drops become possible.

---

## 2. Controls & Input

### 2.1 NES Controller Mapping

| Input | Action |
|---|---|
| D-pad Left/Right | Walk |
| D-pad Up | Aim upward |
| D-pad Down | Crouch / Enter Morph Ball |
| A | Jump / Unmorph |
| B | Fire weapon (beam or missile) |
| Select | Toggle between Beam and Missile modes |
| Start | Pause (opens status screen showing energy, missiles, and collected items) |

### 2.2 Context-Sensitive Inputs

- **Morph Ball mode**: D-pad left/right rolls; B deploys bombs; A or Up unmorphs
- **Aiming up**: Holding Up while pressing B fires vertically. Samus cannot walk while aiming up
- **Crouching**: Samus can fire horizontally while crouching but cannot move

---

## 3. World Structure

### 3.1 Planet Zebes — Overview

Zebes is divided into 5 interconnected areas. Brinstar serves as the central hub, with all other areas accessible from it (directly or through Norfair).

```
                    [Tourian]
                        |
                   (locked door — requires both
                    Kraid and Ridley defeated)
                        |
    [Kraid's Lair] — [BRINSTAR] — [Norfair]
                                      |
                               [Ridley's Lair]
```

### 3.2 Area Descriptions

| Area | Tileset | Description | Key Features |
|---|---|---|---|
| Brinstar | Blue/green rocky caves | Starting area and central hub. Contains most early upgrades | Morph Ball, Long Beam, Bombs, Ice Beam |
| Norfair | Purple/red fiery caverns | Lava-filled tunnels below Brinstar's eastern section | Hi-Jump Boots, Screw Attack, Wave Beam |
| Kraid's Lair | Blue underground hideout | Mini-boss fortress beneath western Brinstar | Boss: Kraid. Missile reward on defeat |
| Ridley's Lair | Purple volcanic fortress | Mini-boss fortress beneath Norfair | Boss: Ridley. Missile reward on defeat |
| Tourian | Mechanical/alien corridors | Final linear area. Home of the Metroids and Mother Brain | Metroids, Zebetites, Mother Brain, escape sequence |

### 3.3 Room Structure & Scrolling

Each room is composed of screens (256×240 pixels, one NES screen). Rooms scroll in either the horizontal or vertical direction — never both simultaneously within the same room. This alternates between connected rooms: horizontal corridors connect to vertical shafts via doors.

**Doors**: Colored hatches connect rooms. Blue doors open with a single shot. Red doors require 5 missiles to open (they turn blue permanently once opened). Doors trigger a brief screen-transition animation.

**Elevators**: Vertical transport between areas. Samus stands on a platform that rises or descends to the connected area.

### 3.4 Progression Gating

| Gate Type | Requirement |
|---|---|
| 1-tile passages | Morph Ball |
| Bomb blocks | Bombs (Morph Ball form) |
| Red doors | 5 missiles per door |
| Tall vertical shafts | Hi-Jump Boots (or bomb jumping) |
| Tourian access | Both Kraid and Ridley defeated |

The non-linear design means most upgrades can be collected in varying order. Mandatory items for completion are: Morph Ball, Missiles (at least one tank), Bombs, and Ice Beam (required for Metroids in Tourian).

---

## 4. Playable Characters

Samus Aran is the sole playable character. She wears the Power Suit, a modular exoskeleton that can be upgraded with items found throughout Zebes (§6).

**Armorless Samus**: After completing the game in under a certain time threshold (§5.2), the player can replay as Samus without her suit. This is a cosmetic change only — gameplay is identical.

---

## 5. Story & Progression

### 5.1 Story Structure

Metroid has minimal in-game narrative. The manual provides the backstory:

- The **Galactic Federation** discovers a dangerous parasitic lifeform called Metroids on planet SR388
- **Space Pirates** steal Metroid specimens and bring them to their base on Planet Zebes
- The Space Pirates plan to weaponize the Metroids using beta rays
- **Samus Aran**, a bounty hunter, is dispatched to infiltrate Zebes, destroy the Metroids, and defeat the Space Pirate leader **Mother Brain**

The game itself is entirely exploration-driven with no dialogue, cutscenes, or text beyond the opening title crawl.

### 5.2 Endings

There are 5 endings based on total completion time. Faster completion reveals more of Samus's appearance beneath her suit.

| Ending | Time (NTSC) | Result |
|---|---|---|
| Best | Under 1 hour | Samus shown in a bikini. Unlocks Armorless Samus for replay |
| Good | 1–3 hours | Samus removes helmet and suit, shown in a leotard |
| Decent | 3–5 hours | Samus removes helmet only, revealing her face and hair |
| Standard | Over 5 hours | Samus raises fist in victory, armor remains on |
| Shame | Over 5 hours (Armorless) | Samus turns her back and covers her face |

The game timer runs continuously from the start of the game and is encoded in the password system (§11.2).

---

## 6. Items & Equipment

### 6.1 Beam Weapons

Beam weapons are **mutually exclusive** — collecting a new beam replaces the current one. Beams cannot be stacked or combined.

| Beam | Location | Effect |
|---|---|---|
| Normal Beam | Default | Short-range projectile. Replaced once any beam is collected |
| Long Beam | Brinstar | Extends beam range to the full width of the screen. Affects all subsequent beam pickups |
| Ice Beam | Brinstar | Freezes enemies on contact. Frozen enemies serve as temporary platforms. Required to defeat Metroids (§7.3). Same damage as Normal Beam, but enemies must thaw and be re-frozen between hits, effectively doubling kill time on non-Metroid enemies |
| Wave Beam | Norfair | Fires a sinusoidal wave pattern. Can pass through walls and solid terrain. Ineffective against Metroids. Wider coverage compensates for no freeze utility |

**Long Beam persistence**: The Long Beam's range extension carries over to all beams collected afterward. It is not lost when switching beam types.

### 6.2 Suit Upgrades

| Upgrade | Location | Effect |
|---|---|---|
| Morph Ball | Brinstar (first item) | Transform into a ball to traverse 1-tile-high passages |
| Bombs | Brinstar | Deploy explosives in Morph Ball form. Destroy certain blocks, damage enemies, enable bomb jumping |
| Hi-Jump Boots | Norfair | Approximately 1.5× normal jump height |
| Screw Attack | Norfair | Spinning jumps damage enemies on contact. One of the most powerful offensive tools in the game |
| Varia Suit | Brinstar or Norfair | Reduces all damage by 50%. Visually changes suit color |

### 6.3 Capacity Expansions

| Item | Count in Game | Effect per Unit |
|---|---|---|
| Energy Tank | 8 hidden (max 6 usable) | +100 maximum energy. Tanks beyond the 6th simply refill current energy |
| Missile Tank | ~21 hidden | +5 maximum missiles each. Additional missiles awarded for defeating Kraid (75) and Ridley (75) |

**Maximum Missile Capacity**: 255 (8-bit cap). Practically reachable through Missile Tanks and boss rewards.

---

## 7. Enemies & Opponents

### 7.1 Common Enemies by Area

#### Brinstar

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zoomer | Yellow, Red | Crawls along floors, walls, and ceilings in a fixed path | Yellow: 1 beam shot. Red: 2 beam shots |
| Skree | — | Clings to ceilings; dives toward Samus when she passes beneath | Vulnerable during dive |
| Zeb | Yellow, Red | Spawns infinitely from pipe fixtures, flies horizontally | Yellow: 1 beam. Red: 2 beams |
| Mellow | — | Phases through walls, swarms in groups | Weak individually |
| Ripper | Yellow, Red | Flies in a straight horizontal line, ignores Samus | Yellow: Freezable only (invincible to damage). Red: Missiles only |
| Rio | Yellow, Red | Flies toward Samus from off-screen in swooping patterns | Yellow: weaker. Red: tougher |
| Waver | Yellow, Red | Flutters in erratic wave patterns, hard to target | Similar durability between variants |

#### Norfair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Nova | Blue, Yellow | Crawls along surfaces. Covered in heat-resistant fur | Blue: 2 beams or 1 missile. Yellow: 4 beams or 1 missile |
| Squeept | — | Leaps from lava in an arc | Resistant to normal beam |
| Ripper II | — | High-speed variant of Ripper, emits fire from rear | Faster than standard Ripper |
| Dragon | — | Emerges from lava, breathes fire | Stationary lava-dweller |
| Multiviola | — | Bounces off walls unpredictably | 1 missile (difficult with beam) |
| Geruta | Purple, Red | Flies using internal energy, heat-emitting skin | Red: strongest attack power in Norfair |
| Gamet | Purple, Red | Spawns from pipes in groups | Red: double attack power of purple |
| Mella | — | Phases through walls in groups (Norfair's Mellow equivalent) | 1 beam (weakest in area) |
| Polyp | — | Spawns from pipes, toxic projectiles | Weak |

#### Kraid's Lair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zeela | Yellow, Blue | Crawls along surfaces in all directions | Blue: stronger attack |
| Sidehopper | — | Hops aggressively toward Samus | 1 missile |
| Memu | — | Phases through walls in groups | Weakest in area |
| Geega | Yellow, Brown | Spawns from pipes horizontally | Brown: double attack power |

#### Ridley's Lair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zebbo | Blue, Yellow | Spawns from pipes | Blue: 1 beam. Yellow: 2 beams |
| Holtz | — | Drops from ceiling in armor, retreats after attack | Several beams or 1 missile |
| Viola | Blue, Yellow | Ground-crawling Multiviola larvae | Blue: 2 beams / 1 missile. Yellow: 4 beams / 1 missile |
| Dessgeega | — | Aggressive jumper with high attack power | Several beams or 1 missile. Second strongest to bosses |

#### Tourian

| Enemy | Behavior | Notes |
|---|---|---|
| Rinka | Ring-shaped projectiles that continuously spawn from fixed points | Cannot be permanently destroyed; re-spawn endlessly |
| Metroid | Latches onto Samus and drains energy continuously | See §7.3 |
| Cannon | Wall/ceiling-mounted turrets firing small projectiles | Stationary defense |

### 7.2 Bosses

#### Fake Kraid (Mini-Boss)

Encountered in Kraid's Lair before the real Kraid. Identical in appearance but much weaker. Defeated with 1 missile or the Screw Attack.

#### Kraid (Mini-Boss I)

- **Location**: Kraid's Lair, deepest room
- **Attacks**: Fires horns from his back (arcing overhead) and spines from his belly (horizontal). Belly projectiles block missiles and remain frozen in place if hit with the Ice Beam while still blocking missiles
- **Strategy**: Most reliable method is Morph Ball + Bombs placed at his feet. Missiles are effective but belly projectiles interfere. Beams are also viable but slow
- **Reward**: 75 missiles. Defeating Kraid is required for Tourian access

#### Ridley (Mini-Boss II)

- **Location**: Ridley's Lair, deepest room
- **Attacks**: Jumps vertically while firing fireballs. Fireballs stun Samus and deplete energy
- **Strategy**: Freeze fireballs with Ice Beam, then fire missiles at Ridley from close range. Alternatively, use the Screw Attack to get behind him and fire rapidly
- **Reward**: 75 missiles. Defeating Ridley is required for Tourian access

#### Mother Brain (Final Boss)

- **Location**: Tourian, at the end of a linear gauntlet
- **Barriers**: 5 Zebetites (organic energy barriers) block the path to Mother Brain. Each requires ~8–12 rapid missile hits. Zebetites regenerate if not destroyed quickly enough — missiles must be fired in rapid succession
- **Attacks**: Mother Brain itself does not attack or move. The room's defenses include Rinkas (ring projectiles, max 3 on screen at a time) and cannon turrets
- **Hits to Kill**: ~30 missiles to Mother Brain after breaking the glass casing (1 missile to shatter the glass). Mother Brain dies at 32 total hits (per RAM data at address $99)
- **Hazard**: Lava rises to cover the floor after the 4th Zebetite, forcing Samus to stand on small platforms while firing

### 7.3 Metroids

Metroids are the signature enemy of the final area (Tourian). They are unique in that:

- **Invulnerable to beams**: Beam shots cause them to recoil but deal no damage
- **Freeze + Missile**: The only way to kill a Metroid is to freeze it with the Ice Beam, then hit it with 5 missiles before it thaws
- **Latching**: If a Metroid contacts Samus, it attaches and continuously drains energy. Samus must enter Morph Ball and bomb herself free, or take enough damage for the invincibility frames to break loose
- **Wave Beam ineffective**: The Wave Beam cannot freeze Metroids, making the Ice Beam mandatory for Tourian

---

## 8. Economy

Metroid has no currency, shops, or trading systems. All upgrades are found in the environment. Resource management is limited to energy and missiles obtained through enemy drops (§1.6).

---

## 9. Minigames & Side Systems

Metroid has no minigames. The sole optional system is **completion time**, which determines the ending received (§5.2). There is no item percentage counter or completion tracker — the game only measures time.

---

## 10. UI & HUD

### 10.1 In-Game HUD

The HUD is displayed as a horizontal bar across the top of the screen:

```
EN  [##][##][##][ ][ ][ ] 74    MISSILES 030
```

| Element | Position | Description |
|---|---|---|
| "EN" label | Top-left | Indicates energy display |
| Energy Tanks | After "EN" | Row of squares. Filled (blue) = full tank. Empty = depleted tank. Max 6 displayed |
| Energy Number | After tanks | Two-digit number (00–99) showing current tank's remaining energy |
| Missile Count | Top-right | Three-digit number showing current missile ammo. Only appears after first Missile Tank is collected |

### 10.2 Pause Screen

Pressing Start opens the inventory/status screen, which displays:

- Current energy and missile counts
- Which upgrades have been collected (indicated by filled icons)
- Morph Ball, Bombs, beams, suit upgrades shown as inventory icons

### 10.3 Escape Sequence HUD

After defeating Mother Brain, a countdown timer appears where Mother Brain was, counting down from 999 (approximately 5 minutes 55 seconds real-time on NTSC). The message **"TIME BOMB SET. GET OUT FAST!!"** is displayed.

---

## 11. Engine & Presentation Systems

### 11.1 Scrolling & Camera

- **Single-axis scrolling**: Each room scrolls either horizontally or vertically, never both. This alternates between connected rooms
- **Camera tracking**: The camera follows Samus's position within the room. No manual camera control
- **Door transitions**: Entering a door freezes gameplay briefly while the next room loads. Samus walks through automatically

### 11.2 Save System — Password

Metroid uses a **24-character password** system (no battery save on the NES cartridge).

- **Password alphabet**: 64 characters — 0–9, A–Z, a–z, ?, -
- **Encoded data**: 144 bits total, storing:
  - Starting area (Brinstar, Norfair, Kraid's Lair, Ridley's Lair, or Tourian)
  - Items collected (each upgrade, Energy Tank, and Missile Tank has a bit)
  - Missile count (8-bit value, 0–255)
  - Boss defeat flags (Kraid, Ridley)
  - Game age / elapsed time (32-bit value, used for ending determination)
  - Samus state (suited / armorless)
- **Password generation**: Displayed on the Game Over screen. Also accessible by dying intentionally
- **Respawn behavior**: Loading a password starts Samus at the starting elevator of the encoded area with 30 energy

### 11.3 Difficulty

Metroid has a single difficulty setting. There are no selectable difficulty levels.

Difficulty is environmental and progressive:
- Brinstar enemies are weakest
- Norfair introduces lava hazards and tougher enemies
- Kraid's and Ridley's Lairs have the strongest standard enemies
- Tourian is linear and demanding: Metroids, Zebetites, and Mother Brain with limited refill opportunities

### 11.4 Camera & View

The game uses a fixed side-view perspective. The viewport is one NES screen (256×240 pixels). Samus is always roughly centered when the room permits scrolling.

### 11.5 Audio System

The soundtrack by Hirokazu Tanaka is context-driven:

| Track | Context |
|---|---|
| Title Theme | Title screen |
| Brinstar | Exploring Brinstar |
| Norfair / Kraid's Lair | Exploring Norfair and Kraid's Lair (shared theme) |
| Ridley's Lair | Exploring Ridley's Lair |
| Tourian | Exploring Tourian |
| Mini-Boss | Fighting Kraid or Ridley |
| Escape | Post-Mother Brain countdown sequence |
| Ending | Credits and ending screen |
| Item Acquisition | Brief jingle when collecting a major upgrade |

Music changes immediately upon entering a new area via door or elevator. No crossfading. Sound effects play on dedicated audio channels and can interrupt or layer with the music.

---

## 12. Open Questions / Unverified

- **Exact beam damage values**: Sources confirm Normal Beam and Ice Beam deal identical damage, but the precise numerical damage-per-hit for each beam type has not been verified from ROM data in this spec. Community sources describe kill counts (e.g., "2 beam shots for red Zoomer") but the underlying damage/HP integers are not widely documented outside disassembly projects.
- **Exact boss HP (Kraid, Ridley)**: Mother Brain dies at 32 hits (verified via RAM address $99). Kraid and Ridley's exact HP values are not confirmed in public-facing guides — they exist in the disassembly but are not consistently cited.
- **Item drop probability distribution**: Drops are frame-based (determined by the frame the enemy dies), but the exact probability table or RNG distribution is not documented in accessible sources for the NES version. A ROM hack ("Metroid Improved RNG") exists to fix the "extremely bad" original RNG.
- **Exact Zebetite HP and regeneration rate**: Community reports ~8–12 missiles each, with fast regeneration. Precise HP and regen-per-frame values are unverified.
- **Damage-per-frame from lava**: The continuous drain rate when standing in lava (with and without Varia Suit) is not precisely documented.
- **Fake Kraid HP**: Documented as "much weaker" than real Kraid. Exact value is 80 HP vs beams / 40 HP vs missiles per one datamining source, but this is not cross-verified.

---

## 13. References

### Wikis & Databases
- [Metroid — StrategyWiki](https://strategywiki.org/wiki/Metroid) — Items, enemies, walkthrough
- [Wikitroid (Fandom)](https://metroid.fandom.com/wiki/List_of_creatures_in_Metroid) — Creature list, endings, items
- [Metroid Database](https://metroiddatabase.com/old_site/m1/met-map.php) — Maps and level data
- [Metroid Recon (RetroPixel)](https://metroid.retropixel.net/games/metroid/items.php) — Weapons, items, and boss guides
- [01Nintendo — Metroid Enemies](https://en.01nintendo.com/metroid/metroid-1/metroid-enemies/) — Complete enemy bestiary with area breakdowns

### Guides & Walkthroughs
- [Omega Metroid — Bosses Walkthrough](https://omegametroid.com/metroid-walkthough/bosses/) — Boss strategies and mechanics
- [GameFAQs — Metroid Guide by BakonBitz](https://gamefaqs.gamespot.com/nes/519689-metroid/faqs/59512) — Comprehensive walkthrough
- [GameFAQs — Boss Guide by KirbyFreak101](https://gamefaqs.gamespot.com/nes/519689-metroid/faqs/13382) — Boss-specific guide

### Technical & Speedrunning
- [TASVideos — NES Metroid Game Resources](https://tasvideos.org/GameResources/NES/Metroid) — Frame data, movement physics, glitches
- [Data Crystal — Metroid RAM Map](https://datacrystal.tcrf.net/wiki/Metroid:RAM_map) — RAM addresses, enemy data structure, Mother Brain hit counter
- [Data Crystal — Metroid](https://datacrystal.tcrf.net/wiki/Metroid) — ROM structure, disassembly links
- [The Cutting Room Floor — Metroid](https://tcrf.net/Metroid) — Unused content, version differences

### Disassembly Projects
- [nmikstas/metroid-disassembly (GitHub)](https://github.com/nmikstas/metroid-disassembly) — Full 6502 disassembly
- [metroidret/m1disasm (GitHub)](https://github.com/metroidret/m1disasm) — Community-maintained disassembly

### Password System
- [Metroid Password Format Guide — emuWorks](https://games.technoplaza.net/mpg/password.txt) — Complete password encoding documentation
- [Wikitroid — List of Metroid Passwords](https://metroid.fandom.com/wiki/List_of_Metroid_passwords) — Notable passwords and their effects

### Endings
- [StrategyWiki — Metroid/Endings](https://strategywiki.org/wiki/Metroid/Endings) — Time thresholds and ending descriptions
- [Metroid Database — Endings](https://metroiddatabase.com/old_site/m1/endings.php) — Ending screenshots and requirements

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

Most items can be collected in any order. The player is free to explore with no map, no objective markers, and no in-game guidance.

### 1.2 Combat System

Combat is real-time. Samus fires her arm cannon in the direction she faces (left or right) or upward while standing. She cannot fire downward or diagonally.

**Fire Rate**: Samus can fire a beam every other frame (button must be released for 1 frame between shots). Firing every frame is possible by alternating directional input while holding the fire button.

**On-screen Projectile Limit**: Up to 3 beam shots on screen simultaneously. Missiles are limited to 1. Samus cannot fire a missile while a beam is active on-screen, but firing a beam/bomb first and quickly switching to missiles allows both to coexist.

### 1.3 Movement & Traversal

**Walking**: Constant speed, no acceleration curve. Samus loses all forward momentum upon landing — no ground slide or momentum carry.

**Jumping**: Variable height based on A button hold duration. Full air control — midair direction can be changed freely.

**Morph Ball**: Samus compresses to 1 tile tall (16 pixels). She can roll left/right and deploy bombs. No jump in this form (without bomb jumping).

**Bomb Jumping**: A bomb explosion provides upward thrust to Morph Ball Samus. A double bomb boost (placing a second bomb at the apex of the first boost) provides greater height for vertical traversal.

**Edge Jump**: At a platform's edge, releasing directional input on the correct frame allows Samus to fall while retaining her standing state, enabling a jump during the fall. Frame-precise technique.

### 1.4 Resource Management

| Resource | Starting Value | Maximum | Refill Method |
|---|---|---|---|
| Energy (HP) | 30 (of 99 base capacity) | 699 (99 base + 100 × 6 Energy Tanks) | Enemy drops |
| Missiles | 0 | 255 (8-bit cap) | Enemy drops |

**Respawn**: Upon death, Samus restarts at the starting elevator of the current area with 30 energy. All collected items and upgrades are retained.

### 1.5 Damage & Defense

**Invincibility Frames**: When Samus takes damage, she gains brief invulnerability (several frames), during which she can pass through enemies. Knockback pushes her in the opposite direction of the hit.

**Lava**: Contact with lava drains energy continuously. Present in Norfair, Ridley's Lair, and the Mother Brain chamber.

### 1.6 Item Drops

Defeated enemies may drop pickups. Drop type is determined by the frame on which the enemy is killed (frame-based RNG, not enemy type). The original NES version has a notoriously poor RNG.

| Drop | Effect |
|---|---|
| Small Energy | Restores 5 energy |
| Large Energy | Restores 20 energy |
| Missile Ammo | Restores 2 missiles |

Before acquiring the first Missile Tank, all drops are energy. Missile ammo drops only become possible after missiles are obtained.

### 1.7 Enemy Spawning

- **Pipe spawners**: Certain fixtures spawn enemies (Zebs, Geega, Gamet, etc.) infinitely at regular intervals
- **Screen-based spawning**: Standard enemies spawn when their screen scrolls into view. Scrolling a screen off-screen and back causes enemies to respawn
- **On-screen limit**: A finite number of enemy slots exist simultaneously; the number of active enemies on-screen affects whether new ones spawn

---

## 2. Controls & Input

| Input | Action |
|---|---|
| D-pad Left/Right | Walk |
| D-pad Up | Aim upward (cannot walk while aiming up) |
| D-pad Down | Crouch (standing) / Enter Morph Ball (crouching) |
| A | Jump / Unmorph (from Morph Ball) |
| B | Fire weapon (beam or missile) / Deploy bomb (Morph Ball) |
| Select | Toggle between Beam and Missile modes |
| Start | Pause (opens status screen) |

**Context-sensitive states**:
- **Morph Ball**: D-pad rolls; B deploys bombs; A or Up unmorphs
- **Crouching**: Can fire horizontally but cannot move
- **Aiming up**: Holding Up locks Samus in place; B fires vertically

---

## 3. World Structure

### 3.1 Planet Zebes — Overview

Zebes is divided into 5 interconnected areas. Brinstar serves as the central hub.

```
                    [Tourian]
                        |
                   (locked — requires both
                    Kraid and Ridley defeated)
                        |
    [Kraid's Lair] — [BRINSTAR] — [Norfair]
                                      |
                               [Ridley's Lair]
```

### 3.2 Area Descriptions

| Area | Tileset | Description |
|---|---|---|
| Brinstar | Blue/green rocky caves | Starting area and central hub. Contains most early upgrades (Morph Ball, Long Beam, Bombs, Ice Beam) |
| Norfair | Purple/red fiery caverns | Lava-filled tunnels below Brinstar's east. Contains Hi-Jump Boots, Screw Attack, Wave Beam |
| Kraid's Lair | Blue underground hideout | Mini-boss fortress beneath western Brinstar. Boss: Kraid |
| Ridley's Lair | Purple volcanic fortress | Mini-boss fortress beneath Norfair. Boss: Ridley |
| Tourian | Mechanical/alien corridors | Final linear area. Metroids, Zebetites, Mother Brain, escape sequence |

### 3.3 Room Structure & Scrolling

Rooms are composed of screens (256×240 pixels each). Each room scrolls in either the horizontal or vertical direction — never both simultaneously. This alternates between connected rooms: horizontal corridors connect to vertical shafts.

**Tiles**: 16×16 pixels. Samus standing is 16×32 (1×2 tiles). Morph Ball is 16×16 (1×1 tile).

**Doors**: Colored hatches connect rooms. Blue doors open with a single beam shot. Red doors require 5 missiles (permanently become blue once opened). Entering a door freezes gameplay briefly during transition.

**Elevators**: Vertical transport between areas. Samus stands on a platform that rises or descends.

### 3.4 Progression Gating

| Gate Type | Requirement |
|---|---|
| 1-tile passages | Morph Ball |
| Bomb blocks | Bombs (Morph Ball form) |
| Red doors | 5 missiles per door |
| Tall vertical shafts | Hi-Jump Boots (or bomb jumping) |
| Tourian access | Both Kraid and Ridley defeated |

**Mandatory items for completion**: Morph Ball, Missiles (at least one tank), Bombs, Ice Beam (required for Metroids in Tourian).

---

## 4. Playable Characters

Samus Aran is the sole playable character. She wears the Power Suit — a modular exoskeleton upgraded by items found throughout Zebes (§6).

**Armorless Samus**: Unlocked by completing the game in under 1 hour (§5.2). Cosmetic change only — gameplay is identical.

---

## 5. Story & Progression

### 5.1 Story Structure

Metroid has minimal in-game narrative. The manual provides backstory:

- The **Galactic Federation** discovers parasitic lifeforms called Metroids on planet SR388
- **Space Pirates** steal Metroid specimens and bring them to Planet Zebes
- The Space Pirates plan to weaponize the Metroids using beta rays
- **Samus Aran**, a bounty hunter, is dispatched to destroy the Metroids and defeat the leader **Mother Brain**

No dialogue, cutscenes, or in-game text beyond the title crawl.

### 5.2 Endings

Five endings based on total completion time. Faster completion reveals more of Samus beneath her suit.

| Ending | Time (NTSC) | Result |
|---|---|---|
| Best | Under 1 hour | Samus in a bikini. Unlocks Armorless Samus for replay |
| Good | 1–3 hours | Samus removes suit, shown in a leotard |
| Decent | 3–5 hours | Samus removes helmet only |
| Standard | Over 5 hours | Samus raises fist, armor stays on |
| Shame | Over 5 hours (Armorless playthrough only) | Samus turns away in shame |

The game timer runs from the start and is encoded in the password (§10.2).

---

## 6. Items & Equipment

### 6.1 Permanent Upgrades

| Upgrade | Location | Effect |
|---|---|---|
| Morph Ball | Brinstar (first item) | Compress to 1×1 tile for narrow passages |
| Bombs | Brinstar | Deploy in Morph Ball form. Destroy bomb blocks, damage enemies, enable bomb jumping |
| Long Beam | Brinstar | Extends beam range to full screen width. Permanent — persists across all beam types |
| Hi-Jump Boots | Norfair | ~1.5× normal jump height |
| Screw Attack | Norfair | Spinning jumps damage/destroy enemies on contact |
| Varia Suit | Brinstar | Reduces all damage (enemies and lava) by 50%. Changes suit color |

### 6.2 Beam Weapons

Beam weapons are **mutually exclusive** — collecting a new beam replaces the current one. Only one beam type active at a time. The Long Beam's range extension applies to all beams.

| Beam | Location | Effect |
|---|---|---|
| Normal Beam | Default | Single projectile. Short range without Long Beam |
| Ice Beam | Brinstar | Freezes enemies on contact. Frozen enemies become temporary platforms. Same damage as Normal Beam, but enemies must thaw before being re-hit (effectively doubles kill time). Required to defeat Metroids (§7.3) |
| Wave Beam | Norfair | Sinusoidal wave pattern. Passes through walls and solid terrain. Ineffective against Metroids |

### 6.3 Capacity Expansions

| Item | Count in Game | Effect per Unit |
|---|---|---|
| Energy Tank | 8 hidden (max 6 usable) | +100 maximum energy. Tanks beyond the 6th refill energy instead |
| Missile Tank | 21 hidden | +5 maximum missiles each |

**Missile capacity math**: 21 tanks × 5 = 105 from tanks + 75 from Kraid + 75 from Ridley = 255 max.

---

## 7. Enemies & Opponents

### 7.1 Common Enemies by Area

#### Brinstar

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zoomer | Yellow, Red | Crawls along floors, walls, and ceilings in a fixed loop | Yellow: 1 beam. Red: 2 beams |
| Skree | — | Clings to ceiling; dives at Samus when she passes beneath, then explodes into fragments | Vulnerable during dive |
| Zeb | Yellow, Red | Spawns infinitely from pipes, flies horizontally | Yellow: 1 beam. Red: 2 beams |
| Mellow | — | Phases through walls, swarms in groups | 1 beam |
| Ripper | Yellow, Red | Flies in a straight horizontal line, ignores Samus entirely | Indestructible (freezable for platforms). Red: missiles only |
| Rio | Yellow, Red | Swoops toward Samus from above in arcing patterns | Red: tougher than yellow |
| Waver | Yellow, Red | Erratic wave-pattern flight, hard to target | Similar between variants |

#### Norfair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Nova | Blue, Yellow | Crawls along surfaces, covered in heat-resistant fur | Blue: 2 beams / 1 missile. Yellow: 4 beams / 1 missile |
| Squeept | — | Leaps from lava in an arc, returns to lava | Resistant to beam |
| Ripper II | — | High-speed Ripper variant, emits fire from rear | Indestructible (freezable) |
| Dragon | — | Emerges from lava, breathes fire | Lava-dwelling, stationary |
| Multiviola | — | Bounces off walls unpredictably | 1 missile (difficult with beam) |
| Geruta | Purple, Red | Flies erratically, heat-emitting skin | Red: strongest attack in Norfair |
| Gamet | Purple, Red | Spawns from pipes in groups | Red: double attack of purple |
| Mella | — | Phases through walls in groups | 1 beam (weakest in area) |

#### Kraid's Lair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zeela | Yellow, Blue | Crawls along surfaces in all directions | Blue: stronger attack |
| Sidehopper | — | Hops aggressively toward Samus with double-jumps | 1 missile |
| Memu | — | Phases through walls in groups | 1 beam (weakest in area) |
| Geega | Yellow, Brown | Spawns from pipes, flies horizontally | Brown: double attack |

#### Ridley's Lair

| Enemy | Variants | Behavior | Durability |
|---|---|---|---|
| Zebbo | Blue, Yellow | Spawns from pipes | Blue: 1 beam. Yellow: 2 beams |
| Holtz | — | Drops from ceiling in armor, attacks then retreats | Several beams / 1 missile |
| Viola | Blue, Yellow | Ground-crawling Multiviola larvae | Blue: 2 beams / 1 missile. Yellow: 4 beams / 1 missile |
| Dessgeega | — | Aggressive jumper, high attack power | Several beams / 1 missile |

#### Tourian

| Enemy | Behavior | Durability |
|---|---|---|
| Rinka | Ring-shaped projectiles spawning from fixed points | Cannot be permanently destroyed; respawn endlessly. Freezable |
| Metroid | Latches onto Samus and drains energy continuously | See §7.3 |
| Cannon | Wall/ceiling turrets firing projectiles | Stationary, destructible |
| Zebetite | Organic barriers blocking path to Mother Brain | See §7.2 (Mother Brain) |

### 7.2 Bosses

#### Fake Kraid

Encountered before real Kraid. Identical appearance, much weaker. Defeated with 1 missile or Screw Attack.

#### Kraid

- **Attacks**: Fires horns from his back (arcing overhead) and spines from belly (horizontal). Belly projectiles block missiles and remain frozen in place if Ice Beam'd — still blocking missile shots
- **Effective strategies**: Morph Ball + Bombs at his feet (most reliable). Missiles work but belly spines interfere
- **Reward**: 75 missiles added to maximum capacity. Required for Tourian access

#### Ridley

- **Attacks**: Jumps vertically while firing fireballs that stun Samus
- **Effective strategies**: Freeze fireballs with Ice Beam, then missile Ridley at close range. Screw Attack allows getting behind him
- **Reward**: 75 missiles added to maximum capacity. Required for Tourian access

#### Mother Brain

- **Approach**: 5 Zebetites block the corridor. Each requires rapid successive missile hits (~8–12). Zebetites regenerate health if fire is interrupted — must be destroyed in one burst
- **Room hazards**: Rinkas (ring projectiles, max 3 on screen — freezable), cannon turrets, and lava that rises after the 4th Zebetite
- **Mother Brain itself**: Stationary, does not attack. 1 missile shatters the glass casing. 32 missiles to destroy Mother Brain (verified: RAM address $99 counts hits, death at 32)
- **Post-defeat**: Escape sequence triggers (§7.4)

### 7.3 Metroids

Unique enemies in Tourian with special rules:

- **Beam-proof**: Beam shots cause recoil but deal no damage
- **Kill method**: Freeze with Ice Beam, then hit with 5 missiles before it thaws
- **Latching**: On contact, attaches to Samus and continuously drains energy. Escape by entering Morph Ball and bombing, or enduring enough damage for invincibility frames to break free
- **Ice Beam mandatory**: Wave Beam cannot freeze them, making Ice Beam the only viable beam for Tourian

### 7.4 Escape Sequence

After Mother Brain's destruction, a time bomb activates. A countdown timer appears counting down from 999 (~5 minutes 55 seconds real-time NTSC). The message **"TIME BOMB SET. GET OUT FAST!!"** displays.

Samus must navigate back through Tourian and reach an elevator leading to the surface of Zebes. Reaching the surface ends the game and triggers the earned ending (§5.2). Running out of time results in death.

---

## 8. UI & HUD

### 8.1 In-Game HUD

The HUD is a horizontal bar across the top of the screen:

```
EN [##][##][##][ ][ ][ ] 74     MISSILES 030
```

| Element | Position | Description |
|---|---|---|
| "EN" label | Top-left | Energy indicator |
| Energy Tanks | After "EN" | Row of squares (max 6). Filled = full tank, empty = depleted |
| Energy Number | After tanks | 00–99, current tank's remaining energy |
| Missile Count | Top-right | 3-digit current ammo. Only visible after first Missile Tank |

### 8.2 Pause Screen

Start opens the status screen displaying:
- Current energy and missile counts
- Collected upgrade icons (Morph Ball, Bombs, beams, suit upgrades)

### 8.3 Escape Timer

Post-Mother Brain: countdown from 999 replaces the gameplay area's bottom, with frantic escape music.

---

## 9. Engine & Presentation Systems

### 9.1 Camera

Fixed side-view. Viewport is 256×240 pixels (one NES screen). Samus is roughly centered when the room permits scrolling. No manual camera control.

### 9.2 Save System — Password

24-character password system (no battery save).

- **Alphabet**: 64 characters — 0–9, A–Z, a–z, ?, -
- **Data encoded** (144 bits total):
  - Starting area (Brinstar, Norfair, Kraid's Lair, Ridley's Lair, or Tourian)
  - Items collected (each upgrade, Energy Tank, and Missile Tank has a bit)
  - Missile count (8-bit, 0–255)
  - Boss defeat flags (Kraid, Ridley)
  - Game age / elapsed time (32-bit value, determines ending)
  - Samus state (suited / armorless)
- **Displayed on**: Game Over screen
- **Resume behavior**: Starts at the encoded area's starting elevator with 30 energy

### 9.3 Difficulty

Single difficulty. No selectable levels. Difficulty is environmental:
- Brinstar: weakest enemies, introductory
- Norfair: lava hazards, tougher enemies
- Kraid's / Ridley's Lair: strongest standard enemies
- Tourian: linear, demanding — Metroids with limited refill opportunities

### 9.4 Audio

Soundtrack by Hirokazu Tanaka. Music is area-driven:

| Track | Context |
|---|---|
| Title Theme | Title screen |
| Brinstar | Brinstar exploration |
| Norfair / Kraid's Lair | Norfair and Kraid's Lair (shared) |
| Ridley's Lair | Ridley's Lair exploration |
| Tourian | Tourian exploration |
| Mini-Boss | Kraid or Ridley encounter |
| Escape | Post-Mother Brain countdown |
| Ending | Credits |
| Item Jingle | Major upgrade collected (brief) |

Music switches immediately on area transition. No crossfading. Sound effects play on dedicated NES audio channels and can layer with music.

---

## 10. Open Questions / Unverified

- **Beam damage integers**: Normal Beam and Ice Beam deal identical damage. Kill counts are documented (e.g., "2 beams for red Zoomer") but underlying damage/HP integers are in disassembly only — not widely published.
- **Kraid and Ridley HP**: Mother Brain dies at 32 hits (verified via RAM). Kraid/Ridley exact HP values exist in disassembly but are not consistently cited in public guides.
- **Item drop probability**: Drops are frame-determined. Exact distribution table for NES version is undocumented in accessible sources.
- **Zebetite HP and regen rate**: Community reports ~8–12 missiles each with fast regeneration. Precise values unverified.
- **Lava damage rate**: Continuous drain per frame (with/without Varia) not precisely documented.
- **Fake Kraid HP**: One datamining source reports 80 HP vs beams / 40 HP vs missiles. Not cross-verified.

---

## 11. References

### Wikis & Databases
- [Metroid — StrategyWiki](https://strategywiki.org/wiki/Metroid)
- [Wikitroid (Fandom)](https://metroid.fandom.com/wiki/List_of_creatures_in_Metroid)
- [Metroid Database — Maps](https://metroiddatabase.com/old_site/m1/met-map.php)
- [Metroid Recon — Items](https://metroid.retropixel.net/games/metroid/items.php)
- [01Nintendo — Enemies](https://en.01nintendo.com/metroid/metroid-1/metroid-enemies/)

### Guides
- [Omega Metroid — Bosses](https://omegametroid.com/metroid-walkthough/bosses/)
- [GameFAQs — Guide by BakonBitz](https://gamefaqs.gamespot.com/nes/519689-metroid/faqs/59512)
- [GameFAQs — Boss Guide by KirbyFreak101](https://gamefaqs.gamespot.com/nes/519689-metroid/faqs/13382)

### Technical & Speedrunning
- [TASVideos — NES Metroid Resources](https://tasvideos.org/GameResources/NES/Metroid)
- [Data Crystal — RAM Map](https://datacrystal.tcrf.net/wiki/Metroid:RAM_map)
- [The Cutting Room Floor — Metroid](https://tcrf.net/Metroid)

### Disassembly Projects
- [nmikstas/metroid-disassembly](https://github.com/nmikstas/metroid-disassembly)
- [metroidret/m1disasm](https://github.com/metroidret/m1disasm)

### Password & Endings
- [Password Format Guide — emuWorks](https://games.technoplaza.net/mpg/password.txt)
- [StrategyWiki — Endings](https://strategywiki.org/wiki/Metroid/Endings)

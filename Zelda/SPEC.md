# The Legend of Zelda — Gameplay Systems Spec

NES, 1986 (Famicom Disk System: Japan 1986, NES cartridge: NA 1987). Platform: NES. This spec covers the NES cartridge version (first quest unless otherwise noted).

---

## 1. Core Gameplay Systems

### Primary Loop

The gameplay loop is: **explore the overworld -> find a dungeon -> clear it for a Triforce piece -> repeat**. The player controls Link across an open overworld, locates nine underground dungeons, defeats their bosses, collects all eight Triforce of Wisdom fragments, then enters the ninth dungeon to defeat Ganon and rescue Princess Zelda.

The game is non-linear. Dungeons 1 through 4 can be attempted in any order; later dungeons require specific items from earlier ones (see §3). The player is free to explore the entire overworld from the start, with no narrative gating.

### Combat System

Combat is real-time, top-down, on a tile-based grid. Link can face and attack in four cardinal directions. There is no diagonal movement or attack.

**Damage unit:** 1 HP equals one Wooden Sword hit. Enemy health is measured in HP. Player damage taken is measured in half-hearts.

**Weapon damage values:**

| Weapon | Damage (HP) | Notes |
|---|---|---|
| Wooden Sword | 1 | Base unit |
| Wooden Sword Beam | 1 | Full-health only |
| White Sword | 2 | 2x Wooden |
| White Sword Beam | 2 | Full-health only |
| Magical Sword | 4 | 4x Wooden |
| Magical Sword Beam | 4 | Full-health only |
| Bomb | 4 | Equal to Magical Sword |
| Arrow | 2 | Equal to White Sword |
| Silver Arrow | 2 | Same as Arrow; required to finish Ganon |
| Magical Rod | 2 | Equal to White Sword |
| Magical Rod + Book | 2 (hit) + fire | Fire trail left on ground deals additional damage |
| Boomerang | 0 | Stuns enemies; kills 1 HP enemies (Keese, Gel) |
| Magical Boomerang | 0 | Same stun/kill; travels full screen |
| Blue Candle / Red Candle | 1 | Fire tile; one use per screen (Blue) or unlimited (Red) |

**Weapon hit priority order:** Boomerang > Beam (Sword/Rod) > Flame/Bomb Slot A > Flame/Bomb Slot B > Sword > Arrow/Rod. When multiple weapons overlap an enemy, they are checked in this order.

**Sword beam:** When Link is at full health (all Heart Containers filled), each sword swing also fires a projectile beam that travels across the screen. The beam deals the same damage as the sword itself.

### Defense System

| Ring | Damage Multiplier | Visual Effect |
|---|---|---|
| No ring | 1x (full damage) | Green tunic |
| Blue Ring | 0.5x (half damage) | Blue tunic |
| Red Ring | 0.25x (quarter damage) | Red tunic |

Damage reduction is applied via bit-shifting: enemy attack values are right-shifted by 1 (Blue Ring) or 2 (Red Ring).

**Shields** block projectiles when Link is facing the projectile source:

| Shield | Blocks |
|---|---|
| Small Shield (starting) | Arrows, Rocks (Octorok) |
| Magical Shield | Arrows, Rocks, Sword projectiles (Lynel), Magic blasts (Wizzrobe), fireballs |

The Magical Shield can be eaten by Like Likes, reverting Link to the Small Shield.

### Resource Management

- **Hearts:** Link's health. Starts with 3 Heart Containers, max 16. Each container holds 2 half-hearts. Damage is measured in half-heart increments.
- **Rupees:** Currency. Max 255. Spent on shop items and arrows (1 Rupee per shot).
- **Bombs:** Carried explosives. Initial max capacity 8; upgradeable to 12, then 16.
- **Keys:** Consumable. Each Key opens one locked door. Max 255. The Magical Key (from Level 8) replaces all Keys and opens all locked doors infinitely.

---

## 2. Controls & Input

NES controller (single-player):

| Input | Action |
|---|---|
| D-Pad | Move Link in four cardinal directions |
| A Button | Use equipped B-item (selected on subscreen) |
| B Button | Sword attack (swing in facing direction) |
| Start | Pause / open inventory subscreen |
| Select | Not used during gameplay |

**Saving:** Press Up + A on Controller 2 while paused to bring up the Save/Continue/Retry screen. Also appears on death.

### Movement Mechanics

- Link moves at 1 pixel per frame in the pressed direction.
- Movement is pixel-precise: a single D-Pad tap moves Link one pixel.
- Held input produces continuous movement with no acceleration curve.
- Direction changes are instant with no momentum.
- **Grid alignment:** While Link can be at any pixel position, continuous movement automatically nudges him toward the underlying 8x8 half-tile grid. If Link is N pixels off-grid, he re-aligns within N pixels of movement. This prevents snagging on obstacle corners.
- Screen transitions trigger when Link reaches a screen edge. The view scrolls to the adjacent screen. Link cannot move during transitions.

---

## 3. World Structure

### Overworld

The overworld is a contiguous 16-by-8 grid of screens (128 screens total). Each screen is 16 tiles wide by 11 tiles tall. Each tile is 16x16 pixels. The visible play area is 256x176 pixels per screen.

**Terrain types:** Forest, mountain/rock, water/lake, river, desert, graveyard, coast/beach, open grassland. The overworld uses seven palette colors: blue, white, brown, green, gray, tan, black.

**Named regions (approximate):**

| Region | Location | Features |
|---|---|---|
| Starting Area | Center-south | Cave with Old Man and first Sword |
| Forest | South/central | Dense trees; hides secret passages |
| Lake Hylia | South-center | Large lake; Raft dock |
| Lost Hills | North | Repeating-screen maze; requires specific path |
| Lost Woods | West-center | Repeating-screen maze; requires specific path |
| Death Mountain | North | Rocky passes; entrance to Level 9 |
| Graveyard | Northwest | 6 screens of graves; Ghinis; Magical Sword location |
| Coast / Beach | East | Eastmost peninsula (secret cave) |
| Desert | Southwest | Leevers spawn |

**Secrets:** Many screens contain hidden caves accessible by:
- Bombing specific walls/rocks
- Burning bushes with the Candle
- Pushing Armos statues
- Playing the Recorder

Secrets include shops, Rupee caches, Heart Containers, item upgrades, and hint-giving Old Men.

### Dungeons

Nine underground dungeons, each with a unique floor plan of interconnected rooms:

| Level | Name | Boss | Key Item Found | Triforce Piece |
|---|---|---|---|---|
| 1 | The Eagle | Aquamentus | Bow, Boomerang | Yes |
| 2 | The Moon | Dodongo | Magical Boomerang | Yes |
| 3 | The Manji | Manhandla | Raft | Yes |
| 4 | The Snake | Gleeok (2 heads) | Stepladder | Yes |
| 5 | The Lizard | Digdogger | Recorder (Whistle) | Yes |
| 6 | The Dragon | Gohma | Magical Rod | Yes |
| 7 | The Demon | Aquamentus | Red Candle | Yes |
| 8 | The Lion | Gleeok (4 heads) | Magical Key, Book of Magic | Yes |
| 9 | Death Mountain | Ganon | Silver Arrow, Red Ring | No (final dungeon) |

**Dungeon rooms** are single-screen chambers. Rooms may contain:
- Locked doors (require a Key)
- Bombable walls (destructible with Bombs)
- Dark rooms (require Candle to illuminate)
- Floor Traps (spike blocks that rush toward Link)
- Old Men who give hints, demand Rupee payment, or offer item choices
- Old Women (Hungry Goriya in Level 7 requires Food/Bait to pass)

**Dungeon items per dungeon:** Map (reveals room layout on minimap), Compass (shows boss/Triforce room location).

### Progression Gating

| Gate | Requires |
|---|---|
| Cross single-tile water gaps | Stepladder (Level 4) |
| Cross large water (docks) | Raft (Level 3) |
| Enter Level 7 | Recorder played at specific overworld screen |
| Enter Level 9 | All 8 Triforce pieces |
| Defeat Ganon | Silver Arrow (Level 9) |
| Access White Sword | 5 Heart Containers |
| Access Magical Sword | 12 Heart Containers |

---

## 4. Playable Characters / Classes

Link is the sole playable character. He has no stats, classes, or leveling. Progression is entirely item-driven (see §6).

**Starting state:** 3 Heart Containers, Small Shield, no sword (must pick up Wooden Sword in starting cave), 0 Rupees, 0 Bombs, 0 Keys.

---

## 5. Story & Progression

### Main Story

Ganon, the Prince of Darkness, has stolen the Triforce of Power and kidnapped Princess Zelda. Before being captured, Zelda shattered the Triforce of Wisdom into eight fragments and scattered them across Hyrule's dungeons. Link must recover all eight pieces, enter Death Mountain, defeat Ganon with the Silver Arrow, and rescue Zelda.

The story is conveyed entirely through the manual and brief NPC text. There are no cutscenes.

### Second Quest

Upon completing the game, or by entering "ZELDA" as the file name, the Second Quest begins:
- The overworld map remains the same, but all cave and dungeon entrance locations are rearranged (except Level 1, which stays in the same place).
- All nine dungeons have completely new floor plans and room layouts.
- Tougher enemies appear earlier.
- Item and secret locations are shuffled.
- Dungeons are significantly more complex and harder to navigate.

There is no further content after completing the Second Quest.

---

## 6. Items & Equipment

### Swords

| Sword | Damage | Location | Requirement |
|---|---|---|---|
| Wooden Sword | 1 | Starting cave (Old Man) | None |
| White Sword | 2 | Overworld cave (waterfall area) | 5+ Heart Containers |
| Magical Sword | 4 | Under grave in Graveyard | 12+ Heart Containers |

Each sword upgrade replaces the previous one. All swords fire a beam projectile when Link is at full health.

### Shields

| Shield | Blocks | Source |
|---|---|---|
| Small Shield | Rocks, Arrows | Starting equipment |
| Magical Shield | Rocks, Arrows, Sword beams, Magic blasts, Fireballs | Shop (90-160 Rupees) |

### Rings

| Ring | Effect | Source | Cost |
|---|---|---|---|
| Blue Ring | Half damage taken | Overworld shop | 250 Rupees |
| Red Ring | Quarter damage taken | Level 9 | Free (dungeon item) |

### Ranged Weapons

| Item | Effect | Source |
|---|---|---|
| Boomerang | Stuns enemies; kills Keese/Gel; half-screen range | Level 1 |
| Magical Boomerang | Same as Boomerang; full-screen range | Level 2 |
| Bow | Required to fire Arrows | Level 1 |
| Arrow | 2 HP damage per shot; costs 1 Rupee per use | Shop (80 Rupees) |
| Silver Arrow | 2 HP damage; required to kill Ganon | Level 9 |
| Bombs | 4 HP damage; destroys weak walls; max 8/12/16 | Drops, shops (20 Rupees per 4) |
| Magical Rod | Fires magic beam (2 HP damage) | Level 6 |
| Book of Magic | Adds fire trail to Magical Rod shots | Level 8 |

### Candles

| Candle | Effect | Source |
|---|---|---|
| Blue Candle | Creates fire tile; one use per screen | Shop (60 Rupees) |
| Red Candle | Creates fire tile; unlimited per screen | Level 7 |

Both light dark dungeon rooms, burn bushes to reveal secrets, and deal 1 HP contact damage to enemies.

### Utility Items

| Item | Effect | Source |
|---|---|---|
| Raft | Lets Link sail from specific dock tiles | Level 3 |
| Stepladder | Crosses single-tile water/gaps | Level 4 |
| Recorder (Whistle) | Warps to a visited dungeon entrance; reveals Level 7 entrance | Level 5 |
| Power Bracelet | Push certain rocks; reveals transport stairs | Overworld |
| Food (Bait) | Lures enemies; offered to Hungry Goriya in Level 7 | Shop (60-100 Rupees) |
| Letter | Give to Old Woman to unlock Potion shop | Overworld |
| Magical Key | Opens all locked doors infinitely; replaces Keys | Level 8 |

### Consumables

| Item | Effect | Source |
|---|---|---|
| Heart | Restores 1 heart | Enemy drops, shops (10 Rupees) |
| Fairy | Restores 3 hearts | Enemy drops; overworld fairy ponds restore all hearts |
| Blue Potion | Restores all hearts; single use | Potion shop (40 Rupees); requires Letter |
| Red Potion | Restores all hearts; two uses (becomes Blue after first) | Potion shop (68 Rupees) |
| Clock | Freezes all enemies on screen | Rare enemy drop |

### Dungeon Navigation Items

| Item | Effect | Per Dungeon |
|---|---|---|
| Map | Reveals dungeon room layout on minimap | 1 per dungeon |
| Compass | Shows boss/Triforce room as blinking dot | 1 per dungeon |
| Key | Opens one locked dungeon door | Multiple per dungeon |
| Triforce Fragment | Quest item; 8 required to enter Level 9 | 1 per dungeon (Levels 1-8) |

Collecting a Triforce Fragment fully restores Link's health.

---

## 7. Enemies & Opponents

### Overworld Enemies

| Enemy | HP | Damage (half-hearts) | Behavior | Notes |
|---|---|---|---|---|
| Red Octorok | 1 | 1 | Wanders, shoots rocks | Most common early enemy |
| Blue Octorok | 2 | 1 | Faster; shoots rocks | Tougher variant |
| Red Moblin | 2 | 1 | Wanders forest, shoots arrows | Shield blocks arrows |
| Blue Moblin | 3 | 1 | Shoots arrows; tougher | Found in deeper forests |
| Red Tektite | 1 | 1 | Leaps erratically | Rocky terrain |
| Blue Tektite | 1 | 1 | Faster jumps | Rocky terrain; rarer |
| Red Leever | 2 | 1 | Emerges from sand, charges | Desert/lake areas |
| Blue Leever | 4 | 1 | Faster; more durable | Desert; rarer |
| Red Lynel | 4 | 2 | Charges; fires sword beams | Mountains; Magical Shield blocks beams |
| Blue Lynel | 6 | 4 | Same as Red, tougher | Mountains; rarer |
| Peahat | 2 | 1 | Flies randomly; invulnerable while moving | Strike only when stationary |
| Armos | 3 | 1 | Dormant statue; charges when touched | Mountain entrances |
| Ghini | 9 | 1 | Floats in graveyard | Touching gravestones spawns more; kill the original to kill all spawns |
| Zora | 2 | 1 | Rises from water, shoots fireball | Cannot be reached by melee; wait for surface |
| Rock | N/A | 1 | Falls from mountain edges | Obstacle; cannot be destroyed |

### Dungeon Enemies

| Enemy | HP | Damage (half-hearts) | Behavior | Notes |
|---|---|---|---|---|
| Gel | 1 | 1 | Slow; seeks contact | Easiest enemy |
| Zol | 1 | 1 | Slow contact; splits into 2 Gels unless killed by Magical Sword | Levels 3-9 |
| Keese (Blue) | 1 | 1 | Flies erratically | All dungeons |
| Keese (Red) | 1 | 1 | Flies; faster | Levels 4, 6, 9 |
| Stalfos | 2 | 1 | Wanders with sword | Levels 1, 7 |
| Rope | 1 | 1 | Charges at Link when in line of sight | Levels 2, 7 |
| Wallmaster | 2 | 1 | Emerges from edges, floats toward Link | Sends Link back to dungeon entrance on contact |
| Red Goriya | 3 | 1 | Throws boomerang | Levels 1, 2, 7 |
| Blue Goriya | 5 | 2 | Throws extended-range boomerang | Levels 2, 7 |
| Moldorm | 2/segment | 1 | Crawling worm; multiple segments | Levels 2, 7 |
| Red Darknut | 4 | 1 | Sword-wielding knight; frontal attacks blocked | Must hit from side or behind; Levels 3, 5, 8 |
| Blue Darknut | 8 | 2 | Same as Red; much tougher | Levels 5, 8 |
| Gibdo | 7 | 2 | Slow mummy; persistent | Levels 5, 8 |
| Pols Voice | 10 | 2 | Hops toward Link | Extremely weak to Arrows (1-hit kill); Levels 5, 8 |
| Like Like | 9 | 1 | Engulfs Link; steals Magical Shield | Keep distance; Levels 4, 6, 9 |
| Vire | 1 | 1 | Jumps toward Link; splits into 2 Red Keese unless killed by Magical Sword | Levels 4, 6, 9 |
| Red Wizzrobe | 4 | 2 | Teleports; fires magic beams; moves in patterns | Bombs kill in one hit; Levels 6, 9 |
| Blue Wizzrobe | 10 | 4 | Teleports; rapid-fire magic; appears briefly | Most dangerous regular enemy; Levels 6, 9 |
| Red Lanmola | 2/segment | 1 | Multi-segment crawling centipede | Level 9 |
| Blue Lanmola | 2/segment | 1 | Faster variant | Level 9 |
| Bubble | N/A | 0 | Bounces around room | Contact disables sword temporarily; invincible |
| Blue Bubble | N/A | 0 | Same as Bubble | Disables sword; cured by touching Red Bubble |
| Red Bubble | N/A | 0 | Same as Bubble | Cures Blue Bubble's sword-disable effect |
| Floor Trap | N/A | 1 | Spike block rushes toward Link when aligned | Avoidance only; indestructible |

### Bosses

| Boss | HP | Dungeons | Attack Pattern | Weakness / Strategy |
|---|---|---|---|---|
| Aquamentus | 6 | 1, 7 | Walks slowly; fires 3 fireballs in spread pattern | Sword strikes; stand off to the side |
| Dodongo | N/A | 2, 5, 7 | Charges across room | Feed 2 Bombs (walk into its open mouth); immune to sword |
| Manhandla | 4 heads | 3, 4, 8 | 4-armed plant; moves and fires projectiles; speeds up as heads are destroyed | Destroy each head; Bombs can kill multiple heads at once |
| Gleeok (2 heads) | ~10/head | 4 | Stationary dragon; fires constant stream of fireballs; severed heads become flying projectiles | Sword each head (10 hits for first head, 6 for second with Wooden Sword) |
| Gleeok (3 heads) | ~10/head | 6 | Same as 2-head; one additional head | Same strategy |
| Gleeok (4 heads) | ~10/head | 8 | Same; four heads | Same strategy; most dangerous Gleeok |
| Digdogger | N/A | 5, 7 | Large eye creature; invulnerable in large form | Play Recorder to shrink; then sword the small form(s). Level 5: shrinks to 1. Level 7: splits into 3 |
| Gohma (Red) | 1 (arrow) | 6 | Moves horizontally; fires fireballs; eye opens/closes | Shoot Arrow into open eye; one-hit kill |
| Gohma (Blue) | 3 (arrows) | 8 | Same as Red; faster | Requires 3 Arrow hits to eye |
| Patra | 11 (core) | 9 | Core orbited by 8 smaller eyes; orbiting eyes must be killed first | Kill all 8 orbiting eyes (6 HP each), then attack core |
| Ganon | 15 | 9 | Invisible; teleports; fires fireballs from random positions | Hit 4 times with Magical Sword (8 with White, 15 with Wooden) to make visible; finish with Silver Arrow |

### Enemy Scaling

Enemies do not level-scale. Each dungeon and overworld screen has fixed enemy spawns. Difficulty increases through dungeon ordering: early dungeons contain Gels, Stalfos, and Keese; late dungeons contain Wizzrobes, Darknuts, and Like Likes.

---

## 8. Economy

### Currency

**Rupees** are the sole currency. Max: 255. Displayed on HUD.

### Income Sources

| Source | Amount |
|---|---|
| Enemy drops (single Rupee) | 1 |
| Enemy drops (blue Rupee) | 5 |
| Secret caves (Old Man gifts) | 10, 30, 100 |
| Gambling (Money Making Game) | Variable (see §9) |
| Bombing/burning secret rooms | 10-100 |

### Shops

Shops are hidden in overworld caves. Four shop variants exist with different stock and prices:

**Common shop items:**

| Item | Price Range (Rupees) |
|---|---|
| Magical Shield | 90-160 |
| Blue Candle | 60 |
| Arrows | 80 |
| Bombs (4-pack) | 20 |
| Key | 80-100 |
| Food (Bait) | 60-100 |
| Blue Ring | 250 |
| Blue Potion | 40 |
| Red Potion | 68 |
| Heart | 10 |

### Rupee Sinks

- Arrow shots (1 Rupee each)
- Shop purchases
- Old Man "door repair charge" (forced payment of 20 Rupees for bombing certain dungeon walls)
- Gambling losses

---

## 9. Minigames & Side Systems

### Money Making Game

Found in hidden overworld caves. The Old Man says "LET'S PLAY MONEY MAKING GAME." Three Rupee-shaped choices are presented. One awards Rupees; the other two cost Rupees. Positions and exact values are randomized. Requires at least 10 Rupees to play.

### Item Drop System

Enemies drop items upon death according to a deterministic system:

**Drop Groups:**

| Group | Drop Rate | Enemies |
|---|---|---|
| A | 31% | Red Octorok, Blue Octorok, Red Moblin, Red Darknut, and others |
| B | 41% | Blue Moblin, Red Goriya, Blue Goriya, Red Lynel, and others |
| C | 59% | Blue Darknut, Blue Lynel, Blue Wizzrobe, Gibdo, and others |
| D | 41% | Red Tektite, Blue Tektite, Red Leever, Stalfos, Rope, and others |
| X | 0% | Gels spawned from Zol, Keese spawned from Vire, Traps, Bubbles |

A **kill counter** increments with each enemy killed (excluding X-group spawns). The counter position determines which item drops from the group's sequence table. Possible drops: Rupee (1), Heart, Fairy, Bomb, Blue Rupee (5), Clock.

**Forced drops (consecutive kills without taking damage):**
- 10 kills without being hit: next dropping enemy gives 5 Rupees (or a Bomb if killed with a Bomb).
- 16 kills without being hit: next dropping enemy gives a Fairy. Resets the streak counter.

### Secrets and Collection

- **Heart Containers:** 13 total beyond starting 3 hearts. 8 from dungeon bosses (Levels 1-8), 5 hidden in overworld caves. Max 16 hearts.
- **Bomb capacity upgrades:** Found in specific dungeon rooms. 8 -> 12 (Level 5) -> 16 (Level 7).

---

## 10. UI & HUD

### Gameplay HUD

The top portion of the screen (3 tile rows) is a persistent HUD, always visible during gameplay. The play area occupies the lower portion.

**HUD layout (left to right):**

| Element | Position | Description |
|---|---|---|
| Minimap | Top-left | Shows overworld position (16x8 grid) or dungeon room layout (if Map collected). Link's position shown as a blinking dot. Compass makes boss room blink. |
| Rupee count | Center-left | Numerical display; "X" prefix; max 255 |
| Key count | Center-left (below Rupees) | Numerical display; "X" prefix |
| Bomb count | Center-left (below Keys) | Numerical display; "X" prefix |
| B-Button item | Center | Icon of currently equipped secondary item |
| A-Button item | Center-right | Always shows Sword icon |
| Life (hearts) | Top-right | Row of heart icons; full = red, half = half-filled, empty = outlined. Displays current/maximum hearts. |

### Inventory Subscreen

Pressing Start pauses the game and reveals the inventory subscreen, which overlays the play area:

- **Item grid:** Shows all collected secondary items (Boomerang, Bombs, Bow + Arrows, Candle, Recorder, Food, Potion, Magical Rod, etc.). Player selects one to assign to the B Button.
- **Triforce tracker:** Shows which of the 8 Triforce fragments have been collected.
- **Dungeon map:** If Map is collected, shows full room layout of current dungeon.

### Damage Feedback

- Link flashes/blinks when hit (invincibility frames).
- Enemies flash when struck.
- No damage numbers displayed.
- Enemy death: enemy sprite disappears in a puff animation.
- Boss death: extended explosion animation before dropping Heart Container and revealing Triforce Fragment room.

---

## 11. Engine & Presentation Systems

### Save System

The Legend of Zelda was the first NES game to feature battery-backed save RAM.

- **3 save slots**, each with a player-chosen name (up to 8 characters).
- **Saved state includes:** inventory, Heart Containers, collected Triforce pieces, collected dungeon items, Rupees (reset to 0 on continue for some versions), bomb count, keys. Dungeon room clear states are saved; enemy positions reset.
- **Save access:** Death screen (Continue / Save / Retry) or Up+A on Controller 2 while paused.
- **Continue:** Respawns Link at the overworld start screen (or dungeon entrance if in a dungeon) with 3 hearts, retaining all items and progress.
- **Retry:** Same as Continue but does not save.

### Camera

Fixed top-down orthographic view. One screen = one "room." The camera scrolls (horizontally or vertically) when Link exits a screen edge, transitioning to the adjacent screen. No free camera movement.

### Audio System

- **Overworld theme:** Plays continuously while on the overworld.
- **Dungeon theme:** Different track plays in all dungeon rooms.
- **Boss fanfare:** Distinct track when entering a boss room (specific to some bosses).
- **Item fanfare:** Short jingle plays when collecting a major item (Triforce piece, Heart Container, dungeon item).
- **Death jingle:** Plays when Link's hearts reach zero.
- **Low health beep:** Continuous beeping sound when Link has 1 or fewer hearts remaining. Persists until healed above threshold.
- **Game Over / Continue screen** has its own track.
- Music restarts on each screen transition.

### Difficulty

No difficulty settings. The Second Quest (see §5) serves as a built-in hard mode with rearranged dungeons, earlier tough enemies, and reorganized secrets.

---

## 12. Open Questions / Unverified

- **Exact boss HP values:** Aquamentus is well-documented at 6 HP. Gleeok head HP (~10 per head with Wooden Sword) is sourced from speedrun communities but not fully ROM-verified in accessible sources. Manhandla per-head HP and Digdogger (shrunken form) HP need ROM disassembly confirmation.
- **Arrow damage vs. bomb damage:** Arrows are widely cited as 2 HP (equal to White Sword). Some sources suggest arrows deal 4 HP (equal to bombs). The disassembly at `github.com/aldonunez/zelda1-disassembly` would be authoritative.
- **Exact drop sequence tables:** The per-position item in each group's 10-entry kill counter sequence is documented in speedrun community charts (ZeldaSpeedRuns) but stored as images, not text. Full table could not be extracted.
- **Shop variant stock:** Four shop types exist with varying prices, but the exact item list per variant and all shop overworld locations are not fully catalogued here.
- **Enemy damage values (complete):** The table above lists damage for common enemies. Contact damage for some rarer enemies (Lanmola, Moldorm segments) may differ and needs verification.
- **Pols Voice microphone weakness:** In the Famicom Disk System version, Pols Voice can be killed by shouting into the Famicom's second controller microphone. This feature does not exist on the NES version (no microphone). On NES, Arrows are the intended weakness.
- **Second Quest enemy placement:** Full room-by-room enemy assignments for the Second Quest dungeons are not documented here.

---

## 13. References

### Wikis & Guides
- [StrategyWiki — The Legend of Zelda / Enemies](https://strategywiki.org/wiki/The_Legend_of_Zelda/Enemies)
- [StrategyWiki — The Legend of Zelda / Items](https://strategywiki.org/wiki/The_Legend_of_Zelda/Items)
- [Zelda Wiki (Fandom) — Items in The Legend of Zelda](https://zelda.fandom.com/wiki/Items_in_The_Legend_of_Zelda)
- [Zelda Wiki (Fandom) — Weapon Strength](https://zelda.fandom.com/wiki/Weapon_Strength)
- [Zelda Wiki (Fandom) — Bosses in The Legend of Zelda](https://zelda.fandom.com/wiki/Bosses_in_The_Legend_of_Zelda)
- [Zelda Dungeon Wiki — The Legend of Zelda Bosses](https://www.zeldadungeon.net/wiki/The_Legend_of_Zelda_Bosses)
- [Zelda Dungeon Wiki — Item Drop Rates](https://www.zeldadungeon.net/wiki/Item_Drop_Rates_(The_Legend_of_Zelda))
- [RPG Classics — Enemies](https://tartarus.rpgclassics.com/zelda1/1stquest/enemies.shtml)
- [RPG Classics — Items](https://tartarus.rpgclassics.com/zelda1/1stquest/items.shtml)
- [RPG Classics — Bosses](https://tartarus.rpgclassics.com/zelda1/1stquest/bosses.shtml)

### Technical & Speedrun Sources
- [Red Candle — Legend of Zelda / Technical Information](https://redcandle.us/Legend_of_Zelda/Technical_Information)
- [Red Candle — Legend of Zelda / Bosses](https://redcandle.us/Legend_of_Zelda/Bosses)
- [ZeldaSpeedRuns — Item Drops Chart](https://www.zeldaspeedruns.com/loz/generalknowledge/item-drops-chart)
- [ZeldaSpeedRuns — Forced Item Drops](https://www.zeldaspeedruns.com/loz/tech/forced-item-drops)
- [NESDev Forums — Attack/Defense Formula](https://forums.nesdev.org/viewtopic.php?t=18039)
- [Troy Gilbert — Deconstructing Zelda: Movement Mechanics](https://troygilbert.com/deconstructing-zelda/movement-mechanics/)

### Disassembly / ROM Data
- [aldonunez/zelda1-disassembly (GitHub)](https://github.com/aldonunez/zelda1-disassembly) — Complete disassembly of The Legend of Zelda
- [camthesaxman/zeldasource (GitHub)](https://github.com/camthesaxman/zeldasource) — Disassembly of Legend of Zelda
- [Invent with Python — NES Zelda Map Data](https://inventwithpython.com/blog/8-bit-nes-legend-of-zelda-map-data.html)

### Community
- [GameFAQs — Enemy and Item HP/damage thread](https://gamefaqs.gamespot.com/boards/563433-the-legend-of-zelda/50876050)
- [GameFAQs — Damage mechanics thread](https://gamefaqs.gamespot.com/boards/563433-the-legend-of-zelda/78144721)
- [SDA Knowledge Base — The Legend of Zelda](https://kb.speeddemosarchive.com/The_Legend_of_Zelda)

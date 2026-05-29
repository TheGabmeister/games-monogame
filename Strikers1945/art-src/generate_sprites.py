from __future__ import annotations

import argparse
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parent
PROJECT_ROOT = ROOT.parent


ASSETS = [
    {
        "svg": "sprites/player/player_ship.svg",
        "png": "Content/sprites/player/player_ship.png",
        "size": (48, 48),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
  <defs>
    <linearGradient id="body" x1="0" y1="4" x2="0" y2="44" gradientUnits="userSpaceOnUse">
      <stop offset="0" stop-color="#f4fbff"/>
      <stop offset="0.52" stop-color="#67c8ec"/>
      <stop offset="1" stop-color="#1f5f9b"/>
    </linearGradient>
    <linearGradient id="wing" x1="6" y1="18" x2="42" y2="38" gradientUnits="userSpaceOnUse">
      <stop offset="0" stop-color="#d6f7ff"/>
      <stop offset="1" stop-color="#2381c6"/>
    </linearGradient>
  </defs>
  <g stroke="#07121e" stroke-width="1.5" stroke-linejoin="round">
    <path d="M24 3 L30 19 L44 29 L31 33 L28 43 L24 38 L20 43 L17 33 L4 29 L18 19 Z" fill="url(#wing)"/>
    <path d="M24 4 C29 13 30 26 27 39 L24 44 L21 39 C18 26 19 13 24 4 Z" fill="url(#body)"/>
    <path d="M21 16 C22 11 26 11 27 16 L27 24 L21 24 Z" fill="#1b2f54"/>
    <path d="M22 17 C23 14 25 14 26 17 L26 20 L22 20 Z" fill="#9be8ff" opacity="0.88"/>
    <path d="M16 30 L8 34 L18 35 Z" fill="#f2d14c"/>
    <path d="M32 30 L40 34 L30 35 Z" fill="#f2d14c"/>
    <circle cx="24" cy="31" r="2.2" fill="#ffffff"/>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/player/player_ship_left.svg",
        "png": "Content/sprites/player/player_ship_left.png",
        "size": (48, 48),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
  <g transform="rotate(-10 24 24)">
    <defs>
      <linearGradient id="body" x1="0" y1="4" x2="0" y2="44" gradientUnits="userSpaceOnUse">
        <stop offset="0" stop-color="#f4fbff"/>
        <stop offset="0.52" stop-color="#67c8ec"/>
        <stop offset="1" stop-color="#1f5f9b"/>
      </linearGradient>
      <linearGradient id="wing" x1="6" y1="18" x2="42" y2="38" gradientUnits="userSpaceOnUse">
        <stop offset="0" stop-color="#d6f7ff"/>
        <stop offset="1" stop-color="#2381c6"/>
      </linearGradient>
    </defs>
    <g stroke="#07121e" stroke-width="1.5" stroke-linejoin="round">
      <path d="M24 3 L30 19 L44 29 L31 33 L28 43 L24 38 L20 43 L17 33 L4 29 L18 19 Z" fill="url(#wing)"/>
      <path d="M24 4 C29 13 30 26 27 39 L24 44 L21 39 C18 26 19 13 24 4 Z" fill="url(#body)"/>
      <path d="M21 16 C22 11 26 11 27 16 L27 24 L21 24 Z" fill="#1b2f54"/>
      <path d="M22 17 C23 14 25 14 26 17 L26 20 L22 20 Z" fill="#9be8ff" opacity="0.88"/>
      <path d="M15 30 L7 34 L18 35 Z" fill="#f2d14c"/>
      <path d="M32 30 L39 34 L30 35 Z" fill="#f2d14c"/>
      <circle cx="24" cy="31" r="2.2" fill="#ffffff"/>
    </g>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/player/player_ship_right.svg",
        "png": "Content/sprites/player/player_ship_right.png",
        "size": (48, 48),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
  <g transform="rotate(10 24 24)">
    <defs>
      <linearGradient id="body" x1="0" y1="4" x2="0" y2="44" gradientUnits="userSpaceOnUse">
        <stop offset="0" stop-color="#f4fbff"/>
        <stop offset="0.52" stop-color="#67c8ec"/>
        <stop offset="1" stop-color="#1f5f9b"/>
      </linearGradient>
      <linearGradient id="wing" x1="6" y1="18" x2="42" y2="38" gradientUnits="userSpaceOnUse">
        <stop offset="0" stop-color="#d6f7ff"/>
        <stop offset="1" stop-color="#2381c6"/>
      </linearGradient>
    </defs>
    <g stroke="#07121e" stroke-width="1.5" stroke-linejoin="round">
      <path d="M24 3 L30 19 L44 29 L31 33 L28 43 L24 38 L20 43 L17 33 L4 29 L18 19 Z" fill="url(#wing)"/>
      <path d="M24 4 C29 13 30 26 27 39 L24 44 L21 39 C18 26 19 13 24 4 Z" fill="url(#body)"/>
      <path d="M21 16 C22 11 26 11 27 16 L27 24 L21 24 Z" fill="#1b2f54"/>
      <path d="M22 17 C23 14 25 14 26 17 L26 20 L22 20 Z" fill="#9be8ff" opacity="0.88"/>
      <path d="M16 30 L9 34 L18 35 Z" fill="#f2d14c"/>
      <path d="M33 30 L41 34 L30 35 Z" fill="#f2d14c"/>
      <circle cx="24" cy="31" r="2.2" fill="#ffffff"/>
    </g>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/enemies/enemy_popcorn.svg",
        "png": "Content/sprites/enemies/enemy_popcorn.png",
        "size": (32, 32),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
  <g stroke="#251006" stroke-width="1.3" stroke-linejoin="round">
    <path d="M16 5 L21 15 L30 19 L22 22 L20 28 L16 24 L12 28 L10 22 L2 19 L11 15 Z" fill="#df6b38"/>
    <path d="M16 6 C20 12 20 20 16 27 C12 20 12 12 16 6 Z" fill="#ffe16b"/>
    <path d="M13 13 L19 13 L18 18 L14 18 Z" fill="#3d1f2a"/>
    <path d="M8 20 L4 23 L10 23 Z" fill="#f7f1a5"/>
    <path d="M24 20 L28 23 L22 23 Z" fill="#f7f1a5"/>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/enemies/enemy_fighter.svg",
        "png": "Content/sprites/enemies/enemy_fighter.png",
        "size": (40, 40),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 40 40">
  <defs>
    <linearGradient id="redBody" x1="0" y1="4" x2="0" y2="36" gradientUnits="userSpaceOnUse">
      <stop offset="0" stop-color="#ffd0b8"/>
      <stop offset="0.55" stop-color="#d6403e"/>
      <stop offset="1" stop-color="#6e1c36"/>
    </linearGradient>
  </defs>
  <g stroke="#210816" stroke-width="1.5" stroke-linejoin="round">
    <path d="M20 4 L26 18 L38 25 L27 27 L24 36 L20 31 L16 36 L13 27 L2 25 L14 18 Z" fill="#a72f49"/>
    <path d="M20 5 C25 13 26 25 22 36 L20 39 L18 36 C14 25 15 13 20 5 Z" fill="url(#redBody)"/>
    <path d="M17 15 C18 11 22 11 23 15 L23 21 L17 21 Z" fill="#22283b"/>
    <path d="M18 15 C19 13 21 13 22 15 L22 18 L18 18 Z" fill="#ffeba4"/>
    <circle cx="11" cy="25" r="2" fill="#f6d94a"/>
    <circle cx="29" cy="25" r="2" fill="#f6d94a"/>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/enemies/enemy_gunship.svg",
        "png": "Content/sprites/enemies/enemy_gunship.png",
        "size": (64, 64),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64">
  <defs>
    <linearGradient id="armor" x1="0" y1="8" x2="0" y2="58" gradientUnits="userSpaceOnUse">
      <stop offset="0" stop-color="#e8e1cf"/>
      <stop offset="0.5" stop-color="#797a6b"/>
      <stop offset="1" stop-color="#2e3432"/>
    </linearGradient>
  </defs>
  <g stroke="#0f1412" stroke-width="2" stroke-linejoin="round">
    <path d="M32 6 L42 23 L61 33 L45 39 L40 56 L32 49 L24 56 L19 39 L3 33 L22 23 Z" fill="#666f66"/>
    <path d="M32 8 C42 20 45 40 36 58 L32 62 L28 58 C19 40 22 20 32 8 Z" fill="url(#armor)"/>
    <path d="M25 20 C27 13 37 13 39 20 L39 30 L25 30 Z" fill="#202b39"/>
    <path d="M27 21 C29 17 35 17 37 21 L37 26 L27 26 Z" fill="#8bd7ff"/>
    <circle cx="18" cy="38" r="4" fill="#d3b84b"/>
    <circle cx="46" cy="38" r="4" fill="#d3b84b"/>
    <path d="M12 36 L12 46" stroke="#171b1a" stroke-width="4" stroke-linecap="round"/>
    <path d="M52 36 L52 46" stroke="#171b1a" stroke-width="4" stroke-linecap="round"/>
    <path d="M28 45 L36 45 L34 55 L30 55 Z" fill="#242927"/>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/bullets/bullet_player.svg",
        "png": "Content/sprites/bullets/bullet_player.png",
        "size": (8, 16),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="8" height="16" viewBox="0 0 8 16">
  <path d="M4 0 L7 5 L5 15 L3 15 L1 5 Z" fill="#7df6ff"/>
  <path d="M4 2 L5.5 6 L4.6 13 L3.4 13 L2.5 6 Z" fill="#ffffff"/>
</svg>''',
    },
    {
        "svg": "sprites/bullets/bullet_enemy_round.svg",
        "png": "Content/sprites/bullets/bullet_enemy_round.png",
        "size": (12, 12),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 12 12">
  <circle cx="6" cy="6" r="5.5" fill="#5a160b"/>
  <circle cx="6" cy="6" r="4.4" fill="#ff5b28"/>
  <circle cx="4.5" cy="4.2" r="2.2" fill="#ffd15f"/>
  <circle cx="3.7" cy="3.4" r="0.9" fill="#ffffff"/>
</svg>''',
    },
    {
        "svg": "sprites/bullets/bullet_enemy_needle.svg",
        "png": "Content/sprites/bullets/bullet_enemy_needle.png",
        "size": (6, 18),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="6" height="18" viewBox="0 0 6 18">
  <path d="M3 0 L5.8 6 L4 18 L2 18 L0.2 6 Z" fill="#4c0c20"/>
  <path d="M3 2 L4.2 6.5 L3.5 15 L2.5 15 L1.8 6.5 Z" fill="#ffb13b"/>
</svg>''',
    },
    {
        "svg": "sprites/powerups/powerup_weapon.svg",
        "png": "Content/sprites/powerups/powerup_weapon.png",
        "size": (24, 24),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
  <circle cx="12" cy="12" r="10.5" fill="#0d4463" stroke="#f8feff" stroke-width="2"/>
  <circle cx="12" cy="12" r="7.5" fill="#35c8ff"/>
  <text x="12" y="16.5" text-anchor="middle" font-family="Arial, sans-serif" font-size="12" font-weight="700" fill="#ffffff" stroke="#063549" stroke-width="0.6">P</text>
</svg>''',
    },
    {
        "svg": "sprites/powerups/powerup_bomb.svg",
        "png": "Content/sprites/powerups/powerup_bomb.png",
        "size": (24, 24),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
  <rect x="3" y="3" width="18" height="18" rx="4" fill="#4c174f" stroke="#fff3ff" stroke-width="2"/>
  <circle cx="12" cy="12" r="7" fill="#e052c6"/>
  <text x="12" y="16.5" text-anchor="middle" font-family="Arial, sans-serif" font-size="12" font-weight="700" fill="#ffffff" stroke="#43114b" stroke-width="0.6">B</text>
</svg>''',
    },
    {
        "svg": "sprites/powerups/powerup_score.svg",
        "png": "Content/sprites/powerups/powerup_score.png",
        "size": (24, 24),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
  <circle cx="12" cy="12" r="10" fill="#6f4b09" stroke="#fff0a6" stroke-width="2"/>
  <circle cx="12" cy="12" r="7.2" fill="#ffd74a"/>
  <path d="M12 5.2 L14 9.5 L18.6 10.1 L15.2 13.3 L16.1 17.8 L12 15.5 L7.9 17.8 L8.8 13.3 L5.4 10.1 L10 9.5 Z" fill="#fff6be" stroke="#8a5b08" stroke-width="0.8" stroke-linejoin="round"/>
</svg>''',
    },
    {
        "svg": "sprites/hud/hud_life_icon.svg",
        "png": "Content/sprites/hud/hud_life_icon.png",
        "size": (24, 24),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
  <g stroke="#07121e" stroke-width="1" stroke-linejoin="round">
    <path d="M12 2 L15 10 L22 15 L15.5 16.7 L14 22 L12 19 L10 22 L8.5 16.7 L2 15 L9 10 Z" fill="#64d7ff"/>
    <path d="M12 3 C14.2 8 14.6 15 12 21 C9.4 15 9.8 8 12 3 Z" fill="#f4fbff"/>
    <path d="M10.4 8.2 C11 6.6 13 6.6 13.6 8.2 L13.6 11 L10.4 11 Z" fill="#1b2f54"/>
  </g>
</svg>''',
    },
    {
        "svg": "sprites/hud/hud_bomb_icon.svg",
        "png": "Content/sprites/hud/hud_bomb_icon.png",
        "size": (24, 24),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">
  <circle cx="11.5" cy="13" r="7.8" fill="#2a1d31" stroke="#fff2cf" stroke-width="1.6"/>
  <path d="M16 7 L19.5 3.5" stroke="#fff2cf" stroke-width="2" stroke-linecap="round"/>
  <path d="M18.5 3.5 C20 4.1 20.5 5.4 20.1 6.8" fill="none" stroke="#ffb33c" stroke-width="1.6" stroke-linecap="round"/>
  <path d="M8 10 C9.5 8 12.5 8 14 10" fill="none" stroke="#f06ad7" stroke-width="2" stroke-linecap="round"/>
</svg>''',
    },
    {
        "svg": "sprites/fx/explosion.svg",
        "png": "Content/sprites/fx/explosion.png",
        "size": (384, 64),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="384" height="64" viewBox="0 0 384 64">
  <defs>
    <radialGradient id="hot" cx="50%" cy="45%" r="55%">
      <stop offset="0" stop-color="#ffffff"/>
      <stop offset="0.3" stop-color="#ffe56b"/>
      <stop offset="0.62" stop-color="#ff6b2b"/>
      <stop offset="1" stop-color="#631018" stop-opacity="0"/>
    </radialGradient>
  </defs>
  <g>
    <circle cx="32" cy="32" r="8" fill="url(#hot)"/>
    <circle cx="96" cy="32" r="15" fill="url(#hot)"/>
    <circle cx="91" cy="27" r="7" fill="#fff3a0"/>
    <circle cx="160" cy="32" r="23" fill="url(#hot)"/>
    <path d="M160 6 L167 24 L188 18 L174 34 L193 48 L169 45 L160 61 L151 45 L127 48 L146 34 L132 18 L153 24 Z" fill="#ff8d28" opacity="0.85"/>
    <circle cx="224" cy="32" r="27" fill="url(#hot)"/>
    <path d="M224 4 L232 25 L257 18 L239 36 L259 54 L232 49 L224 62 L216 49 L189 54 L209 36 L191 18 L216 25 Z" fill="#ffd85a" opacity="0.72"/>
    <circle cx="288" cy="32" r="24" fill="#7e2024" opacity="0.55"/>
    <circle cx="279" cy="25" r="15" fill="#ff8d28" opacity="0.65"/>
    <circle cx="299" cy="35" r="12" fill="#ffe56b" opacity="0.5"/>
    <circle cx="352" cy="32" r="28" fill="#4b342f" opacity="0.28"/>
    <circle cx="339" cy="26" r="13" fill="#8c4b32" opacity="0.45"/>
    <circle cx="363" cy="36" r="16" fill="#61202a" opacity="0.36"/>
  </g>
</svg>''',
    },
    {
        "svg": "backgrounds/bg_tile.svg",
        "png": "Content/backgrounds/bg_tile.png",
        "size": (600, 800),
        "art": r'''<svg xmlns="http://www.w3.org/2000/svg" width="600" height="800" viewBox="0 0 600 800">
  <defs>
    <linearGradient id="sea" x1="0" y1="0" x2="600" y2="800" gradientUnits="userSpaceOnUse">
      <stop offset="0" stop-color="#163d5e"/>
      <stop offset="0.5" stop-color="#0f5f73"/>
      <stop offset="1" stop-color="#163d5e"/>
    </linearGradient>
    <pattern id="waves" width="80" height="80" patternUnits="userSpaceOnUse">
      <path d="M0 38 C20 30 40 46 60 38 C68 35 74 34 80 35" fill="none" stroke="#6fb6c5" stroke-width="2" opacity="0.25"/>
      <path d="M8 66 C28 58 48 74 68 66" fill="none" stroke="#a7d8dc" stroke-width="1.3" opacity="0.18"/>
    </pattern>
  </defs>
  <rect width="600" height="800" fill="url(#sea)"/>
  <rect width="600" height="800" fill="url(#waves)"/>
  <path d="M0 250 C60 216 128 226 166 276 C202 323 178 390 107 410 C59 424 26 414 0 390 Z" fill="#47623f" opacity="0.93"/>
  <path d="M600 520 C530 493 461 514 433 568 C405 621 443 682 515 691 C550 696 580 685 600 668 Z" fill="#4b693d" opacity="0.92"/>
  <path d="M98 255 C130 263 151 293 149 330 C147 364 119 389 82 391 C55 393 31 385 0 363 L0 390 C26 414 59 424 107 410 C178 390 202 323 166 276 C148 252 124 241 98 255 Z" fill="#7b7b48" opacity="0.45"/>
  <path d="M524 526 C489 542 465 573 469 615 C473 651 503 681 548 685 C570 687 586 681 600 668 L600 633 C582 656 549 666 519 650 C485 633 479 584 506 555 C515 546 525 539 524 526 Z" fill="#837a49" opacity="0.42"/>
  <path d="M250 458 C290 442 331 450 360 478 C331 503 285 506 244 486 Z" fill="#426339" opacity="0.72"/>
  <path d="M260 469 C292 460 324 463 347 480 C322 492 289 495 259 483 Z" fill="#8e854d" opacity="0.33"/>
  <path d="M0 8 C90 2 166 2 246 8 C336 15 455 15 600 8" fill="none" stroke="#9ad6df" stroke-width="1.3" opacity="0.18"/>
  <path d="M0 792 C90 786 166 786 246 792 C336 799 455 799 600 792" fill="none" stroke="#9ad6df" stroke-width="1.3" opacity="0.18"/>
</svg>''',
    },
]


def write_svgs() -> None:
    for asset in ASSETS:
        path = ROOT / asset["svg"]
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(asset["art"] + "\n", encoding="utf-8", newline="\n")


def export_pngs(inkscape: str) -> None:
    for asset in ASSETS:
        width, height = asset["size"]
        source = ROOT / asset["svg"]
        target = PROJECT_ROOT / asset["png"]
        target.parent.mkdir(parents=True, exist_ok=True)
        subprocess.run(
            [
                inkscape,
                str(source),
                "--export-type=png",
                f"--export-filename={target}",
                f"--export-width={width}",
                f"--export-height={height}",
            ],
            check=True,
        )


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate Strikers 1945 placeholder sprite sources.")
    parser.add_argument("--export", action="store_true", help="Also export PNGs using Inkscape.")
    parser.add_argument("--inkscape", default="inkscape", help="Inkscape executable to use with --export.")
    args = parser.parse_args()

    write_svgs()
    print(f"Sprite SVG sources generated under {ROOT}")

    if args.export:
        export_pngs(args.inkscape)
        print("Sprite PNGs exported under Content/")


if __name__ == "__main__":
    main()

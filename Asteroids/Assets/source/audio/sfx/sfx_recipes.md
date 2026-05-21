# SFX Recipes

These sound effects were generated with ffmpeg lavfi sources. Regenerate from the project root with the commands below.

```powershell
ffmpeg -y -f lavfi -i "sine=frequency=960:duration=0.14,volume=0.75,afade=t=out:st=0.035:d=0.105" -ar 44100 -ac 1 Content\audio\sfx\shoot.wav
ffmpeg -y -f lavfi -i "anoisesrc=color=pink:duration=0.70:amplitude=0.22,lowpass=f=520,highpass=f=90,afade=t=in:st=0:d=0.03,afade=t=out:st=0.65:d=0.05" -ar 44100 -ac 1 Content\audio\sfx\thrust_loop.wav
ffmpeg -y -f lavfi -i "anoisesrc=color=brown:duration=0.22:amplitude=0.45,lowpass=f=900,afade=t=out:st=0.04:d=0.18" -ar 44100 -ac 1 Content\audio\sfx\asteroid_hit.wav
ffmpeg -y -f lavfi -i "anoisesrc=color=brown:duration=0.85:amplitude=0.65,lowpass=f=1300,afade=t=out:st=0.12:d=0.73" -ar 44100 -ac 1 Content\audio\sfx\ship_explosion.wav
ffmpeg -y -f lavfi -i "sine=frequency=520:duration=0.055,volume=0.25,afade=t=out:st=0.025:d=0.03" -ar 44100 -ac 1 Content\audio\sfx\menu_move.wav
ffmpeg -y -f lavfi -i "sine=frequency=740:duration=0.12,volume=0.28,afade=t=out:st=0.05:d=0.07" -ar 44100 -ac 1 Content\audio\sfx\menu_select.wav
ffmpeg -y -f lavfi -i "sine=frequency=180:duration=0.50,vibrato=f=8:d=0.7,volume=0.28,afade=t=in:st=0:d=0.05,afade=t=out:st=0.35:d=0.15" -ar 44100 -ac 1 Content\audio\sfx\ufo_spawn.wav
ffmpeg -y -f lavfi -i "sine=frequency=880:duration=0.13,volume=0.30,afade=t=out:st=0.04:d=0.09" -ar 44100 -ac 1 Content\audio\sfx\ufo_shoot.wav
ffmpeg -y -f lavfi -i "aevalsrc=0.28*sin(2*PI*(220+1200*t)*t):d=0.55,afade=t=out:st=0.35:d=0.20" -ar 44100 -ac 1 Content\audio\sfx\hyperspace.wav
ffmpeg -y -f lavfi -i "sine=frequency=330:duration=0.32,volume=0.22,afade=t=in:st=0:d=0.03,afade=t=out:st=0.18:d=0.14" -ar 44100 -ac 1 Content\audio\sfx\wave_start.wav
```

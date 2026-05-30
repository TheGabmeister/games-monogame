from __future__ import annotations

import argparse
import struct
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parent
PROJECT_ROOT = ROOT.parent
CONTENT = PROJECT_ROOT / "Content"
DEFAULT_SOUNDFONT = Path(r"D:\Tools\GeneralUser-GS\GeneralUser-GS.sf2")
SAMPLE_RATE = 44100
LOOP_SECONDS = 15


SFX = [
    {
        "name": "sfx_player_shot",
        "duration": 0.10,
        "filter": "aevalsrc=0.22*sin(2*PI*(1200+1800*t)*t)*exp(-24*t):s=44100:d=0.10",
    },
    {
        "name": "sfx_enemy_shot",
        "duration": 0.16,
        "filter": "aevalsrc=0.20*sin(2*PI*(620-180*t)*t)*exp(-14*t):s=44100:d=0.16",
    },
    {
        "name": "sfx_enemy_hit",
        "duration": 0.08,
        "filter": "anoisesrc=color=white:amplitude=0.16:duration=0.08,highpass=f=1800,afade=t=out:st=0.01:d=0.07",
    },
    {
        "name": "sfx_enemy_explode",
        "duration": 0.45,
        "filter": "anoisesrc=color=brown:amplitude=0.34:duration=0.45,lowpass=f=1800,afade=t=out:st=0.05:d=0.40",
    },
    {
        "name": "sfx_player_explode",
        "duration": 0.72,
        "filter": "anoisesrc=color=brown:amplitude=0.42:duration=0.72,lowpass=f=1200,afade=t=out:st=0.10:d=0.62",
    },
    {
        "name": "sfx_powerup",
        "duration": 0.34,
        "filter": "aevalsrc=0.12*(sin(2*PI*660*t)+sin(2*PI*990*t)+sin(2*PI*1320*t))*exp(-4*t):s=44100:d=0.34",
    },
    {
        "name": "sfx_powerup_weapon",
        "duration": 0.48,
        "filter": "aevalsrc=0.12*(sin(2*PI*(740+260*t)*t)+sin(2*PI*(1110+390*t)*t))*exp(-3*t):s=44100:d=0.48",
    },
    {
        "name": "sfx_bomb",
        "duration": 1.05,
        "filter": "anoisesrc=color=pink:amplitude=0.46:duration=1.05,lowpass=f=950,afade=t=out:st=0.12:d=0.93",
    },
    {
        "name": "sfx_graze",
        "duration": 0.09,
        "filter": "aevalsrc=0.16*sin(2*PI*2600*t)*exp(-18*t):s=44100:d=0.09",
    },
    {
        "name": "sfx_menu_move",
        "duration": 0.07,
        "filter": "aevalsrc=0.14*sin(2*PI*880*t)*exp(-18*t):s=44100:d=0.07",
    },
    {
        "name": "sfx_menu_select",
        "duration": 0.18,
        "filter": "aevalsrc=0.16*(sin(2*PI*660*t)+sin(2*PI*990*t))*exp(-8*t):s=44100:d=0.18",
    },
    {
        "name": "sfx_stage_clear",
        "duration": 1.40,
        "filter": "aevalsrc=0.11*(sin(2*PI*523.25*t)+sin(2*PI*659.25*t)+sin(2*PI*783.99*t))*exp(-1.6*t):s=44100:d=1.40",
    },
]


TPQ = 480


def run(args: list[str]) -> None:
    print(" ".join(args))
    subprocess.run(args, check=True)


def vlq(value: int) -> bytes:
    data = [value & 0x7F]
    value >>= 7
    while value:
        data.insert(0, (value & 0x7F) | 0x80)
        value >>= 7
    return bytes(data)


def meta_tempo(bpm: int) -> bytes:
    micros = round(60_000_000 / bpm)
    return b"\x00\xff\x51\x03" + micros.to_bytes(3, "big")


def event(delta: int, status: int, *data: int) -> bytes:
    return vlq(delta) + bytes([status, *data])


def write_midi(path: Path, bpm: int, duration_beats: float, scheduled: list[tuple[int, bytes]]) -> None:
    scheduled.sort(key=lambda item: (item[0], item[1][0] & 0xF0))
    track = bytearray(meta_tempo(bpm))
    last_tick = 0
    for tick, payload in scheduled:
        track += vlq(tick - last_tick) + payload
        last_tick = tick

    end_tick = max(last_tick, round(duration_beats * TPQ))
    track += vlq(end_tick - last_tick) + b"\xff\x2f\x00"

    header = b"MThd" + struct.pack(">IHHH", 6, 0, 1, TPQ)
    chunk = b"MTrk" + struct.pack(">I", len(track)) + bytes(track)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(header + chunk)


def note(events: list[tuple[int, bytes]], channel: int, pitch: int, start: float, length: float, velocity: int) -> None:
    start_tick = round(start * TPQ)
    end_tick = round((start + length) * TPQ)
    events.append((start_tick, bytes([0x90 | channel, pitch, velocity])))
    events.append((end_tick, bytes([0x80 | channel, pitch, 0])))


def program(events: list[tuple[int, bytes]], channel: int, patch: int) -> None:
    events.append((0, bytes([0xC0 | channel, patch])))


def stage_events() -> list[tuple[int, bytes]]:
    events: list[tuple[int, bytes]] = []
    program(events, 0, 81)
    program(events, 1, 38)
    program(events, 2, 88)

    chords = [(57, 60, 64), (53, 57, 60), (55, 59, 62), (52, 56, 59)] * 2
    melody = [69, 72, 76, 72, 67, 71, 74, 71, 67, 69, 74, 69, 68, 71, 76, 71]
    for bar, chord in enumerate(chords):
        beat = bar * 4
        for pitch in chord:
            note(events, 2, pitch + 12, beat, 3.85, 54)
        for step in range(4):
            note(events, 1, chord[0] - 12, beat + step, 0.42, 82)
        for step in range(8):
            note(events, 0, melody[(bar * 8 + step) % len(melody)], beat + step * 0.5, 0.34, 92)
        for step in range(8):
            note(events, 9, 42, beat + step * 0.5, 0.08, 58)
        note(events, 9, 36, beat, 0.08, 88)
        note(events, 9, 38, beat + 2, 0.08, 78)
    return events


def title_events() -> list[tuple[int, bytes]]:
    events: list[tuple[int, bytes]] = []
    program(events, 0, 4)
    program(events, 1, 33)
    program(events, 2, 95)

    chords = [(60, 64, 67), (55, 59, 62), (57, 60, 64), (53, 57, 60)] * 2
    melody = [72, 76, 79, 76, 74, 77, 81, 77, 72, 74, 79, 74, 71, 76, 79, 76]
    for bar, chord in enumerate(chords):
        beat = bar * 4
        for pitch in chord:
            note(events, 2, pitch, beat, 3.9, 48)
        note(events, 1, chord[0] - 24, beat, 1.7, 72)
        note(events, 1, chord[0] - 12, beat + 2, 1.7, 68)
        for step in range(4):
            note(events, 0, melody[(bar * 4 + step) % len(melody)], beat + step, 0.72, 78)
        note(events, 9, 42, beat, 0.08, 34)
        note(events, 9, 42, beat + 2, 0.08, 30)
    return events


def gameover_events() -> list[tuple[int, bytes]]:
    events: list[tuple[int, bytes]] = []
    program(events, 0, 48)
    program(events, 1, 89)
    for pitch in (64, 60, 57):
        note(events, 0, pitch, 0, 1.5, 78)
    for pitch in (62, 59, 55):
        note(events, 0, pitch, 1.7, 1.5, 70)
    for pitch in (57, 53, 50):
        note(events, 1, pitch, 3.2, 1.6, 64)
    note(events, 9, 49, 0, 0.1, 70)
    return events


MUSIC = [
    {"name": "music_stage", "bpm": 128, "beats": 32, "seconds": LOOP_SECONDS, "events": stage_events},
    {"name": "music_title", "bpm": 128, "beats": 32, "seconds": LOOP_SECONDS, "events": title_events},
    {"name": "music_gameover", "bpm": 120, "beats": 10, "seconds": 5, "events": gameover_events},
]


def generate_sfx(ffmpeg: str) -> None:
    target_dir = CONTENT / "audio" / "sfx"
    target_dir.mkdir(parents=True, exist_ok=True)
    for spec in SFX:
        target = target_dir / f'{spec["name"]}.wav'
        run([
            ffmpeg,
            "-y",
            "-f",
            "lavfi",
            "-i",
            spec["filter"],
            "-t",
            str(spec["duration"]),
            "-ac",
            "1",
            "-ar",
            str(SAMPLE_RATE),
            "-c:a",
            "pcm_s16le",
            str(target),
        ])


def generate_music(ffmpeg: str, fluidsynth: str, soundfont: Path) -> None:
    midi_dir = ROOT / "music"
    wav_dir = ROOT / "rendered"
    target_dir = CONTENT / "audio" / "music"
    midi_dir.mkdir(parents=True, exist_ok=True)
    wav_dir.mkdir(parents=True, exist_ok=True)
    target_dir.mkdir(parents=True, exist_ok=True)

    for spec in MUSIC:
        midi = midi_dir / f'{spec["name"]}.mid'
        wav = wav_dir / f'{spec["name"]}.wav'
        ogg = target_dir / f'{spec["name"]}.ogg'

        write_midi(midi, spec["bpm"], spec["beats"], spec["events"]())
        run([fluidsynth, "-ni", "-F", str(wav), "-r", str(SAMPLE_RATE), str(soundfont), str(midi)])
        run([
            ffmpeg,
            "-y",
            "-i",
            str(wav),
            "-af",
            f"atrim=0:{spec['seconds']},asetpts=N/SR/TB,loudnorm=I=-18:TP=-1.5:LRA=11,aresample={SAMPLE_RATE}",
            "-c:a",
            "libvorbis",
            "-q:a",
            "4",
            str(ogg),
        ])


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate Strikers 1945 placeholder audio.")
    parser.add_argument("--ffmpeg", default="ffmpeg")
    parser.add_argument("--fluidsynth", default="fluidsynth")
    parser.add_argument("--soundfont", type=Path, default=DEFAULT_SOUNDFONT)
    parser.add_argument("--sfx-only", action="store_true")
    parser.add_argument("--music-only", action="store_true")
    args = parser.parse_args()

    if not args.sfx_only and not args.soundfont.exists():
        raise FileNotFoundError(f"Soundfont not found: {args.soundfont}")

    if not args.music_only:
        generate_sfx(args.ffmpeg)
    if not args.sfx_only:
        generate_music(args.ffmpeg, args.fluidsynth, args.soundfont)


if __name__ == "__main__":
    main()

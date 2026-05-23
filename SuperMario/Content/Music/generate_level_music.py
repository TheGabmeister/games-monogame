from __future__ import annotations

from pathlib import Path

from midiutil import MIDIFile


PPQ = 480
OUT_DIR = Path(__file__).resolve().parent


def new_song(
    tempo_bpm: int, numerator: int, denominator: int, total_beats: float
) -> MIDIFile:
    midi = MIDIFile(numTracks=3, ticks_per_quarternote=PPQ)
    midi.addTempo(0, 0, tempo_bpm)
    midi.addTimeSignature(0, 0, numerator, denominator.bit_length() - 1, 0x18)

    for track in range(3):
        midi.addText(track, total_beats, "loop-end")

    return midi


def write_midi_file(path: Path, midi: MIDIFile) -> None:
    with path.open("wb") as file:
        midi.writeFile(file)


def add_program(midi: MIDIFile, track: int, channel: int, program: int) -> None:
    midi.addProgramChange(track, channel, 0, program)


def add_control(
    midi: MIDIFile, track: int, channel: int, controller: int, value: int
) -> None:
    midi.addControllerEvent(track, channel, 0, controller, value)


def add_note(
    midi: MIDIFile,
    track: int,
    channel: int,
    start_beat: float,
    duration_beat: float,
    note: int,
    velocity: int,
) -> None:
    midi.addNote(track, channel, note, start_beat, duration_beat, velocity)


def add_chord(
    midi: MIDIFile,
    track: int,
    channel: int,
    start_beat: float,
    duration_beat: float,
    notes: tuple[int, ...],
    velocity: int,
) -> None:
    for note in notes:
        add_note(midi, track, channel, start_beat, duration_beat, note, velocity)


def write_bounce() -> None:
    midi = new_song(120, 4, 4, 20)

    add_program(midi, 0, 0, 80)
    add_control(midi, 0, 0, 7, 96)
    add_control(midi, 0, 0, 10, 72)
    melody_notes = (
        (0, 0.5, 76),
        (0.5, 0.5, 79),
        (1, 0.5, 84),
        (1.5, 0.5, 79),
        (2, 0.5, 81),
        (2.5, 0.5, 79),
        (3, 1, 76),
        (4, 0.5, 74),
        (4.5, 0.5, 76),
        (5, 0.5, 79),
        (5.5, 0.5, 83),
        (6, 0.5, 81),
        (6.5, 0.5, 79),
        (7, 1, 72),
        (8, 0.5, 76),
        (8.5, 0.5, 79),
        (9, 0.5, 84),
        (9.5, 0.5, 88),
        (10, 0.5, 86),
        (10.5, 0.5, 84),
        (11, 1, 79),
        (12, 0.5, 77),
        (12.5, 0.5, 81),
        (13, 0.5, 84),
        (13.5, 0.5, 89),
        (14, 0.5, 88),
        (14.5, 0.5, 84),
        (15, 1, 81),
        (16, 0.5, 84),
        (16.5, 0.5, 79),
        (17, 0.5, 76),
        (17.5, 0.5, 72),
        (18, 0.5, 74),
        (18.5, 0.5, 76),
        (19, 1, 72),
    )
    for start, duration, note in melody_notes:
        add_note(midi, 0, 0, start, duration, note, 96)

    add_program(midi, 1, 1, 33)
    add_control(midi, 1, 1, 7, 92)
    add_control(midi, 1, 1, 10, 44)
    roots = (36, 43, 45, 41, 36)
    for bar, root in enumerate(roots):
        beat = bar * 4
        add_note(midi, 1, 1, beat, 0.75, root, 88)
        add_note(midi, 1, 1, beat + 1, 0.5, root + 12, 70)
        add_note(midi, 1, 1, beat + 2, 0.75, root + 7, 82)
        add_note(midi, 1, 1, beat + 3, 0.5, root + 12, 70)

    beat = 0.0
    while beat < 20:
        add_note(midi, 2, 9, beat, 0.1, 42, 48)
        beat += 0.5
    beat = 0.0
    while beat < 20:
        add_note(midi, 2, 9, beat, 0.1, 36, 86)
        beat += 2
    beat = 1.0
    while beat < 20:
        add_note(midi, 2, 9, beat, 0.1, 38, 78)
        beat += 2

    write_midi_file(OUT_DIR / "level_bounce.mid", midi)


def write_cavern() -> None:
    midi = new_song(96, 4, 4, 16)

    add_program(midi, 0, 0, 18)
    add_control(midi, 0, 0, 7, 84)
    add_control(midi, 0, 0, 10, 52)
    arp = (57, 60, 64, 67, 59, 62, 65, 69, 55, 59, 62, 67, 52, 56, 59, 64)
    for index in range(32):
        add_note(midi, 0, 0, index * 0.5, 0.42, arp[index % len(arp)], 78)

    add_program(midi, 1, 1, 38)
    add_control(midi, 1, 1, 7, 108)
    add_control(midi, 1, 1, 10, 40)
    roots = (33, 31, 29, 28)
    for bar, root in enumerate(roots):
        add_note(midi, 1, 1, bar * 4, 3.5, root, 94)
        add_note(midi, 1, 1, bar * 4 + 3.5, 0.35, root + 12, 72)

    beat = 0.0
    while beat < 16:
        add_note(midi, 2, 9, beat, 0.15, 45, 82)
        beat += 2
    beat = 1.5
    while beat < 16:
        add_note(midi, 2, 9, beat, 0.15, 41, 74)
        beat += 4
    beat = 0.5
    while beat < 16:
        add_note(midi, 2, 9, beat, 0.08, 44, 38)
        beat += 0.5

    write_midi_file(OUT_DIR / "level_cavern.mid", midi)


def write_sky() -> None:
    midi = new_song(144, 3, 4, 24)

    add_program(midi, 0, 0, 46)
    add_control(midi, 0, 0, 7, 88)
    add_control(midi, 0, 0, 10, 66)
    chords = (
        (67, 71, 74),
        (69, 72, 76),
        (62, 66, 69),
        (64, 67, 71),
        (60, 64, 67),
        (62, 66, 69),
        (67, 71, 74),
        (66, 69, 74),
    )
    for bar, chord in enumerate(chords):
        base = bar * 3
        for offset in (0, 0.5, 1, 1.5, 2, 2.5):
            note = chord[int((offset * 2) % 3)]
            add_note(midi, 0, 0, base + offset, 0.35, note, 72)

    add_program(midi, 1, 1, 9)
    add_control(midi, 1, 1, 7, 72)
    add_control(midi, 1, 1, 10, 80)
    bell_notes = (
        (0, 1, 86),
        (3, 1, 88),
        (6, 1, 81),
        (9, 1, 83),
        (12, 1, 79),
        (15, 1, 81),
        (18, 1, 86),
        (21, 1.5, 85),
    )
    for start, duration, note in bell_notes:
        add_note(midi, 1, 1, start, duration, note, 76)

    add_program(midi, 2, 2, 48)
    add_control(midi, 2, 2, 7, 62)
    add_control(midi, 2, 2, 10, 48)
    pad_roots = (55, 57, 50, 52, 48, 50, 55, 54)
    for bar, root in enumerate(pad_roots):
        add_chord(midi, 2, 2, bar * 3, 2.75, (root, root + 7, root + 12), 54)

    write_midi_file(OUT_DIR / "level_sky.mid", midi)


def main() -> None:
    write_bounce()
    write_cavern()
    write_sky()


if __name__ == "__main__":
    main()

$ErrorActionPreference = "Stop"

$OutDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Ppq = 480

function Get-Bytes16([int]$value) {
    return [byte[]](
        (($value -shr 8) -band 0xff),
        ($value -band 0xff)
    )
}

function Get-Bytes32([int]$value) {
    return [byte[]](
        (($value -shr 24) -band 0xff),
        (($value -shr 16) -band 0xff),
        (($value -shr 8) -band 0xff),
        ($value -band 0xff)
    )
}

function Get-Vlq([int]$value) {
    $bytes = @([byte]($value -band 0x7f))
    $value = $value -shr 7

    while ($value -gt 0) {
        $bytes = ,([byte](($value -band 0x7f) -bor 0x80)) + $bytes
        $value = $value -shr 7
    }

    return [byte[]]$bytes
}

function New-Event([int]$time, [int]$order, [byte[]]$data) {
    return [pscustomobject]@{
        Time = $time
        Order = $order
        Data = $data
    }
}

function Add-MidiEvent($events, [int]$time, [int]$order, [byte[]]$data) {
    [void]$events.Add((New-Event $time $order $data))
}

function Add-Program($events, [int]$channel, [int]$program) {
    Add-MidiEvent $events 0 0 ([byte[]]((0xc0 + $channel), $program))
}

function Add-Control($events, [int]$channel, [int]$controller, [int]$value) {
    Add-MidiEvent $events 0 0 ([byte[]]((0xb0 + $channel), $controller, $value))
}

function Add-Note($events, [int]$channel, [double]$startBeat, [double]$durationBeat, [int]$note, [int]$velocity) {
    $start = [int][Math]::Round($startBeat * $Ppq)
    $end = [int][Math]::Round(($startBeat + $durationBeat) * $Ppq)
    Add-MidiEvent $events $start 2 ([byte[]]((0x90 + $channel), $note, $velocity))
    Add-MidiEvent $events $end 1 ([byte[]]((0x80 + $channel), $note, 0))
}

function Add-Chord($events, [int]$channel, [double]$startBeat, [double]$durationBeat, [int[]]$notes, [int]$velocity) {
    foreach ($note in $notes) {
        Add-Note $events $channel $startBeat $durationBeat $note $velocity
    }
}

function New-TrackBytes($events, [int]$totalTicks) {
    $track = [System.Collections.Generic.List[byte]]::new()
    $lastTime = 0
    $sorted = $events | Sort-Object Time, Order

    foreach ($event in $sorted) {
        foreach ($b in (Get-Vlq ($event.Time - $lastTime))) {
            [void]$track.Add($b)
        }
        foreach ($b in $event.Data) {
            [void]$track.Add($b)
        }
        $lastTime = $event.Time
    }

    foreach ($b in (Get-Vlq ($totalTicks - $lastTime))) {
        [void]$track.Add($b)
    }
    foreach ($b in ([byte[]](0xff, 0x2f, 0x00))) {
        [void]$track.Add($b)
    }

    return [byte[]]$track.ToArray()
}

function New-TempoTrack([int]$tempoBpm, [int]$numerator, [int]$denominator, [int]$totalTicks) {
    $events = [System.Collections.Generic.List[object]]::new()
    $mpqn = [int][Math]::Round(60000000 / $tempoBpm)
    $denominatorPower = [int][Math]::Round([Math]::Log($denominator, 2))

    Add-MidiEvent $events 0 0 ([byte[]](0xff, 0x51, 0x03, (($mpqn -shr 16) -band 0xff), (($mpqn -shr 8) -band 0xff), ($mpqn -band 0xff)))
    Add-MidiEvent $events 0 0 ([byte[]](0xff, 0x58, 0x04, $numerator, $denominatorPower, 0x18, 0x08))

    return New-TrackBytes $events $totalTicks
}

function Write-MidiFile([string]$path, [int]$tempoBpm, [int]$numerator, [int]$denominator, [double]$totalBeats, [object[]]$tracks) {
    $totalTicks = [int][Math]::Round($totalBeats * $Ppq)
    $chunks = [System.Collections.Generic.List[byte[]]]::new()
    [void]$chunks.Add((New-TempoTrack $tempoBpm $numerator $denominator $totalTicks))

    foreach ($trackEvents in $tracks) {
        [void]$chunks.Add((New-TrackBytes $trackEvents $totalTicks))
    }

    $bytes = [System.Collections.Generic.List[byte]]::new()
    foreach ($b in ([System.Text.Encoding]::ASCII.GetBytes("MThd"))) { [void]$bytes.Add($b) }
    foreach ($b in (Get-Bytes32 6)) { [void]$bytes.Add($b) }
    foreach ($b in (Get-Bytes16 1)) { [void]$bytes.Add($b) }
    foreach ($b in (Get-Bytes16 $chunks.Count)) { [void]$bytes.Add($b) }
    foreach ($b in (Get-Bytes16 $Ppq)) { [void]$bytes.Add($b) }

    foreach ($chunk in $chunks) {
        foreach ($b in ([System.Text.Encoding]::ASCII.GetBytes("MTrk"))) { [void]$bytes.Add($b) }
        foreach ($b in (Get-Bytes32 $chunk.Length)) { [void]$bytes.Add($b) }
        foreach ($b in $chunk) { [void]$bytes.Add($b) }
    }

    [System.IO.File]::WriteAllBytes($path, [byte[]]$bytes.ToArray())
}

function New-BounceTrack {
    $melody = [System.Collections.Generic.List[object]]::new()
    Add-Program $melody 0 80
    Add-Control $melody 0 7 96
    Add-Control $melody 0 10 72
    $melodyNotes = @(
        @(0, .5, 76), @(.5, .5, 79), @(1, .5, 84), @(1.5, .5, 79),
        @(2, .5, 81), @(2.5, .5, 79), @(3, 1, 76),
        @(4, .5, 74), @(4.5, .5, 76), @(5, .5, 79), @(5.5, .5, 83),
        @(6, .5, 81), @(6.5, .5, 79), @(7, 1, 72),
        @(8, .5, 76), @(8.5, .5, 79), @(9, .5, 84), @(9.5, .5, 88),
        @(10, .5, 86), @(10.5, .5, 84), @(11, 1, 79),
        @(12, .5, 77), @(12.5, .5, 81), @(13, .5, 84), @(13.5, .5, 89),
        @(14, .5, 88), @(14.5, .5, 84), @(15, 1, 81),
        @(16, .5, 84), @(16.5, .5, 79), @(17, .5, 76), @(17.5, .5, 72),
        @(18, .5, 74), @(18.5, .5, 76), @(19, 1, 72)
    )
    foreach ($n in $melodyNotes) { Add-Note $melody 0 $n[0] $n[1] $n[2] 96 }

    $bass = [System.Collections.Generic.List[object]]::new()
    Add-Program $bass 1 33
    Add-Control $bass 1 7 92
    Add-Control $bass 1 10 44
    $roots = @(36, 43, 45, 41, 36)
    for ($bar = 0; $bar -lt 5; $bar++) {
        $root = $roots[$bar]
        $beat = $bar * 4
        Add-Note $bass 1 $beat .75 $root 88
        Add-Note $bass 1 ($beat + 1) .5 ($root + 12) 70
        Add-Note $bass 1 ($beat + 2) .75 ($root + 7) 82
        Add-Note $bass 1 ($beat + 3) .5 ($root + 12) 70
    }

    $drums = [System.Collections.Generic.List[object]]::new()
    for ($beat = 0; $beat -lt 20; $beat += .5) { Add-Note $drums 9 $beat .1 42 48 }
    for ($beat = 0; $beat -lt 20; $beat += 2) { Add-Note $drums 9 $beat .1 36 86 }
    for ($beat = 1; $beat -lt 20; $beat += 2) { Add-Note $drums 9 $beat .1 38 78 }

    return @($melody, $bass, $drums)
}

function New-CavernTrack {
    $lead = [System.Collections.Generic.List[object]]::new()
    Add-Program $lead 0 18
    Add-Control $lead 0 7 84
    Add-Control $lead 0 10 52
    $arp = @(57, 60, 64, 67, 59, 62, 65, 69, 55, 59, 62, 67, 52, 56, 59, 64)
    for ($i = 0; $i -lt 32; $i++) {
        Add-Note $lead 0 ($i * .5) .42 $arp[$i % $arp.Length] 78
    }

    $bass = [System.Collections.Generic.List[object]]::new()
    Add-Program $bass 1 38
    Add-Control $bass 1 7 108
    Add-Control $bass 1 10 40
    $roots = @(33, 31, 29, 28)
    for ($bar = 0; $bar -lt 4; $bar++) {
        Add-Note $bass 1 ($bar * 4) 3.5 $roots[$bar] 94
        Add-Note $bass 1 (($bar * 4) + 3.5) .35 ($roots[$bar] + 12) 72
    }

    $drums = [System.Collections.Generic.List[object]]::new()
    for ($beat = 0; $beat -lt 16; $beat += 2) { Add-Note $drums 9 $beat .15 45 82 }
    for ($beat = 1.5; $beat -lt 16; $beat += 4) { Add-Note $drums 9 $beat .15 41 74 }
    for ($beat = .5; $beat -lt 16; $beat += .5) { Add-Note $drums 9 $beat .08 44 38 }

    return @($lead, $bass, $drums)
}

function New-SkyTrack {
    $harp = [System.Collections.Generic.List[object]]::new()
    Add-Program $harp 0 46
    Add-Control $harp 0 7 88
    Add-Control $harp 0 10 66
    $chords = @(
        @(67, 71, 74), @(69, 72, 76), @(62, 66, 69), @(64, 67, 71),
        @(60, 64, 67), @(62, 66, 69), @(67, 71, 74), @(66, 69, 74)
    )
    for ($bar = 0; $bar -lt 8; $bar++) {
        $base = $bar * 3
        foreach ($offset in @(0, .5, 1, 1.5, 2, 2.5)) {
            $note = $chords[$bar][$([int](($offset * 2) % 3))]
            Add-Note $harp 0 ($base + $offset) .35 $note 72
        }
    }

    $bells = [System.Collections.Generic.List[object]]::new()
    Add-Program $bells 1 9
    Add-Control $bells 1 7 72
    Add-Control $bells 1 10 80
    $bellNotes = @(
        @(0, 1, 86), @(3, 1, 88), @(6, 1, 81), @(9, 1, 83),
        @(12, 1, 79), @(15, 1, 81), @(18, 1, 86), @(21, 1.5, 85)
    )
    foreach ($n in $bellNotes) { Add-Note $bells 1 $n[0] $n[1] $n[2] 76 }

    $strings = [System.Collections.Generic.List[object]]::new()
    Add-Program $strings 2 48
    Add-Control $strings 2 7 62
    Add-Control $strings 2 10 48
    $padRoots = @(55, 57, 50, 52, 48, 50, 55, 54)
    for ($bar = 0; $bar -lt 8; $bar++) {
        Add-Chord $strings 2 ($bar * 3) 2.75 ([int[]]($padRoots[$bar], $padRoots[$bar] + 7, $padRoots[$bar] + 12)) 54
    }

    return @($harp, $bells, $strings)
}

Write-MidiFile (Join-Path $OutDir "level_bounce.mid") 120 4 4 20 (New-BounceTrack)
Write-MidiFile (Join-Path $OutDir "level_cavern.mid") 96 4 4 16 (New-CavernTrack)
Write-MidiFile (Join-Path $OutDir "level_sky.mid") 144 3 4 24 (New-SkyTrack)

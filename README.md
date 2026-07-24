# Shred to Zero

A top-down rhythm-combat game where you play a guitarist fighting through a building of goons to reach and disarm a bomb before it hits zero. Fire notes on the beat to hit harder — shred the countdown to zero.

Built in **Unity 6.4** (`6000.4.0f1`) with the **Universal Render Pipeline (2D)** and the new **Input System**.

## The loop

- **Move** Diablo-style: hold the **left mouse button** to walk toward the cursor, release to stop.
- **Target**: whichever enemy is under (or near) the cursor becomes your current target; shots home toward it. With no enemy under the cursor, you fire at the bare cursor point.
- **Shred**: keys **1 / 2 / 3** fire the three note types. Fire *on the beat* and the note is **empowered** — bonus damage, faster, bigger, brighter.
- **Beat the clock**: a bomb counts down at the top of the screen. Reach zero and it detonates (lose); reach the bomb and disarm it (win).

## Notes & weaknesses

Each note is a "colour", and every enemy is weak to one type and resists another — so you read each goon and pick the right note instead of mashing one key.

| Note | Flavor | Colour |
|------|--------|--------|
| **Power** | heavy crunchy chord | red-orange |
| **Bass**  | low thumping groove  | blue |
| **Lead**  | screaming high solo  | yellow |

Weakness triangle: **Power → Bass → Lead → Power** (each beats the next). Hitting an enemy's weakness deals bonus damage; hitting what it resists is reduced.

## Rhythm system

Timing syncs to samples, not frames. The `Conductor` is the single source of musical time — it reads `AudioSettings.dspTime` (the audio hardware sample clock) and starts the song with `AudioSource.PlayScheduled` for a sample-accurate beat 0. Everything that needs to know "where are we in the song?" asks the Conductor rather than reading `Time.deltaTime`.

- `bpm`, `firstBeatOffset`, and `inputLatencySeconds` are exposed for per-song and per-machine calibration.
- A built-in **metronome** click helps you feel timing before a real track is in place.
- The on-beat window is generous (~110 ms by default) and tunable on the guitar.

## Project layout

```
Assets/
  Scripts/
    Rhythm/   Conductor.cs        — musical clock (dspTime-based), on-beat check
    Combat/   NoteType.cs         — the three note types + colours
              NoteProjectile.cs   — a fired note in flight
              Enemy.cs            — health + weakness/resistance affinity
              EnemyProjectile.cs  — a goon's shot, hurts the player
              EnemyAttack.cs      — fires at the player on the beat
    Player/   PlayerController.cs — top-down move, cursor targeting, aim
              GuitarWeapon.cs     — key → note type, on-beat empower
              PlayerHealth.cs     — HP, i-frames, death
    Game/     BombTimer.cs        — the countdown (tick/boom, win/lose)
              RunManager.cs       — run state, enemy count, win/lose screen
              DisarmZone.cs       — stand-on-the-bomb defuse objective
    Audio/    AudioManager.cs     — pooled one-shot SFX player
  Audio/      Impact/ Digital/ Interface/  — Kenney CC0 SFX (see Credits)
  Prefabs/    note & projectile prefabs
  Scenes/     SampleScene.unity
  Settings/   URP 2D renderer & pipeline assets
```

## Getting started

1. Open the project in **Unity 6.4 (`6000.4.0f1`)** or newer via Unity Hub.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press **Play**.

Controls:

- **Left mouse (hold)** — walk toward the cursor
- **Move cursor** — aim / pick a target
- **1 / 2 / 3** — fire Power / Bass / Lead notes (on the beat for bonus damage)

> Tip: many components ship with `verboseLogs` on so you can watch damage, on-beat calls, and firing in the Console. Turn those off once combat feels right.

## Credits & third-party assets

### Audio — sound effects
Sound effects by **[Kenney](https://kenney.nl)**, licensed **[CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/)** (public domain — no attribution required, credited here in thanks):

| Pack | Used for | Link |
|------|----------|------|
| **Impact Sounds** | note hits, enemy damage/death, player hurt, explosion | https://kenney.nl/assets/impact-sounds |
| **Digital Audio** | enemy laser/zap shots, power-ups | https://kenney.nl/assets/digital-audio |
| **Interface Sounds** | bomb ticks, UI, win/lose stings | https://kenney.nl/assets/interface-sounds |

Other sound effects (credit the artist(s) once used — verify each track's exact license first):

| Source | Used for | Link |
|--------|----------|------|
| **Metronome — Pixabay** | metronome click | https://pixabay.com/sound-effects/film-special-effects-metronome-85688/ |

### Audio — music
_Main track: TBD — royalty-free synthwave. Add the track name, artist, source URL, and license here once chosen (a CC-BY track **must** be credited)._

**Candidate tracks to credit once used:**

| Source | Notes | Link |
|--------|-------|------|
| **Arclight — Epic Orchestral/Rock Soundtrack** | check per-track license & credit the artist(s) before shipping | https://opengameart.org/content/arclight-epic-orchestralrock-soundtrack |

> CC0 assets are free to use with no attribution required. CC-BY assets are free but **must** be credited — list them above with a link before shipping.

## Status

Early combat slice — player, guitar, typed notes, chasing enemies, and a real-time bomb timer are in. Planned hooks already stubbed in the code: riff drops on enemy death, combo/score juice on `GuitarWeapon.OnFired`, and time penalties/bonuses via `BombTimer.AddTime`.

# Linksaur — Game Design Document

## Overview

**Linksaur** is a 2D endless-runner mobile game built in Unity 6 (URP). The theme is LinkedIn and professional networking: you play as a character running through the corporate social media world, collecting professional Connections while dodging the distractions of rival platforms.

**Engine:** Unity 6000.3.7f1 (Universal Render Pipeline)  
**Target platform:** Mobile (iOS / Android)

---

## Core Gameplay Loop

1. Tap to start from the Main Menu.
2. The player character runs automatically from left to right across an infinitely scrolling world.
3. **Tap / touch** to jump; tap again while airborne for a **double jump**.
4. Collect **Connections** to increase your score.
5. Dodge or survive **Obstacles** (social media distractors).
6. Pick up **Power-ups** that appear periodically for temporary advantages.
7. The world scrolls faster over time — survive as long as possible.
8. When you hit a fatal obstacle, a Game Over screen appears. You can **Revive** once per run (via a rewarded ad) or **Play Again**.

---

## Controls

| Input | Action |
|---|---|
| Tap / Click (grounded) | Jump |
| Tap / Click (airborne) | Double Jump |
| Pause button (HUD) | Pause / Unpause |
| Android Back / Escape key | Pause / Unpause |

---

## Scoring

- **Currency unit:** Connections
- Each **Connection pickup** awards its `ConnectionValue` (typically 1–3 depending on the prefab variant).
- **Combo bonus:** Collecting 5 Connections in a row without taking a hit awards a bonus **+5 Connections** and resets the combo counter.
- Any negative score event (hitting Instagram or Snapchat) resets the combo counter.
- Score is displayed live on the HUD as `Connections: X`.
- **High score** is saved locally and shown on the Main Menu and Game Over screen.

---

## Obstacles

Obstacles spawn from the right side of the screen and scroll left. They appear every **1.5–3 seconds** at random heights (ground level or mid-air).

| Obstacle | Tag | Effect | Notes |
|---|---|---|---|
| **TikTok** | `TikTok` | Instant **Game Over** | 30% spawn chance; has a 1.5 s cooldown between spawns to avoid back-to-back instant deaths |
| **Instagram** | `Instagram` | **−3 Connections** | 25% spawn chance; disappears on contact |
| **Snapchat** | `Snapchat` | **−5 Connections** | 20% spawn chance; disappears on contact |

> A 25% chance exists each obstacle interval that nothing spawns (roll > 0.75).

---

## Power-ups

Power-ups spawn every **15–25 seconds** at random mid-air or ground-level positions. Only one power-up can be active at a time — collecting a new one immediately cancels the current one.

The active power-up is shown on the HUD with a name label and a draining progress bar.

### Rocket
- **Duration:** 5 seconds
- **Effect:** Doubles the world scroll speed (`ScrollSpeed × 2`).
- **Visual:** Rocket particle effect plays on the player.
- **Strategy:** High risk / high reward — obstacles come at you twice as fast.

### Coffee Break
- **Duration:** 8 seconds
- **Effect:** Halves the world scroll speed (`ScrollSpeed × 0.5`), giving more reaction time.
- **Visual:** Player sprite turns brown (`#8B4513`).
- **Strategy:** Great for catching your breath when the speed ramps up.

### Shield
- **Duration:** Until hit (infinite, no timer bar)
- **Effect:** Blocks the next **TikTok** collision that would otherwise end the run. The shield absorbs the hit and deactivates.
- **Visual:** A shield circle graphic appears around the player.
- **Note:** On Revive, the player is granted a **3-second temporary shield** automatically.

### Recruiter Mode
- **Duration:** 6 seconds
- **Effect:** Nearby Connections (within a radius of 5 units) are magnetically attracted toward the player at 8 units/second, auto-collecting them.
- **Visual:** A glow effect appears around the player.
- **Strategy:** Pairs well with dense Connection clusters.

---

## Difficulty Progression

The world scroll speed increases continuously during a run:

| Parameter | Value |
|---|---|
| Starting speed | 5 units/second |
| Ramp rate | +0.05 units/second per second |
| Maximum speed | 12 units/second |

At max speed, the game is roughly 2.4× harder than the start. Power-ups (Rocket / Coffee Break) temporarily shift speed up or down from the current ramped value.

---

## Connection Pickups

Connections appear every **1.5–3.5 seconds** at three possible heights: ground (−3), mid (−1), or high (+1). There are multiple prefab variants with different `ConnectionValue` amounts (1, 2, or 3).

---

## Daily Challenges

A daily challenge rotates automatically based on the day of the year. Challenge types:

| Type | Description |
|---|---|
| **Collect** | Reach a target Connection score in a single run |
| **Survive** | Survive for a set duration (framework in place, timer not yet wired) |
| **NoPowerup** | Complete a run without using any power-up (tracker not yet wired) |

**Reward:** Completing a daily challenge grants **+50 Connections** added to your score.

---

## Skins / Cosmetics

The **Skin System** allows the player character's appearance to be customized:

- Skins are defined as `SkinData` ScriptableObjects with:
  - **Skin name**
  - **Sprite override**
  - **Tint color**
  - **Unlock level** (minimum level required)
  - **Premium flag** (paid/unlockable)
- The selected skin is applied automatically at the start of each run.
- Skin selection is saved via `PlayerPrefs`.

---

## Revive System

On the Game Over screen, a **Revive** button shows a rewarded ad. If the user watches the ad:

- The game resumes from where it ended.
- The player is granted a **3-second invulnerability shield**.
- The Revive button becomes disabled (one revive per run).

---

## Ads

| Ad Type | Trigger |
|---|---|
| **Interstitial** | Shown automatically every 3rd game over |
| **Rewarded** | Shown when player taps Revive on Game Over screen |

---

## Leaderboard

`LeaderboardManager` records scores on game over. The framework is in place for displaying a leaderboard; the Main Menu has a leaderboard button wired up.

---

## UI Screens

| Screen | Description |
|---|---|
| **Main Menu** | Shows best score; Play and Leaderboard buttons |
| **HUD** | Live score counter, pause button, active power-up label + timer bar |
| **Game Over** | Final score, best score, Play Again / Revive / Share buttons; panel animates in with a scale-up tween |

---

## Audio

| Sound | Trigger |
|---|---|
| Jump sound | Every jump (including double jump) |
| Collect sound | Collecting a Connection (positive score) |
| Hit sound | Game Over |

---

## Technical Architecture

### Singletons (DontDestroyOnLoad)
- `GameManager` — state machine, score, speed, events
- `PowerUpManager` — active power-up state and routines
- `SkinManager` — cosmetic skin selection
- `ChallengeManager` — daily challenge rotation
- `AdManager` — interstitial and rewarded ad wrappers
- `LeaderboardManager` — score tracking

### Key Systems
- **Object Pool** — all obstacles, connections, and power-ups are pooled (pre-warmed at start) to avoid GC spikes.
- **ScrollingObject** — moves all world objects left at `GameManager.ScrollSpeed`.
- **SpawnManager** — three parallel coroutine loops: obstacles, connections, power-ups.
- **SaveManager** — wraps `PlayerPrefs` for high score and games-played persistence.

### Events (static C# events)
`OnGameStart`, `OnGameOver`, `OnScoreChanged`, `OnGamePaused`, `OnGameUnpaused`, `OnPowerUpChanged`

### Input
Unity Input System (`Pointer.current`) for cross-platform tap/click. UI pointer detection prevents jumps from triggering when tapping HUD buttons.

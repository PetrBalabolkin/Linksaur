# Linksaur Scene Structure - Quick Reference

## Two Game Scenes

### 1. **SampleScene.unity** (UI Development/Preview)
- Minimal game objects
- Focus on UI testing
- Contains full Canvas with all menu panels
- Used for menu flow testing

### 2. **GameScene.unity** (Main Game)
- Complete game setup
- Includes Player, Ground, Managers, Background, EventSystem
- Contains full Canvas with all menu panels
- All managers bundled in "Managers" GameObject

---

## Canvas Structure (Both Scenes Identical)

```
Canvas (1920x1080, UI Layer)
├── HudPanel [INACTIVE]
│   ├── ScoreText (TextMeshProUGUI)
│   ├── PowerUpPanel
│   │   ├── Label
│   │   └── Fill (Image progress bar)
│   └── PauseButton (Interactive)
│
├── MainMenuPanel [ACTIVE]
│   ├── Title ("LINKSAURUS")
│   ├── PlayButton (Interactive)
│   └── HighScoreText
│
└── GameOverPanel [INACTIVE]
    ├── GameOverTitle ("GAME OVER")
    ├── FinalScore
    └── PlayAgainButton (Interactive)
```

---

## UI Controller Scripts (Attached to Canvas)

| Script | Panel | Visible Initially | Triggered By |
|--------|-------|------------------|--------------|
| **MainMenuUI.cs** | MainMenuPanel | ✅ YES | Game Start |
| **HudUI.cs** | HudPanel | ❌ NO | Playing State |
| **GameOverUI.cs** | GameOverPanel | ❌ NO | Game Over |

---

## Game Flow State Machine

```
MENU                  PLAYING               PAUSED
├─ Menu visible       ├─ HUD visible        ├─ Time frozen
├─ HUD hidden         ├─ Menu hidden        ├─ No UI change
└─ Pause unavailable  ├─ GameOver hidden    └─ Game frozen
                      └─ Pause button active
                              │
                              ├→ ESC Key
                              ├→ Pause Button
                              └→ Collision → GAMEOVER
                                            ├─ Menu hidden
                                            ├─ HUD hidden
                                            └─ GameOver visible (animated)
```

---

## Quick Event Map

**GameManager Events → UI Listeners:**

```
OnGameStart event
  ├→ MainMenuUI.Hide()
  ├→ HudUI.Show()
  └→ GameOverUI.Hide()

OnGameOver event
  ├→ MainMenuUI.Hide()
  ├→ HudUI.Hide()
  └→ GameOverUI.Show() [with scale animation]

OnScoreChanged event
  └→ HudUI.UpdateScore()

OnPowerUpChanged event
  └→ HudUI.HandlePowerUpChanged()
```

---

## Panel Visibility Matrix

| State | MainMenu | HUD | GameOver |
|-------|----------|-----|----------|
| Menu | 🟢 Visible | 🔴 Hidden | 🔴 Hidden |
| Playing | 🔴 Hidden | 🟢 Visible | 🔴 Hidden |
| Paused | 🔴 Hidden | 🟢 Visible | 🔴 Hidden |
| GameOver | 🔴 Hidden | 🔴 Hidden | 🟢 Visible |

---

## Key GameManager Methods

```
StartGame()          → GameState: Menu → Playing
                     → Resets score, speed, triggers OnGameStart

TriggerGameOver()    → GameState: Playing → GameOver
                     → Updates high score, triggers OnGameOver

PauseGame()          → GameState: Playing → Paused
                     → Time.timeScale = 0

UnpauseGame()        → GameState: Paused → Playing
                     → Time.timeScale = 1
```

---

## Panel Animation Details

**Only GameOverPanel has animation:**
```csharp
Show() {
    panel.localScale = Vector3.zero
    StartCoroutine(ScalePanel(Vector3.one))
    // Scales from 0 → 1 over ~0.5 seconds
}
```

**MainMenuPanel & HudPanel:**
```
SetActive(true/false)   // Instant, no animation
```

---

## Missing/TODO Items

- ❌ Pause Menu UI (no visual feedback when paused)
- ❌ Screen dimming on pause
- ❌ Transition animations (fade/slide in)
- ❌ Settings/Options menu
- ❌ Share button implementation (logged but not functional)
- ❌ Revive button UI feedback

---

## Prefab References

Located in: `Assets/Prefabs/UI/`

- `MainMenuPanel.prefab` - Reusable menu template
- `HudPanel.prefab` - Reusable HUD template  
- `GameOverPanel.prefab` - Reusable game-over template
- `MainCanvas.prefab` - Complete canvas (not used in scenes)

**Note:** Current scenes use scene instances, not prefab instances

---

## Component References Summary

**Canvas Components:**
- RectTransform (UI positioning)
- Canvas (UI rendering)
- CanvasScaler (responsive scaling)
- GraphicRaycaster (input detection)
- MainMenuUI (script)
- HudUI (script)
- GameOverUI (script)

**Panel Components:**
- RectTransform (positioning/sizing)
- CanvasRenderer (rendering)
- Image (background graphics)
- Button (interactive, HUD/GameOver only)

---

## How to Debug Menu Flow

1. **Check Panel Visibility:**
   - Play game in editor
   - Open "Console" tab to see Debug.Log() calls
   - HudUI logs "HUD Showing" / "HUD Hiding"

2. **Verify GameState:**
   - Add watch on `GameManager.Instance.CurrentState`
   - Should be: Menu → Playing → GameOver → Menu cycle

3. **Test Event Wiring:**
   - Add breakpoints in UI manager Show()/Hide() methods
   - Trigger state changes (click Play, lose game, etc.)
   - Should hit breakpoints in correct order

4. **Check EventSystem:**
   - Ensure EventSystem GameObject exists in scene
   - Required for button clicks and input handling

---

## Performance Notes

- **Time.timeScale = 0 on Pause** - All animations and physics freeze
- **OnEnable/OnDisable hooks** - Proper event subscription/cleanup in HudUI
- **No animation curves** - Raw Lerp used for GameOver scale animation
- **Real-time score updates** - No batching, updates every frame of score change


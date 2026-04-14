# Linksaur Project: Menu Flow & Scene Structure Analysis

## Project Overview

**Project Name:** Linksaur  
**Game Type:** Mobile 2D scrolling/matching game  
**Primary Scenes:** 
- `Assets/Scenes/SampleScene.unity` (Menu/UI Preview Scene)
- `Assets/Scenes/GameScene.unity` (Main Game Scene)

---

## Game State Flow

The game follows a clear state machine with 4 states defined in `GameManager.cs`:

```csharp
public enum GameState { Menu, Playing, GameOver, Paused }
```

### State Transitions

```
Menu 
  ↓ (PlayButton clicked)
Playing 
  ├→ Paused (PauseButton clicked or ESC key)
  │   ↓ (UnpauseButton or ESC key)
  │   ↑ (Back to Playing)
  └→ GameOver (Player loses/collision)
      ↓ (PlayAgainButton or Revive)
      ↑ (Back to Menu)
```

---

## Scene Hierarchy & UI Structure

### Both Scenes Have Identical UI Setup

Both `SampleScene.unity` and `GameScene.unity` contain the same Canvas hierarchy with three panel components:

```
Canvas
├── HudPanel (Active: FALSE at start, TRUE when Playing)
│   ├── ScoreText (TextMeshProUGUI)
│   ├── PowerUpPanel (Contains power-up UI)
│   │   ├── Label (TextMeshProUGUI)
│   │   └── Fill (Image - fills as power-up duration counts down)
│   └── PauseButton (Button)
│
├── MainMenuPanel (Active: TRUE at start, FALSE when Playing)
│   ├── Title (TextMeshProUGUI - "LINKSAURUS")
│   ├── PlayButton (Button)
│   └── HighScoreText (TextMeshProUGUI)
│
└── GameOverPanel (Active: FALSE at start, TRUE when GameOver)
    ├── GameOverTitle (TextMeshProUGUI - "GAME OVER")
    ├── FinalScore (TextMeshProUGUI)
    └── PlayAgainButton (Button)
```

### Canvas Configuration
- **Canvas Scaler Mode:** Scale With Screen Size
- **Reference Resolution:** 1920 x 1080
- **UI Layer:** Layer 5

---

## UI Manager Components

All three UI panels are controlled by dedicated manager scripts attached to the Canvas:

### 1. **MainMenuUI.cs**
- **Responsibility:** Manages main menu visibility and interactions
- **References:** MainMenuPanel, HighScoreText, PlayButton
- **Key Methods:**
  - `StartGame()` - Calls `GameManager.Instance.StartGame()`
  - `Show()` - Activates MainMenuPanel, updates high score display
  - `Hide()` - Deactivates MainMenuPanel
- **Event Listeners:**
  - `GameManager.OnGameStart` → Hide menu
  - Button click → StartGame()
- **Initial State:** Menu is VISIBLE

```csharp
private void Start() {
    _playButton.onClick.AddListener(StartGame);
    UpdateHighScore();
    GameManager.OnGameStart += Hide;
}
```

### 2. **HudUI.cs**
- **Responsibility:** Manages in-game HUD (score, power-ups, pause button)
- **References:** HudPanel, ScoreText, PowerUpPanel, PowerUpFill, PauseButton
- **Key Methods:**
  - `Show()` - Activates HudPanel
  - `Hide()` - Deactivates HudPanel
  - `TogglePause()` - Toggles game pause state
  - `UpdateScore()` - Updates score text
  - `HandlePowerUpChanged()` - Shows/hides power-up indicator and updates fill
- **Event Listeners:**
  - `GameManager.OnGameStart` → Show HUD
  - `GameManager.OnGameOver` → Hide HUD
  - `PowerUpManager.OnPowerUpChanged` → Update power-up display
  - `GameManager.OnScoreChanged` → Update score text
  - Button click → TogglePause()
- **Initial State:** HUD is HIDDEN (inactive)
- **Power-Up Visual Feedback:** Animated fill bar showing remaining duration

```csharp
private void UpdateScore() {
    _scoreText.text = $"Connections: {GameManager.Instance.CurrentScore}";
}

private void TogglePause() {
    if (GameManager.Instance.CurrentState == GameState.Paused)
        GameManager.Instance.UnpauseGame();
    else
        GameManager.Instance.PauseGame();
}
```

### 3. **GameOverUI.cs**
- **Responsibility:** Manages game-over screen
- **References:** GameOverPanel, FinalScoreText, HighScoreText, PlayAgainButton, ReviveButton, ShareButton
- **Key Methods:**
  - `Show()` - Activates GameOverPanel, updates score text, scales in panel (animation)
  - `Hide()` - Deactivates GameOverPanel
  - `PlayAgain()` - Calls `GameManager.Instance.StartGame()`
  - `Revive()` - Shows rewarded ad, revives player (keeps game running)
  - `Share()` - Shares score (not implemented)
  - `ScalePanel()` - Coroutine that animates panel scale from 0 to 1
- **Event Listeners:**
  - `GameManager.OnGameOver` → Show panel with animation
  - `GameManager.OnGameStart` → Hide panel
  - Button clicks → PlayAgain(), Revive(), Share()
- **Initial State:** GameOverPanel is HIDDEN (inactive)
- **Animation:** Panel scales in using coroutine when shown (2x duration speed)

```csharp
private IEnumerator ScalePanel(Vector3 targetScale) {
    float t = 0;
    Vector3 startScale = _panel.transform.localScale;
    while (t < 1) {
        t += Time.deltaTime * 2f;  // 2x speed scaling
        _panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
        yield return null;
    }
}
```

---

## GameManager.cs - State Control Center

**Location:** `Assets/Scripts/Core/GameManager.cs`  
**Pattern:** Singleton (persistent, survives scene loads)

### State Variables
```csharp
public GameState CurrentState = GameState.Menu;
public int CurrentScore = 0;
public int HighScore = 0;
public float ScrollSpeed = InitialScrollSpeed;
```

### Key Methods

#### StartGame()
```csharp
public void StartGame() {
    CurrentScore = 0;
    _consecutiveConnections = 0;
    ScrollSpeed = InitialScrollSpeed;
    CurrentState = GameState.Playing;
    Time.timeScale = 1f;
    
    CancelInvoke(nameof(IncreaseSpeed));
    InvokeRepeating(nameof(IncreaseSpeed), SpeedIncreaseInterval, SpeedIncreaseInterval);
    
    OnGameStart?.Invoke();      // Triggers HudUI.Show(), MainMenuUI.Hide()
    OnScoreChanged?.Invoke();
}
```

#### PauseGame() / UnpauseGame()
```csharp
public void PauseGame() {
    if (CurrentState != GameState.Playing) return;
    _previousState = CurrentState;
    CurrentState = GameState.Paused;
    Time.timeScale = 0f;        // STOPS all physics/animations
    OnGamePaused?.Invoke();
}

public void UnpauseGame() {
    if (CurrentState != GameState.Paused) return;
    CurrentState = _previousState;
    Time.timeScale = 1f;        // RESUMES
    OnGameUnpaused?.Invoke();
}
```

#### TriggerGameOver()
```csharp
public void TriggerGameOver() {
    if (CurrentState == GameState.GameOver) return;
    
    CurrentState = GameState.GameOver;
    CancelInvoke(nameof(IncreaseSpeed));  // Stop speed increase
    
    if (CurrentScore > HighScore) {
        HighScore = CurrentScore;          // Update high score
    }
    
    SaveManager.Save();
    OnGameOver?.Invoke();        // Triggers GameOverUI.Show()
    
    if (_gamesPlayed % 3 == 0)
        AdManager.Instance.ShowInterstitialAd();
}
```

### Events Broadcasted
- **OnGameStart** - Fired when game begins (Menu → Playing)
- **OnGameOver** - Fired when player loses (Playing → GameOver)
- **OnGamePaused** - Fired when game is paused (Playing → Paused)
- **OnGameUnpaused** - Fired when game resumes (Paused → Playing)
- **OnScoreChanged** - Fired when score updates during gameplay

### Pause Input
- **ESC Key** - Toggles pause (handled in Update method)
- **PauseButton** - UI button in HUD panel

---

## Menu Transitions Summary

### Initial Load (SampleScene or GameScene)
**Visible:** MainMenuPanel only  
**Hidden:** HudPanel, GameOverPanel  
**GameState:** Menu  
**Time.timeScale:** 1f (but no game running)

### When Player Clicks "Play"
1. Button calls `MainMenuUI.StartGame()`
2. Which calls `GameManager.Instance.StartGame()`
3. **Sequence:**
   - GameState → Playing
   - ScrollSpeed reset to initial value
   - `OnGameStart` event fired
   - **HudUI.Show()** - Activates HudPanel
   - **MainMenuUI.Hide()** - Deactivates MainMenuPanel
4. **Result:** HUD visible, score/pause button shown, game loop running

### During Gameplay
**Visible:** HudPanel  
**Hidden:** MainMenuPanel, GameOverPanel  
**Interaction:** Score updates in real-time, power-ups show as they're collected  
**Pause Available:** Via PauseButton or ESC key

### When Pause Button Clicked
1. `HudUI.TogglePause()` called
2. `GameManager.PauseGame()` called
3. **Effect:** Time.timeScale = 0 (everything stops)
4. **Current Issue:** No dedicated pause menu UI shown
5. Can unpause with same button or ESC key

### When Player Loses
1. `PlayerController` detects collision
2. `GameManager.TriggerGameOver()` called
3. **Sequence:**
   - GameState → GameOver
   - `OnGameOver` event fired
   - **GameOverUI.Show()** - Activates GameOverPanel with scale animation
   - **HudUI.Hide()** - Deactivates HudPanel
4. **Result:** GameOverPanel visible with:
   - Final score displayed
   - High score shown
   - PlayAgainButton or ReviveButton options

### When "Play Again" or Revive
1. Button calls appropriate method in GameOverUI
2. Which calls `GameManager.Instance.StartGame()`
3. **Cycle repeats:** Returns to "When Player Clicks Play" above

---

## Prefab Assets

Located in `Assets/Prefabs/UI/`:
- **MainCanvas.prefab** - Complete UI Canvas with all panels (not used in current scenes)
- **MainMenuPanel.prefab** - Reusable menu panel
- **HudPanel.prefab** - Reusable HUD panel
- **GameOverPanel.prefab** - Reusable game-over panel

**Note:** Current scenes have panels as scene instances, not prefab instances.

---

## Current Visual Layout

### Main Menu State
```
┌─────────────────────────────────┐
│                                 │
│          LINKSAURUS             │  ← Title Text
│                                 │
│          [Play Button]          │
│                                 │
│    Best: 42 connections         │  ← High Score Text
│                                 │
└─────────────────────────────────┘
```

### Playing State
```
┌─────────────────────────────────┐
│  Connections: 15   [⏸ Pause]    │  ← HUD Panel (top)
│                                 │
│         [GAME AREA]             │
│      (Player sprite here)        │
│         [Scrolling obstacles]    │
│                                 │
│  PowerUp: SHIELD ████░░░░░░     │  ← Shows when active
└─────────────────────────────────┘
```

### Game Over State
```
┌─────────────────────────────────┐
│                                 │
│          GAME OVER              │  ← Title
│                                 │
│     Final Score: 127            │  ← Score
│     Best: 150                   │  ← High Score
│                                 │
│  [Play Again] [Revive] [Share]  │  ← Buttons
│                                 │
└─────────────────────────────────┘
(Scales in with animation)
```

### Paused State (Missing)
**Issue:** No dedicated pause menu UI exists  
**Current Behavior:** Game pauses (Time.timeScale = 0), but no visual UI change  
**Time.timeScale:** Set to 0 (freezes physics, animations, etc.)  
**To Resume:** Click PauseButton or ESC key

---

## Key Implementation Details

### Event-Driven Architecture
- All UI updates driven by GameManager events
- UI managers subscribe to events on Start
- Unsubscribe on Destroy (memory leak prevention)
- Ensures proper sync between game state and UI visibility

### Animation Implementation
Only GameOverUI has animation:
- **Type:** Coroutine-based scaling animation
- **Duration:** ~0.5 seconds (t < 1 with 2x speed)
- **Effect:** Panel scales from Vector3.zero to Vector3.one
- **No tweening library used** - Raw Lerp implementation

### Missing Features
1. **Pause Menu UI** - No visual feedback when paused
2. **Transition animations** - Panel show/hide is instant (except GameOver)
3. **Settings/Options** - Not implemented
4. **Pause screen dimming** - Background not darkened when paused

### Score Management
- **Real-time updates** - Score changes broadcast via OnScoreChanged event
- **High score persistence** - Saved to disk via SaveManager
- **Combo system** - 5+ consecutive connections = +5 bonus
- **Speed progression** - Game speed increases every 10 seconds

### Audio Integration
- Collect sound on positive score change
- Hit sound on game over
- Reward sound on revive (implied)

---

## File Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          ← Main state controller
│   │   ├── SaveManager.cs
│   │   └── AdManager.cs
│   ├── UI/
│   │   ├── MainMenuUI.cs           ← Menu panel controller
│   │   ├── HudUI.cs                ← In-game HUD controller
│   │   ├── GameOverUI.cs           ← Game-over panel controller
│   │   └── LeaderboardManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs     ← Triggers GameOver on collision
│   │   └── PowerUpManager.cs
│   └── Spawning/
│       └── SpawnManager.cs
│
├── Scenes/
│   ├── SampleScene.unity           ← Menu preview scene
│   └── GameScene.unity             ← Main game scene
│
└── Prefabs/UI/
    ├── MainMenuPanel.prefab
    ├── HudPanel.prefab
    └── GameOverPanel.prefab
```

---

## State Diagram

```
                    ┌─────────────────┐
                    │   MENU STATE    │
                    │ MainMenu ACTIVE │
                    │ HUD INACTIVE    │
                    └────────┬────────┘
                             │
                    PlayButton clicked
                             │
                             ↓
        ┌────────────────────────────────────┐
        │     PLAYING STATE                  │
        │ MainMenu HIDDEN                    │
        │ HUD ACTIVE                         │
        │ GameOver HIDDEN                    │
        │ Speed increases every 10 seconds   │
        └────────────────────┬───────────────┘
                      ▲      │
                      │      ├─→ ESC or PauseButton
                      │      │
        UnpauseButton │      ↓
           or ESC     │  ┌──────────────┐
                      │  │PAUSED STATE  │
                      │  │Time.scale=0  │
                      │  │No UI change  │
                      └──┤ (Missing UI) │
                         └──────────────┘
        
        └───────────┬────────────────────────
                    │
             Player collision detected
                    │
                    ↓
        ┌────────────────────────────┐
        │   GAMEOVER STATE           │
        │ MainMenu HIDDEN            │
        │ HUD HIDDEN                 │
        │ GameOver ACTIVE (animated) │
        │ High score updated         │
        └──────────────┬─────────────┘
                       │
          PlayAgain or Revive button
                       │
                       ↓
                  Back to MENU
```

---

## Conclusion

The Linksaur menu system uses a clean, event-driven architecture with a central GameManager singleton controlling state transitions. Three dedicated UI manager scripts handle menu visibility, making the system maintainable and extensible.

**Strengths:**
- Clear separation of concerns
- Event-driven UI updates
- No direct UI-to-UI dependencies
- Persistent GameManager across scenes

**Improvements Needed:**
- Implement dedicated Pause Menu UI with visual feedback
- Add transition animations for menu panels
- Create pause screen darkening effect
- Add settings/options menu
- Consider using a tween library for smoother animations


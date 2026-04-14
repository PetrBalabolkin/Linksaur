# Linksaur UI Architecture Diagram

## Event-Driven Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                         GAME MANAGER                               │
│                     (Singleton Pattern)                            │
│                                                                    │
│  State: Menu | Playing | GameOver | Paused                        │
│  Properties: CurrentScore, HighScore, ScrollSpeed                 │
│                                                                    │
│  Public Events:                                                    │
│  ├─ OnGameStart      (Menu → Playing)                            │
│  ├─ OnGameOver       (Playing → GameOver)                        │
│  ├─ OnGamePaused     (Playing → Paused)                          │
│  ├─ OnGameUnpaused   (Paused → Playing)                          │
│  └─ OnScoreChanged   (Score updated)                             │
└────────────────────────────────────────────────────────────────────┘
              ▲                    ▲                    ▲
              │                    │                    │
              │ Invokes            │ Invokes            │ Invokes
              │                    │                    │
    ┌─────────┴────────┐  ┌────────┴──────────┐  ┌─────┴──────────┐
    │                  │  │                   │  │                │
    │  MAINMENU UI     │  │    HUD UI         │  │  GAMEOVER UI   │
    │  (Canvas)        │  │    (Canvas)       │  │  (Canvas)      │
    │                  │  │                   │  │                │
    │ OnGameStart      │  │ OnGameStart       │  │ OnGameOver     │
    │  └─ Hide()       │  │  └─ Show()        │  │  └─ Show()     │
    │                  │  │                   │  │  [Animation]   │
    │ OnGameOver       │  │ OnGameOver        │  │                │
    │  └─ Hide()       │  │  └─ Hide()        │  │ OnGameStart    │
    │                  │  │                   │  │  └─ Hide()     │
    │ Button.Click     │  │ OnScoreChanged    │  │                │
    │  └─ StartGame()  │  │  └─ UpdateScore() │  │ Button.Click   │
    │  └─ GM.Start()   │  │                   │  │  └─ PlayAgain()│
    │                  │  │ OnPowerUpChanged  │  │  └─ Revive()   │
    │ References:      │  │  └─ ShowPowerUp() │  │                │
    │ ├─ _panel        │  │                   │  │ References:    │
    │ ├─ _highScore    │  │ References:       │  │ ├─ _panel      │
    │ └─ _playButton   │  │ ├─ _hudPanel      │  │ ├─ _finalScore │
    │                  │  │ ├─ _scoreText     │  │ ├─ _highScore  │
    │ Initial State:   │  │ ├─ _powerUpPanel  │  │ └─ _playAgain  │
    │ VISIBLE ✓        │  │ ├─ _pauseButton   │  │                │
    │                  │  │ └─ _powerUpFill   │  │ Initial State: │
    │                  │  │                   │  │ HIDDEN ✗       │
    │                  │  │ Initial State:    │  │                │
    │                  │  │ HIDDEN ✗          │  │ Animation:     │
    │                  │  │                   │  │ Scale 0→1      │
    └──────────────────┘  └───────────────────┘  └────────────────┘
```

---

## UI Hierarchy in Unity Scene

```
CANVAS (RectTransform, Canvas, CanvasScaler, GraphicRaycaster)
├── ATTACH: MainMenuUI.cs
├── ATTACH: HudUI.cs
├── ATTACH: GameOverUI.cs
│
├─ HudPanel (RectTransform) [INACTIVE AT START]
│  │
│  ├─ ScoreText (TextMeshProUGUI)
│  │  └─ Shows: "Connections: 42"
│  │
│  ├─ PowerUpPanel (RectTransform) [Shows when active]
│  │  ├─ Label (TextMeshProUGUI)
│  │  │  └─ Shows: "SHIELD", "ROCKET", etc.
│  │  └─ Fill (Image)
│  │     └─ fillAmount: 0→1 (power-up duration countdown)
│  │
│  └─ PauseButton (Button, Image)
│     └─ OnClick → HudUI.TogglePause()
│
├─ MainMenuPanel (RectTransform, Image) [ACTIVE AT START]
│  │
│  ├─ Title (TextMeshProUGUI)
│  │  └─ Text: "LINKSAURUS"
│  │
│  ├─ PlayButton (Button, Image)
│  │  └─ OnClick → MainMenuUI.StartGame()
│  │
│  └─ HighScoreText (TextMeshProUGUI)
│     └─ Shows: "Best: 150 connections"
│
└─ GameOverPanel (RectTransform, Image) [INACTIVE AT START]
   │
   ├─ GameOverTitle (TextMeshProUGUI)
   │  └─ Text: "GAME OVER"
   │
   ├─ FinalScore (TextMeshProUGUI)
   │  └─ Shows: "Score: 127"
   │
   └─ PlayAgainButton (Button, Image)
      ├─ OnClick → GameOverUI.PlayAgain()
      └─ [Also shows Revive & Share buttons]
```

---

## Data Flow: Score Update

```
┌──────────────────────┐
│  Player Collision    │
│   OR Collectible     │
│   touched/collected  │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────┐
│ GameManager.AddConnections(amount)
│ - Update CurrentScore
│ - Trigger combo logic
│ - Play sound effect
└──────────┬───────────────┘
           │
           ▼
┌──────────────────────────┐
│ OnScoreChanged.Invoke()
│ (Public Event)
└──────────┬───────────────┘
           │
           ├──→ HudUI.UpdateScore()
           │    └─ _scoreText.text = 
           │       $"Connections: {score}"
           │
           └──→ [Any other listeners]
```

---

## State Transitions Flow

```
       ┌─ MENU STATE ─────────────────┐
       │                              │
       │ UI Visible:                  │
       │ ├─ MainMenuPanel: ACTIVE     │
       │ ├─ HudPanel: INACTIVE        │
       │ └─ GameOverPanel: INACTIVE   │
       │                              │
       │ Can Do:                      │
       │ └─ Click PlayButton          │
       │                              │
       └──┬──────────────────────────┘
          │ PlayButton.onClick
          │ → MainMenuUI.StartGame()
          │ → GM.StartGame()
          ▼
       ┌─ PLAYING STATE ──────────────────┐
       │                                  │
       │ GameManager.StartGame():         │
       │ - Reset CurrentScore = 0         │
       │ - Reset ScrollSpeed             │
       │ - Set TimeScale = 1             │
       │ - Invoke OnGameStart            │
       │                                  │
       │ UI Actions:                      │
       │ - MainMenuUI.Hide()             │
       │ - HudUI.Show()                  │
       │ - GameOverUI.Hide()             │
       │                                  │
       │ UI Visible:                      │
       │ ├─ MainMenuPanel: INACTIVE      │
       │ ├─ HudPanel: ACTIVE             │
       │ └─ GameOverPanel: INACTIVE      │
       │                                  │
       │ Can Do:                          │
       │ ├─ Move player                  │
       │ ├─ Collect items (score++)      │
       │ └─ Click PauseButton            │
       │                                  │
       └──┬─────────────────┬───────────┘
          │                 │
      [Pause]           [Collision]
          │                 │
          ▼                 ▼
    ┌─ PAUSED STATE ─┐  ┌─ GAMEOVER STATE ─────┐
    │                │  │                      │
    │ TimeScale = 0  │  │ GM.TriggerGameOver():│
    │ All frozen     │  │ - Update HighScore   │
    │ No UI change   │  │ - Save game data     │
    │ (Missing!)     │  │ - Play hit sound     │
    │                │  │ - Invoke OnGameOver  │
    │ Can Do:        │  │                      │
    │ └─ Unpause     │  │ UI Actions:          │
    │   (ESC/Button) │  │ - HudUI.Hide()       │
    │                │  │ - GameOverUI.Show()  │
    └──┬─────────────┘  │   [With Scale Anim] │
       │                │                      │
       │ [Unpause]      │ UI Visible:          │
       │ TimeScale = 1  │ ├─ GameOverPanel:    │
       └─────┬──────────┘ │   ACTIVE           │
             │            │ ├─ HudPanel:       │
             └────────────┼→   INACTIVE        │
                          │ └─ MainMenuPanel:  │
                          │   INACTIVE         │
                          │                    │
                          │ Can Do:            │
                          │ ├─ PlayAgain()     │
                          │ └─ Revive()        │
                          │                    │
                          └──┬─────────────────┘
                             │ [PlayAgain/Revive]
                             │ → GM.StartGame()
                             └──→ Back to PLAYING
```

---

## Message Sequence: Game Lifecycle

```
User Action                GameManager              UI Scripts
─────────────              ───────────              ──────────

1. Scene loads
                      CS = Menu
                      TimeScale = 1            MainMenuPanel.Start()
                                              └─ Show()
                                              └─ MainMenuPanel ACTIVE ✓

2. Click "Play"
PlayButton clicked ──→ StartGame()
                      ├─ CS = Playing
                      ├─ Score = 0
                      ├─ TimeScale = 1
                      └─ OnGameStart.Invoke()
                                              MainMenuUI.Hide()
                                              └─ MainMenuPanel INACTIVE
                                              HudUI.Show()
                                              └─ HudPanel ACTIVE ✓

3. Collect 10 items
Player collects ──→ AddConnections(10)
                    ├─ Score += 10
                    └─ OnScoreChanged.Invoke()
                                              HudUI.UpdateScore()
                                              └─ Text = "Connections: 10"

4. Press ESC/Pause
[ESC Key] ────────→ PauseGame()
                    ├─ CS = Paused
                    ├─ TimeScale = 0
                    └─ OnGamePaused.Invoke()
                                              (No action - BUG!)

5. Press ESC again
[ESC Key] ────────→ UnpauseGame()
                    ├─ CS = Playing
                    ├─ TimeScale = 1
                    └─ OnGameUnpaused.Invoke()
                                              (No action)

6. Collision detected
PlayerController ──→ TriggerGameOver()
                    ├─ CS = GameOver
                    ├─ HighScore update
                    └─ OnGameOver.Invoke()
                                              HudUI.Hide()
                                              └─ HudPanel INACTIVE
                                              GameOverUI.Show()
                                              ├─ Update score text
                                              ├─ GameOverPanel.scale = 0
                                              └─ StartCoroutine(ScalePanel)
                                                 └─ Animate scale 0→1

7. Coroutine animates
                                              ScalePanel() running
                                              └─ ~0.5 sec animation
                                              └─ GameOverPanel visible ✓

8. Click "Play Again"
Button clicked ──→ PlayAgain()
                    └─ StartGame()
                      └─ Back to step 2
```

---

## Component Attachment Reference

```
Scene Hierarchy              Components                 Scripts
──────────────              ──────────────             ────────

Canvas
├─ RectTransform            Main UI component
├─ Canvas                   Render mode setting
├─ CanvasScaler             Responsive scaling
├─ GraphicRaycaster         Input handling
│
├─ MainMenuUI (Script) ←─────→ MainMenuUI.cs
├─ HudUI (Script)       ←─────→ HudUI.cs
├─ GameOverUI (Script)  ←─────→ GameOverUI.cs
│
├─ HudPanel
│  ├─ RectTransform
│  ├─ ScoreText          ← HudUI._scoreText
│  ├─ PowerUpPanel
│  │  ├─ Label           ← HudUI._powerUpLabel
│  │  └─ Fill (Image)    ← HudUI._powerUpFill
│  └─ PauseButton        ← HudUI._pauseButton
│                           [OnClick.AddListener]
│
├─ MainMenuPanel
│  ├─ RectTransform
│  ├─ Image (background)
│  ├─ Title              (Display only)
│  ├─ PlayButton         ← MainMenuUI._playButton
│  │                        [OnClick.AddListener]
│  └─ HighScoreText      ← MainMenuUI._highScoreText
│
└─ GameOverPanel
   ├─ RectTransform
   ├─ Image (background)
   ├─ GameOverTitle      (Display only)
   ├─ FinalScore         ← GameOverUI._finalScoreText
   └─ PlayAgainButton    ← GameOverUI._playAgainButton
                            [OnClick.AddListener]
```

---

## Code Execution Path: Main Menu → Play

```
MainMenuUI.Start()
└─ _playButton.onClick.AddListener(StartGame)

[User clicks Play Button]
└─ MainMenuUI.StartGame()
   └─ GameManager.Instance.StartGame()
      ├─ CurrentScore = 0
      ├─ ScrollSpeed = InitialScrollSpeed
      ├─ CurrentState = GameState.Playing
      ├─ Time.timeScale = 1f
      ├─ CancelInvoke(IncreaseSpeed)
      ├─ InvokeRepeating(IncreaseSpeed, interval, interval)
      │
      └─ OnGameStart?.Invoke()
         │
         ├─ MainMenuUI.Hide()
         │  └─ _panel.SetActive(false)
         │
         ├─ HudUI.Show()
         │  ├─ Debug.Log("HUD Showing")
         │  └─ _hudPanel.SetActive(true)
         │
         ├─ GameOverUI.Hide()
         │  └─ _panel.SetActive(false)
         │
         └─ OnScoreChanged?.Invoke()
            └─ HudUI.UpdateScore()
               └─ _scoreText.text = "Connections: 0"
```

---

## Pause Feature - Current Implementation

```
❌ MISSING FUNCTIONALITY

PauseButton.onClick
└─ HudUI.TogglePause()
   └─ GameManager.PauseGame()
      ├─ CurrentState = GameState.Paused
      ├─ Time.timeScale = 0f
      └─ OnGamePaused?.Invoke()

OnGamePaused event:
└─ (No listeners!)
   ├─ MainMenuUI: Not listening ✗
   ├─ HudUI: Not listening ✗
   └─ GameOverUI: Not listening ✗

Result:
├─ Game pauses (Time.scale = 0)
├─ But no visual UI change
├─ User sees frozen game without pause menu
└─ Very poor UX!

Solution Needed:
├─ Create PauseMenuUI.cs
├─ Listen to OnGamePaused / OnGameUnpaused
├─ Show/Hide pause menu panel
├─ Add screen dimming effect
└─ Add Resume button
```

---

## Summary: 3-Panel Architecture

```
Three independent UI panels, one manager script each:

     ┌──────────────────┬──────────────────┬──────────────────┐
     │  MainMenuPanel   │    HudPanel      │  GameOverPanel   │
     ├──────────────────┼──────────────────┼──────────────────┤
     │  Manager:        │  Manager:        │  Manager:        │
     │  MainMenuUI.cs   │  HudUI.cs        │  GameOverUI.cs   │
     │                  │                  │                  │
     │  Visible: Menu   │  Visible: Play   │  Visible: Over   │
     │  Hidden: Other   │  Visible: Pause  │  Hidden: Other   │
     │                  │  Hidden: Other   │                  │
     │  Content:        │  Content:        │  Content:        │
     │  ├─ Title        │  ├─ Score text   │  ├─ Game Over    │
     │  ├─ Play Button  │  ├─ PowerUp UI   │  ├─ Final Score  │
     │  └─ Best Score   │  └─ Pause Button │  ├─ Best Score   │
     │                  │                  │  └─ Buttons      │
     │  Animation:      │  Animation:      │  Animation:      │
     │  Instant         │  Instant         │  Scale (0→1)     │
     └──────────────────┴──────────────────┴──────────────────┘

All driven by GameManager events - loose coupling ✓
```


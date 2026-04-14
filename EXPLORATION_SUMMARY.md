# Linksaur Project - Menu Flow & Scene Structure Exploration

## 📋 Executive Summary

I've completed a comprehensive exploration of the Linksaur project's scene structure, menu system, and game state management. The project uses a clean **event-driven architecture** with a central **GameManager** singleton that broadcasts state changes to three independent UI controller scripts.

---

## 📂 Exploration Deliverables

Three detailed reports have been created:

### 1. **MENU_FLOW_ANALYSIS.md** (16 KB)
**Most Comprehensive Report**

Contains:
- Complete game state flow and transitions
- Detailed breakdown of all three UI manager scripts (MainMenuUI, HudUI, GameOverUI)
- GameManager architecture and all state control methods
- Menu transition sequences with code examples
- Prefab asset inventory
- Visual layout mockups for each game state
- Event-driven architecture explanation
- Current implementation details
- Missing features and improvement recommendations

**Start here** for a complete understanding of how the menu system works.

---

### 2. **SCENE_STRUCTURE_SUMMARY.md** (5.5 KB)
**Quick Reference Guide**

Contains:
- Quick comparison of both scenes (SampleScene vs GameScene)
- Canvas hierarchy overview
- UI controller script reference table
- Game flow state machine diagram
- Event-to-UI mapping
- Panel visibility matrix
- Key GameManager methods
- Animation implementation details
- Missing features checklist
- Debugging tips

**Use this** as a quick lookup guide when you need fast answers.

---

### 3. **UI_ARCHITECTURE_DIAGRAM.md** (18 KB)
**Visual & Technical Deep Dive**

Contains:
- Event-driven architecture diagram
- Complete UI hierarchy tree
- Data flow diagrams
- Detailed state transition flow
- Message sequence (lifecycle diagram)
- Component attachment reference
- Code execution paths
- Pause feature analysis (current bug identified)
- 3-panel architecture summary

**Refer to this** for understanding data flow and component relationships.

---

## 🎮 Key Findings

### Game States
```
Menu → Playing ↔ Paused → GameOver → Menu (cycle)
```

### UI Panel Structure
- **MainMenuPanel** - Visible at menu, hidden during play
- **HudPanel** - Hidden at menu, visible during play (and pause)
- **GameOverPanel** - Hidden except when game ends, animates in with scale

### Controllers
All three panels have dedicated manager scripts attached to Canvas:
- `MainMenuUI.cs` - Manages main menu
- `HudUI.cs` - Manages in-game HUD  
- `GameOverUI.cs` - Manages game-over screen

### Event System
GameManager broadcasts 5 main events:
1. `OnGameStart` - Menu→Playing transition
2. `OnGameOver` - Playing→GameOver transition
3. `OnGamePaused` - Playing→Paused transition
4. `OnGameUnpaused` - Paused→Playing transition
5. `OnScoreChanged` - Score updates

### Architecture Strength
✅ **Loose coupling** - UI scripts don't know about each other  
✅ **Event-driven** - Changes broadcast, not pulled  
✅ **Extensible** - Easy to add new panels/managers  
✅ **Maintainable** - Clear separation of concerns  

---

## 🔍 Critical Finding: Pause Menu Bug

**Issue:** When player presses ESC or PauseButton:
1. Game pauses correctly (Time.timeScale = 0)
2. GameManager broadcasts `OnGamePaused` event
3. **But no UI scripts listen to this event!**

**Result:** Game freezes without any visual pause menu  
**Impact:** Poor UX - player doesn't know if pause worked

**Solution:** Create `PauseMenuUI.cs` script that:
- Listens to `OnGamePaused` and `OnGameUnpaused` events
- Shows a pause menu panel on pause
- Hides it on unpause
- Optionally dims the background

---

## 📊 Scenes Explored

### SampleScene.unity
- **Purpose:** UI development/preview
- **Content:** Minimal (just Camera, Light, Canvas)
- **Use:** Testing menu flows without full game setup

### GameScene.unity
- **Purpose:** Main playable game
- **Content:** Player, Ground, Managers, Background, Canvas
- **Managers:** GameManager, AdManager, SpawnManager, etc.
- **Use:** Full game experience

**Both have identical Canvas structure** - good for consistency

---

## 🏗️ Architecture Patterns Used

### Singleton Pattern
- **GameManager** - One instance, persists across scenes
- `Instance` property for global access

### Event System
- C# `Action` delegates for pub/sub pattern
- GameManager as publisher, UI scripts as subscribers

### Manager Pattern
- Each UI panel has dedicated manager script
- Scripts attached to Canvas root
- Handle panel visibility and interactions

### State Machine
- GameState enum: Menu, Playing, GameOver, Paused
- GameManager tracks current state
- State changes drive UI updates

---

## 📈 Current Visual Layout

### Menu State
```
        LINKSAURUS
        [Play Button]
        Best: X connections
```

### Playing State
```
Connections: 42  [Pause]
    [Game Area]
[PowerUp: SHIELD ▓▓▓░░░░]
```

### Game Over State
```
        GAME OVER
        Score: 127
        Best: 150
[Play Again] [Revive] [Share]
(Scales in with animation)
```

### Paused State (Missing)
```
(Same as Playing - no pause menu UI!)
(Game frozen but no visual feedback)
```

---

## 🔧 Implementation Notes

### Score Management
- Real-time updates via OnScoreChanged event
- Persistent save to disk (SaveManager)
- Combo bonus system (5+ consecutive = +5 bonus)
- High score updated on game over

### Animations
- **GameOverPanel only:** Coroutine-based scale animation (0→1 over ~0.5s)
- **Other panels:** Instant SetActive() - no animation
- No tweening library used - raw Lerp implementation

### Input
- ESC key toggles pause (handled in GameManager.Update)
- Buttons wired via OnClick listeners
- EventSystem required in scene for input handling

### Audio
- Collect sound on positive score
- Hit sound on game over
- Rewarded/interstitial ads handled by AdManager

---

## 🚀 Next Steps & Recommendations

### High Priority
1. **Implement Pause Menu UI**
   - Create PauseMenuUI.cs
   - Listen to OnGamePaused / OnGameUnpaused events
   - Show pause panel with Resume button
   - Add background dimming effect

### Medium Priority
2. **Add Transition Animations**
   - Fade in/out for menu panels
   - Slide animations for transitions
   - Use LeanTween or DOTween for smoother animations

3. **Settings/Options Menu**
   - Sound on/off toggle
   - Graphics quality settings
   - Leaderboard settings

### Polish
4. **Add Settings Menu**
5. **Improve pause screen feedback**
6. **Add screen shake on collision**
7. **Implement share functionality**

---

## 📁 File Structure Quick Reference

```
Assets/
├── Scripts/
│   ├── Core/GameManager.cs          ← State controller
│   ├── UI/MainMenuUI.cs             ← Menu manager
│   ├── UI/HudUI.cs                  ← HUD manager
│   ├── UI/GameOverUI.cs             ← GameOver manager
│   ├── Player/PlayerController.cs    ← Triggers GameOver
│   └── ...
├── Scenes/
│   ├── SampleScene.unity             ← Menu preview
│   └── GameScene.unity               ← Main game
└── Prefabs/UI/
    ├── MainMenuPanel.prefab
    ├── HudPanel.prefab
    ├── GameOverPanel.prefab
    └── MainCanvas.prefab
```

---

## 🎯 Key Code Entry Points

**To understand the menu flow, start with:**
1. `GameManager.cs` - Lines 56-68 (StartGame method)
2. `MainMenuUI.cs` - Lines 15-50 (Start and event handling)
3. `HudUI.cs` - Lines 22-79 (Show/Hide and pause toggle)
4. `GameOverUI.cs` - Lines 36-88 (Show animation and buttons)

**To see state transitions:**
1. Open GameScene.unity in Editor
2. Play the game
3. Watch the Console for Debug.Log messages
4. Check GameManager.CurrentState in the Inspector

---

## 📚 Document Usage Guide

| Need | Read |
|------|------|
| Complete overview | MENU_FLOW_ANALYSIS.md |
| Quick facts | SCENE_STRUCTURE_SUMMARY.md |
| Diagrams & flow | UI_ARCHITECTURE_DIAGRAM.md |
| Debugging guide | SCENE_STRUCTURE_SUMMARY.md → "How to Debug Menu Flow" |
| Code paths | UI_ARCHITECTURE_DIAGRAM.md → "Code Execution Path" |
| Missing features | All three documents have "Missing Features" sections |

---

## ✅ Exploration Checklist

What was analyzed:

- ✅ Scene hierarchy (both SampleScene and GameScene)
- ✅ Canvas structure and UI panels
- ✅ All three UI manager scripts (full code review)
- ✅ GameManager state control (complete analysis)
- ✅ Event system and broadcasting
- ✅ Menu transitions and state flows
- ✅ Prefab assets inventory
- ✅ Animation implementation
- ✅ Input handling (buttons, ESC key)
- ✅ Audio integration
- ✅ Score management
- ✅ Pause feature (found bug!)
- ✅ Game-over flow with animation
- ✅ Data persistence (SaveManager)
- ✅ Current limitations and gaps

---

## 💡 Summary

The Linksaur menu system is **well-architected** with clean separation of concerns and event-driven communication. The main gap is the **missing Pause Menu UI**, which would be a straightforward addition following the existing pattern.

The three documents provided offer multiple perspectives:
- **MENU_FLOW_ANALYSIS** for deep understanding
- **SCENE_STRUCTURE_SUMMARY** for quick lookups  
- **UI_ARCHITECTURE_DIAGRAM** for visual learning

Use them as references when implementing new features or debugging issues.

---

**Generated:** April 14, 2026  
**Project:** Linksaur (Unity 2024 LTS)  
**Engine:** Unity 6000.3.7f1  
**Platform:** Mobile/WebGL


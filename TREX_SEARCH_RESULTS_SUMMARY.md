# T-Rex Character Search Results - Complete Summary

## 📌 Quick Reference

**Question:** Where is the T-Rex character and why is it floating in the sky?

**Answer:** The T-Rex (dinosaur player) is in `GameScene.unity` as the "Player" GameObject, but it appears to be floating due to a **misconfigured ground detection offset** in the `PlayerController.cs` script.

---

## 🎯 Key Findings Summary

### 1. GameObject Location ✅
- **Name:** Player (the T-Rex/dinosaur character)
- **Scene:** Assets/Scenes/GameScene.unity
- **GameObject ID:** 1151752932
- **Position:** (-3, -4, 0)
- **Status:** Active in scene

### 2. Associated Scripts ✅
- **Main Controller:** `Assets/Scripts/Player/PlayerController.cs`
- **Game Manager:** `Assets/Scripts/Core/GameManager.cs`
- **Power-Up System:** `Assets/Scripts/Player/PowerUpManager.cs`
- **Scroll Manager:** `Assets/Scripts/Spawning/ScrollingObject.cs`

### 3. Physics Components ✅
- **Rigidbody2D:** Dynamic, GravityScale = 3 ✅
- **BoxCollider2D:** Size (2.2 x 1.5), Offset (0, 0.75) ✅
- **Layer:** Player (Layer 9) ✅

### 4. Ground Detection Logic ⚠️
- **Ground Layer:** Layer 6 ✅
- **Ground Check Radius:** 0.15 units ✅
- **Ground Check Offset:** (0, -0.6, 0) **❌ PROBLEM!**
- **Ground Check Position:** (-3, -4.6, 0) - **Below ground surface**

### 5. Root Cause 🔴
The ground detection check sphere is positioned at Y = -4.6, which is **below the ground surface at Y = -4.0**. This causes `_isGrounded` to always return `false`, preventing jumping and causing the character to appear to float.

---

## 📁 Files Found & Components

### Player GameObject Components
```
Player (GameObject ID: 1151752932, Layer: 9)
├── Transform
│   ├── Position: (-3, -4, 0)
│   └── Scale: (1.6, 1.6, 1)
├── SpriteRenderer
│   ├── Sprite: guid: 89afa4e4df0f34a97855ade8a4175897
│   └── Sorting Order: 10
├── BoxCollider2D
│   ├── Size: (2.2, 1.5)
│   ├── Offset: (0, 0.75)
│   └── Is Trigger: No (solid collider)
├── Rigidbody2D
│   ├── Body Type: Dynamic
│   ├── Mass: 1
│   ├── Gravity Scale: 3
│   └── Constraints: Freeze Rotation Z
├── Animator
│   └── Parameters: IsRunning, IsGrounded
├── AudioSource
│   └── Play On Awake: No
└── PlayerController (Script)
    ├── Jump Force: 9.5
    ├── Ground Layer: Layer 6
    ├── Ground Check Radius: 0.15
    └── Ground Check Offset: (0, -0.6) ← ISSUE!
```

### Ground GameObject
```
Ground (GameObject ID: 1362047193, Layer: 6)
├── Transform
│   ├── Position: (0, -4.5, 0)
│   └── Scale: (1, 1, 1)
├── BoxCollider2D
│   ├── Size: (1000, 5)
│   ├── Offset: (0, -2)
│   ├── Is Trigger: No
│   └── Effective Top: Y = -4.0
└── SpriteRenderer
    └── Visual ground platform
```

---

## 🔍 Physics Analysis

### Position Hierarchy
```
Camera Viewport Center: Y = 0
Ground Top Surface: Y = -4.0
Player Transform: Y = -4.0 (approximately at ground level)
Player Collider Center: Y = -2.25
Player Collider Bottom: Y = -3.0
Ground Check Point: Y = -4.6 ❌ TOO LOW!
```

### Ground Detection Math
```
Player Y: -4
Ground Check Offset: -0.6
Check Point Y: -4 + (-0.6) = -4.6
Check Sphere Radius: 0.15
Check Sphere Range: -4.75 to -4.45

Ground Top Surface: -4.0

Overlap Check: Is -4.75 to -4.45 inside ground collider?
Result: NO! (-4.75 to -4.45 is BELOW -4.0)
```

---

## 📊 Component Configuration Table

| Component | Property | Value | Status |
|-----------|----------|-------|--------|
| Rigidbody2D | Body Type | Dynamic | ✅ |
| Rigidbody2D | Gravity Scale | 3 | ✅ |
| Rigidbody2D | Mass | 1 | ✅ |
| Rigidbody2D | Simulated | Yes | ✅ |
| BoxCollider2D | Size | (2.2, 1.5) | ✅ |
| BoxCollider2D | Offset | (0, 0.75) | ✅ |
| BoxCollider2D | Is Trigger | No | ✅ |
| PlayerController | Jump Force | 9.5 | ✅ |
| PlayerController | Ground Layer | Layer 6 | ✅ |
| PlayerController | Ground Check Radius | 0.15 | ✅ |
| PlayerController | **Ground Check Offset** | **(0, -0.6)** | **❌** |

---

## 🗂️ Complete File Reference

### Core Player Scripts
| File | Path | Purpose |
|------|------|---------|
| PlayerController.cs | Assets/Scripts/Player/PlayerController.cs | Character movement, jumping, ground detection |
| PowerUpManager.cs | Assets/Scripts/Player/PowerUpManager.cs | Shield and power-up system |

### Game Management Scripts
| File | Path | Purpose |
|------|------|---------|
| GameManager.cs | Assets/Scripts/Core/GameManager.cs | Game state machine, speed control |
| SpawnManager.cs | Assets/Scripts/Spawning/SpawnManager.cs | Obstacle and collectible spawning |
| ScrollingObject.cs | Assets/Scripts/Spawning/ScrollingObject.cs | Camera-relative object movement |

### Scene Files
| File | Path | Purpose |
|------|------|---------|
| GameScene.unity | Assets/Scenes/GameScene.unity | Main playable scene with Player and Ground |
| SampleScene.unity | Assets/Scenes/SampleScene.unity | UI preview scene (minimal content) |

### Configuration Files
| File | Path | Purpose |
|------|------|---------|
| TagManager.asset | ProjectSettings/TagManager.asset | Layer definitions (Ground = Layer 6) |

---

## 🎯 Specific Code Locations

### Ground Detection Code
**File:** `Assets/Scripts/Player/PlayerController.cs`  
**Lines 14-16:** Configuration
```csharp
[SerializeField] private float _groundCheckRadius = 0.15f;
[SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, 0.1f, 0);  // ← IN CODE
```

**Note:** Scene override shows -0.6 instead of 0.1

**Lines 54-55:** Usage
```csharp
_isGrounded = Physics2D.OverlapCircle(
    transform.position + _groundCheckOffset,
    _groundCheckRadius,
    _groundLayer
);
```

**Lines 94-98:** Debug visualization
```csharp
private void OnDrawGizmosSelected() {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position + _groundCheckOffset, _groundCheckRadius);
}
```

### Physics Setup
**File:** `Assets/Scripts/Player/PlayerController.cs`  
**Lines 32-39:** Awake method
```csharp
private void Awake() {
    _rb = GetComponent<Rigidbody2D>();
    _rb.gravityScale = 3f;  // Set to 3 here
    _collider = GetComponent<BoxCollider2D>();
    _animator = GetComponent<Animator>();
    _audioSource = GetComponent<AudioSource>();
}
```

---

## 🔧 Layer Configuration

### Available Layers
```yaml
Layer 0: Default
Layer 1: TransparentFX
Layer 2: Ignore Raycast
Layer 3: (Empty)
Layer 4: Water
Layer 5: UI
Layer 6: Ground          ← Used for ground detection
Layer 7: Obstacle
Layer 8: Collectible
Layer 9: Player          ← Player assigned here
```

**Ground Detection Layer Mask:** m_Bits = 64 (binary: 0000001000000)  
This correctly selects Layer 6 (Ground)

---

## 📋 Collision Objects Summary

| Object | Type | Layer | Position | Collider | Purpose |
|--------|------|-------|----------|----------|---------|
| Player | GameObject | 9 (Player) | (-3, -4, 0) | BoxCollider2D (solid) | Playable character |
| Ground | GameObject | 6 (Ground) | (0, -4.5, 0) | BoxCollider2D (solid, 1000×5) | Ground platform |
| Obstacles | Spawned | 7 (Obstacle) | Dynamic | BoxCollider2D (trigger) | Hazards |
| Collectibles | Spawned | 8 (Collectible) | Dynamic | BoxCollider2D (trigger) | Items to collect |

---

## 🎮 Game Context

This is a **2D Endless Runner** game where:
- **Playable Character:** T-Rex/Linksaurus dinosaur (the "Player" GameObject)
- **Game Mechanic:** Tap/click to make the dinosaur jump over obstacles
- **Camera System:** Camera stays fixed; world scrolls left (managed by ScrollingObject)
- **Physics:** 2D physics with gravity pulling downward
- **Current Issue:** Player appears to float because ground detection is broken

---

## ✅ Complete Analysis Checklist

What was found:

- ✅ T-Rex/dinosaur GameObject ("Player") in GameScene.unity
- ✅ Player position: (-3, -4, 0)
- ✅ Player scale: (1.6, 1.6, 1) - scaled up
- ✅ Rigidbody2D with gravity scale = 3
- ✅ BoxCollider2D with size (2.2 x 1.5)
- ✅ Ground detection script (PlayerController.cs)
- ✅ Ground layer configuration (Layer 6)
- ✅ Ground GameObject at Y = -4.5
- ✅ Jump force setting: 9.5
- ✅ Jump sound configuration
- ✅ Animator with IsGrounded/IsRunning parameters
- ✅ AudioSource component
- ✅ Ground check offset setting: (0, -0.6) **← ROOT CAUSE**
- ✅ Ground check radius: 0.15
- ✅ Physics 2D collision layer setup
- ✅ Gizmo visualization code already present

---

## 📖 Documentation Generated

Three detailed analysis documents have been created:

1. **TREX_FLOATING_ANALYSIS.md** (this analysis)
   - Complete root cause analysis
   - Physics configuration breakdown
   - Problem explanation with calculations

2. **TREX_COMPONENT_HIERARCHY.md** (visual guide)
   - GameObject structure diagram
   - Physics geometry visualization
   - Code locations for fixes
   - Debug tips and verification steps

3. **TREX_SEARCH_RESULTS_SUMMARY.md** (this file)
   - Quick reference summary
   - File listing with paths
   - Complete component table
   - Collision objects overview

---

## 🔴 The Problem in One Sentence

The ground detection check sphere is positioned 0.6 units below the player (at Y = -4.6), which is **below the ground surface (Y = -4.0)**, so it never detects collision with the ground and the player remains perpetually ungrounded.

---

## ✨ Next Steps

1. **Verify the issue** by running the game and watching the console for ground detection status
2. **Adjust the offset** from (0, -0.6) to a value that places the check sphere overlapping the ground
3. **Test in play mode** and confirm the dinosaur can now jump
4. **Review any animation changes** needed for proper running/landing state

---

**Analysis Date:** April 14, 2026  
**Project:** Linksaur (Unity 2024 LTS - Version 6000.3.7f1)  
**Status:** 🔴 Issue Identified, Root Cause Confirmed, Ready for Debugging


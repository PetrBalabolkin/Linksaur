# T-Rex Component Hierarchy & Physics Diagram

## 🎮 Player GameObject Structure

```
┌─────────────────────────────────────────────────────────────┐
│ Player (GameObject ID: 1151752932)                          │
│ Layer: 9 (Player) | Tag: Player | Active: Yes              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ ├─ Transform (ID: 1151752937)                              │
│ │  ├─ Position: (-3, -4, 0)                                │
│ │  ├─ Rotation: (0, 0, 0) - No rotation                    │
│ │  ├─ Scale: (1.6, 1.6, 1) - Scaled up 1.6x              │
│ │  └─ Parent: None (Root in scene)                        │
│ │                                                          │
│ ├─ SpriteRenderer (ID: 1151752933) ✨                     │
│ │  ├─ Sprite: guid: 89afa4e4df0f34a97855ade8a4175897     │
│ │  ├─ Sprite Size: 0.16 x 0.16 units                     │
│ │  ├─ Material: Default sprite material                   │
│ │  ├─ Sorting Order: 10 (drawn on top)                   │
│ │  ├─ Color: White (1,1,1,1)                             │
│ │  └─ Flip X/Y: No                                       │
│ │                                                          │
│ ├─ BoxCollider2D (ID: 1151752935) 📦                     │
│ │  ├─ Enabled: Yes                                        │
│ │  ├─ Is Trigger: NO (solid collider)                    │
│ │  ├─ Offset: (0, 0.75) - Moved up from center           │
│ │  ├─ Size: (2.2, 1.5) - Width x Height                 │
│ │  ├─ Density: 1                                         │
│ │  ├─ Edge Radius: 0 (sharp corners)                     │
│ │  └─ Used by: Rigidbody2D (collisions)                 │
│ │                                                          │
│ ├─ Rigidbody2D (ID: 1151752936) ⚙️                       │
│ │  ├─ Body Type: Dynamic (0)                              │
│ │  ├─ Mass: 1                                              │
│ │  ├─ Gravity Scale: 3 ⬇️ (strong gravity!)              │
│ │  ├─ Linear Damping: 0 (no air resistance)              │
│ │  ├─ Angular Damping: 0.05                              │
│ │  ├─ Simulated: Yes                                     │
│ │  ├─ Is Kinematic: No (respects forces)                │
│ │  ├─ Collision Detection: Continuous (1)                │
│ │  ├─ Constraints: Freeze Rotation Z                     │
│ │  ├─ Interpolation: Rigidbody2D                         │
│ │  ├─ Sleeping Mode: Start Asleep                        │
│ │  └─ Current Velocity: (variable in play mode)         │
│ │                                                          │
│ ├─ Animator (script required) 🎬                          │
│ │  ├─ Controller: (AnimatorController)                    │
│ │  ├─ Avatar: None (2D sprite, no humanoid)              │
│ │  └─ Parameters: IsRunning, IsGrounded (hashes)         │
│ │                                                          │
│ ├─ AudioSource (ID: 1151752938) 🔊                       │
│ │  ├─ Enabled: Yes                                        │
│ │  ├─ Play On Awake: No                                   │
│ │  ├─ Volume: 1.0                                         │
│ │  ├─ Pitch: 1.0                                          │
│ │  └─ Current Clip: Jump sound (set at runtime)          │
│ │                                                          │
│ └─ PlayerController (Script - ID: 1151752934) 🎮          │
│    ├─ Status: Enabled                                      │
│    ├─ Namespace: Linksaurus.Player                        │
│    ├─ Script: Assets/Scripts/Player/PlayerController.cs  │
│    │                                                      │
│    ├─ JUMP SETTINGS                                       │
│    │  ├─ Jump Force: 9.5                                 │
│    │  └─ Jump Sound: guid: 949a59cad73ed4ee3b...        │
│    │                                                      │
│    ├─ GROUND DETECTION ⚠️ KEY SETTINGS                  │
│    │  ├─ Ground Layer: Layer 6 (Ground)                 │
│    │  │  └─ m_Bits: 64 (binary: 0000001000000)         │
│    │  ├─ Ground Check Radius: 0.15                      │
│    │  └─ Ground Check Offset: (0, -0.6, 0) 🔴 ISSUE   │
│    │     └─ Check point in play: (-3, -4.6, 0)         │
│    │                                                      │
│    └─ INTERNAL TRACKING                                   │
│       ├─ _isGrounded: bool (calculated each frame)       │
│       ├─ _shieldActive: bool (PowerUp system)           │
│       └─ _rb, _animator, _collider, _audioSource: refs  │
│                                                          │
└─────────────────────────────────────────────────────────────┘
```

---

## 📏 Physics Geometry Visualization

### Side View (Y-axis)

```
     ↑ Y-axis
     │
     ├─ 0.5    ╔════════════════╗  Ground top (effective)
     │         ║                ║
     ├─ -2.0   ║ Camera View    ║
     │         ║                ║
     ├─ -2.5   ║  PLAYER ZONE   ║
     │  ┌────┐ ║    (visible)   ║
     │  │████│ ║                ║  Player collider
     │  │████│ ║                ║  (centered at Y=-2.5)
     │  │████│ ║                ║
     │  └────┘ ║                ║
     ├─ -4.0   ║────────────────║  Ground collider top
     │         ║                ║
     ├─ -4.5   ╠════════════════╣  Ground position (Y=-4.5)
     │         ║   GROUND       ║
     │         ║  COLLIDER      ║  Size: (1000, 5)
     │         ║  (active)      ║  Offset: (0, -2)
     │         ║                ║
     ├─ -9.0   ║════════════════║  Ground bottom
     │         ║                ║
     └─────────┴────────────────┴──→ X-axis

KEY POSITIONS:
  • Player Y position: -4
  • Player collider center: -4 + 0.75 = -2.25
  • Player collider top: -2.25 + 0.75 = -1.5
  • Player collider bottom: -2.25 - 0.75 = -3.0
  
  • Ground check point: -4 + (-0.6) = -4.6 🔴 PROBLEM!
  • Ground check sphere: Y from -4.75 to -4.45
  • Ground top surface: -4.0
  
  ❌ NO COLLISION: -4.75 to -4.45 is BELOW -4.0
```

---

## 🔍 Ground Detection Problem Illustration

```
CURRENT SETUP (BROKEN):
═══════════════════════════════════════════════════════════

Player at Y = -4

                                         Player Collider
                                         ┌──────────┐
                                         │          │ Center: (-3, -2.25)
                                         │  ████    │ Top: (-3, -1.5)
                                         │  ████    │ Bottom: (-3, -3.0)
                                         └──────────┘
                                              ↓
                                    Ground Check Offset
                                         (-0.6)
                                              ↓
                                         Check Sphere
                                         ◯ Radius: 0.15
                                         Center: (-3, -4.6)
                                         Range: -4.75 to -4.45

GROUND LAYER:
═════════════════════════════════════════════════════════════
Ground at Y = -4.5

                ┌──────────────────────────────────┐ Top: -4.0
                │         GROUND (Active)          │
                │     Size: (1000, 5)              │
                │     Offset: (0, -2)              │
                │                                  │
                │                                  │
                │                                  │
                └──────────────────────────────────┘ Bottom: -9.0

COLLISION CHECK:
═════════════════════════════════════════════════════════════
Ground top: -4.0
Check sphere center: -4.6
Check sphere extends: -4.75 to -4.45

        -4.0    ══════════ Ground Top
        -4.45   ◯◯◯◯◯◯◯◯◯ Check Sphere (radius 0.15)
        -4.6    ◯ Center
        -4.75   ◯◯◯◯◯◯◯◯◯

RESULT: ❌ NO OVERLAP = _isGrounded = false (ALWAYS!)

═════════════════════════════════════════════════════════════

PROPOSED FIX:
═════════════════════════════════════════════════════════════

Change _groundCheckOffset from (0, -0.6) to (0, -1.5)

Check sphere center: -4 + (-1.5) = -5.5
Check sphere extends: -5.65 to -5.35
(This would be too low still...)

OR better: Change to (0, -2.5)
Check sphere center: -4 + (-2.5) = -6.5
Check sphere extends: -6.65 to -6.35
(Still below ground...)

ACTUALLY CORRECT FIX:
Player collider bottom is at -3.0
Add margin of 0.5:
Check point should be at -3.0 - 0.5 = -3.5

_groundCheckOffset should be: (0, -3.5 - (-4)) = (0, 0.5)
OR based on player height:
_groundCheckOffset = (0, -(collider height/2) - margin)
                   = (0, -0.75 - 0.5)
                   = (0, -1.25)

Let's verify:
  Player Y: -4
  Check offset: -1.25
  Check point: -5.25
  Check sphere extends: -5.4 to -5.1
  Ground bottom: -9.0, Ground top: -4.0
  
  Result: ✅ OVERLAPS with ground (-5.4 to -5.1 is within -9.0 to -4.0)
```

---

## 🎯 The Core Issue Summary

| Item | Value | Status |
|------|-------|--------|
| Player Y Position | -4 | ✅ Correct |
| Player Collider Height | 1.5 | ✅ Correct |
| Player Collider Offset | +0.75 | ✅ Correct |
| Player Collider Effective Center | -2.25 | ✅ Derived correctly |
| Ground Layer Detection | Layer 6 | ✅ Correct |
| Ground Check Radius | 0.15 | ✅ Reasonable |
| **Ground Check Offset** | **-0.6** | **❌ TOO LOW** |
| Ground Check Position | -4.6 | **❌ BELOW GROUND** |
| Ground Top Surface | -4.0 | ✅ Correct |
| Rigidbody Gravity Scale | 3 | ✅ Correct |
| **Result** | **Never Grounded** | **🔴 BUG** |

---

## 🔧 Code Locations for Fix

### File: `Assets/Scripts/Player/PlayerController.cs`

**Lines 14-16** (Settings to adjust):
```csharp
[SerializeField] private float _groundCheckRadius = 0.15f;
[SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, -0.6, z: 0);  // ← CHANGE THIS
```

**Line 55** (Where it's used):
```csharp
_isGrounded = Physics2D.OverlapCircle(
    transform.position + _groundCheckOffset,  // Using offset here
    _groundCheckRadius,
    _groundLayer
);
```

**Line 97** (Gizmo visualization - for debugging):
```csharp
Gizmos.DrawWireSphere(transform.position + _groundCheckOffset, _groundCheckRadius);
// This will show the check sphere in Scene view - verify it overlaps Ground!
```

---

## 📊 Complete Component Checklist

### Requirements from Script Header
```csharp
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
```

| Component | Present | Correct |
|-----------|---------|---------|
| Rigidbody2D | ✅ Yes | ✅ Yes |
| BoxCollider2D | ✅ Yes | ✅ Yes |
| Animator | ✅ Yes (implicit) | ✅ Yes |
| AudioSource | ✅ Yes (bonus) | ✅ Yes |
| SpriteRenderer | ✅ Yes | ✅ Yes |

---

## 🎬 Animation States

The Animator uses these parameters (hashes):

```csharp
IsRunningHash = Animator.StringToHash("IsRunning")
IsGroundedHash = Animator.StringToHash("IsGrounded")
```

These are set in `Update()` method:
- `IsRunning`: Set based on `GameManager.Instance.CurrentState == GameState.Playing`
- `IsGrounded`: Set based on `Physics2D.OverlapCircle()` result

The dinosaur animation likely depends on these states for:
- Running animation when IsRunning = true
- Falling/jumping animation when IsGrounded = false
- Landing animation when transitioning to IsGrounded = true

---

## 📝 Debug Tips

To visualize the problem:

1. **In Unity, Enable Gizmo Drawing:**
   - Select Player GameObject
   - In Scene view, ensure Gizmos are enabled (top right toggle)
   - You should see a red wire sphere showing the ground check area

2. **Add Debug Logging:**
   ```csharp
   private void Update() {
       // ... existing code ...
       
       _isGrounded = Physics2D.OverlapCircle(...);
       
       // Add this temporarily:
       if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) {
           Debug.Log($"Grounded: {_isGrounded}, Position: {transform.position}, CheckPos: {transform.position + _groundCheckOffset}");
       }
   }
   ```

3. **Verify in Play Mode:**
   - Play the game
   - Open the Scene view
   - Look for the red sphere - it should touch the green ground collider
   - Click on Player - check Inspector for _isGrounded value

---

**Status:** 🔴 **ISSUE IDENTIFIED - Ready for Fix**


# THEGAMEIO RACING
## 3D Urban Circuit Racing Game | African Coastal Metropolis at Night

**THEGAMEIO RACING** is a high-performance 3D mobile racing game built in Unity (Universal Render Pipeline - URP) targeting Android and PC platforms. The visual direction features an African coastal metropolis at night, with restrained street lighting, authentic road infrastructure, original vehicle physics, responsive touch/keyboard controls, and a multiplayer-ready architecture.

---

## Quick Start & Testing

### 1. Opening in Unity
- **Unity Version:** Unity 6 (`6000.6.2f1`)
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0)
- **Active Scene:** Open `Assets/_Project/Scenes/VehiclePhysicsTest.unity`
- Hit **Play** in the Unity Editor.

### 2. Controls

| Action | PC / Editor Keyboard | Android Mobile Touch UI |
|---|---|---|
| **Steer Left** | `A` or `Left Arrow` | On-screen `<` button (bottom left) |
| **Steer Right** | `D` or `Right Arrow` | On-screen `>` button (bottom left) |
| **Accelerate** | `W` or `Up Arrow` | On-screen `GAS` button (bottom right) |
| **Brake / Reverse** | `S` or `Down Arrow` | On-screen `BRAKE` button (bottom right) |
| **Handbrake (Drift)** | `Space` | On-screen `DRIFT` button (middle right) |
| **Reset Vehicle** | `R` | On-screen `RESET` button (top right) |
| **Cycle Camera** | `C` | Toggle between Close Chase, Far Chase, and Bumper views |

---

## Project Structure

```
Assets/_Project/
├── Art/
│   ├── Materials/         # URP Lit asphalt, markings, barriers, emissive streetlights
│   ├── Textures/          # UI card previews, route concept art, hero references
│   └── Models/            # Original 3D meshes and models
├── Editor/
│   ├── BuildPipeline.cs   # Android and Windows build automation & player configuration
│   └── TestTrackSceneBuilder.cs # Editor utility to build and refresh test environments
├── Prefabs/
│   ├── Cars/              # Configured vehicle prefabs
│   ├── Environment/       # Modular city buildings, streetlights, barriers
│   ├── Tracks/            # Curbs, rumble strips, checkpoints
│   └── UI/                # Mobile on-screen driving HUD
├── Resources/
│   └── Vehicles/          # Runtime-loadable VehicleConfig ScriptableObjects
├── Scenes/
│   └── VehiclePhysicsTest.unity # Primary playable test track scene
├── ScriptableObjects/
│   ├── Vehicles/          # All 5 original car configs (Regular, Tuner, Speedo, Muscle, Rally)
│   ├── Routes/            # 4 Route definitions (Downtown, Waterfront, Market, Port)
│   └── RaceModes/         # Pool, Time Trial, and AI Race settings
└── Scripts/
    ├── Core/              # Game bootstrapper, state managers, pool logic
    ├── Vehicle/           # Rigidbody + WheelCollider vehicle physics controller & builder
    ├── Input/             # Decoupled IVehicleInput, Keyboard, and Mobile touch input reader
    ├── Camera/            # Third-person smooth chase camera with lookahead & collision avoidance
    ├── UI/                # Mobile HUD, digital speedometer, touch buttons
    ├── Race/              # Checkpoint validation, lap timing, finish & tie-break logic
    ├── AI/                # Deterministic waypoint racing line AI drivers
    └── Online/            # Clean service interfaces for player pools, fees, and leaderboards
```

---

## Vehicle Specifications & Configurations

The game includes five original car designs with tuned physics:

1. **Regular (Starter Balanced)**
   - *Role:* Accessible handling, predictable steering, balanced acceleration.
   - *Stats:* Top Speed: 215 km/h | Motor Torque: 1,900 Nm | Drivetrain: AWD
2. **Tuner / Drift**
   - *Role:* Street performance, high acceleration, tuned rear-wheel slip for drifting.
   - *Stats:* Top Speed: 240 km/h | Motor Torque: 2,400 Nm | Drivetrain: RWD
3. **Speedo Hypercar**
   - *Role:* High top speed, strong downforce, high aerodynamic stability.
   - *Stats:* Top Speed: 320 km/h | Motor Torque: 3,100 Nm | Drivetrain: AWD
4. **Classic Muscle**
   - *Role:* High torque, heavy chassis, strong straight-line punch.
   - *Stats:* Top Speed: 225 km/h | Motor Torque: 2,800 Nm | Drivetrain: RWD
5. **Street Rally**
   - *Role:* Maximum grip, responsive cornering, sharp turn-in.
   - *Stats:* Top Speed: 230 km/h | Motor Torque: 2,100 Nm | Drivetrain: AWD

---

## Build Instructions

### Android Build
1. In Unity Hub, ensure **Android Build Support** (with OpenJDK and Android SDK/NDK tools) is installed for Unity `6000.6.2f1`.
2. In the Unity Editor menu, select:
   - `Tools > THEGAMEIO > Configure Android Project Settings`
   - `Tools > THEGAMEIO > Build Android APK`
3. Output APK will be generated at `Builds/Android/THEGAMEIO_Racing.apk`.

### Standalone Windows Build (Instant Verification)
- In Unity Editor menu, select `Tools > THEGAMEIO > Build Windows Standalone (Fast Verification)`.
- Output executable will be generated at `Builds/Windows/THEGAMEIO_Racing.exe`.

---

## Asset Licensing & Attribution
- All car geometry, textures, scripts, and materials in this codebase are 100% original.
- Concept art and visual reference files in `Assets/_Project/Art/Textures/` are extracted directly from the supplied client reference presentation `thegameio_Racing_Game.ppt` for internal development guidance.

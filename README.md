# GAMEIUM
## 3D Urban Circuit Racing Game | African Coastal Metropolis at Night

**GAMEIUM** is a high-performance 3D mobile racing game built in Unity (Universal Render Pipeline - URP) targeting Android and PC platforms. The visual direction features an African coastal metropolis at night, with restrained street lighting, authentic road infrastructure, original vehicle physics, responsive touch/keyboard controls, full race loop, autonomous AI competitors, and a 3D showroom garage.

---

## Quick Start & Testing

### 1. Opening in Unity
- **Unity Version:** Unity 6 (`6000.6.2f1`)
- **Render Pipeline:** Universal Render Pipeline (URP 17.6.0)
- **Active Scene:** Open `Assets/_Project/Scenes/MainMenu.unity` or `Assets/_Project/Scenes/DowntownCircuit.unity`
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
| **Pause Race** | `Escape` or `P` | On-screen `PAUSE` button (top right) |
| **Cycle Camera** | `C` | Toggle between Close Chase, Far Chase, and Bumper views |

---

## Project Structure

```
Assets/_Project/
├── Art/
│   ├── Materials/         # URP Lit asphalt, markings, barriers, emissive streetlights
│   ├── Textures/          # UI card previews, route concept art, hero references, logos
│   └── Models/            # Original 3D meshes and models
├── Editor/
│   ├── BuildPipeline.cs   # Android build automation & player configuration
│   └── TestTrackSceneBuilder.cs # Editor utility to build and refresh test environments
├── Scenes/
│   ├── MainMenu.unity     # Title screen, 4-route selector, 3D showroom turntable
│   ├── DowntownCircuit.unity # Route 1: 1.6km closed-loop circuit with 12 CPs & 24 AI waypoints
│   ├── SampleStreet_GateA.unity # Gate A: 120m representative visual avenue sprint race
│   └── VehiclePhysicsTest.unity # Vehicle physics calibration track
├── ScriptableObjects/
│   ├── Vehicles/          # All 5 original car configs (Regular, Tuner, Speedo, Muscle, Rally)
│   └── VehicleDatabase.asset # Canonical database holding all vehicle configs
└── Scripts/
    ├── Core/              # Game bootstrapper, state managers
    ├── Vehicle/           # Rigidbody + WheelCollider vehicle physics controller & builder
    ├── Input/             # Decoupled IVehicleInput, Keyboard, and Mobile touch input reader
    ├── Camera/            # Third-person smooth chase camera with lookahead & collision avoidance
    ├── UI/                # MainMenuManager, HUDController, RaceCountdownUI, PauseMenuUI, RaceResultsUI
    ├── Race/              # RaceManager, CheckpointManager, Checkpoint anti-cheat logic
    ├── AI/                # AIRacingController deterministic waypoint AI with obstacle avoidance
    └── Audio/             # ProceduralAudio 100% license-free engine sound & sound effects synthesizer
```

---

## Vehicle Specifications & Configurations

The game includes five original car designs with tuned physics:

1. **Cheetah GT (Starter Balanced)**
   - *Role:* Accessible handling, predictable steering, balanced acceleration.
   - *Stats:* Top Speed: 220 km/h | Motor Torque: 1,950 Nm | Drivetrain: AWD
2. **Drift King (Tuner Drift)**
   - *Role:* Street performance, high acceleration, tuned rear-wheel slip for drifting.
   - *Stats:* Top Speed: 240 km/h | Motor Torque: 2,400 Nm | Drivetrain: RWD
3. **Apex Hypercar (Hypercar Speed)**
   - *Role:* High top speed, strong downforce, high aerodynamic stability.
   - *Stats:* Top Speed: 320 km/h | Motor Torque: 3,100 Nm | Drivetrain: AWD
4. **Marauder Muscle (Classic Muscle)**
   - *Role:* High torque, heavy chassis, strong straight-line punch.
   - *Stats:* Top Speed: 225 km/h | Motor Torque: 2,800 Nm | Drivetrain: RWD
5. **Dune Raider (Street Rally)**
   - *Role:* Raised suspension, maximum grip, responsive cornering, sharp turn-in.
   - *Stats:* Top Speed: 230 km/h | Motor Torque: 2,100 Nm | Drivetrain: AWD

---

## Build Instructions

### Android Build
1. In Unity Hub, ensure **Android Build Support** (with OpenJDK and Android SDK/NDK tools) is installed for Unity `6000.6.2f1`.
2. In the Unity Editor menu, select:
   - `Tools > GAMEIUM > Configure Android Project Settings`
   - `Tools > GAMEIUM > Build Android APK`
3. Output APK will be generated at `Builds/Android/GAMEIUM.apk`.

---

## Asset Licensing & Attribution
- All car geometry, textures, scripts, and materials in this codebase are 100% original.
- Environmental PBR textures (Asphalt, Pavement, Plaster, Metal) obtained from Poly Haven under CC0 1.0 Universal license.
- Procedural audio synthesizer generates all combustion engine, countdown, and chime sound effects at runtime without external audio dependencies.

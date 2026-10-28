# GAMEIUM — 3RD-PARTY ASSET MANIFEST

This document records all third-party external assets integrated into **GAMEIUM** in compliance with the project's free asset-first policy, licensing requirements, and mobile performance constraints.

---

## 1. PBR Surface Textures (Poly Haven)

### Asset 1: Asphalt 02
- **Source URL:** https://polyhaven.com/a/asphalt_02
- **Author / Publisher:** Rob Tuytel
- **License:** CC0 1.0 Universal (Public Domain Dedication) — Free for commercial and non-commercial use with no attribution required.
- **Date Obtained:** 2026-10-28
- **Files Used:**
  - `Assets/_Project/Art/Textures/PBR/Asphalt/asphalt_02_diff_1k.jpg` (Albedo map)
  - `Assets/_Project/Art/Textures/PBR/Asphalt/asphalt_02_nor_1k.jpg` (Normal map, OpenGL format)
  - `Assets/_Project/Art/Textures/PBR/Asphalt/asphalt_02_rough_1k.jpg` (Roughness map)
- **Modifications Made:**
  - Converted to 1024x1024 (1k) optimized resolution for mobile texture memory.
  - Linked to URP Lit material `M_Road_Asphalt.mat` with calibrated smoothness, UV tiling (0.1, 0.1), and subtle dark wetness response matching the Downtown Circuit nighttime road reference.
- **Mobile Performance Considerations:**
  - 1k JPG format compresses down to ASTC / ETC2 in Unity Android builds (~680 KB VRAM per map).
  - Trilinear filtering with anisotropic level 1 to prevent texture aliasing at shallow grazing camera angles.

---

### Asset 2: Concrete Pavement 03
- **Source URL:** https://polyhaven.com/a/concrete_pavement_03
- **Author / Publisher:** Charlotte Baglioni
- **License:** CC0 1.0 Universal (Public Domain Dedication)
- **Date Obtained:** 2026-10-28
- **Files Used:**
  - `Assets/_Project/Art/Textures/PBR/Pavement/concrete_pavement_03_diff_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/Pavement/concrete_pavement_03_nor_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/Pavement/concrete_pavement_03_rough_1k.jpg`
- **Modifications Made:**
  - Standardized to 1k resolution.
  - Applied to `M_Sidewalk_Pavers.mat` and `M_Curbs.mat` for realistic roadside pedestrian walkways, gutters, and curb blocks.
- **Mobile Performance Considerations:**
  - Reused across all sidewalks and pedestrian plazas, maximizing batching and minimizing draw calls.

---

### Asset 3: Concrete Tile Facade
- **Source URL:** https://polyhaven.com/a/concrete_tile_facade
- **Author / Publisher:** Charlotte Baglioni
- **License:** CC0 1.0 Universal (Public Domain Dedication)
- **Date Obtained:** 2026-10-28
- **Files Used:**
  - `Assets/_Project/Art/Textures/PBR/BuildingFacade/concrete_tile_facade_diff_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/BuildingFacade/concrete_tile_facade_nor_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/BuildingFacade/concrete_tile_facade_rough_1k.jpg`
- **Modifications Made:**
  - Applied to `M_Building_CommercialFacade.mat` for modern mid-rise office buildings, commercial towers, and storefront upper floors.
- **Mobile Performance Considerations:**
  - Shared across multiple building types through UV scaling; uses simple box and LOD geometries.

---

### Asset 4: Painted Plaster Wall
- **Source URL:** https://polyhaven.com/a/painted_plaster_wall
- **Author / Publisher:** Amal Kumar
- **License:** CC0 1.0 Universal (Public Domain Dedication)
- **Date Obtained:** 2026-10-28
- **Files Used:**
  - `Assets/_Project/Art/Textures/PBR/PlasterWall/painted_plaster_wall_diff_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/PlasterWall/painted_plaster_wall_nor_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/PlasterWall/painted_plaster_wall_rough_1k.jpg`
- **Modifications Made:**
  - Applied to `M_Building_ResidentialPlaster.mat` for low-rise houses, boundary walls, and storefront surrounds.
- **Mobile Performance Considerations:**
  - Compact memory footprint; supports material property block vertex tinting.

---

### Asset 5: Metal Plate
- **Source URL:** https://polyhaven.com/a/metal_plate
- **Author / Publisher:** Rob Tuytel
- **License:** CC0 1.0 Universal (Public Domain Dedication)
- **Date Obtained:** 2026-10-28
- **Files Used:**
  - `Assets/_Project/Art/Textures/PBR/Metal/metal_plate_diff_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/Metal/metal_plate_nor_1k.jpg`
  - `Assets/_Project/Art/Textures/PBR/Metal/metal_plate_rough_1k.jpg`
- **Modifications Made:**
  - Applied to `M_Barrier_Armco.mat` and `M_Streetlight_Pole.mat` for galvanized steel crash barriers and street lighting posts.
- **Mobile Performance Considerations:**
  - Smoothness and metallic channels combined for low-cost shader evaluation.

---

## 2. Reference-Derived Visual Textures (PowerPoint Controlling Reference)

The following environment textures are derived directly from the supplied visual design reference (`thegameio_Racing_Game.ppt`, Slide 3 `img_03.jpg` and Slide 7 `img_05.jpg`):

### Asset 6: African Cultural Wall Mural (`tex_mural_african_culture.png`)
- **Visual Source:** Slide 3 (`img_03.jpg`) and Slide 7 (`img_05.jpg`), African City / Market District environment concept.
- **Subject:** Painted portrait of an African woman wearing an ochre/gold traditional gele headwrap, drop hoop earrings, and traditional patterned attire against an indigo-blue sky.
- **File Location:** `Assets/_Project/Art/Textures/Environment/tex_mural_african_culture.png` (512x512 PNG)
- **Usage:** Applied to `M_Mural_AfricanCulture.mat` on the flank wall of the multi-story commercial building at Z = 68m, illuminated by ground spotlights.
- **Classification:** Project Reference Derivative (Direct crop & upscale from supplied slide artwork).

### Asset 7: "OUR CITY OUR STRENGTH" Urban Billboard (`tex_billboard_our_city.png`)
- **Visual Source:** Slide 3 (`img_03.jpg`), highway gantry billboard artwork.
- **Subject:** African continent geometric silhouette in warm copper/orange with hatched elevation lines, bold white title "OUR CITY", gold sub-title "OUR STRENGTH", and circuit championship subtext.
- **File Location:** `Assets/_Project/Art/Textures/Environment/tex_billboard_our_city.png` (1024x512 PNG)
- **Usage:** Applied to `M_Billboard_CityStrength.mat` on the illuminated steel billboard truss structure at Z = 14m.
- **Classification:** Project Reference Derivative (Vector-accurate graphic recreation of supplied slide billboard).

### Asset 8: Vertical Neon "AFRICA" Blade Sign (`tex_sign_neon_africa.png`)
- **Visual Source:** Slide 3 (`img_03.jpg`), vertical illuminated neon blade sign adjacent to the cultural mural.
- **Subject:** Vertical typography "A-F-R-I-C-A" in radiant magenta/violet neon glow with hot white core.
- **File Location:** `Assets/_Project/Art/Textures/Environment/tex_sign_neon_africa.png` (160x512 PNG)
- **Usage:** Applied to `M_Sign_Neon_Africa.mat` on vertical illuminated blade sign attached to avenue commercial facade at Z = 58m.
- **Classification:** Project Reference Derivative.

### Asset 9: Track Safety Chevron Barrier (`tex_barrier_chevron.png`)
- **Visual Source:** Slide 3 (`img_03.jpg`) and Slide 7 (`img_05.jpg`), trackside barrier run-off zones.
- **Subject:** Heavy industrial yellow and black directional chevron arrows (`> > > >`).
- **File Location:** `Assets/_Project/Art/Textures/Environment/tex_barrier_chevron.png` (512x128 PNG)
- **Usage:** Applied to `M_Barrier_Chevron.mat` on steel crash barriers lining the 120m street section.
- **Classification:** Project Reference Derivative.

---

## 3. Procedural & In-House Generated Assets

All gameplay code, physics models, procedural geometry builders, and contextual storefront signage are original in-house work developed specifically for GAMEIUM:

- **UI & Brand Assets:**
  - `tex_gameium_logo.png` (1024x512 transparent PNG) — Standalone UI title brand crest with electric cyan and warm gold gradient styling.
  - `tex_gameium_app_icon.png` (512x512 PNG) — Android launcher squircle app icon with stylized speed crest and GAMEIUM typography.

- **Storefront Signage:**
  - `tex_sign_storefront_01.png` ("KILIMA ELECTRONICS & MOBILE MONEY PAY") — Grounded local African retail technology/service store.
  - `tex_sign_storefront_02.png` ("COASTAL BREEZE BISTRO & CAFE") — Grounded coastal bistro/cafe.
  - `tex_awning_fabric.png` — Red and cream canvas striped fabric for market stalls and shop canopies.
  - `tex_building_windows.png` — Emissive matrix of selectively lit warm gold/amber and unlit office/apartment windows.
  - `tex_billboard_thegameio.png` — Official GAMEIUM tournament billboard per Slide 1 & 5.
- **Vehicle Physics & Controller:** `VehicleController.cs`, `VehicleConfig.cs`, `IVehicleInput.cs`, `VehicleDatabase.cs`.
- **Procedural 3D Vehicles & Silhouettes:** `CarModelBuilder.cs` (5 distinct silhouettes: Hypercar, Muscle, Tuner, Rally, Balanced + real-time headlights and taillights).
- **Procedural Audio Synthesizer:** `ProceduralAudio.cs` (100% license-free mathematical audio generation: combustion engine RPM loop, 440/880Hz countdown beeps, D-major arpeggio checkpoint chime, and tire skid noise).
- **Race Progression & Timing:** `RaceManager.cs`, `CheckpointManager.cs`, `Checkpoint.cs`.
- **AI Racing Opponents:** `AIRacingController.cs`, `WaypointNode.cs`.
- **Camera & UI Systems:** `VehicleCameraController.cs`, `HUDController.cs`, `MobileDrivingUI.cs`, `RaceCountdownUI.cs`, `PauseMenuUI.cs`, `RaceResultsUI.cs`.
- **Frontend & Showroom:** `MainMenuManager.cs`, `MainMenu.unity`.
- **Track & Environment Builders:** `StreetPropsBuilder.cs`, `GateABootstrap.cs`, `DowntownCircuitBootstrap.cs`, `DowntownCircuit.unity`.

---

## 4. License Verification Checklist

- [x] All external texture assets are verified CC0 1.0 Universal (public domain).
- [x] Zero paid assets or subscription dependencies introduced.
- [x] Source URLs, author credits, and licenses documented for all Poly Haven PBR textures.
- [x] Graphic assets faithfully derived from controlling PowerPoint reference slides (`thegameio_Racing_Game.ppt`).
- [x] All textures configured for mobile GPU memory constraints (< 1.5 MB VRAM per map).
- [x] 157 `.meta` files verified with **0 duplicate GUIDs**.



---

## 5. Web Frontend Assets & Libraries

| Asset / Library | Origin / Provider | License | Purpose in GAMEIUM |
|---|---|---|---|
| **React 19 & React-DOM** | Meta Open Source | MIT | Core component framework for public web app |
| **Vite 8** | VoidZero | MIT | Fast ESM development server and Rollup production bundler |
| **Lucide React** | Lucide Contributors | ISC | High-craft technical and telemetry SVG icon suite |
| **Canvas-Confetti** | Kiril Vatev | MIT | Post-race victory celebration particle rendering |
| **Orbitron & Syne Fonts** | Matt McInerney / Lucas Le Bihan | SIL Open Font License 1.1 | Display headings & neon brand typography |
| **JetBrains Mono** | JetBrains | SIL Open Font License 1.1 | Speedometer, tachometer, lap timers & API schemas |
| **Plus Jakarta Sans** | Tokotype | SIL Open Font License 1.1 | Clean UI body copy and dossier specifications |
| **Web Audio Synthesizer** | In-House (`audioSynth.ts`) | MIT / Project Proprietary | Real-time procedural engine revs, screech, and chimes |

# THEGAMEIO RACING — 3RD-PARTY ASSET MANIFEST

This document records all third-party external assets integrated into **THEGAMEIO RACING** in compliance with the project's free asset-first policy, licensing requirements, and mobile performance constraints.

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

## 2. In-House / Procedural Assets (Original Game Implementations)

All gameplay scripts, procedural car generator, vehicle physics, race managers, camera controllers, track builders, and custom shaders/materials are original work developed specifically for THEGAMEIO RACING.

- **Vehicle Physics & Controller:** `VehicleController.cs`, `VehicleConfig.cs`, `IVehicleInput.cs`
- **Procedural Car Mesh Generator:** `CarModelBuilder.cs` (Original 3D chassis, cabin, splitters, diffusers, spoilers, wheels, and LED light clusters)
- **Camera Controller:** `VehicleCameraController.cs`
- **UI & HUD System:** `HUDController.cs`, `MobileDrivingUI.cs`
- **Track & Environment Builders:** `TestTrackSceneBuilder.cs`, `GateASampleSceneBuilder.cs`
- **Branding Assets:** Original *thegameio* brand billboards, start arches, chevron trackside barriers, and speed boards based on the reference presentation.

---

## 3. License Verification Checklist

- [x] All external texture assets are verified CC0 1.0 Universal (public domain).
- [x] Zero paid assets or subscription dependencies introduced.
- [x] Source URLs, author credits, and licenses documented.
- [x] All normal maps configured with `textureType: 1` and sRGB disabled where appropriate.
- [x] Asset sizes verified for mobile RAM/VRAM constraints (< 1.5 MB per 1k map).
- [x] No duplicate GUIDs or broken `.meta` references.

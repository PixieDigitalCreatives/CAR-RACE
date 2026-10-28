#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThegameioRacing.Vehicle;
using ThegameioRacing.Camera;
using ThegameioRacing.Input;
using ThegameioRacing.UI;
using ThegameioRacing.Race;
using ThegameioRacing.AI;

namespace ThegameioRacing.Environment
{
    /// <summary>
    /// Master environment and race director for Route 3: Market District.
    /// Builds a closed-loop ~1.4km urban market racing circuit featuring:
    /// - Sector 1: Market Central Avenue (Dense shopfronts, striped fabric canopies, warm lighting)
    /// - Sector 2: Spice Bazaar Chicane (Technical 90-degree left and right turns)
    /// - Sector 3: Artisan & Fabric Alleyway (Narrow street rhythm, close-quarters racing)
    /// - Sector 4: Plaza Return (Sweeping turn back into Market Central)
    /// Complete with 12 sequential checkpoints, 22-node AI waypoint loop,
    /// 4-car starting grid, full in-race HUD, countdown, pause, and podium results.
    /// </summary>
    [ExecuteAlways]
    public class MarketDistrictBootstrap : MonoBehaviour
    {
        [Header("Race Configuration")]
        [SerializeField] private int totalLaps = 2;
        [SerializeField] private VehicleConfig playerConfig;
        [SerializeField] private VehicleDatabase vehicleDatabase;
        [SerializeField] private bool autoBuildOnStart = true;

        [Header("Track Geometry Settings")]
        [SerializeField] private float mainRoadWidth = 12f; // Tighter market street width
        [SerializeField] private float curbHeight = 0.18f;
        [SerializeField] private float sidewalkWidth = 3.2f;

        [Header("Material Cache")]
        [SerializeField] private Material matAsphalt;
        [SerializeField] private Material matSidewalk;
        [SerializeField] private Material matCurb;
        [SerializeField] private Material matCommercialFacade;
        [SerializeField] private Material matPlaster;
        [SerializeField] private Material matMetal;
        [SerializeField] private Material matMarkingWhite;
        [SerializeField] private Material matMarkingYellow;
        [SerializeField] private Material matBarrierChevron;
        [SerializeField] private Material matSignKilima;
        [SerializeField] private Material matSignCafe;
        [SerializeField] private Material matAwning;
        [SerializeField] private Material matWindows;
        [SerializeField] private Material matMural;
        [SerializeField] private Material matBillboardGameium;
        [SerializeField] private Material matStreetlightBulb;
        [SerializeField] private Material matGlass;
        [SerializeField] private Material matWood;

        private GameObject playerCar;

        private void Awake()
        {
            if (!autoBuildOnStart) return;

            LoadOrFindMaterials();
            SetupNightAtmosphere();

            if (transform.Find("CircuitGeometry") == null)
            {
                BuildCircuitGeometry();
            }

            if (Application.isPlaying)
            {
                EnsureEventSystem();
                ResolvePlayerVehicleConfig();
                SpawnRaceGridAndSystems();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private void ResolvePlayerVehicleConfig()
        {
            string selectedId = PlayerPrefs.GetString("SelectedVehicleId", "");
            if (!string.IsNullOrEmpty(selectedId) && vehicleDatabase != null)
            {
                playerConfig = vehicleDatabase.GetVehicleById(selectedId);
            }

            if (playerConfig == null && vehicleDatabase != null)
            {
                playerConfig = vehicleDatabase.GetDefaultVehicle();
            }

            if (playerConfig == null)
            {
#if UNITY_EDITOR
                playerConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleConfig>(
                    "Assets/_Project/ScriptableObjects/Vehicles/VehicleConfig_Regular.asset");
                if (vehicleDatabase == null)
                {
                    vehicleDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleDatabase>(
                        "Assets/_Project/ScriptableObjects/Vehicles/VehicleDatabase.asset");
                }
#endif
                if (playerConfig == null)
                {
                    playerConfig = ScriptableObject.CreateInstance<VehicleConfig>();
                    playerConfig.displayName = "Cheetah GT";
                    playerConfig.role = VehicleRole.StarterBalanced;
                    playerConfig.topSpeedKmh = 220f;
                    playerConfig.motorTorque = 1950f;
                    playerConfig.brakeTorque = 4200f;
                    playerConfig.primaryColor = new Color(0.12f, 0.45f, 0.95f);
                    playerConfig.accentColor = new Color(0.98f, 0.55f, 0.12f);
                }
            }
        }

        private void LoadOrFindMaterials()
        {
#if UNITY_EDITOR
            if (matAsphalt == null)
                matAsphalt = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Road_Asphalt.mat");
            if (matSidewalk == null)
                matSidewalk = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Sidewalk_Pavers.mat");
            if (matCurb == null)
                matCurb = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Curbs.mat");
            if (matCommercialFacade == null)
                matCommercialFacade = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Building_CommercialFacade.mat");
            if (matPlaster == null)
                matPlaster = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Building_ResidentialPlaster.mat");
            if (matMetal == null)
                matMetal = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Metal_Plate.mat");
            if (matMarkingWhite == null)
                matMarkingWhite = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Road_Marking_White.mat");
            if (matMarkingYellow == null)
                matMarkingYellow = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Road_Marking_Yellow.mat");
            if (matBarrierChevron == null)
                matBarrierChevron = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Barrier_Chevron.mat");
            if (matSignKilima == null)
                matSignKilima = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Sign_Kilima.mat");
            if (matSignCafe == null)
                matSignCafe = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Sign_Cafe.mat");
            if (matAwning == null)
                matAwning = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Awning_Fabric.mat");
            if (matWindows == null)
                matWindows = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Building_Windows.mat");
            if (matMural == null)
                matMural = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Mural_AfricanCulture.mat");
            if (matBillboardGameium == null)
                matBillboardGameium = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Billboard_Thegameio.mat");
            if (matStreetlightBulb == null)
                matStreetlightBulb = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Streetlight_Bulb.mat");
            if (matGlass == null)
                matGlass = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Store_Glass.mat");
#endif
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (matAsphalt == null) matAsphalt = CreateFallbackMat(litShader, new Color(0.18f, 0.18f, 0.22f), 0.35f, 0.05f);
            if (matSidewalk == null) matSidewalk = CreateFallbackMat(litShader, new Color(0.65f, 0.65f, 0.68f), 0.2f, 0.0f);
            if (matCurb == null) matCurb = CreateFallbackMat(litShader, new Color(0.75f, 0.75f, 0.75f), 0.25f, 0.0f);
            if (matCommercialFacade == null) matCommercialFacade = CreateFallbackMat(litShader, new Color(0.45f, 0.48f, 0.52f), 0.25f, 0.05f);
            if (matPlaster == null) matPlaster = CreateFallbackMat(litShader, new Color(0.82f, 0.78f, 0.72f), 0.18f, 0.0f);
            if (matMetal == null) matMetal = CreateFallbackMat(litShader, new Color(0.35f, 0.35f, 0.38f), 0.55f, 0.75f);
            if (matMarkingWhite == null) matMarkingWhite = CreateFallbackMat(litShader, new Color(0.95f, 0.95f, 0.98f), 0.3f, 0.0f);
            if (matMarkingYellow == null) matMarkingYellow = CreateFallbackMat(litShader, new Color(0.96f, 0.78f, 0.12f), 0.3f, 0.0f);
            if (matBarrierChevron == null) matBarrierChevron = CreateFallbackMat(litShader, new Color(0.95f, 0.75f, 0.1f), 0.35f, 0.1f);
            if (matSignKilima == null) matSignKilima = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.75f, 0.35f), new Color(2.4f, 1.8f, 1.1f));
            if (matSignCafe == null) matSignCafe = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.65f, 0.25f), new Color(2.5f, 1.6f, 0.9f));
            if (matAwning == null) matAwning = CreateFallbackMat(litShader, new Color(0.85f, 0.25f, 0.18f), 0.15f, 0.0f);
            if (matWindows == null) matWindows = CreateFallbackEmissiveMat(litShader, new Color(0.95f, 0.85f, 0.55f), new Color(1.8f, 1.4f, 0.9f));
            if (matMural == null) matMural = CreateFallbackMat(litShader, new Color(0.65f, 0.35f, 0.25f), 0.25f, 0.05f);
            if (matBillboardGameium == null) matBillboardGameium = CreateFallbackEmissiveMat(litShader, new Color(0.12f, 0.15f, 0.22f), new Color(1.3f, 1.3f, 1.3f));
            if (matStreetlightBulb == null) matStreetlightBulb = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.82f, 0.52f), new Color(4.5f, 3.2f, 1.5f));
            if (matGlass == null) matGlass = CreateFallbackMat(litShader, new Color(0.12f, 0.18f, 0.25f, 0.85f), 0.92f, 0.1f);
            if (matWood == null) matWood = CreateFallbackMat(litShader, new Color(0.45f, 0.32f, 0.20f), 0.15f, 0.0f);
        }

        private Material CreateFallbackMat(Shader shader, Color col, float smooth, float metal)
        {
            Material m = new Material(shader);
            m.SetColor("_BaseColor", col);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            return m;
        }

        private Material CreateFallbackEmissiveMat(Shader shader, Color baseCol, Color emCol)
        {
            Material m = new Material(shader);
            m.SetColor("_BaseColor", baseCol);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emCol);
            return m;
        }

        private void SetupNightAtmosphere()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.06f, 0.08f, 0.14f);     // Warm market twilight
            RenderSettings.ambientEquatorColor = new Color(0.10f, 0.11f, 0.16f); // Amber haze
            RenderSettings.ambientGroundColor = new Color(0.04f, 0.03f, 0.03f);  // Warm pavement

            if (GameObject.Find("Market_Moonlight") == null && transform.Find("Market_Moonlight") == null)
            {
                GameObject moonObj = new GameObject("Market_Moonlight");
                moonObj.transform.SetParent(transform, false);
                moonObj.transform.rotation = Quaternion.Euler(60f, -40f, 0f);

                Light moon = moonObj.AddComponent<Light>();
                moon.type = LightType.Directional;
                moon.color = new Color(0.70f, 0.72f, 0.82f);
                moon.intensity = 0.18f;
                moon.shadows = LightShadows.Soft;
            }
        }

        #region Circuit Geometry Construction

        private void BuildCircuitGeometry()
        {
            GameObject circuitRoot = new GameObject("CircuitGeometry");
            circuitRoot.transform.SetParent(transform, false);

            // Route 3: Market District Track Loop (~1.4 km technical layout)
            Vector3[] circuitTrackPoints = new Vector3[]
            {
                // Sector 1: Market Central Avenue
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 100f),
                new Vector3(0f, 0f, 200f),

                // Sector 2: Spice Bazaar Chicane (Sharp right, then left)
                new Vector3(25f, 0f, 260f),
                new Vector3(80f, 0f, 290f),
                new Vector3(150f, 0f, 280f),
                new Vector3(210f, 0f, 230f),

                // Sector 3: Artisan Alleyway (Southbound technical turns)
                new Vector3(230f, 0f, 150f),
                new Vector3(190f, 0f, 80f),
                new Vector3(220f, 0f, 10f),

                // Sector 4: Plaza Return
                new Vector3(180f, 0f, -40f),
                new Vector3(90f, 0f, -40f)
            };

            for (int i = 0; i < circuitTrackPoints.Length; i++)
            {
                Vector3 pA = circuitTrackPoints[i];
                Vector3 pB = circuitTrackPoints[(i + 1) % circuitTrackPoints.Length];
                BuildRoadSegment(circuitRoot.transform, pA, pB, mainRoadWidth, i);
            }

            // Finish Line Gantry at Z = 20m
            BuildFinishLineGantry(circuitRoot.transform, new Vector3(0f, 0f, 20f), Quaternion.identity, mainRoadWidth);

            // Market District Props & Stalls
            SpawnMarketLandmarks(circuitRoot.transform);
        }

        private void BuildRoadSegment(Transform parent, Vector3 pA, Vector3 pB, float width, int segmentIndex)
        {
            Vector3 delta = pB - pA;
            float length = delta.magnitude;
            if (length < 0.1f) return;

            Vector3 center = (pA + pB) * 0.5f;
            Quaternion rot = Quaternion.LookRotation(delta);

            GameObject seg = new GameObject($"RoadSegment_{segmentIndex:00}");
            seg.transform.SetParent(parent, false);
            seg.transform.position = center;
            seg.transform.rotation = rot;

            // Asphalt Driving Surface
            GameObject asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asphalt.name = "Asphalt";
            asphalt.transform.SetParent(seg.transform, false);
            asphalt.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            asphalt.transform.localScale = new Vector3(width, 0.1f, length);
            if (matAsphalt != null) asphalt.GetComponent<Renderer>().sharedMaterial = matAsphalt;

            // Curbs & Sidewalks
            float halfWidth = width * 0.5f;
            CreateSegmentPart("Curb_L", seg.transform, new Vector3(-(halfWidth + 0.15f), curbHeight * 0.5f, 0f), new Vector3(0.3f, curbHeight, length), matCurb);
            CreateSegmentPart("Curb_R", seg.transform, new Vector3((halfWidth + 0.15f), curbHeight * 0.5f, 0f), new Vector3(0.3f, curbHeight, length), matCurb);
            float swX = halfWidth + 0.3f + sidewalkWidth * 0.5f;
            CreateSegmentPart("Sidewalk_L", seg.transform, new Vector3(-swX, (curbHeight - 0.02f) * 0.5f, 0f), new Vector3(sidewalkWidth, curbHeight - 0.02f, length), matSidewalk);
            CreateSegmentPart("Sidewalk_R", seg.transform, new Vector3(swX, (curbHeight - 0.02f) * 0.5f, 0f), new Vector3(sidewalkWidth, curbHeight - 0.02f, length), matSidewalk);

            // Center Yellow Line
            GameObject yellowLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            yellowLine.name = "YellowLine";
            yellowLine.transform.SetParent(seg.transform, false);
            yellowLine.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            yellowLine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            yellowLine.transform.localScale = new Vector3(0.24f, length, 1f);
            if (matMarkingYellow != null) yellowLine.GetComponent<Renderer>().sharedMaterial = matMarkingYellow;
            Collider yCol = yellowLine.GetComponent<Collider>();
            if (yCol != null) DestroyImmediate(yCol);

            // Warm Streetlights
            StreetPropsBuilder.CreateStreetlight(seg.transform, new Vector3(-(halfWidth + 0.35f), curbHeight, 0f), Quaternion.identity, matMetal, matStreetlightBulb);
            StreetPropsBuilder.CreateStreetlight(seg.transform, new Vector3(halfWidth + 0.35f, curbHeight, 0f), Quaternion.Euler(0f, 180f, 0f), matMetal, matStreetlightBulb);

            // Crash Safety Barriers
            StreetPropsBuilder.CreateSafetyBarrier(seg.transform, new Vector3(-(halfWidth + 0.15f), curbHeight, 0f), Quaternion.identity, matMetal, matBarrierChevron, length * 0.95f);
            StreetPropsBuilder.CreateSafetyBarrier(seg.transform, new Vector3(halfWidth + 0.15f, curbHeight, 0f), Quaternion.identity, matMetal, matBarrierChevron, length * 0.95f);
        }

        private void CreateSegmentPart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            if (mat != null) obj.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void BuildFinishLineGantry(Transform parent, Vector3 worldPos, Quaternion worldRot, float width)
        {
            GameObject gantryRoot = new GameObject("FinishLine_Gantry");
            gantryRoot.transform.SetParent(parent, false);
            gantryRoot.transform.position = worldPos;
            gantryRoot.transform.rotation = worldRot;

            float halfSpan = (width * 0.5f) + 0.8f;
            float gantryHeight = 6.4f;

            CreateSegmentPart("TrussTower_L", gantryRoot.transform, new Vector3(-halfSpan, gantryHeight * 0.5f, 0f), new Vector3(0.7f, gantryHeight, 0.7f), matMetal);
            CreateSegmentPart("TrussTower_R", gantryRoot.transform, new Vector3(halfSpan, gantryHeight * 0.5f, 0f), new Vector3(0.7f, gantryHeight, 0.7f), matMetal);
            CreateSegmentPart("TrussBeam", gantryRoot.transform, new Vector3(0f, gantryHeight, 0f), new Vector3(halfSpan * 2f + 0.8f, 0.8f, 0.8f), matMetal);
            CreateSegmentPart("FinishBanner", gantryRoot.transform, new Vector3(0f, gantryHeight - 0.75f, 0f), new Vector3(width * 0.85f, 1.1f, 0.15f), matBarrierChevron);
        }

        private void SpawnMarketLandmarks(Transform parent)
        {
            GameObject landmarks = new GameObject("MarketLandmarks");
            landmarks.transform.SetParent(parent, false);

            // Market Stalls along Sidewalks
            for (float mz = 30f; mz <= 180f; mz += 35f)
            {
                StreetPropsBuilder.CreateMarketStall(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 2.5f, 0.18f, mz), Quaternion.Euler(0f, 90f, 0f), matWood, matAwning, $"Stall_West_{mz:F0}");
                StreetPropsBuilder.CreateMarketStall(landmarks.transform, new Vector3(mainRoadWidth * 0.5f + 2.5f, 0.18f, mz + 15f), Quaternion.Euler(0f, -90f, 0f), matWood, matAwning, $"Stall_East_{mz:F0}");
            }

            // Storefronts in Market Central
            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(-15f, 0f, 65f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignKilima, matAwning, matGlass, matMetal, "Market_Electronics");

            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(15f, 0f, 110f), Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Market_Spices");

            // African Cultural Wall Mural on Corner Building
            StreetPropsBuilder.CreateCommercialBuilding(landmarks.transform, new Vector3(85f, 0f, 315f), Quaternion.Euler(0f, 180f, 0f),
                matCommercialFacade, matWindows, matMural, matMetal, true, 20f, 24f, 16f);

            // GAMEIUM Championship Billboard at Chicane Apex
            StreetPropsBuilder.CreateUrbanBillboard(landmarks.transform, new Vector3(160f, 0f, 305f), Quaternion.Euler(0f, 175f, 0f),
                matMetal, matBillboardGameium, 10f, 5f, 6.5f);

            // Spectators along Market Sidewalks
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 1.8f, 0.18f, 50f), Quaternion.Euler(0f, 85f, 0f), 5);
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(mainRoadWidth * 0.5f + 1.8f, 0.18f, 140f), Quaternion.Euler(0f, -85f, 0f), 6);
        }

        #endregion

        #region Race Setup & Systems

        private void SpawnRaceGridAndSystems()
        {
            // 1. Spawn Player Vehicle
            Vector3 playerPos = new Vector3(-1.8f, 0.2f, 15f);
            playerCar = CarModelBuilder.BuildProceduralCar("PlayerCar_Market", playerConfig);
            playerCar.transform.position = playerPos;
            playerCar.transform.rotation = Quaternion.identity;

            VehicleInputReader inputReader = playerCar.AddComponent<VehicleInputReader>();
            VehicleController carController = playerCar.GetComponent<VehicleController>();
            carController.SetInputSource(inputReader);

            // 2. Camera Setup
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            GameObject camObj = mainCam != null ? mainCam.gameObject : new GameObject("Main Camera");
            if (mainCam == null)
            {
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            VehicleCameraController camController = camObj.GetComponent<VehicleCameraController>();
            if (camController == null) camController = camObj.AddComponent<VehicleCameraController>();
            camController.SetTarget(playerCar.transform);
            camObj.transform.position = playerPos + new Vector3(0f, 2.6f, -6.5f);
            camObj.transform.LookAt(playerPos + Vector3.up * 1.1f);

            // 3. UI Hierarchy
            BuildGameHUD(carController, inputReader);

            // 4. Circuit Race System
            SetupCircuitRaceSystem(carController);
        }

        private void BuildGameHUD(VehicleController car, VehicleInputReader inputReader)
        {
            if (FindAnyObjectByType<HUDController>() != null) return;

            GameObject canvasObj = new GameObject("Canvas_HUD");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();
            hud.BindVehicle(car);
            mobileUI.BindInputReader(inputReader);

            canvasObj.AddComponent<RaceCountdownUI>();
            canvasObj.AddComponent<PauseMenuUI>();
            canvasObj.AddComponent<RaceResultsUI>();
        }

        private void SetupCircuitRaceSystem(VehicleController playerVehicle)
        {
            GameObject raceRoot = new GameObject("Race_System");
            raceRoot.transform.SetParent(transform, false);

            CheckpointManager cpManager = raceRoot.AddComponent<CheckpointManager>();
            cpManager.SetTotalLaps(totalLaps);
            RaceManager raceManager = raceRoot.AddComponent<RaceManager>();

            // 12 Sequential Checkpoints
            Vector3[] cpPoints = new Vector3[]
            {
                new Vector3(0f, 2.5f, 20f),      // CP 0: Start / Finish
                new Vector3(0f, 2.5f, 110f),     // CP 1: Market Straight 1
                new Vector3(0f, 2.5f, 190f),     // CP 2: Market Straight 2
                new Vector3(20f, 2.5f, 250f),    // CP 3: Entry Bazaar
                new Vector3(70f, 2.5f, 285f),    // CP 4: Bazaar Apex East
                new Vector3(145f, 2.5f, 280f),   // CP 5: Bazaar Chicane Mid
                new Vector3(205f, 2.5f, 235f),   // CP 6: Chicane Exit South
                new Vector3(230f, 2.5f, 160f),   // CP 7: Alleyway North
                new Vector3(195f, 2.5f, 85f),    // CP 8: Alleyway Mid S-curve
                new Vector3(220f, 2.5f, 20f),    // CP 9: Alleyway South
                new Vector3(170f, 2.5f, -38f),   // CP 10: Plaza Approach
                new Vector3(70f, 2.5f, -38f)     // CP 11: Final Curve to Finish
            };

            List<Checkpoint> checkpoints = new List<Checkpoint>();
            for (int i = 0; i < cpPoints.Length; i++)
            {
                bool isFinish = (i == 0);
                GameObject cpObj = new GameObject($"Checkpoint_{i:00}" + (isFinish ? "_FinishLine" : ""));
                cpObj.transform.SetParent(raceRoot.transform, false);
                cpObj.transform.position = cpPoints[i];

                Vector3 nextPos = cpPoints[(i + 1) % cpPoints.Length];
                Vector3 dir = (nextPos - cpPoints[i]).normalized;
                if (dir.sqrMagnitude > 0.001f) cpObj.transform.rotation = Quaternion.LookRotation(dir);

                BoxCollider box = cpObj.AddComponent<BoxCollider>();
                box.size = new Vector3(mainRoadWidth + 4f, 7f, 3f);
                box.isTrigger = true;

                Checkpoint cp = cpObj.AddComponent<Checkpoint>();
                cp.Initialize(i, isFinish, cpManager);
                checkpoints.Add(cp);
            }
            cpManager.SetCheckpoints(checkpoints);

            // 22-node Looping AI Waypoints
            WaypointNode[] aiNodes = BuildMarketWaypointLoop(raceRoot.transform);

            raceManager.RegisterPlayer(playerVehicle);

            // 3 AI Opponents
            VehicleConfig speedoConfig = LoadOrCreateVehicleConfig("VehicleConfig_Speedo", VehicleRole.HypercarSpeed,
                new Color(0.95f, 0.28f, 0.08f), new Color(0.15f, 0.15f, 0.18f), 245f, 2200f);
            SpawnAIOpponent("AI_ApexHypercar", speedoConfig, new Vector3(1.8f, 0.2f, 10f), aiNodes[0], raceManager, 1.02f);

            VehicleConfig muscleConfig = LoadOrCreateVehicleConfig("VehicleConfig_ClassicMuscle", VehicleRole.ClassicMuscle,
                new Color(0.15f, 0.22f, 0.35f), new Color(0.85f, 0.70f, 0.15f), 220f, 2100f);
            SpawnAIOpponent("AI_MarauderMuscle", muscleConfig, new Vector3(-1.8f, 0.2f, 4f), aiNodes[0], raceManager, 0.96f);

            VehicleConfig tunerConfig = LoadOrCreateVehicleConfig("VehicleConfig_TunerDrift", VehicleRole.TunerDrift,
                new Color(0.68f, 0.12f, 0.85f), new Color(0.12f, 0.88f, 0.75f), 225f, 2050f);
            SpawnAIOpponent("AI_DriftKing", tunerConfig, new Vector3(1.8f, 0.2f, -2f), aiNodes[0], raceManager, 0.98f);
        }

        private WaypointNode[] BuildMarketWaypointLoop(Transform parent)
        {
            GameObject wpRoot = new GameObject("AI_Waypoint_Loop");
            wpRoot.transform.SetParent(parent, false);

            Vector3[] path = new Vector3[]
            {
                new Vector3(0f, 0.2f, 20f),
                new Vector3(0f, 0.2f, 80f),
                new Vector3(0f, 0.2f, 140f),
                new Vector3(0f, 0.2f, 190f),
                new Vector3(15f, 0.2f, 240f),
                new Vector3(45f, 0.2f, 275f),
                new Vector3(85f, 0.2f, 288f),
                new Vector3(135f, 0.2f, 285f),
                new Vector3(180f, 0.2f, 260f),
                new Vector3(210f, 0.2f, 220f),
                new Vector3(228f, 0.2f, 170f),
                new Vector3(220f, 0.2f, 120f),
                new Vector3(192f, 0.2f, 80f),
                new Vector3(198f, 0.2f, 45f),
                new Vector3(220f, 0.2f, 15f),
                new Vector3(210f, 0.2f, -20f),
                new Vector3(175f, 0.2f, -40f),
                new Vector3(125f, 0.2f, -40f),
                new Vector3(70f, 0.2f, -38f),
                new Vector3(25f, 0.2f, -28f),
                new Vector3(0f, 0.2f, -10f),
                new Vector3(0f, 0.2f, 5f)
            };

            float[] speeds = new float[]
            {
                190f, 205f, 210f, 175f,
                140f, 120f, 135f, 150f,
                135f, 115f, 140f, 145f,
                125f, 130f, 145f, 135f,
                145f, 160f, 165f, 155f, 175f, 190f
            };

            WaypointNode[] nodes = new WaypointNode[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                GameObject obj = new GameObject($"Waypoint_{i:00}");
                obj.transform.SetParent(wpRoot.transform, false);
                obj.transform.position = path[i];
                nodes[i] = obj.AddComponent<WaypointNode>();
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                WaypointNode next = nodes[(i + 1) % nodes.Length];
                nodes[i].SetParameters(speeds[i], 6.0f, next);
            }

            return nodes;
        }

        private VehicleConfig LoadOrCreateVehicleConfig(string assetName, VehicleRole role, Color primary, Color accent, float speed, float torque)
        {
            VehicleConfig cfg = null;
#if UNITY_EDITOR
            cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleConfig>($"Assets/_Project/ScriptableObjects/Vehicles/{assetName}.asset");
#endif
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<VehicleConfig>();
                cfg.displayName = assetName;
                cfg.role = role;
                cfg.topSpeedKmh = speed;
                cfg.motorTorque = torque;
                cfg.brakeTorque = 4200f;
                cfg.primaryColor = primary;
                cfg.accentColor = accent;
            }
            return cfg;
        }

        private void SpawnAIOpponent(string name, VehicleConfig config, Vector3 pos, WaypointNode initialNode, RaceManager manager, float skillMult)
        {
            GameObject aiCar = CarModelBuilder.BuildProceduralCar(name, config);
            aiCar.tag = "Untagged";
            aiCar.transform.position = pos;
            aiCar.transform.rotation = Quaternion.identity;

            AIRacingController aiController = aiCar.AddComponent<AIRacingController>();
            aiController.SetTargetWaypoint(initialNode);

            VehicleController vCtrl = aiCar.GetComponent<VehicleController>();
            manager.RegisterAI(vCtrl);
        }

        #endregion
    }
}

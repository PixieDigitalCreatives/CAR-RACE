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
    /// Master environment and race director for Route 2: Waterfront Run.
    /// Builds a closed-loop ~2.2km coastal racing circuit featuring:
    /// - Sector 1: Coastal Palm Boulevard (Start/Finish straight beside the ocean water with palm trees)
    /// - Sector 2: Marina Esplanade & Pier Turn (Sweeping right bends, coastal seawall, yacht basin view)
    /// - Sector 3: Beachside Causeway & Bridge over the tidal inlet
    /// - Sector 4: Palm Promenade return to the main coastal straight
    /// Complete with 14 sequential checkpoints, 26-node AI waypoint loop,
    /// 4-car starting grid, full in-race HUD, countdown, pause, and podium results.
    /// </summary>
    [ExecuteAlways]
    public class WaterfrontRunBootstrap : MonoBehaviour
    {
        [Header("Race Configuration")]
        [SerializeField] private int totalLaps = 2;
        [SerializeField] private VehicleConfig playerConfig;
        [SerializeField] private VehicleDatabase vehicleDatabase;
        [SerializeField] private bool autoBuildOnStart = true;

        [Header("Track Geometry Settings")]
        [SerializeField] private float mainRoadWidth = 14f;
        [SerializeField] private float curbHeight = 0.18f;
        [SerializeField] private float sidewalkWidth = 3.5f;

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
        [SerializeField] private Material matSignCafe;
        [SerializeField] private Material matAwning;
        [SerializeField] private Material matWindows;
        [SerializeField] private Material matBillboardCity;
        [SerializeField] private Material matBillboardGameium;
        [SerializeField] private Material matStreetlightBulb;
        [SerializeField] private Material matGlass;
        [SerializeField] private Material matWater;
        [SerializeField] private Material matPalmBark;
        [SerializeField] private Material matPalmFronds;

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
            if (matSignCafe == null)
                matSignCafe = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Sign_Cafe.mat");
            if (matAwning == null)
                matAwning = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Awning_Fabric.mat");
            if (matWindows == null)
                matWindows = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Building_Windows.mat");
            if (matBillboardCity == null)
                matBillboardCity = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Billboard_OurCity.mat");
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
            if (matSignCafe == null) matSignCafe = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.65f, 0.25f), new Color(2.5f, 1.6f, 0.9f));
            if (matAwning == null) matAwning = CreateFallbackMat(litShader, new Color(0.2f, 0.45f, 0.75f), 0.15f, 0.0f);
            if (matWindows == null) matWindows = CreateFallbackEmissiveMat(litShader, new Color(0.95f, 0.85f, 0.55f), new Color(1.8f, 1.4f, 0.9f));
            if (matBillboardCity == null) matBillboardCity = CreateFallbackEmissiveMat(litShader, new Color(0.15f, 0.35f, 0.45f), new Color(1.2f, 1.2f, 1.2f));
            if (matBillboardGameium == null) matBillboardGameium = CreateFallbackEmissiveMat(litShader, new Color(0.12f, 0.15f, 0.22f), new Color(1.3f, 1.3f, 1.3f));
            if (matStreetlightBulb == null) matStreetlightBulb = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.82f, 0.52f), new Color(4.5f, 3.2f, 1.5f));
            if (matGlass == null) matGlass = CreateFallbackMat(litShader, new Color(0.12f, 0.18f, 0.25f, 0.85f), 0.92f, 0.1f);

            // Coastal Water & Palms
            if (matWater == null) matWater = CreateFallbackMat(litShader, new Color(0.04f, 0.12f, 0.22f), 0.92f, 0.15f);
            if (matPalmBark == null) matPalmBark = CreateFallbackMat(litShader, new Color(0.38f, 0.28f, 0.18f), 0.15f, 0.0f);
            if (matPalmFronds == null) matPalmFronds = CreateFallbackMat(litShader, new Color(0.15f, 0.42f, 0.18f), 0.35f, 0.0f);
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
            RenderSettings.ambientSkyColor = new Color(0.03f, 0.08f, 0.16f);     // Deep ocean night blue
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.10f, 0.20f); // Cool maritime mist
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.04f, 0.06f);  // Dark wet shore

            if (GameObject.Find("Waterfront_Moonlight") == null && transform.Find("Waterfront_Moonlight") == null)
            {
                GameObject moonObj = new GameObject("Waterfront_Moonlight");
                moonObj.transform.SetParent(transform, false);
                moonObj.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

                Light moon = moonObj.AddComponent<Light>();
                moon.type = LightType.Directional;
                moon.color = new Color(0.62f, 0.74f, 0.92f); // Silvery ocean moonlight
                moon.intensity = 0.22f;
                moon.shadows = LightShadows.Soft;
            }
        }

        #region Circuit Geometry Construction

        private void BuildCircuitGeometry()
        {
            GameObject circuitRoot = new GameObject("CircuitGeometry");
            circuitRoot.transform.SetParent(transform, false);

            // Route 2: Waterfront Run Track Loop (~2.2 km)
            Vector3[] circuitTrackPoints = new Vector3[]
            {
                // Sector 1: Coastal Palm Boulevard (Start/Finish straight)
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 120f),
                new Vector3(0f, 0f, 260f),
                new Vector3(0f, 0f, 400f),

                // Sector 2: Marina Esplanade & Pier Turn
                new Vector3(40f, 0f, 500f),
                new Vector3(120f, 0f, 560f),
                new Vector3(240f, 0f, 570f),
                new Vector3(360f, 0f, 550f),
                new Vector3(460f, 0f, 480f),

                // Sector 3: Beachside Causeway & Bridge
                new Vector3(500f, 0f, 360f),
                new Vector3(490f, 0f, 220f),
                new Vector3(440f, 0f, 100f),

                // Sector 4: Palm Promenades Return
                new Vector3(350f, 0f, 0f),
                new Vector3(220f, 0f, -60f),
                new Vector3(100f, 0f, -50f)
            };

            for (int i = 0; i < circuitTrackPoints.Length; i++)
            {
                Vector3 pA = circuitTrackPoints[i];
                Vector3 pB = circuitTrackPoints[(i + 1) % circuitTrackPoints.Length];
                BuildRoadSegment(circuitRoot.transform, pA, pB, mainRoadWidth, i);
            }

            // Finish Line Overhead Gantry at Z = 25m
            BuildFinishLineGantry(circuitRoot.transform, new Vector3(0f, 0f, 25f), Quaternion.identity, mainRoadWidth);

            // Ocean Water Plane (West side of the coastal boulevard)
            StreetPropsBuilder.CreateOceanWaterPlane(circuitRoot.transform, new Vector3(-350f, -0.4f, 250f), new Vector2(600f, 900f), matWater);

            // Coastal Seawall & Promenade along ocean edge
            for (float sz = -30f; sz <= 430f; sz += 40f)
            {
                StreetPropsBuilder.CreateCoastalSeawall(circuitRoot.transform, new Vector3(-mainRoadWidth * 0.5f - 4.5f, 0f, sz), Quaternion.identity, 39f, matCurb, matMetal);
                StreetPropsBuilder.CreatePalmTree(circuitRoot.transform, new Vector3(-mainRoadWidth * 0.5f - 2.8f, 0.18f, sz + 15f), Quaternion.Euler(0f, Random.Range(0, 360), 0f), matPalmBark, matPalmFronds, 8.5f);
            }

            // Landmarks along Waterfront Circuit
            SpawnWaterfrontLandmarks(circuitRoot.transform);
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

            // Double Yellow Center Line
            GameObject yellowLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            yellowLine.name = "YellowLine";
            yellowLine.transform.SetParent(seg.transform, false);
            yellowLine.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            yellowLine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            yellowLine.transform.localScale = new Vector3(0.24f, length, 1f);
            if (matMarkingYellow != null) yellowLine.GetComponent<Renderer>().sharedMaterial = matMarkingYellow;
            Collider yCol = yellowLine.GetComponent<Collider>();
            if (yCol != null) DestroyImmediate(yCol);

            // Streetlights
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

        private void SpawnWaterfrontLandmarks(Transform parent)
        {
            GameObject landmarks = new GameObject("WaterfrontLandmarks");
            landmarks.transform.SetParent(parent, false);

            // Coastal Cafes along Boulevard (Right side)
            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(16f, 0f, 80f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Marina_Breeze_Cafe");

            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(16f, 0f, 190f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Ocean_Terrace_Lounge");

            // Luxury Marina Towers overlooking ocean
            StreetPropsBuilder.CreateCommercialBuilding(landmarks.transform, new Vector3(20f, 0f, 320f), Quaternion.Euler(0f, -90f, 0f),
                matCommercialFacade, matWindows, null, matMetal, false, 24f, 36f, 18f);

            // GAMEIUM Official Championship Billboard at Marina Apex
            StreetPropsBuilder.CreateUrbanBillboard(landmarks.transform, new Vector3(240f, 0f, 595f), Quaternion.Euler(0f, 180f, 0f),
                matMetal, matBillboardGameium, 12f, 6f, 7f);

            // Spectators along Promenade
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 2f, 0.18f, 40f), Quaternion.Euler(0f, 90f, 0f), 5);
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 2f, 0.18f, 150f), Quaternion.Euler(0f, 90f, 0f), 6);
        }

        #endregion

        #region Race Setup & Systems

        private void SpawnRaceGridAndSystems()
        {
            // 1. Spawn Player Vehicle
            Vector3 playerPos = new Vector3(-1.8f, 0.2f, 15f);
            playerCar = CarModelBuilder.BuildProceduralCar("PlayerCar_Waterfront", playerConfig);
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

            // 14 Sequential Checkpoints
            Vector3[] cpPoints = new Vector3[]
            {
                new Vector3(0f, 2.5f, 25f),      // CP 0: Start / Finish Line
                new Vector3(0f, 2.5f, 150f),     // CP 1: Boulevard Mid
                new Vector3(0f, 2.5f, 320f),     // CP 2: Boulevard North
                new Vector3(25f, 2.5f, 460f),    // CP 3: Entry Marina Curve
                new Vector3(100f, 2.5f, 540f),   // CP 4: Marina Apex West
                new Vector3(240f, 2.5f, 570f),   // CP 5: Marina Esplanade Mid
                new Vector3(380f, 2.5f, 530f),   // CP 6: Marina Apex East
                new Vector3(475f, 2.5f, 440f),   // CP 7: Causeway Turn
                new Vector3(495f, 2.5f, 310f),   // CP 8: Causeway Straight
                new Vector3(480f, 2.5f, 180f),   // CP 9: Causeway South
                new Vector3(420f, 2.5f, 70f),    // CP 10: Inlet Bridge
                new Vector3(310f, 2.5f, -20f),   // CP 11: Promenade Return East
                new Vector3(190f, 2.5f, -60f),   // CP 12: Promenade Return Mid
                new Vector3(60f, 2.5f, -40f)     // CP 13: Final curve to Finish
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

            // 26-node Looping AI Waypoints
            WaypointNode[] aiNodes = BuildWaterfrontWaypointLoop(raceRoot.transform);

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

        private WaypointNode[] BuildWaterfrontWaypointLoop(Transform parent)
        {
            GameObject wpRoot = new GameObject("AI_Waypoint_Loop");
            wpRoot.transform.SetParent(parent, false);

            Vector3[] path = new Vector3[]
            {
                new Vector3(0f, 0.2f, 25f),
                new Vector3(0f, 0.2f, 100f),
                new Vector3(0f, 0.2f, 180f),
                new Vector3(0f, 0.2f, 260f),
                new Vector3(0f, 0.2f, 350f),
                new Vector3(20f, 0.2f, 440f),
                new Vector3(65f, 0.2f, 510f),
                new Vector3(135f, 0.2f, 555f),
                new Vector3(220f, 0.2f, 570f),
                new Vector3(300f, 0.2f, 565f),
                new Vector3(385f, 0.2f, 535f),
                new Vector3(450f, 0.2f, 480f),
                new Vector3(490f, 0.2f, 400f),
                new Vector3(500f, 0.2f, 320f),
                new Vector3(495f, 0.2f, 240f),
                new Vector3(475f, 0.2f, 160f),
                new Vector3(440f, 0.2f, 95f),
                new Vector3(390f, 0.2f, 35f),
                new Vector3(325f, 0.2f, -15f),
                new Vector3(255f, 0.2f, -48f),
                new Vector3(185f, 0.2f, -60f),
                new Vector3(120f, 0.2f, -55f),
                new Vector3(65f, 0.2f, -42f),
                new Vector3(20f, 0.2f, -25f),
                new Vector3(0f, 0.2f, -10f),
                new Vector3(0f, 0.2f, 5f)
            };

            float[] speeds = new float[]
            {
                220f, 235f, 240f, 240f, 220f,
                180f, 155f, 160f, 210f, 225f,
                195f, 165f, 160f, 220f, 225f,
                210f, 175f, 160f, 155f, 165f,
                175f, 160f, 150f, 170f, 190f, 210f
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
                nodes[i].SetParameters(speeds[i], 7.0f, next);
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

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
    /// Master environment and race director for Route 4: Industrial Port.
    /// Builds a closed-loop ~1.8km industrial dockside racing circuit featuring:
    /// - Sector 1: Container Crane Straight (Broad 16m straight under towering cargo crane gantries)
    /// - Sector 2: Container Yard Drift Zone (Chicanes defined by stacked colored shipping containers)
    /// - Sector 3: Warehouse Row (Heavy steel industrial facilities with high-mast floodlights)
    /// - Sector 4: Wharf Apron Return (Fast sweeping turn along the shipping docks)
    /// Complete with 12 sequential checkpoints, 24-node AI waypoint loop,
    /// 4-car starting grid, full in-race HUD, countdown, pause, and podium results.
    /// </summary>
    [ExecuteAlways]
    public class IndustrialPortBootstrap : MonoBehaviour
    {
        [Header("Race Configuration")]
        [SerializeField] private int totalLaps = 2;
        [SerializeField] private VehicleConfig playerConfig;
        [SerializeField] private VehicleDatabase vehicleDatabase;
        [SerializeField] private bool autoBuildOnStart = true;

        [Header("Track Geometry Settings")]
        [SerializeField] private float mainRoadWidth = 16f; // Wide industrial apron
        [SerializeField] private float curbHeight = 0.20f;
        [SerializeField] private float sidewalkWidth = 3.0f;

        [Header("Material Cache")]
        [SerializeField] private Material matAsphalt;
        [SerializeField] private Material matSidewalk;
        [SerializeField] private Material matCurb;
        [SerializeField] private Material matSteel;
        [SerializeField] private Material matYellowCrane;
        [SerializeField] private Material matMarkingWhite;
        [SerializeField] private Material matMarkingYellow;
        [SerializeField] private Material matBarrierChevron;
        [SerializeField] private Material matContainerBlue;
        [SerializeField] private Material matContainerRed;
        [SerializeField] private Material matContainerOrange;
        [SerializeField] private Material matBillboardGameium;
        [SerializeField] private Material matFloodlightBulb;

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
            if (matSteel == null)
                matSteel = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Metal_Plate.mat");
            if (matMarkingWhite == null)
                matMarkingWhite = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Road_Marking_White.mat");
            if (matMarkingYellow == null)
                matMarkingYellow = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Road_Marking_Yellow.mat");
            if (matBarrierChevron == null)
                matBarrierChevron = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Barrier_Chevron.mat");
            if (matBillboardGameium == null)
                matBillboardGameium = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Billboard_Thegameio.mat");
            if (matFloodlightBulb == null)
                matFloodlightBulb = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Streetlight_Bulb.mat");
#endif
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (matAsphalt == null) matAsphalt = CreateFallbackMat(litShader, new Color(0.16f, 0.16f, 0.18f), 0.35f, 0.05f);
            if (matSidewalk == null) matSidewalk = CreateFallbackMat(litShader, new Color(0.55f, 0.55f, 0.58f), 0.2f, 0.0f);
            if (matCurb == null) matCurb = CreateFallbackMat(litShader, new Color(0.65f, 0.65f, 0.65f), 0.25f, 0.0f);
            if (matSteel == null) matSteel = CreateFallbackMat(litShader, new Color(0.32f, 0.35f, 0.38f), 0.55f, 0.75f);
            if (matYellowCrane == null) matYellowCrane = CreateFallbackMat(litShader, new Color(0.92f, 0.75f, 0.10f), 0.40f, 0.20f);
            if (matMarkingWhite == null) matMarkingWhite = CreateFallbackMat(litShader, new Color(0.95f, 0.95f, 0.98f), 0.3f, 0.0f);
            if (matMarkingYellow == null) matMarkingYellow = CreateFallbackMat(litShader, new Color(0.96f, 0.78f, 0.12f), 0.3f, 0.0f);
            if (matBarrierChevron == null) matBarrierChevron = CreateFallbackMat(litShader, new Color(0.95f, 0.75f, 0.1f), 0.35f, 0.1f);
            if (matBillboardGameium == null) matBillboardGameium = CreateFallbackEmissiveMat(litShader, new Color(0.12f, 0.15f, 0.22f), new Color(1.3f, 1.3f, 1.3f));
            if (matFloodlightBulb == null) matFloodlightBulb = CreateFallbackEmissiveMat(litShader, new Color(0.95f, 0.98f, 1.0f), new Color(4.8f, 4.8f, 5.0f));

            // Container Colors
            if (matContainerBlue == null) matContainerBlue = CreateFallbackMat(litShader, new Color(0.12f, 0.35f, 0.65f), 0.35f, 0.15f);
            if (matContainerRed == null) matContainerRed = CreateFallbackMat(litShader, new Color(0.75f, 0.20f, 0.15f), 0.35f, 0.15f);
            if (matContainerOrange == null) matContainerOrange = CreateFallbackMat(litShader, new Color(0.85f, 0.48f, 0.10f), 0.35f, 0.15f);
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
            RenderSettings.ambientSkyColor = new Color(0.04f, 0.06f, 0.11f);     // Industrial port night
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.10f, 0.14f); // Sodium haze
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.03f, 0.04f);  // Dark concrete apron

            if (GameObject.Find("Port_Moonlight") == null && transform.Find("Port_Moonlight") == null)
            {
                GameObject moonObj = new GameObject("Port_Moonlight");
                moonObj.transform.SetParent(transform, false);
                moonObj.transform.rotation = Quaternion.Euler(55f, -25f, 0f);

                Light moon = moonObj.AddComponent<Light>();
                moon.type = LightType.Directional;
                moon.color = new Color(0.65f, 0.72f, 0.85f);
                moon.intensity = 0.20f;
                moon.shadows = LightShadows.Soft;
            }
        }

        #region Circuit Geometry Construction

        private void BuildCircuitGeometry()
        {
            GameObject circuitRoot = new GameObject("CircuitGeometry");
            circuitRoot.transform.SetParent(transform, false);

            // Route 4: Industrial Port Track Loop (~1.8 km loop)
            Vector3[] circuitTrackPoints = new Vector3[]
            {
                // Sector 1: Container Crane Straight
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 140f),
                new Vector3(0f, 0f, 280f),

                // Sector 2: Container Maze Drift Zone (Sweeping wide hairpin East)
                new Vector3(30f, 0f, 380f),
                new Vector3(100f, 0f, 440f),
                new Vector3(200f, 0f, 450f),
                new Vector3(300f, 0f, 420f),
                new Vector3(360f, 0f, 340f),

                // Sector 3: Warehouse Row (Southbound straight)
                new Vector3(370f, 0f, 220f),
                new Vector3(340f, 0f, 100f),
                new Vector3(300f, 0f, 0f),

                // Sector 4: Wharf Apron Return
                new Vector3(220f, 0f, -50f),
                new Vector3(110f, 0f, -45f)
            };

            for (int i = 0; i < circuitTrackPoints.Length; i++)
            {
                Vector3 pA = circuitTrackPoints[i];
                Vector3 pB = circuitTrackPoints[(i + 1) % circuitTrackPoints.Length];
                BuildRoadSegment(circuitRoot.transform, pA, pB, mainRoadWidth, i);
            }

            // Finish Line Gantry at Z = 25m
            BuildFinishLineGantry(circuitRoot.transform, new Vector3(0f, 0f, 25f), Quaternion.identity, mainRoadWidth);

            // Industrial Port Landmarks: Gantry Cranes, Containers, Floodlight Towers
            SpawnPortLandmarks(circuitRoot.transform);
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

            // Heavy Industrial Asphalt Surface
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

            // Crash Safety Barriers with Chevrons
            StreetPropsBuilder.CreateSafetyBarrier(seg.transform, new Vector3(-(halfWidth + 0.15f), curbHeight, 0f), Quaternion.identity, matSteel, matBarrierChevron, length * 0.95f);
            StreetPropsBuilder.CreateSafetyBarrier(seg.transform, new Vector3(halfWidth + 0.15f, curbHeight, 0f), Quaternion.identity, matSteel, matBarrierChevron, length * 0.95f);
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
            float gantryHeight = 7.0f;

            CreateSegmentPart("TrussTower_L", gantryRoot.transform, new Vector3(-halfSpan, gantryHeight * 0.5f, 0f), new Vector3(0.8f, gantryHeight, 0.8f), matSteel);
            CreateSegmentPart("TrussTower_R", gantryRoot.transform, new Vector3(halfSpan, gantryHeight * 0.5f, 0f), new Vector3(0.8f, gantryHeight, 0.8f), matSteel);
            CreateSegmentPart("TrussBeam", gantryRoot.transform, new Vector3(0f, gantryHeight, 0f), new Vector3(halfSpan * 2f + 0.8f, 0.9f, 0.9f), matSteel);
            CreateSegmentPart("FinishBanner", gantryRoot.transform, new Vector3(0f, gantryHeight - 0.85f, 0f), new Vector3(width * 0.85f, 1.2f, 0.15f), matBarrierChevron);
        }

        private void SpawnPortLandmarks(Transform parent)
        {
            GameObject landmarks = new GameObject("PortLandmarks");
            landmarks.transform.SetParent(parent, false);

            // 1. Towering Container Gantry Cranes over main straight
            StreetPropsBuilder.CreateCargoGantryCrane(landmarks.transform, new Vector3(0f, 0f, 90f), Quaternion.identity, matSteel, matYellowCrane);
            StreetPropsBuilder.CreateCargoGantryCrane(landmarks.transform, new Vector3(0f, 0f, 210f), Quaternion.identity, matSteel, matYellowCrane);

            // 2. High-Mast Floodlight Towers
            StreetPropsBuilder.CreateFloodlightTower(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 4f, 0f, 50f), Quaternion.Euler(0f, 90f, 0f), matSteel, matFloodlightBulb, 16f);
            StreetPropsBuilder.CreateFloodlightTower(landmarks.transform, new Vector3(mainRoadWidth * 0.5f + 4f, 0f, 180f), Quaternion.Euler(0f, -90f, 0f), matSteel, matFloodlightBulb, 16f);
            StreetPropsBuilder.CreateFloodlightTower(landmarks.transform, new Vector3(210f, 0f, 470f), Quaternion.Euler(0f, 180f, 0f), matSteel, matFloodlightBulb, 16f);

            // 3. Stacked Shipping Containers along Drift Zone (Sector 2)
            Material[] containerMats = new Material[] { matContainerBlue, matContainerRed, matContainerOrange };
            for (int i = 0; i < 6; i++)
            {
                float z = 320f + i * 22f;
                float x = -mainRoadWidth * 0.5f - 5f;
                Material m1 = containerMats[i % containerMats.Length];
                Material m2 = containerMats[(i + 1) % containerMats.Length];

                // Ground tier
                StreetPropsBuilder.CreateShippingContainer(landmarks.transform, new Vector3(x, 1.3f, z), Quaternion.Euler(0f, 15f, 0f), m1, 6f);
                // Second tier
                StreetPropsBuilder.CreateShippingContainer(landmarks.transform, new Vector3(x, 3.9f, z), Quaternion.Euler(0f, 15f, 0f), m2, 6f);
            }

            // Outer Drift Perimeter Containers
            for (int j = 0; j < 5; j++)
            {
                float angle = j * 25f;
                Vector3 cPos = new Vector3(150f + j * 40f, 1.3f, 465f);
                StreetPropsBuilder.CreateShippingContainer(landmarks.transform, cPos, Quaternion.Euler(0f, angle, 0f), containerMats[j % 3], 6f);
                StreetPropsBuilder.CreateShippingContainer(landmarks.transform, cPos + Vector3.up * 2.6f, Quaternion.Euler(0f, angle, 0f), containerMats[(j + 2) % 3], 6f);
            }

            // GAMEIUM Championship Billboard at Port Drift Zone Entry
            StreetPropsBuilder.CreateUrbanBillboard(landmarks.transform, new Vector3(310f, 0f, 450f), Quaternion.Euler(0f, 195f, 0f),
                matSteel, matBillboardGameium, 12f, 6f, 7f);

            // Spectators safely behind industrial barriers
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(-mainRoadWidth * 0.5f - 2f, 0.2f, 35f), Quaternion.Euler(0f, 85f, 0f), 5);
            StreetPropsBuilder.CreateSpectatorCluster(landmarks.transform, new Vector3(mainRoadWidth * 0.5f + 2f, 0.2f, 140f), Quaternion.Euler(0f, -85f, 0f), 6);
        }

        #endregion

        #region Race Setup & Systems

        private void SpawnRaceGridAndSystems()
        {
            // 1. Spawn Player Vehicle
            Vector3 playerPos = new Vector3(-2.2f, 0.2f, 15f);
            playerCar = CarModelBuilder.BuildProceduralCar("PlayerCar_Port", playerConfig);
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
                new Vector3(0f, 2.5f, 25f),      // CP 0: Start / Finish Line
                new Vector3(0f, 2.5f, 150f),     // CP 1: Crane Straight Mid
                new Vector3(0f, 2.5f, 270f),     // CP 2: Crane Straight North
                new Vector3(30f, 2.5f, 370f),    // CP 3: Entry Drift Zone
                new Vector3(105f, 2.5f, 435f),   // CP 4: Container Chicane 1
                new Vector3(210f, 2.5f, 445f),   // CP 5: Container Chicane 2
                new Vector3(305f, 2.5f, 410f),   // CP 6: Drift Hairpin Apex
                new Vector3(365f, 2.5f, 320f),   // CP 7: Exit Drift Zone South
                new Vector3(365f, 2.5f, 210f),   // CP 8: Warehouse Row North
                new Vector3(335f, 2.5f, 95f),    // CP 9: Warehouse Row South
                new Vector3(285f, 2.5f, -10f),   // CP 10: Wharf Apron Approach
                new Vector3(100f, 2.5f, -40f)    // CP 11: Final Wharf Curve to Finish
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

            // 24-node Looping AI Waypoints
            WaypointNode[] aiNodes = BuildPortWaypointLoop(raceRoot.transform);

            raceManager.RegisterPlayer(playerVehicle);

            // 3 AI Opponents
            VehicleConfig speedoConfig = LoadOrCreateVehicleConfig("VehicleConfig_Speedo", VehicleRole.HypercarSpeed,
                new Color(0.95f, 0.28f, 0.08f), new Color(0.15f, 0.15f, 0.18f), 245f, 2200f);
            SpawnAIOpponent("AI_ApexHypercar", speedoConfig, new Vector3(2.2f, 0.2f, 10f), aiNodes[0], raceManager, 1.02f);

            VehicleConfig muscleConfig = LoadOrCreateVehicleConfig("VehicleConfig_ClassicMuscle", VehicleRole.ClassicMuscle,
                new Color(0.15f, 0.22f, 0.35f), new Color(0.85f, 0.70f, 0.15f), 220f, 2100f);
            SpawnAIOpponent("AI_MarauderMuscle", muscleConfig, new Vector3(-2.2f, 0.2f, 4f), aiNodes[0], raceManager, 0.96f);

            VehicleConfig tunerConfig = LoadOrCreateVehicleConfig("VehicleConfig_TunerDrift", VehicleRole.TunerDrift,
                new Color(0.68f, 0.12f, 0.85f), new Color(0.12f, 0.88f, 0.75f), 225f, 2050f);
            SpawnAIOpponent("AI_DriftKing", tunerConfig, new Vector3(2.2f, 0.2f, -2f), aiNodes[0], raceManager, 0.98f);
        }

        private WaypointNode[] BuildPortWaypointLoop(Transform parent)
        {
            GameObject wpRoot = new GameObject("AI_Waypoint_Loop");
            wpRoot.transform.SetParent(parent, false);

            Vector3[] path = new Vector3[]
            {
                new Vector3(0f, 0.2f, 25f),
                new Vector3(0f, 0.2f, 90f),
                new Vector3(0f, 0.2f, 160f),
                new Vector3(0f, 0.2f, 230f),
                new Vector3(0f, 0.2f, 290f),
                new Vector3(18f, 0.2f, 350f),
                new Vector3(55f, 0.2f, 405f),
                new Vector3(110f, 0.2f, 438f),
                new Vector3(180f, 0.2f, 450f),
                new Vector3(250f, 0.2f, 442f),
                new Vector3(315f, 0.2f, 400f),
                new Vector3(355f, 0.2f, 340f),
                new Vector3(370f, 0.2f, 260f),
                new Vector3(365f, 0.2f, 190f),
                new Vector3(345f, 0.2f, 130f),
                new Vector3(320f, 0.2f, 65f),
                new Vector3(290f, 0.2f, 5f),
                new Vector3(245f, 0.2f, -35f),
                new Vector3(185f, 0.2f, -50f),
                new Vector3(125f, 0.2f, -48f),
                new Vector3(65f, 0.2f, -38f),
                new Vector3(20f, 0.2f, -22f),
                new Vector3(0f, 0.2f, -8f),
                new Vector3(0f, 0.2f, 5f)
            };

            float[] speeds = new float[]
            {
                210f, 225f, 230f, 230f, 215f,
                185f, 155f, 165f, 175f, 170f,
                145f, 140f, 175f, 205f, 210f,
                205f, 190f, 160f, 165f, 175f,
                180f, 190f, 200f, 210f
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

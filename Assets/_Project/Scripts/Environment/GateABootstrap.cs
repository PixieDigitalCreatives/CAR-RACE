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
    /// Runtime and in-editor bootstrapper for Gate A: Representative Nighttime Street Sample.
    /// Builds the 120m African coastal avenue with Poly Haven CC0 PBR textures,
    /// authentic streetlights, storefronts, commercial towers with lit windows,
    /// race crash barriers, finish line gantry, billboards, spectators, player car, AI opponents,
    /// anti-cheat checkpoints, waypoint spline, and complete in-race HUD/UI loop.
    /// </summary>
    [ExecuteAlways]
    public class GateABootstrap : MonoBehaviour
    {
        [Header("Vehicle Configuration")]
        [SerializeField] private VehicleConfig activeConfig;
        [SerializeField] private VehicleDatabase vehicleDatabase;

        [Header("Sample Settings")]
        [SerializeField] private float avenueLength = 120f;
        [SerializeField] private float roadWidth = 14f;
        [SerializeField] private float sidewalkWidth = 3.8f;
        [SerializeField] private bool autoBuildOnStart = true;

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
        [SerializeField] private Material matBillboardCity;
        [SerializeField] private Material matBillboardThegameio;
        [SerializeField] private Material matStreetlightBulb;
        [SerializeField] private Material matGlass;
        [SerializeField] private Material matSignNeonAfrica;

        private GameObject spawnedCar;

        private void Awake()
        {
            if (!autoBuildOnStart) return;

            LoadOrFindMaterials();
            SetupNightAtmosphere();

            if (transform.Find("StreetGeometry") == null)
            {
                BuildStreetSample();
            }

            if (Application.isPlaying)
            {
                EnsureEventSystem();
                ResolveVehicleConfig();
                SpawnCarAndControls();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        private void ResolveVehicleConfig()
        {
            string selectedId = PlayerPrefs.GetString("SelectedVehicleId", "");
            if (!string.IsNullOrEmpty(selectedId) && vehicleDatabase != null)
            {
                activeConfig = vehicleDatabase.GetVehicleById(selectedId);
            }

            if (activeConfig == null && vehicleDatabase != null)
            {
                activeConfig = vehicleDatabase.GetDefaultVehicle();
            }

            if (activeConfig == null)
            {
#if UNITY_EDITOR
                activeConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleConfig>(
                    "Assets/_Project/ScriptableObjects/Vehicles/VehicleConfig_Regular.asset");
                if (vehicleDatabase == null)
                {
                    vehicleDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleDatabase>(
                        "Assets/_Project/ScriptableObjects/Vehicles/VehicleDatabase.asset");
                }
#endif
                if (activeConfig == null)
                {
                    activeConfig = ScriptableObject.CreateInstance<VehicleConfig>();
                    activeConfig.displayName = "Regular";
                    activeConfig.topSpeedKmh = 215f;
                    activeConfig.motorTorque = 1900f;
                    activeConfig.brakeTorque = 4000f;
                    activeConfig.primaryColor = new Color(0.12f, 0.45f, 0.95f);
                    activeConfig.accentColor = new Color(0.98f, 0.55f, 0.12f);
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
            if (matBillboardCity == null)
                matBillboardCity = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Billboard_OurCity.mat");
            if (matBillboardThegameio == null)
                matBillboardThegameio = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Billboard_Thegameio.mat");
            if (matStreetlightBulb == null)
                matStreetlightBulb = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Streetlight_Bulb.mat");
            if (matGlass == null)
                matGlass = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Store_Glass.mat");
            if (matSignNeonAfrica == null)
                matSignNeonAfrica = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_Sign_Neon_Africa.mat");
#endif
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            // Runtime fallback materials if not assigned
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
            if (matAwning == null) matAwning = CreateFallbackMat(litShader, new Color(0.75f, 0.25f, 0.2f), 0.15f, 0.0f);
            if (matWindows == null) matWindows = CreateFallbackEmissiveMat(litShader, new Color(0.95f, 0.85f, 0.55f), new Color(1.8f, 1.4f, 0.9f));
            if (matMural == null) matMural = CreateFallbackMat(litShader, new Color(0.65f, 0.35f, 0.25f), 0.25f, 0.05f);
            if (matBillboardCity == null) matBillboardCity = CreateFallbackEmissiveMat(litShader, new Color(0.15f, 0.35f, 0.45f), new Color(1.2f, 1.2f, 1.2f));
            if (matBillboardThegameio == null) matBillboardThegameio = CreateFallbackEmissiveMat(litShader, new Color(0.12f, 0.15f, 0.22f), new Color(1.3f, 1.3f, 1.3f));
            if (matStreetlightBulb == null) matStreetlightBulb = CreateFallbackEmissiveMat(litShader, new Color(1f, 0.82f, 0.52f), new Color(4.5f, 3.2f, 1.5f));
            if (matGlass == null) matGlass = CreateFallbackMat(litShader, new Color(0.12f, 0.18f, 0.25f, 0.85f), 0.92f, 0.1f);
            if (matSignNeonAfrica == null) matSignNeonAfrica = CreateFallbackEmissiveMat(litShader, new Color(0.9f, 0.2f, 0.8f), new Color(2.5f, 0.6f, 2.2f));
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
            RenderSettings.ambientSkyColor = new Color(0.04f, 0.07f, 0.13f);     // Midnight indigo
            RenderSettings.ambientEquatorColor = new Color(0.06f, 0.09f, 0.16f); // Cool slate
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.03f, 0.05f);  // Dark asphalt bounce

            // Check scene root and child to avoid duplicate directional moonlight
            if (GameObject.Find("Directional Moonlight") == null &&
                GameObject.Find("Directional_Moonlight") == null &&
                transform.Find("Directional_Moonlight") == null)
            {
                GameObject moonObj = new GameObject("Directional_Moonlight");
                moonObj.transform.SetParent(transform, false);
                moonObj.transform.rotation = Quaternion.Euler(58f, -42f, 0f);

                Light moon = moonObj.AddComponent<Light>();
                moon.type = LightType.Directional;
                moon.color = new Color(0.58f, 0.68f, 0.85f); // Cool silvery night blue
                moon.intensity = 0.16f;
                moon.shadows = LightShadows.Soft;
            }
        }

        public void BuildStreetSample()
        {
            GameObject streetRoot = new GameObject("StreetGeometry");
            streetRoot.transform.SetParent(transform, false);

            // 1. Road Avenue
            StreetPropsBuilder.CreateRoadAvenue(
                streetRoot.transform, matAsphalt, matSidewalk, matCurb,
                avenueLength, roadWidth, sidewalkWidth, 0.18f);

            // 2. Road Markings
            StreetPropsBuilder.CreateRoadMarkings(
                streetRoot.transform, matMarkingWhite, matMarkingYellow, avenueLength, roadWidth);

            // 3. Streetlights (Overhanging the driving lanes)
            float streetHalf = roadWidth * 0.5f;
            float[] lightPositionsZ = new float[] { 10f, 35f, 60f, 85f, 110f };
            foreach (float lz in lightPositionsZ)
            {
                // Left Streetlight: overhangs towards World +X over left lane
                StreetPropsBuilder.CreateStreetlight(
                    streetRoot.transform,
                    new Vector3(-(streetHalf + 0.35f), 0.18f, lz),
                    Quaternion.identity,
                    matMetal, matStreetlightBulb);

                // Right Streetlight: overhangs towards World -X over right lane
                StreetPropsBuilder.CreateStreetlight(
                    streetRoot.transform,
                    new Vector3(streetHalf + 0.35f, 0.18f, lz),
                    Quaternion.Euler(0f, 180f, 0f),
                    matMetal, matStreetlightBulb);
            }

            // 4. Safety Crash Barriers
            float bLeft = -(streetHalf + 0.15f);
            float bRight = (streetHalf + 0.15f);
            for (float bz = 8f; bz <= avenueLength - 8f; bz += 4f)
            {
                StreetPropsBuilder.CreateSafetyBarrier(
                    streetRoot.transform, new Vector3(bLeft, 0.18f, bz), Quaternion.identity,
                    matMetal, matBarrierChevron, 3.9f);

                StreetPropsBuilder.CreateSafetyBarrier(
                    streetRoot.transform, new Vector3(bRight, 0.18f, bz), Quaternion.identity,
                    matMetal, matBarrierChevron, 3.9f);
            }

            // 5. Spectators
            StreetPropsBuilder.CreateSpectatorCluster(streetRoot.transform, new Vector3(-(streetHalf + 2.0f), 0.18f, 22f), Quaternion.Euler(0f, 85f, 0f), 4);
            StreetPropsBuilder.CreateSpectatorCluster(streetRoot.transform, new Vector3(-(streetHalf + 2.2f), 0.18f, 55f), Quaternion.Euler(0f, 95f, 0f), 5);
            StreetPropsBuilder.CreateSpectatorCluster(streetRoot.transform, new Vector3((streetHalf + 2.0f), 0.18f, 26f), Quaternion.Euler(0f, -80f, 0f), 4);
            StreetPropsBuilder.CreateSpectatorCluster(streetRoot.transform, new Vector3((streetHalf + 2.1f), 0.18f, 72f), Quaternion.Euler(0f, -90f, 0f), 6);

            // 6. Buildings & Storefronts
            float bldLineLeft = -(streetHalf + 3.8f);
            float bldLineRight = (streetHalf + 3.8f);

            // Left Side: Facing road (World +X), back extending away (World -X)
            StreetPropsBuilder.CreateStorefront(
                streetRoot.transform, new Vector3(bldLineLeft, 0.18f, 24f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignKilima, matAwning, matGlass, matMetal, "Kilima_Electronics");

            StreetPropsBuilder.CreateStorefront(
                streetRoot.transform, new Vector3(bldLineLeft, 0.18f, 40f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Coastal_Breeze_Cafe");

            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineLeft - 2f, 0.18f, 68f), Quaternion.Euler(0f, -90f, 0f),
                matCommercialFacade, matWindows, matMural, matMetal,
                hasMural: true, width: 20f, height: 26f, depth: 16f);

            // Vertical Neon Blade Sign ("AFRICA") attached to avenue building corner at Z = 58m
            StreetPropsBuilder.CreateVerticalBladeSign(
                streetRoot.transform, new Vector3(bldLineLeft + 0.5f, 2.8f, 58f), Quaternion.identity,
                matMetal, matSignNeonAfrica, width: 1.4f, height: 7.5f, depth: 0.25f);

            // Right Side: Facing road (World -X), back extending away (World +X)
            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineRight + 2f, 0.18f, 32f), Quaternion.Euler(0f, 90f, 0f),
                matCommercialFacade, matWindows, null, matMetal,
                hasMural: false, width: 22f, height: 32f, depth: 18f);

            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineRight + 1f, 0.18f, 66f), Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matWindows, null, matMetal,
                hasMural: false, width: 18f, height: 20f, depth: 14f);

            // 7. Urban Billboards (Oriented towards oncoming traffic)
            // Right Billboard ("OUR CITY OUR STRENGTH" at Z = 14m)
            StreetPropsBuilder.CreateUrbanBillboard(
                streetRoot.transform, new Vector3(bldLineRight + 1.5f, 0.18f, 14f), Quaternion.Euler(0f, 15f, 0f),
                matMetal, matBillboardCity, 9.5f, 4.8f, 6f);

            // Left Billboard ("THEGAMEIO RACING" at Z = 92m)
            StreetPropsBuilder.CreateUrbanBillboard(
                streetRoot.transform, new Vector3(bldLineLeft - 1.5f, 0.18f, 92f), Quaternion.Euler(0f, -15f, 0f),
                matMetal, matBillboardThegameio, 9.5f, 4.8f, 6f);

            // 8. Finish Line Overhead Gantry at Z = 115m
            BuildFinishLineGantry(streetRoot.transform, 115f, roadWidth);
        }

        private void BuildFinishLineGantry(Transform parent, float zPos, float width)
        {
            GameObject gantryRoot = new GameObject("FinishLine_Gantry");
            gantryRoot.transform.SetParent(parent, false);

            float halfSpan = (width * 0.5f) + 0.8f;
            float gantryHeight = 6.4f;

            // Left Tower
            CreatePrimitiveCube("TrussTower_L", gantryRoot.transform, new Vector3(-halfSpan, gantryHeight * 0.5f, zPos), new Vector3(0.7f, gantryHeight, 0.7f), matMetal);
            // Right Tower
            CreatePrimitiveCube("TrussTower_R", gantryRoot.transform, new Vector3(halfSpan, gantryHeight * 0.5f, zPos), new Vector3(0.7f, gantryHeight, 0.7f), matMetal);
            // Overhead Crossbeam
            CreatePrimitiveCube("TrussBeam", gantryRoot.transform, new Vector3(0f, gantryHeight, zPos), new Vector3(halfSpan * 2f + 0.8f, 0.8f, 0.8f), matMetal);
            // Finish Banner
            CreatePrimitiveCube("FinishBanner", gantryRoot.transform, new Vector3(0f, gantryHeight - 0.75f, zPos), new Vector3(width * 0.85f, 1.1f, 0.15f), matBarrierChevron);

            // Dual Downward Spotlights illuminating the finish line
            CreateFinishSpotlight(gantryRoot.transform, new Vector3(-3.5f, gantryHeight - 0.2f, zPos));
            CreateFinishSpotlight(gantryRoot.transform, new Vector3(3.5f, gantryHeight - 0.2f, zPos));
        }

        private void CreateFinishSpotlight(Transform parent, Vector3 localPos)
        {
            GameObject spotObj = new GameObject("FinishSpotlight");
            spotObj.transform.SetParent(parent, false);
            spotObj.transform.localPosition = localPos;
            spotObj.transform.localRotation = Quaternion.Euler(85f, 0f, 0f); // Point almost straight down onto asphalt

            Light light = spotObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 3.5f;
            light.range = 14f;
            light.spotAngle = 65f;
            light.shadows = LightShadows.None;
        }

        private void CreatePrimitiveCube(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localScale = localScale;

            MeshRenderer mr = cube.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }

        private void SpawnCarAndControls()
        {
            Vector3 spawnPos = new Vector3(-1.75f, 0.2f, 15f); // Grid Slot 01 (Left lane)
            spawnedCar = CarModelBuilder.BuildProceduralCar("PlayerCar_GateA", activeConfig);
            spawnedCar.transform.position = spawnPos;
            spawnedCar.transform.rotation = Quaternion.identity;

            VehicleInputReader inputReader = spawnedCar.AddComponent<VehicleInputReader>();
            VehicleController carController = spawnedCar.GetComponent<VehicleController>();
            carController.SetInputSource(inputReader);

            // Main Camera
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
            camController.SetTarget(spawnedCar.transform);
            camObj.transform.position = spawnPos + new Vector3(0f, 2.6f, -6.5f);
            camObj.transform.LookAt(spawnPos + Vector3.up * 1.1f);

            // In-Race HUD & Touch Driving UI
            BuildGameHUD(carController, inputReader);

            // Build Race Manager, Checkpoints, AI Opponents & Waypoints
            SetupRaceEnvironment(carController);
        }

        private void SetupRaceEnvironment(VehicleController playerCar)
        {
            GameObject raceRoot = new GameObject("Race_System");
            raceRoot.transform.SetParent(transform, false);

            CheckpointManager cpManager = raceRoot.AddComponent<CheckpointManager>();
            RaceManager raceManager = raceRoot.AddComponent<RaceManager>();

            // 1. Checkpoints
            List<Checkpoint> checkpoints = new List<Checkpoint>();
            float[] cpDistances = new float[] { 25f, 55f, 85f, 115f };
            for (int i = 0; i < cpDistances.Length; i++)
            {
                bool isFinish = (i == cpDistances.Length - 1);
                GameObject cpObj = new GameObject($"Checkpoint_{i:00}" + (isFinish ? "_Finish" : ""));
                cpObj.transform.SetParent(raceRoot.transform, false);
                cpObj.transform.position = new Vector3(0f, 2.5f, cpDistances[i]);

                BoxCollider box = cpObj.AddComponent<BoxCollider>();
                box.size = new Vector3(roadWidth + 2f, 6f, 2.5f);
                box.isTrigger = true;

                Checkpoint cp = cpObj.AddComponent<Checkpoint>();
                cp.Initialize(i, isFinish, cpManager);
                checkpoints.Add(cp);
            }
            cpManager.SetCheckpoints(checkpoints);

            // 2. Waypoint Spline for AI navigation
            GameObject waypointsRoot = new GameObject("AI_Waypoints");
            waypointsRoot.transform.SetParent(raceRoot.transform, false);

            WaypointNode[] nodes = new WaypointNode[5];
            Vector3[] wpPositions = new Vector3[]
            {
                new Vector3(1.75f, 0.2f, 25f),
                new Vector3(1.20f, 0.2f, 55f),
                new Vector3(-0.60f, 0.2f, 85f),
                new Vector3(0.00f, 0.2f, 115f),
                new Vector3(0.00f, 0.2f, 135f)
            };
            float[] wpSpeeds = new float[] { 145f, 168f, 182f, 195f, 60f };

            for (int i = 0; i < wpPositions.Length; i++)
            {
                GameObject nodeObj = new GameObject($"Waypoint_{i:00}");
                nodeObj.transform.SetParent(waypointsRoot.transform, false);
                nodeObj.transform.position = wpPositions[i];
                nodes[i] = nodeObj.AddComponent<WaypointNode>();
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                WaypointNode next = (i < nodes.Length - 1) ? nodes[i + 1] : null;
                nodes[i].SetParameters(wpSpeeds[i], 6.5f, next);
            }

            // 3. Register Player
            raceManager.RegisterPlayer(playerCar);

            // 4. Spawn AI Opponents on Starting Grid
            // Slot 2: Speedo Hypercar (Right lane, slightly behind player)
            VehicleConfig speedoConfig = LoadOrCreateVehicleConfig("VehicleConfig_Speedo", VehicleRole.HypercarSpeed,
                new Color(0.95f, 0.28f, 0.08f), new Color(0.15f, 0.15f, 0.18f), 245f, 2200f);
            SpawnAIOpponent("AI_ApexHypercar", speedoConfig, new Vector3(1.75f, 0.2f, 11f), nodes[0], raceManager, 1.02f);

            // Slot 3: Classic Muscle (Left lane, 2nd row)
            VehicleConfig muscleConfig = LoadOrCreateVehicleConfig("VehicleConfig_ClassicMuscle", VehicleRole.ClassicMuscle,
                new Color(0.15f, 0.22f, 0.35f), new Color(0.85f, 0.70f, 0.15f), 220f, 2100f);
            SpawnAIOpponent("AI_MarauderMuscle", muscleConfig, new Vector3(-1.75f, 0.2f, 5f), nodes[0], raceManager, 0.95f);

            // Slot 4: Tuner Drift (Right lane, 2nd row)
            VehicleConfig tunerConfig = LoadOrCreateVehicleConfig("VehicleConfig_TunerDrift", VehicleRole.TunerDrift,
                new Color(0.68f, 0.12f, 0.85f), new Color(0.12f, 0.88f, 0.75f), 225f, 2050f);
            SpawnAIOpponent("AI_DriftKing", tunerConfig, new Vector3(1.75f, 0.2f, -1f), nodes[0], raceManager, 0.98f);
        }

        private VehicleConfig LoadOrCreateVehicleConfig(string assetName, VehicleRole role, Color primary, Color accent, float speed, float torque)
        {
            VehicleConfig cfg = null;
#if UNITY_EDITOR
            cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleConfig>(
                $"Assets/_Project/ScriptableObjects/Vehicles/{assetName}.asset");
#endif
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<VehicleConfig>();
                cfg.displayName = role.ToString();
                cfg.role = role;
                cfg.primaryColor = primary;
                cfg.accentColor = accent;
                cfg.topSpeedKmh = speed;
                cfg.motorTorque = torque;
                cfg.brakeTorque = 4000f;
            }
            return cfg;
        }

        private void SpawnAIOpponent(string name, VehicleConfig config, Vector3 position, WaypointNode startWaypoint, RaceManager raceManager, float skill)
        {
            GameObject aiCar = CarModelBuilder.BuildProceduralCar(name, config);
            aiCar.transform.position = position;
            aiCar.transform.rotation = Quaternion.identity;
            aiCar.tag = "Untagged"; // Keep "Player" unique to player vehicle

            AIRacingController aiDriver = aiCar.AddComponent<AIRacingController>();
            aiDriver.SetTargetWaypoint(startWaypoint);

            VehicleController controller = aiCar.GetComponent<VehicleController>();
            raceManager.RegisterAI(controller);
        }

        private void BuildGameHUD(VehicleController car, VehicleInputReader inputReader)
        {
            if (FindFirstObjectByType<HUDController>() != null) return;

            GameObject canvasObj = new GameObject("Canvas_HUD");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Driving Telemetry HUD & Mobile Controls
            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();
            hud.BindVehicle(car);
            mobileUI.BindInputReader(inputReader);

            // In-Race Full Loop Overlay UI (Countdown, Pause, Results)
            canvasObj.AddComponent<RaceCountdownUI>();
            canvasObj.AddComponent<PauseMenuUI>();
            canvasObj.AddComponent<RaceResultsUI>();

            // In-Race Mobile Pause Button (Top-Right)
            BuildPauseButton(canvasObj.transform);
        }

        private void BuildPauseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("Btn_HUD_Pause");
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            rt.sizeDelta = new Vector2(120f, 52f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.24f, 0.85f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                RaceManager.Instance?.TogglePause();
            });

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = "⏸ PAUSE";
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.9f, 0.95f, 1f);
        }
    }
}

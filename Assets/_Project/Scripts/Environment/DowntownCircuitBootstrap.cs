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
    /// Master environment and race director for Route 1: Downtown Circuit.
    /// Builds a closed-loop ~1.6km urban racing circuit featuring:
    /// - Sector 1: Avenue of Independence (Start/Finish Pit Straight with towers, murals, neon signs)
    /// - Sector 2: Coastal Ocean Boulevard (Sweeping coastal curve and seaside palm-lined high-speed straight)
    /// - Sector 3: Market District Chicane (Technical S-bends with storefronts, awnings, and dense lighting)
    /// - Sector 4: Highway Overpass & Civic Square (Curved return to main straight)
    /// Complete with 12 sequential anti-cheat checkpoints, 24-node AI waypoint loop,
    /// 4-car starting grid, full in-race HUD, countdown, pause, and podium results.
    /// </summary>
    [ExecuteAlways]
    public class DowntownCircuitBootstrap : MonoBehaviour
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

            if (GameObject.Find("Directional Moonlight") == null &&
                GameObject.Find("Directional_Moonlight") == null &&
                transform.Find("Directional_Moonlight") == null)
            {
                GameObject moonObj = new GameObject("Directional_Moonlight");
                moonObj.transform.SetParent(transform, false);
                moonObj.transform.rotation = Quaternion.Euler(58f, -42f, 0f);

                Light moon = moonObj.AddComponent<Light>();
                moon.type = LightType.Directional;
                moon.color = new Color(0.58f, 0.68f, 0.85f);
                moon.intensity = 0.16f;
                moon.shadows = LightShadows.Soft;
            }
        }

        #region Circuit Geometry Construction

        public void BuildCircuitGeometry()
        {
            GameObject circuitRoot = new GameObject("CircuitGeometry");
            circuitRoot.transform.SetParent(transform, false);

            // Circuit Node Path (Closed Loop ~1.6km)
            Vector3[] circuitTrackPoints = new Vector3[]
            {
                // Sector 1: Avenue of Independence (Start/Finish straight)
                new Vector3(0f, 0f, -40f),
                new Vector3(0f, 0f, 60f),
                new Vector3(0f, 0f, 160f),
                new Vector3(0f, 0f, 240f),

                // Sector 2: Coastal Ocean Boulevard (Sweeping right curve -> ocean straight)
                new Vector3(25f, 0f, 290f),
                new Vector3(70f, 0f, 330f),
                new Vector3(130f, 0f, 345f),
                new Vector3(230f, 0f, 345f),
                new Vector3(330f, 0f, 345f),
                new Vector3(410f, 0f, 335f),

                // Sector 3: Market District Chicane (Turn right South, technical S-curves)
                new Vector3(445f, 0f, 290f),
                new Vector3(440f, 0f, 230f),
                new Vector3(405f, 0f, 180f),
                new Vector3(430f, 0f, 130f),
                new Vector3(420f, 0f, 70f),

                // Sector 4: Highway Overpass & Civic Square Return
                new Vector3(380f, 0f, 10f),
                new Vector3(310f, 0f, -30f),
                new Vector3(220f, 0f, -50f),
                new Vector3(130f, 0f, -50f),
                new Vector3(50f, 0f, -45f)
            };

            // Build connected road segments along the closed circuit
            for (int i = 0; i < circuitTrackPoints.Length; i++)
            {
                Vector3 pA = circuitTrackPoints[i];
                Vector3 pB = circuitTrackPoints[(i + 1) % circuitTrackPoints.Length];

                BuildRoadSegment(circuitRoot.transform, pA, pB, mainRoadWidth, i);
            }

            // Start/Finish Overhead Truss Gantry at Z = 20m
            BuildFinishLineGantry(circuitRoot.transform, new Vector3(0f, 0f, 20f), Quaternion.identity, mainRoadWidth);

            // Landmark Props along Circuit
            SpawnCircuitLandmarks(circuitRoot.transform);
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

            // 1. Asphalt Driving Surface
            GameObject asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asphalt.name = "Asphalt";
            asphalt.transform.SetParent(seg.transform, false);
            asphalt.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            asphalt.transform.localScale = new Vector3(width, 0.1f, length);
            if (matAsphalt != null) asphalt.GetComponent<Renderer>().sharedMaterial = matAsphalt;

            // 2. Curbs
            float halfWidth = width * 0.5f;
            CreateSegmentPart("Curb_L", seg.transform, new Vector3(-(halfWidth + 0.15f), curbHeight * 0.5f, 0f), new Vector3(0.3f, curbHeight, length), matCurb);
            CreateSegmentPart("Curb_R", seg.transform, new Vector3((halfWidth + 0.15f), curbHeight * 0.5f, 0f), new Vector3(0.3f, curbHeight, length), matCurb);

            // 3. Sidewalks
            float swX = halfWidth + 0.3f + sidewalkWidth * 0.5f;
            CreateSegmentPart("Sidewalk_L", seg.transform, new Vector3(-swX, (curbHeight - 0.02f) * 0.5f, 0f), new Vector3(sidewalkWidth, curbHeight - 0.02f, length), matSidewalk);
            CreateSegmentPart("Sidewalk_R", seg.transform, new Vector3(swX, (curbHeight - 0.02f) * 0.5f, 0f), new Vector3(sidewalkWidth, curbHeight - 0.02f, length), matSidewalk);

            // 4. Center Marking (Double yellow)
            GameObject yellowLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            yellowLine.name = "YellowLine";
            yellowLine.transform.SetParent(seg.transform, false);
            yellowLine.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            yellowLine.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            yellowLine.transform.localScale = new Vector3(0.24f, length, 1f);
            if (matMarkingYellow != null) yellowLine.GetComponent<Renderer>().sharedMaterial = matMarkingYellow;
            Collider yCol = yellowLine.GetComponent<Collider>();
            if (yCol != null) DestroyImmediate(yCol);

            // 5. Streetlights at start of segment
            StreetPropsBuilder.CreateStreetlight(seg.transform, new Vector3(-(halfWidth + 0.35f), curbHeight, 0f), Quaternion.identity, matMetal, matStreetlightBulb);
            StreetPropsBuilder.CreateStreetlight(seg.transform, new Vector3(halfWidth + 0.35f, curbHeight, 0f), Quaternion.Euler(0f, 180f, 0f), matMetal, matStreetlightBulb);

            // 6. Crash Safety Barriers on outer perimeter
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

            CreateDownlight(gantryRoot.transform, new Vector3(-3.5f, gantryHeight - 0.2f, 0f));
            CreateDownlight(gantryRoot.transform, new Vector3(3.5f, gantryHeight - 0.2f, 0f));
        }

        private void CreateDownlight(Transform parent, Vector3 localPos)
        {
            GameObject spotObj = new GameObject("FinishSpotlight");
            spotObj.transform.SetParent(parent, false);
            spotObj.transform.localPosition = localPos;
            spotObj.transform.localRotation = Quaternion.Euler(85f, 0f, 0f);

            Light light = spotObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 3.5f;
            light.range = 14f;
            light.spotAngle = 65f;
            light.shadows = LightShadows.None;
        }

        private void SpawnCircuitLandmarks(Transform parent)
        {
            GameObject landmarks = new GameObject("CircuitLandmarks");
            landmarks.transform.SetParent(parent, false);

            // Sector 1: Commercial Towers & Culture Mural
            StreetPropsBuilder.CreateCommercialBuilding(landmarks.transform, new Vector3(-16f, 0f, 70f), Quaternion.Euler(0f, -90f, 0f),
                matCommercialFacade, matWindows, matMural, matMetal, hasMural: true, width: 22f, height: 32f, depth: 18f);

            StreetPropsBuilder.CreateVerticalBladeSign(landmarks.transform, new Vector3(-11f, 3.5f, 60f), Quaternion.identity,
                matMetal, matSignNeonAfrica, 1.4f, 8f, 0.3f);

            StreetPropsBuilder.CreateUrbanBillboard(landmarks.transform, new Vector3(16f, 0f, 30f), Quaternion.Euler(0f, 20f, 0f),
                matMetal, matBillboardCity, 10f, 5f, 6.5f);

            // Sector 2: Ocean Boulevard Cafes & GAMEIUM Billboard
            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(200f, 0f, 358f), Quaternion.Euler(0f, 180f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Ocean_Cafe");

            StreetPropsBuilder.CreateUrbanBillboard(landmarks.transform, new Vector3(340f, 0f, 358f), Quaternion.Euler(0f, 170f, 0f),
                matMetal, matBillboardThegameio, 10f, 5f, 6.5f);

            // Sector 3: Market District Electronics & Murals
            StreetPropsBuilder.CreateStorefront(landmarks.transform, new Vector3(458f, 0f, 210f), Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matSignKilima, matAwning, matGlass, matMetal, "Market_Electronics");

            StreetPropsBuilder.CreateCommercialBuilding(landmarks.transform, new Vector3(430f, 0f, 90f), Quaternion.Euler(0f, 45f, 0f),
                matCommercialFacade, matWindows, null, matMetal, hasMural: false, width: 20f, height: 26f, depth: 16f);

            // Sector 4: Civic Square Tower
            StreetPropsBuilder.CreateCommercialBuilding(landmarks.transform, new Vector3(180f, 0f, -65f), Quaternion.Euler(0f, 0f, 0f),
                matCommercialFacade, matWindows, null, matMetal, hasMural: false, width: 28f, height: 40f, depth: 22f);
        }

        #endregion

        #region Race Setup & Systems

        private void SpawnRaceGridAndSystems()
        {
            // 1. Spawn Player Vehicle at Grid Slot 01
            Vector3 playerPos = new Vector3(-1.8f, 0.2f, 15f);
            playerCar = CarModelBuilder.BuildProceduralCar("PlayerCar_Downtown", playerConfig);
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

            // 3. Master HUD & Driving UI
            BuildGameHUD(carController, inputReader);

            // 4. Circuit Race System (Checkpoints, AI Waypoints, AI Opponents)
            SetupCircuitRaceSystem(carController);
        }

        private void SetupCircuitRaceSystem(VehicleController playerVehicle)
        {
            GameObject raceRoot = new GameObject("Race_System");
            raceRoot.transform.SetParent(transform, false);

            CheckpointManager cpManager = raceRoot.AddComponent<CheckpointManager>();
            cpManager.SetTotalLaps(totalLaps);
            RaceManager raceManager = raceRoot.AddComponent<RaceManager>();

            // 12 Sequential Checkpoints forming closed loop
            Vector3[] cpPoints = new Vector3[]
            {
                new Vector3(0f, 2.5f, 20f),      // CP 0: Start / Finish Line
                new Vector3(0f, 2.5f, 140f),     // CP 1: Avenue mid straight
                new Vector3(20f, 2.5f, 280f),    // CP 2: Entry coastal bend
                new Vector3(90f, 2.5f, 340f),    // CP 3: Coastal apex
                new Vector3(210f, 2.5f, 345f),   // CP 4: Ocean Boulevard West
                new Vector3(340f, 2.5f, 345f),   // CP 5: Ocean Boulevard East
                new Vector3(435f, 2.5f, 300f),   // CP 6: Market entry turn
                new Vector3(425f, 2.5f, 200f),   // CP 7: Market Chicane 1
                new Vector3(415f, 2.5f, 100f),   // CP 8: Market Chicane 2
                new Vector3(350f, 2.5f, -10f),   // CP 9: Highway entrance
                new Vector3(220f, 2.5f, -50f),   // CP 10: Highway sweep
                new Vector3(40f, 2.5f, -45f)     // CP 11: Civic Square final turn before finish
            };

            List<Checkpoint> checkpoints = new List<Checkpoint>();
            for (int i = 0; i < cpPoints.Length; i++)
            {
                bool isFinish = (i == 0); // In closed circuits, CP 0 is the Lap & Finish Line
                GameObject cpObj = new GameObject($"Checkpoint_{i:00}" + (isFinish ? "_FinishLine" : ""));
                cpObj.transform.SetParent(raceRoot.transform, false);
                cpObj.transform.position = cpPoints[i];

                // Orient checkpoint forward along route
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

            // 24 Continuous Looping AI Waypoints
            WaypointNode[] aiNodes = BuildCircuitWaypointLoop(raceRoot.transform);

            // Register Player in Master Race Manager
            raceManager.RegisterPlayer(playerVehicle);

            // Spawn 3 AI Competitors on Grid Slots 2, 3, and 4
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

        private WaypointNode[] BuildCircuitWaypointLoop(Transform parent)
        {
            GameObject wpRoot = new GameObject("AI_Waypoint_Loop");
            wpRoot.transform.SetParent(parent, false);

            Vector3[] path = new Vector3[]
            {
                // Sector 1: Avenue of Independence (Start straight)
                new Vector3(0f, 0.2f, 20f),
                new Vector3(0f, 0.2f, 90f),
                new Vector3(0f, 0.2f, 160f),
                new Vector3(0f, 0.2f, 230f),

                // Sector 2: Coastal Ocean Boulevard (Sweeping corner & straight)
                new Vector3(20f, 0.2f, 280f),
                new Vector3(55f, 0.2f, 320f),
                new Vector3(105f, 0.2f, 342f),
                new Vector3(170f, 0.2f, 345f),
                new Vector3(240f, 0.2f, 345f),
                new Vector3(310f, 0.2f, 345f),
                new Vector3(375f, 0.2f, 342f),

                // Sector 3: Market District Chicane (Technical S-bends)
                new Vector3(430f, 0.2f, 310f),
                new Vector3(442f, 0.2f, 260f),
                new Vector3(432f, 0.2f, 210f),
                new Vector3(408f, 0.2f, 170f),
                new Vector3(424f, 0.2f, 125f),
                new Vector3(420f, 0.2f, 75f),

                // Sector 4: Highway Overpass & Civic Square Return
                new Vector3(390f, 0.2f, 20f),
                new Vector3(335f, 0.2f, -20f),
                new Vector3(255f, 0.2f, -48f),
                new Vector3(175f, 0.2f, -50f),
                new Vector3(95f, 0.2f, -48f),
                new Vector3(25f, 0.2f, -40f),
                new Vector3(0f, 0.2f, -15f)
            };

            float[] speeds = new float[]
            {
                210f, 220f, 220f, 180f, // S1 straight
                145f, 135f, 155f, 225f, 230f, 230f, 195f, // S2 coastal sweeper & straight
                140f, 125f, 120f, 110f, 115f, 135f, // S3 technical market chicanes
                160f, 165f, 155f, 170f, 165f, 140f, 180f // S4 highway & return to straight
            };

            WaypointNode[] nodes = new WaypointNode[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                GameObject obj = new GameObject($"Waypoint_{i:00}");
                obj.transform.SetParent(wpRoot.transform, false);
                obj.transform.position = path[i];
                nodes[i] = obj.AddComponent<WaypointNode>();
            }

            // Connect nodes into an infinite loop: nodes[23].NextNode = nodes[0]
            for (int i = 0; i < nodes.Length; i++)
            {
                WaypointNode next = nodes[(i + 1) % nodes.Length];
                nodes[i].SetParameters(speeds[i], 6.5f, next);
            }

            return nodes;
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
            aiCar.tag = "Untagged";

            AIRacingController aiDriver = aiCar.AddComponent<AIRacingController>();
            aiDriver.SetTargetWaypoint(startWaypoint);

            VehicleController controller = aiCar.GetComponent<VehicleController>();
            raceManager.RegisterAI(controller);
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

            // Telemetry HUD & Mobile Touch Controls
            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();
            hud.BindVehicle(car);
            mobileUI.BindInputReader(inputReader);

            // In-Race Full Loop Overlays
            canvasObj.AddComponent<RaceCountdownUI>();
            canvasObj.AddComponent<PauseMenuUI>();
            canvasObj.AddComponent<RaceResultsUI>();

            // Pause Button
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

        #endregion
    }
}

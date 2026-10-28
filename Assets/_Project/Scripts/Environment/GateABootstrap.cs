using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThegameioRacing.Vehicle;
using ThegameioRacing.Camera;
using ThegameioRacing.Input;
using ThegameioRacing.UI;

namespace ThegameioRacing.Environment
{
    /// <summary>
    /// Runtime and in-editor bootstrapper for Gate A: Representative Nighttime Street Sample.
    /// Builds the 120m African coastal avenue with Poly Haven CC0 PBR textures,
    /// authentic streetlights, storefronts, commercial towers with lit windows,
    /// race crash barriers, billboards, spectators, player car, and HUD.
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

            if (transform.Find("Directional_Moonlight") == null)
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

            // 3. Streetlights
            float streetHalf = roadWidth * 0.5f;
            float[] lightPositionsZ = new float[] { 10f, 35f, 60f, 85f, 110f };
            foreach (float lz in lightPositionsZ)
            {
                StreetPropsBuilder.CreateStreetlight(
                    streetRoot.transform,
                    new Vector3(-(streetHalf + 0.35f), 0.18f, lz),
                    Quaternion.Euler(0f, 90f, 0f),
                    matMetal, matStreetlightBulb);

                StreetPropsBuilder.CreateStreetlight(
                    streetRoot.transform,
                    new Vector3(streetHalf + 0.35f, 0.18f, lz),
                    Quaternion.Euler(0f, -90f, 0f),
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
            float bldLineLeft = -(streetHalf + 4.0f);
            float bldLineRight = (streetHalf + 4.0f);

            StreetPropsBuilder.CreateStorefront(
                streetRoot.transform, new Vector3(bldLineLeft, 0.18f, 24f), Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matSignKilima, matAwning, matGlass, matMetal, "Kilima_Electronics");

            StreetPropsBuilder.CreateStorefront(
                streetRoot.transform, new Vector3(bldLineLeft, 0.18f, 40f), Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal, "Coastal_Breeze_Cafe");

            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineLeft - 2f, 0.18f, 68f), Quaternion.Euler(0f, 90f, 0f),
                matCommercialFacade, matWindows, matMural, matMetal,
                hasMural: true, width: 20f, height: 26f, depth: 16f);

            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineRight + 2f, 0.18f, 32f), Quaternion.Euler(0f, -90f, 0f),
                matCommercialFacade, matWindows, null, matMetal,
                hasMural: false, width: 22f, height: 32f, depth: 18f);

            StreetPropsBuilder.CreateCommercialBuilding(
                streetRoot.transform, new Vector3(bldLineRight + 1f, 0.18f, 66f), Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matWindows, null, matMetal,
                hasMural: false, width: 18f, height: 20f, depth: 14f);

            // 7. Urban Billboards
            StreetPropsBuilder.CreateUrbanBillboard(
                streetRoot.transform, new Vector3(bldLineRight + 1.5f, 0.18f, 14f), Quaternion.Euler(0f, -75f, 0f),
                matMetal, matBillboardCity, 9.5f, 4.8f, 6f);

            StreetPropsBuilder.CreateUrbanBillboard(
                streetRoot.transform, new Vector3(bldLineLeft - 1.5f, 0.18f, 92f), Quaternion.Euler(0f, 105f, 0f),
                matMetal, matBillboardThegameio, 9.5f, 4.8f, 6f);
        }

        private void SpawnCarAndControls()
        {
            Vector3 spawnPos = new Vector3(-1.75f, 0.2f, 15f); // Grid Slot 01
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

            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();
            hud.BindVehicle(car);
            mobileUI.BindInputReader(inputReader);
        }
    }
}

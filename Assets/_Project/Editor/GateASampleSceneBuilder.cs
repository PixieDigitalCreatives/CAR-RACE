using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ThegameioRacing.Vehicle;
using ThegameioRacing.Camera;
using ThegameioRacing.Input;
using ThegameioRacing.UI;
using ThegameioRacing.Environment;

namespace ThegameioRacing.EditorTools
{
    public static class GateASampleSceneBuilder
    {
        public const string SCENE_PATH = "Assets/_Project/Scenes/SampleStreet_GateA.unity";
        private const string MAT_DIR = "Assets/_Project/Art/Materials";
        private const string TEX_PBR_DIR = "Assets/_Project/Art/Textures/PBR";
        private const string TEX_ENV_DIR = "Assets/_Project/Art/Textures/Environment";
        private const string CONFIG_DIR = "Assets/_Project/ScriptableObjects/Vehicles";

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            string triggerPath = "Temp/trigger_build_gate_a.flag";
            if (File.Exists(triggerPath))
            {
                EditorApplication.update -= CheckTrigger;
                try { File.Delete(triggerPath); } catch {}
                BuildGateAScene();
                EditorApplication.update += CheckTrigger;
            }
        }

        [MenuItem("Tools/GAMEIUM/Build Gate A Street Sample Scene", false, 2)]
        public static void BuildGateAScene()
        {
            Debug.Log("[GAMEIUM] Generating Gate A - Representative Nighttime Street Sample...");

            EnsureDirectories();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Shaders & Materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            Material matAsphalt = GetOrCreatePbrMat(litShader, "M_Road_Asphalt",
                $"{TEX_PBR_DIR}/Asphalt/asphalt_02_diff_1k.jpg",
                $"{TEX_PBR_DIR}/Asphalt/asphalt_02_nor_1k.jpg",
                new Vector2(0.12f, 0.12f), 0.35f, 0.05f, new Color(0.82f, 0.85f, 0.90f));

            Material matSidewalk = GetOrCreatePbrMat(litShader, "M_Sidewalk_Pavers",
                $"{TEX_PBR_DIR}/Pavement/concrete_pavement_03_diff_1k.jpg",
                $"{TEX_PBR_DIR}/Pavement/concrete_pavement_03_nor_1k.jpg",
                new Vector2(0.25f, 0.25f), 0.22f, 0.02f, Color.white);

            Material matCurb = GetOrCreatePbrMat(litShader, "M_Curbs",
                $"{TEX_PBR_DIR}/Pavement/concrete_pavement_03_diff_1k.jpg",
                $"{TEX_PBR_DIR}/Pavement/concrete_pavement_03_nor_1k.jpg",
                new Vector2(0.5f, 0.5f), 0.28f, 0.02f, new Color(0.88f, 0.88f, 0.88f));

            Material matCommercialFacade = GetOrCreatePbrMat(litShader, "M_Building_CommercialFacade",
                $"{TEX_PBR_DIR}/BuildingFacade/concrete_tile_facade_diff_1k.jpg",
                $"{TEX_PBR_DIR}/BuildingFacade/concrete_tile_facade_nor_1k.jpg",
                new Vector2(0.2f, 0.2f), 0.25f, 0.05f, Color.white);

            Material matPlaster = GetOrCreatePbrMat(litShader, "M_Building_ResidentialPlaster",
                $"{TEX_PBR_DIR}/PlasterWall/painted_plaster_wall_diff_1k.jpg",
                $"{TEX_PBR_DIR}/PlasterWall/painted_plaster_wall_nor_1k.jpg",
                new Vector2(0.25f, 0.25f), 0.18f, 0.02f, new Color(0.92f, 0.88f, 0.82f));

            Material matMetal = GetOrCreatePbrMat(litShader, "M_Metal_Plate",
                $"{TEX_PBR_DIR}/Metal/metal_plate_diff_1k.jpg",
                $"{TEX_PBR_DIR}/Metal/metal_plate_nor_1k.jpg",
                new Vector2(0.5f, 0.5f), 0.55f, 0.75f, Color.white);

            // Road Markings
            Material matMarkingWhite = CreateSimpleMat(litShader, "M_Road_Marking_White", new Color(0.95f, 0.95f, 0.98f), 0.3f, 0.0f);
            Material matMarkingYellow = CreateSimpleMat(litShader, "M_Road_Marking_Yellow", new Color(0.96f, 0.78f, 0.12f), 0.3f, 0.0f);

            // Barrier Chevron
            Material matBarrierChevron = GetOrCreateTexMat(litShader, "M_Barrier_Chevron",
                $"{TEX_ENV_DIR}/tex_barrier_chevron.png", new Vector2(1f, 1f), 0.35f, 0.1f);

            // Storefront Signs (Emissive Backlit)
            Material matSignKilima = CreateEmissiveTexMat(litShader, "M_Sign_Kilima",
                $"{TEX_ENV_DIR}/tex_sign_storefront_01.png", new Color(2.4f, 1.8f, 1.1f));

            Material matSignCafe = CreateEmissiveTexMat(litShader, "M_Sign_Cafe",
                $"{TEX_ENV_DIR}/tex_sign_storefront_02.png", new Color(2.5f, 1.6f, 0.9f));

            // Awning Fabric
            Material matAwning = GetOrCreateTexMat(litShader, "M_Awning_Fabric",
                $"{TEX_ENV_DIR}/tex_awning_fabric.png", new Vector2(4f, 1f), 0.15f, 0.0f);

            // Windows Grid (Emissive Lit Matrix)
            Material matWindows = CreateEmissiveTexMat(litShader, "M_Building_Windows",
                $"{TEX_ENV_DIR}/tex_building_windows.png", new Color(1.8f, 1.4f, 0.9f));

            // Cultural Mural
            Material matMural = GetOrCreateTexMat(litShader, "M_Mural_AfricanCulture",
                $"{TEX_ENV_DIR}/tex_mural_african_culture.png", Vector2.one, 0.25f, 0.05f);

            // Urban Billboards
            Material matBillboardCity = CreateEmissiveTexMat(litShader, "M_Billboard_OurCity",
                $"{TEX_ENV_DIR}/tex_billboard_our_city.png", new Color(1.2f, 1.2f, 1.2f));

            Material matBillboardThegameio = CreateEmissiveTexMat(litShader, "M_Billboard_Thegameio",
                $"{TEX_ENV_DIR}/tex_billboard_thegameio.png", new Color(1.3f, 1.3f, 1.3f));

            // Streetlight Bulb (High-Intensity Warm Emissive)
            Material matStreetlightBulb = CreateEmissiveMat(litShader, "M_Streetlight_Bulb",
                new Color(1f, 0.82f, 0.52f), new Color(4.5f, 3.2f, 1.5f));

            // Glass Material
            Material matGlass = CreateSimpleMat(litShader, "M_Store_Glass", new Color(0.12f, 0.18f, 0.25f, 0.85f), 0.92f, 0.1f);

            // Vertical Neon Sign (Vibrant Neon Glow)
            Material matSignNeonAfrica = CreateEmissiveTexMat(litShader, "M_Sign_Neon_Africa",
                $"{TEX_ENV_DIR}/tex_sign_neon_africa.png", new Color(2.2f, 0.8f, 2.5f));

            // =========================================================================
            // 2. ROOT GAME OBJECTS
            // =========================================================================
            GameObject sceneRoot = new GameObject("GateA_StreetSample");

            // Atmosphere & Nighttime Lighting
            SetupNightLighting(sceneRoot);

            // Avenue Roadway & Markings
            GameObject roadSection = StreetPropsBuilder.CreateRoadAvenue(
                sceneRoot.transform, matAsphalt, matSidewalk, matCurb,
                length: 120f, roadWidth: 14f, sidewalkWidth: 3.8f, curbHeight: 0.18f);

            StreetPropsBuilder.CreateRoadMarkings(
                sceneRoot.transform, matMarkingWhite, matMarkingYellow, length: 120f, roadWidth: 14f);

            // Streetlights (Overhanging the driving lanes)
            float[] lightPositionsZ = new float[] { 10f, 35f, 60f, 85f, 110f };
            float streetHalf = 7f;
            foreach (float lz in lightPositionsZ)
            {
                // Left Streetlight (facing roadway towards +X)
                StreetPropsBuilder.CreateStreetlight(
                    sceneRoot.transform,
                    new Vector3(-(streetHalf + 0.35f), 0.18f, lz),
                    Quaternion.identity,
                    matMetal, matStreetlightBulb);

                // Right Streetlight (facing roadway towards -X)
                StreetPropsBuilder.CreateStreetlight(
                    sceneRoot.transform,
                    new Vector3(streetHalf + 0.35f, 0.18f, lz),
                    Quaternion.Euler(0f, 180f, 0f),
                    matMetal, matStreetlightBulb);
            }

            // Safety Crash Barriers with Chevron Markings
            float barrierOffsetLeft = -(streetHalf + 0.15f);
            float barrierOffsetRight = (streetHalf + 0.15f);
            for (float bz = 8f; bz <= 112f; bz += 4f)
            {
                // Left barrier line
                StreetPropsBuilder.CreateSafetyBarrier(
                    sceneRoot.transform,
                    new Vector3(barrierOffsetLeft, 0.18f, bz),
                    Quaternion.identity,
                    matMetal, matBarrierChevron, length: 3.9f);

                // Right barrier line
                StreetPropsBuilder.CreateSafetyBarrier(
                    sceneRoot.transform,
                    new Vector3(barrierOffsetRight, 0.18f, bz),
                    Quaternion.identity,
                    matMetal, matBarrierChevron, length: 3.9f);
            }

            // Spectator Clusters (Safely positioned on sidewalks behind barriers)
            StreetPropsBuilder.CreateSpectatorCluster(sceneRoot.transform, new Vector3(-(streetHalf + 2.0f), 0.18f, 22f), Quaternion.Euler(0f, 85f, 0f), 4);
            StreetPropsBuilder.CreateSpectatorCluster(sceneRoot.transform, new Vector3(-(streetHalf + 2.2f), 0.18f, 55f), Quaternion.Euler(0f, 95f, 0f), 5);
            StreetPropsBuilder.CreateSpectatorCluster(sceneRoot.transform, new Vector3((streetHalf + 2.0f), 0.18f, 26f), Quaternion.Euler(0f, -80f, 0f), 4);
            StreetPropsBuilder.CreateSpectatorCluster(sceneRoot.transform, new Vector3((streetHalf + 2.1f), 0.18f, 72f), Quaternion.Euler(0f, -90f, 0f), 6);

            // =========================================================================
            // 3. BUILDINGS & STOREFRONTS (MATCHING THE POWERPOINT)
            // =========================================================================
            float buildingLineLeft = -(streetHalf + 3.8f);
            float buildingLineRight = (streetHalf + 3.8f);

            // Storefront 1: Kilima Electronics & Mobile Pay (Left, Z = 24m)
            StreetPropsBuilder.CreateStorefront(
                sceneRoot.transform,
                new Vector3(buildingLineLeft, 0.18f, 24f),
                Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignKilima, matAwning, matGlass, matMetal,
                "Kilima_Electronics");

            // Storefront 2: Coastal Breeze Bistro & Cafe (Left, Z = 40m)
            StreetPropsBuilder.CreateStorefront(
                sceneRoot.transform,
                new Vector3(buildingLineLeft, 0.18f, 40f),
                Quaternion.Euler(0f, -90f, 0f),
                matPlaster, matSignCafe, matAwning, matGlass, matMetal,
                "Coastal_Breeze_Cafe");

            // Commercial Tower with African Cultural Mural (Left, Z = 68m)
            StreetPropsBuilder.CreateCommercialBuilding(
                sceneRoot.transform,
                new Vector3(buildingLineLeft - 2f, 0.18f, 68f),
                Quaternion.Euler(0f, -90f, 0f),
                matCommercialFacade, matWindows, matMural, matMetal,
                hasMural: true, width: 20f, height: 26f, depth: 16f);

            // Vertical Neon Blade Sign ("AFRICA") beside the mural at Z = 58m
            StreetPropsBuilder.CreateVerticalBladeSign(
                sceneRoot.transform,
                new Vector3(buildingLineLeft + 0.5f, 2.8f, 58f),
                Quaternion.identity,
                matMetal, matSignNeonAfrica,
                width: 1.4f, height: 7.5f, depth: 0.25f);

            // Right-Side Commercial Building (Right, Z = 32m)
            StreetPropsBuilder.CreateCommercialBuilding(
                sceneRoot.transform,
                new Vector3(buildingLineRight + 2f, 0.18f, 32f),
                Quaternion.Euler(0f, 90f, 0f),
                matCommercialFacade, matWindows, null, matMetal,
                hasMural: false, width: 22f, height: 32f, depth: 18f);

            // Right-Side Residential Mixed Building (Right, Z = 66m)
            StreetPropsBuilder.CreateCommercialBuilding(
                sceneRoot.transform,
                new Vector3(buildingLineRight + 1f, 0.18f, 66f),
                Quaternion.Euler(0f, 90f, 0f),
                matPlaster, matWindows, null, matMetal,
                hasMural: false, width: 18f, height: 20f, depth: 14f);

            // =========================================================================
            // 4. URBAN BILLBOARDS (ORIENTED TOWARDS ONCOMING TRAFFIC)
            // =========================================================================
            // "OUR CITY OUR STRENGTH" Billboard (Right, Z = 14m)
            StreetPropsBuilder.CreateUrbanBillboard(
                sceneRoot.transform,
                new Vector3(buildingLineRight + 1.5f, 0.18f, 14f),
                Quaternion.Euler(0f, 15f, 0f),
                matMetal, matBillboardCity,
                boardWidth: 9.5f, boardHeight: 4.8f, postHeight: 6f);

            // "GAMEIUM" Billboard (Left, Z = 92m)
            StreetPropsBuilder.CreateUrbanBillboard(
                sceneRoot.transform,
                new Vector3(buildingLineLeft - 1.5f, 0.18f, 92f),
                Quaternion.Euler(0f, -15f, 0f),
                matMetal, matBillboardThegameio,
                boardWidth: 9.5f, boardHeight: 4.8f, postHeight: 6f);

            // =========================================================================
            // 5. PLAYER CAR, CAMERA & HUD
            // =========================================================================
            VehicleConfig starterConfig = AssetDatabase.LoadAssetAtPath<VehicleConfig>($"{CONFIG_DIR}/VehicleConfig_Regular.asset");
            if (starterConfig == null)
            {
                TestTrackSceneBuilder.CreateAllVehicleConfigs();
                starterConfig = AssetDatabase.LoadAssetAtPath<VehicleConfig>($"{CONFIG_DIR}/VehicleConfig_Regular.asset");
            }

            GameObject carObj = CarModelBuilder.BuildProceduralCar("PlayerCar_GateA", starterConfig);
            // Placed in starting grid slot 1
            carObj.transform.position = new Vector3(-1.75f, 0.2f, 15f);
            carObj.transform.rotation = Quaternion.identity;

            VehicleInputReader inputReader = carObj.AddComponent<VehicleInputReader>();
            VehicleController carController = carObj.GetComponent<VehicleController>();
            carController.SetInputSource(inputReader);

            // Main Camera with Chase Controller
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            camObj.AddComponent<AudioListener>();
            VehicleCameraController camController = camObj.AddComponent<VehicleCameraController>();
            camController.SetTarget(carObj.transform);
            camObj.transform.position = carObj.transform.position + new Vector3(0f, 2.6f, -6.5f);
            camObj.transform.LookAt(carObj.transform.position + Vector3.up * 1.1f);

            // In-Race HUD & Touch Driving UI
            BuildGameHUD(carController, inputReader);

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            TestTrackSceneBuilder.AddSceneToBuildSettings(SCENE_PATH);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GAMEIUM] Gate A Scene successfully generated and verified at: {SCENE_PATH}");
        }

        private static void SetupNightLighting(GameObject root)
        {
            // Faint cool moonlight (African coastal night sky)
            GameObject moonObj = new GameObject("Moonlight_Directional");
            moonObj.transform.SetParent(root.transform, false);
            moonObj.transform.rotation = Quaternion.Euler(58f, -42f, 0f);

            Light moon = moonObj.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.58f, 0.68f, 0.85f); // Cool silvery night blue
            moon.intensity = 0.16f;
            moon.shadows = LightShadows.Soft;

            // Ambient Lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.04f, 0.07f, 0.13f);     // Deep midnight indigo
            RenderSettings.ambientEquatorColor = new Color(0.06f, 0.09f, 0.16f); // Cool slate horizon
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.03f, 0.05f);  // Dark asphalt bounce
        }

        private static void BuildGameHUD(VehicleController car, VehicleInputReader inputReader)
        {
            GameObject canvasObj = new GameObject("Canvas_HUD");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();
            hud.BindVehicle(car);
            mobileUI.BindInputReader(inputReader);
        }

        // =========================================================================
        // MATERIAL HELPERS
        // =========================================================================

        private static Material GetOrCreatePbrMat(
            Shader shader, string matName, string diffPath, string norPath,
            Vector2 tiling, float smoothness, float metallic, Color tint)
        {
            string matPath = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            Texture2D diff = AssetDatabase.LoadAssetAtPath<Texture2D>(diffPath);
            if (diff != null)
            {
                mat.SetTexture("_BaseMap", diff);
                mat.SetTextureScale("_BaseMap", tiling);
            }

            Texture2D nor = AssetDatabase.LoadAssetAtPath<Texture2D>(norPath);
            if (nor != null)
            {
                mat.SetTexture("_BumpMap", nor);
                mat.SetTextureScale("_BumpMap", tiling);
                mat.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material GetOrCreateTexMat(
            Shader shader, string matName, string texPath, Vector2 tiling, float smoothness, float metallic)
        {
            string matPath = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTextureScale("_BaseMap", tiling);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateEmissiveTexMat(Shader shader, string matName, string texPath, Color emissiveColor)
        {
            string matPath = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetTexture("_EmissionMap", tex);
            }

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissiveColor);
            mat.SetFloat("_Smoothness", 0.4f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateEmissiveMat(Shader shader, string matName, Color baseColor, Color emissiveColor)
        {
            string matPath = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetColor("_BaseColor", baseColor);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissiveColor);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateSimpleMat(Shader shader, string matName, Color color, float smoothness, float metallic)
        {
            string matPath = $"{MAT_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/_Project/Scenes")) Directory.CreateDirectory("Assets/_Project/Scenes");
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);
            if (!Directory.Exists(CONFIG_DIR)) Directory.CreateDirectory(CONFIG_DIR);
        }
    }
}

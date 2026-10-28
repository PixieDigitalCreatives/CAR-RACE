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

namespace ThegameioRacing.EditorTools
{
    public static class TestTrackSceneBuilder
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/VehiclePhysicsTest.unity";
        private const string CONFIG_DIR = "Assets/_Project/ScriptableObjects/Vehicles";

        [MenuItem("Tools/GAMEIUM/Build Vehicle Physics Test Scene", false, 1)]
        public static void BuildTestScene()
        {
            Debug.Log("[GAMEIUM] Starting Test Track Scene generation...");

            EnsureDirectories();
            CreateAllVehicleConfigs();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Shaders
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

            // Materials
            Material matAsphalt = CreateMat(litShader, "Mat_TrackAsphalt", new Color(0.18f, 0.18f, 0.20f), 0.25f, 0.05f);
            Material matMarkingWhite = CreateMat(litShader, "Mat_RoadMarkingWhite", new Color(0.92f, 0.92f, 0.94f), 0.3f, 0.0f);
            Material matMarkingYellow = CreateMat(litShader, "Mat_RoadMarkingYellow", new Color(0.95f, 0.78f, 0.12f), 0.3f, 0.0f);
            Material matBarrier = CreateMat(litShader, "Mat_TrackBarrier", new Color(0.85f, 0.25f, 0.22f), 0.4f, 0.3f);
            Material matCurbRed = CreateMat(litShader, "Mat_CurbRed", new Color(0.85f, 0.15f, 0.15f), 0.3f, 0.0f);
            Material matCurbWhite = CreateMat(litShader, "Mat_CurbWhite", new Color(0.9f, 0.9f, 0.9f), 0.3f, 0.0f);
            Material matStreetlightWarm = CreateEmissiveMat(litShader, "Mat_StreetlightEmissive", new Color(1f, 0.85f, 0.55f), new Color(3.5f, 2.6f, 1.2f));

            // Environment Root
            GameObject envRoot = new GameObject("Environment");

            // 1. Atmosphere & Lighting (African Coastal City Night)
            SetupNightLighting(envRoot, matStreetlightWarm);

            // 2. Track Surface and Elements
            GameObject trackRoot = new GameObject("TrackGeometry");
            trackRoot.transform.SetParent(envRoot.transform, false);

            BuildTrackSurfaces(trackRoot.transform, matAsphalt, matMarkingWhite, matMarkingYellow, matBarrier, matCurbRed, matCurbWhite, matStreetlightWarm);

            // 3. Vehicle Setup
            VehicleConfig starterConfig = AssetDatabase.LoadAssetAtPath<VehicleConfig>($"{CONFIG_DIR}/VehicleConfig_Regular.asset");
            if (starterConfig == null)
            {
                starterConfig = ScriptableObject.CreateInstance<VehicleConfig>();
                starterConfig.vehicleId = "car_regular";
                starterConfig.displayName = "Regular";
                AssetDatabase.CreateAsset(starterConfig, $"{CONFIG_DIR}/VehicleConfig_Regular.asset");
            }

            GameObject carObj = CarModelBuilder.BuildProceduralCar("PlayerCar_Regular", starterConfig);
            carObj.transform.position = new Vector3(0f, 0.1f, 10f);
            carObj.transform.rotation = Quaternion.identity;

            VehicleInputReader inputReader = carObj.AddComponent<VehicleInputReader>();
            VehicleController carController = carObj.GetComponent<VehicleController>();
            carController.SetInputSource(inputReader);

            // 4. Smooth Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            UnityEngine.Camera cam = camObj.AddComponent<UnityEngine.Camera>();
            camObj.AddComponent<AudioListener>();
            VehicleCameraController camController = camObj.AddComponent<VehicleCameraController>();
            camController.SetTarget(carObj.transform);
            camObj.transform.position = carObj.transform.position + new Vector3(0f, 3f, -7f);
            camObj.transform.LookAt(carObj.transform.position + Vector3.up * 1.2f);

            // 5. Canvas HUD & Mobile Touch Controls
            BuildGameHUD(carController, inputReader);

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddSceneToBuildSettings(SCENE_PATH);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GAMEIUM] Test Scene successfully generated at: {SCENE_PATH}");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/_Project/Scenes")) Directory.CreateDirectory("Assets/_Project/Scenes");
            if (!Directory.Exists(CONFIG_DIR)) Directory.CreateDirectory(CONFIG_DIR);
            if (!Directory.Exists("Assets/_Project/Art/Materials")) Directory.CreateDirectory("Assets/_Project/Art/Materials");
        }

        public static void CreateAllVehicleConfigs()
        {
            // 1. Regular
            CreateOrUpdateConfig("VehicleConfig_Regular", "car_regular", "Regular",
                "Balanced starter vehicle. Predictable steering, accessible handling for beginners.",
                VehicleRole.StarterBalanced, 1350f, 215f, 1900f, 4000f, 34f, 14f, 65, 60, 75, 70,
                new Color(0.12f, 0.45f, 0.95f), new Color(0.98f, 0.55f, 0.12f), DrivetrainType.AllWheelDrive,
                "Assets/_Project/Art/Textures/Car_Regular_Card.png");

            // 2. Tuner / Drift
            CreateOrUpdateConfig("VehicleConfig_TunerDrift", "car_tuner_drift", "Tuner / Drift",
                "Modified street performance car. Strong acceleration, adjustable rear grip for aggressive drift rotation.",
                VehicleRole.TunerDrift, 1250f, 240f, 2400f, 4200f, 38f, 16f, 80, 85, 78, 68,
                new Color(0.92f, 0.12f, 0.22f), new Color(0.15f, 0.15f, 0.15f), DrivetrainType.RearWheelDrive,
                "Assets/_Project/Art/Textures/Car_TunerDrift_Card.png");

            // 3. Speedo Hypercar
            CreateOrUpdateConfig("VehicleConfig_Speedo", "car_speedo", "Speedo Hypercar",
                "Highest top-speed vehicle. Extreme straight-line acceleration and aerodynamic stability.",
                VehicleRole.HypercarSpeed, 1420f, 320f, 3100f, 5200f, 28f, 10f, 98, 95, 65, 80,
                new Color(0.96f, 0.82f, 0.08f), new Color(0.08f, 0.08f, 0.10f), DrivetrainType.AllWheelDrive,
                "Assets/_Project/Art/Textures/Car_Speedo_Card.png");

            // 4. Classic Muscle
            CreateOrUpdateConfig("VehicleConfig_ClassicMuscle", "car_classic_muscle", "Classic Muscle",
                "High-torque heavy vehicle. Raw low-end torque, authoritative engine rumble and broad stance.",
                VehicleRole.ClassicMuscle, 1680f, 225f, 2800f, 3800f, 32f, 12f, 75, 80, 55, 62,
                new Color(0.22f, 0.22f, 0.24f), new Color(0.92f, 0.35f, 0.12f), DrivetrainType.RearWheelDrive,
                "Assets/_Project/Art/Textures/Car_ClassicMuscle_Card.png");

            // 5. Street Rally
            CreateOrUpdateConfig("VehicleConfig_StreetRally", "car_street_rally", "Street Rally",
                "Grip and corner-control specialist. Razor-sharp steering, high traction and agile direction changes.",
                VehicleRole.StreetRally, 1200f, 230f, 2100f, 4500f, 36f, 15f, 72, 78, 95, 88,
                new Color(0.08f, 0.75f, 0.42f), new Color(0.95f, 0.95f, 0.95f), DrivetrainType.AllWheelDrive,
                "Assets/_Project/Art/Textures/Car_StreetRally_Card.png");
        }

        private static void CreateOrUpdateConfig(
            string assetName, string id, string displayName, string desc,
            VehicleRole role, float mass, float topSpeed, float motorTorque, float brakeTorque,
            float maxSteer, float highSpeedSteer, int statSpeed, int statAccel, int statHandling, int statBraking,
            Color primary, Color accent, DrivetrainType drivetrain, string cardTexturePath)
        {
            string path = $"{CONFIG_DIR}/{assetName}.asset";
            VehicleConfig config = AssetDatabase.LoadAssetAtPath<VehicleConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<VehicleConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.vehicleId = id;
            config.displayName = displayName;
            config.description = desc;
            config.role = role;
            config.mass = mass;
            config.topSpeedKmh = topSpeed;
            config.motorTorque = motorTorque;
            config.brakeTorque = brakeTorque;
            config.maxSteerAngle = maxSteer;
            config.highSpeedSteerAngle = highSpeedSteer;
            config.statTopSpeed = statSpeed;
            config.statAcceleration = statAccel;
            config.statHandling = statHandling;
            config.statBraking = statBraking;
            config.primaryColor = primary;
            config.accentColor = accent;
            config.drivetrain = drivetrain;

            Texture2D card = AssetDatabase.LoadAssetAtPath<Texture2D>(cardTexturePath);
            if (card != null) config.previewCard = card;

            EditorUtility.SetDirty(config);
        }

        private static void SetupNightLighting(GameObject root, Material streetlightMat)
        {
            // Directional Light: Deep cool moonlight
            GameObject moonLight = new GameObject("Moonlight_Directional");
            moonLight.transform.SetParent(root.transform, false);
            moonLight.transform.rotation = Quaternion.Euler(50f, -40f, 0f);

            Light lightComp = moonLight.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(0.45f, 0.58f, 0.78f);
            lightComp.intensity = 0.55f;
            lightComp.shadows = LightShadows.Soft;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.11f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.05f, 0.07f, 0.12f);
            RenderSettings.fogDensity = 0.0035f;
        }

        private static void BuildTrackSurfaces(
            Transform parent, Material matAsphalt, Material matWhite, Material matYellow,
            Material matBarrier, Material matCurbRed, Material matCurbWhite, Material matStreetlightWarm)
        {
            // Ground Plane
            CreatePlane("OceanGroundPlane", parent, new Vector3(0f, -0.5f, 150f), new Vector3(80f, 1f, 80f), matAsphalt);

            // Main Straightaway (0 to 300m)
            CreateRoadSection("MainStraight", parent, new Vector3(0f, 0f, 150f), new Vector3(18f, 0.2f, 320f), matAsphalt);

            // Center yellow dashed lines
            for (int z = 10; z < 300; z += 12)
            {
                CreateBox("LaneLineYellow", parent, new Vector3(0f, 0.11f, z), new Vector3(0.25f, 0.01f, 6f), matYellow);
            }

            // White Edge Markings
            CreateBox("EdgeLine_L", parent, new Vector3(-8.2f, 0.11f, 150f), new Vector3(0.35f, 0.01f, 320f), matWhite);
            CreateBox("EdgeLine_R", parent, new Vector3(8.2f, 0.11f, 150f), new Vector3(0.35f, 0.01f, 320f), matWhite);

            // Start / Finish Line
            CreateBox("StartFinishLine", parent, new Vector3(0f, 0.115f, 15f), new Vector3(16.5f, 0.01f, 3f), matWhite);

            // Side Curbs
            CreateBox("Curb_L", parent, new Vector3(-9.1f, 0.15f, 150f), new Vector3(0.8f, 0.25f, 320f), matCurbRed);
            CreateBox("Curb_R", parent, new Vector3(9.1f, 0.15f, 150f), new Vector3(0.8f, 0.25f, 320f), matCurbWhite);

            // Safety Barriers (Outer Walls)
            CreateBox("BarrierWall_L", parent, new Vector3(-10.2f, 0.65f, 150f), new Vector3(0.6f, 1.1f, 320f), matBarrier);
            CreateBox("BarrierWall_R", parent, new Vector3(10.2f, 0.65f, 150f), new Vector3(0.6f, 1.1f, 320f), matBarrier);

            // Braking Distance Markers (at 250m, 200m, 150m)
            CreateDistanceMarker("BrakeMarker_150m", parent, new Vector3(11.5f, 1.5f, 150f), "150M", matWhite);
            CreateDistanceMarker("BrakeMarker_100m", parent, new Vector3(11.5f, 1.5f, 200f), "100M", matYellow);
            CreateDistanceMarker("BrakeMarker_50m", parent, new Vector3(11.5f, 1.5f, 250f), "50M", matBarrier);

            // Slalom Zone (Bollards between z = 50m and z = 130m)
            for (int z = 60; z <= 120; z += 20)
            {
                float xOffset = ((z / 20) % 2 == 0) ? -2.5f : 2.5f;
                CreateBollard($"SlalomBollard_{z}", parent, new Vector3(xOffset, 0.5f, z), matYellow);
            }

            // Hairpin Turn (at z = 310m turning right)
            CreateHairpinTurn(parent, new Vector3(0f, 0f, 310f), matAsphalt, matBarrier, matCurbRed);

            // Uneven Suspension Testing Bump / Ramp (at z = 35m)
            CreateBump("SuspensionTestBump", parent, new Vector3(0f, 0.08f, 35f), new Vector3(16f, 0.16f, 2.5f), matAsphalt);

            // Streetlights along the avenue
            for (int z = 20; z <= 300; z += 40)
            {
                CreateStreetlight($"Streetlight_L_{z}", parent, new Vector3(-10.8f, 0f, z), matStreetlightWarm, true);
                CreateStreetlight($"Streetlight_R_{z}", parent, new Vector3(10.8f, 0f, z), matStreetlightWarm, false);
            }
        }

        private static void CreateHairpinTurn(Transform parent, Vector3 center, Material matRoad, Material matBarrier, Material matCurb)
        {
            GameObject turnRoot = new GameObject("HairpinTurn");
            turnRoot.transform.SetParent(parent, false);
            turnRoot.transform.position = center;

            // Turn apron
            CreateBox("HairpinApron", turnRoot.transform, new Vector3(25f, 0f, 25f), new Vector3(50f, 0.2f, 50f), matRoad);

            // Outer Curved Barriers
            for (int angle = 0; angle <= 90; angle += 15)
            {
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(rad) * 45f;
                float z = Mathf.Cos(rad) * 45f;
                CreateBox($"OuterBarrier_{angle}", turnRoot.transform, new Vector3(x, 0.65f, z), new Vector3(6f, 1.1f, 0.6f), matBarrier, Quaternion.Euler(0f, angle + 90f, 0f));
            }
        }

        private static void CreateStreetlight(string name, Transform parent, Vector3 basePos, Material matEmissive, bool isLeft)
        {
            GameObject lightRoot = new GameObject(name);
            lightRoot.transform.SetParent(parent, false);
            lightRoot.transform.position = basePos;

            // Pole
            CreateBox("Pole", lightRoot.transform, new Vector3(0f, 4.0f, 0f), new Vector3(0.25f, 8.0f, 0.25f), matEmissive);

            // Arm extending over road
            float armX = isLeft ? 1.8f : -1.8f;
            CreateBox("Arm", lightRoot.transform, new Vector3(armX * 0.5f, 7.8f, 0f), new Vector3(Mathf.Abs(armX), 0.2f, 0.2f), matEmissive);

            // Lamp Fixture
            GameObject fixture = CreateBox("Fixture", lightRoot.transform, new Vector3(armX, 7.6f, 0f), new Vector3(0.8f, 0.15f, 0.4f), matEmissive);

            // Warm Spot/Point Light
            GameObject lightSrc = new GameObject("LightSource");
            lightSrc.transform.SetParent(fixture.transform, false);
            lightSrc.transform.localPosition = new Vector3(0f, -0.2f, 0f);

            Light pLight = lightSrc.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1f, 0.85f, 0.65f);
            pLight.range = 28f;
            pLight.intensity = 2.2f;
            pLight.shadows = LightShadows.None; // mobile friendly
        }

        private static void CreateDistanceMarker(string name, Transform parent, Vector3 pos, string text, Material mat)
        {
            GameObject marker = CreateBox(name, parent, pos, new Vector3(0.15f, 1.8f, 1.2f), mat);
        }

        private static void CreateBollard(string name, Transform parent, Vector3 pos, Material mat)
        {
            GameObject bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bollard.name = name;
            bollard.transform.SetParent(parent, false);
            bollard.transform.position = pos;
            bollard.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);

            MeshRenderer mr = bollard.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }

        private static void CreateBump(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            CreateBox(name, parent, pos, scale, mat);
        }

        private static GameObject CreateRoadSection(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            return CreateBox(name, parent, pos, scale, mat);
        }

        private static GameObject CreatePlane(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Quaternion? rot = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            if (rot.HasValue) go.transform.rotation = rot.Value;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return go;
        }

        private static void BuildGameHUD(VehicleController carController, VehicleInputReader inputReader)
        {
            GameObject canvasObj = new GameObject("InGameHUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            HUDController hud = canvasObj.AddComponent<HUDController>();
            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();

            // Speedometer Panel
            GameObject speedPanel = new GameObject("SpeedPanel");
            speedPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rtSpeed = speedPanel.AddComponent<RectTransform>();
            rtSpeed.anchorMin = new Vector2(0.5f, 0.05f);
            rtSpeed.anchorMax = new Vector2(0.5f, 0.05f);
            rtSpeed.pivot = new Vector2(0.5f, 0.5f);
            rtSpeed.anchoredPosition = new Vector2(0f, 60f);
            rtSpeed.sizeDelta = new Vector2(260f, 100f);

            Text speedTxt = CreateUIText("SpeedText", speedPanel.transform, "0 KM/H", 38, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            speedTxt.rectTransform.sizeDelta = new Vector2(260f, 60f);

            Text gearTxt = CreateUIText("GearText", speedPanel.transform, "N", 24, FontStyle.Bold, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleCenter);
            gearTxt.rectTransform.anchoredPosition = new Vector2(0f, -32f);

            Text titleTxt = CreateUIText("TitleText", canvasObj.transform, "GAMEIUM // VEHICLE TEST", 18, FontStyle.Bold, new Color(0.95f, 0.85f, 0.2f), TextAnchor.UpperLeft);
            titleTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.95f);
            titleTxt.rectTransform.anchorMax = new Vector2(0.04f, 0.95f);
            titleTxt.rectTransform.anchoredPosition = new Vector2(160f, -20f);
            titleTxt.rectTransform.sizeDelta = new Vector2(400f, 40f);

            Text helpTxt = CreateUIText("HelpText", canvasObj.transform, "W/S: Drive | A/D: Steer | SPACE: Handbrake | R: Reset | C: Camera", 14, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperLeft);
            helpTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.95f);
            helpTxt.rectTransform.anchorMax = new Vector2(0.04f, 0.95f);
            helpTxt.rectTransform.anchoredPosition = new Vector2(240f, -50f);
            helpTxt.rectTransform.sizeDelta = new Vector2(600f, 35f);

            // Wire into HUDController via serialized fields
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("targetVehicle").objectReferenceValue = carController;
            so.FindProperty("speedText").objectReferenceValue = speedTxt;
            so.FindProperty("gearText").objectReferenceValue = gearTxt;
            so.FindProperty("carNameText").objectReferenceValue = titleTxt;
            so.FindProperty("controlsHelpText").objectReferenceValue = helpTxt;
            so.ApplyModifiedProperties();

            // Mobile On-Screen Controls (Left, Right, Accelerate, Brake, Handbrake, Reset)
            BuildTouchButtons(canvasObj.transform, mobileUI);
        }

        private static void BuildTouchButtons(Transform canvasParent, MobileDrivingUI mobileUI)
        {
            // Steer Left
            CreateTouchButton("Btn_SteerLeft", canvasParent, new Vector2(0.08f, 0.15f), new Vector2(90f, 90f), "<",
                () => mobileUI.OnLeftDown(), () => mobileUI.OnLeftUp());

            // Steer Right
            CreateTouchButton("Btn_SteerRight", canvasParent, new Vector2(0.22f, 0.15f), new Vector2(90f, 90f), ">",
                () => mobileUI.OnRightDown(), () => mobileUI.OnRightUp());

            // Throttle (Accelerate)
            CreateTouchButton("Btn_Throttle", canvasParent, new Vector2(0.92f, 0.22f), new Vector2(95f, 95f), "GAS",
                () => mobileUI.OnThrottleDown(), () => mobileUI.OnThrottleUp(), new Color(0.15f, 0.75f, 0.35f, 0.75f));

            // Brake / Reverse
            CreateTouchButton("Btn_Brake", canvasParent, new Vector2(0.80f, 0.15f), new Vector2(85f, 85f), "BRAKE",
                () => mobileUI.OnBrakeDown(), () => mobileUI.OnBrakeUp(), new Color(0.85f, 0.25f, 0.25f, 0.75f));

            // Handbrake
            CreateTouchButton("Btn_Handbrake", canvasParent, new Vector2(0.92f, 0.45f), new Vector2(75f, 65f), "DRIFT",
                () => mobileUI.OnHandbrakeDown(), () => mobileUI.OnHandbrakeUp(), new Color(0.95f, 0.65f, 0.15f, 0.75f));

            // Reset
            CreateTouchButton("Btn_Reset", canvasParent, new Vector2(0.92f, 0.88f), new Vector2(75f, 45f), "RESET",
                () => mobileUI.OnResetClicked(), null, new Color(0.35f, 0.35f, 0.45f, 0.75f));
        }

        private static GameObject CreateTouchButton(
            string name, Transform parent, Vector2 anchor, Vector2 size, string label,
            System.Action onDown, System.Action onUp, Color? bgColor = null)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.2f, 0.25f, 0.35f, 0.75f);

            EventTrigger trigger = btnObj.AddComponent<EventTrigger>();
            if (onDown != null)
            {
                var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                entryDown.callback.AddListener((data) => onDown());
                trigger.triggers.Add(entryDown);
            }
            if (onUp != null)
            {
                var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                entryUp.callback.AddListener((data) => onUp());
                trigger.triggers.Add(entryUp);
            }

            Text txt = CreateUIText("Label", btnObj.transform, label, 18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            txt.rectTransform.sizeDelta = size;

            return btnObj;
        }

        private static Text CreateUIText(string name, Transform parent, string text, int fontSize, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();

            Text t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            return t;
        }

        private static Material CreateMat(Shader s, string name, Color color, float smoothness, float metallic)
        {
            string path = $"Assets/_Project/Art/Materials/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(s) { name = name };
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material CreateEmissiveMat(Shader s, string name, Color baseColor, Color emissiveColor)
        {
            string path = $"Assets/_Project/Art/Materials/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(s) { name = name };
                mat.color = baseColor;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissiveColor);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        public static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
            foreach (var s in original)
            {
                if (s.path == scenePath) return;
            }

            EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[original.Length + 1];
            System.Array.Copy(original, updated, original.Length);
            updated[original.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}

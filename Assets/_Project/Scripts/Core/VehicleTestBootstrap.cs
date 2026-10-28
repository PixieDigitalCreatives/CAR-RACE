#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThegameioRacing.Vehicle;
using ThegameioRacing.Camera;
using ThegameioRacing.Input;
using ThegameioRacing.UI;

namespace ThegameioRacing.Core
{
    /// <summary>
    /// Runtime bootstrapper for the Vehicle Physics Test Track.
    /// Builds and verifies the track, lighting, player vehicle, camera, and HUD.
    /// Ensures 100% playable experience out-of-the-box.
    /// </summary>
    public class VehicleTestBootstrap : MonoBehaviour
    {
        [Header("Vehicle Configuration")]
        [SerializeField] private VehicleConfig activeConfig;
        [SerializeField] private VehicleDatabase vehicleDatabase;
        [SerializeField] private bool generateTrackIfMissing = true;

        [Header("Runtime Spawn References")]
        private GameObject spawnedCar;
        private VehicleController carController;
        private VehicleCameraController cameraController;
        private VehicleInputReader inputReader;

        private void Awake()
        {
            EnsureEventSystem();

            if (activeConfig == null && vehicleDatabase != null)
            {
                activeConfig = vehicleDatabase.GetDefaultVehicle();
            }

            if (activeConfig == null)
            {
#if UNITY_EDITOR
                activeConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleConfig>("Assets/_Project/ScriptableObjects/Vehicles/VehicleConfig_Regular.asset");
                if (vehicleDatabase == null)
                {
                    vehicleDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleDatabase>("Assets/_Project/ScriptableObjects/Vehicles/VehicleDatabase.asset");
                }
#endif
                if (activeConfig == null)
                {
                    // Runtime fallback config
                    activeConfig = ScriptableObject.CreateInstance<VehicleConfig>();
                    activeConfig.displayName = "Regular";
                    activeConfig.topSpeedKmh = 215f;
                    activeConfig.motorTorque = 1900f;
                    activeConfig.brakeTorque = 4000f;
                    activeConfig.primaryColor = new Color(0.12f, 0.45f, 0.95f);
                    activeConfig.accentColor = new Color(0.98f, 0.55f, 0.12f);
                }
            }

            if (generateTrackIfMissing && GameObject.Find("TrackGeometry") == null)
            {
                BuildRuntimeTrack();
            }

            SpawnPlayerVehicle();
            SetupCamera();
            BuildRuntimeHUD();
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

        private void SpawnPlayerVehicle()
        {
            Vector3 spawnPos = new Vector3(0f, 0.15f, 10f);
            spawnedCar = CarModelBuilder.BuildProceduralCar("PlayerCar_Regular", activeConfig);
            spawnedCar.transform.position = spawnPos;
            spawnedCar.transform.rotation = Quaternion.identity;

            inputReader = spawnedCar.AddComponent<VehicleInputReader>();
            carController = spawnedCar.GetComponent<VehicleController>();
            carController.SetInputSource(inputReader);
        }

        private void SetupCamera()
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            GameObject camObj;

            if (mainCam != null)
            {
                camObj = mainCam.gameObject;
            }
            else
            {
                camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                camObj.AddComponent<UnityEngine.Camera>();
                camObj.AddComponent<AudioListener>();
            }

            cameraController = camObj.GetComponent<VehicleCameraController>();
            if (cameraController == null)
            {
                cameraController = camObj.AddComponent<VehicleCameraController>();
            }

            cameraController.SetTarget(spawnedCar.transform);
            camObj.transform.position = spawnedCar.transform.position + new Vector3(0f, 3f, -7f);
            camObj.transform.LookAt(spawnedCar.transform.position + Vector3.up * 1.2f);
        }

        private void BuildRuntimeTrack()
        {
            GameObject trackRoot = new GameObject("TrackGeometry");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            Material matAsphalt = CreateMaterial(litShader, new Color(0.18f, 0.18f, 0.20f), 0.2f, 0.05f);
            Material matYellow = CreateMaterial(litShader, new Color(0.95f, 0.78f, 0.12f), 0.3f, 0.0f);
            Material matWhite = CreateMaterial(litShader, new Color(0.92f, 0.92f, 0.94f), 0.3f, 0.0f);
            Material matBarrier = CreateMaterial(litShader, new Color(0.85f, 0.25f, 0.22f), 0.4f, 0.2f);
            Material matStreetlight = CreateEmissiveMaterial(litShader, new Color(1f, 0.88f, 0.65f), new Color(3.0f, 2.2f, 1.2f));

            // Ground plane (ocean / ground)
            CreatePlane("GroundPlane", trackRoot.transform, new Vector3(0f, -0.4f, 150f), new Vector3(60f, 1f, 60f), matAsphalt);

            // Main Straight Road (300m length, 18m width)
            CreateBox("MainAsphaltStraight", trackRoot.transform, new Vector3(0f, 0f, 150f), new Vector3(18f, 0.2f, 320f), matAsphalt);

            // Yellow Center Line
            for (int z = 10; z < 300; z += 12)
            {
                CreateBox("LaneLineYellow", trackRoot.transform, new Vector3(0f, 0.11f, z), new Vector3(0.25f, 0.01f, 6f), matYellow);
            }

            // White Edge Markings
            CreateBox("EdgeLine_L", trackRoot.transform, new Vector3(-8.2f, 0.11f, 150f), new Vector3(0.35f, 0.01f, 320f), matWhite);
            CreateBox("EdgeLine_R", trackRoot.transform, new Vector3(8.2f, 0.11f, 150f), new Vector3(0.35f, 0.01f, 320f), matWhite);

            // Start Line
            CreateBox("StartLine", trackRoot.transform, new Vector3(0f, 0.115f, 15f), new Vector3(16.5f, 0.01f, 3f), matWhite);

            // Safety Barrier Walls
            CreateBox("BarrierWall_L", trackRoot.transform, new Vector3(-9.8f, 0.65f, 150f), new Vector3(0.6f, 1.1f, 320f), matBarrier);
            CreateBox("BarrierWall_R", trackRoot.transform, new Vector3(9.8f, 0.65f, 150f), new Vector3(0.6f, 1.1f, 320f), matBarrier);

            // Slalom Bollards (50m - 130m)
            for (int z = 60; z <= 120; z += 20)
            {
                float x = ((z / 20) % 2 == 0) ? -2.5f : 2.5f;
                GameObject bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bollard.name = $"SlalomBollard_{z}";
                bollard.transform.SetParent(trackRoot.transform, false);
                bollard.transform.position = new Vector3(x, 0.5f, z);
                bollard.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
                bollard.GetComponent<MeshRenderer>().sharedMaterial = matYellow;
            }

            // Hairpin Turn Apron & Curved Barriers (z = 310m)
            CreateBox("HairpinApron", trackRoot.transform, new Vector3(25f, 0f, 325f), new Vector3(50f, 0.2f, 50f), matAsphalt);
            for (int angle = 0; angle <= 90; angle += 15)
            {
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Sin(rad) * 45f;
                float z = 310f + Mathf.Cos(rad) * 45f;
                CreateBox($"OuterBarrier_{angle}", trackRoot.transform, new Vector3(x, 0.65f, z), new Vector3(6f, 1.1f, 0.6f), matBarrier, Quaternion.Euler(0f, angle + 90f, 0f));
            }

            // Streetlights
            for (int z = 20; z <= 300; z += 40)
            {
                CreateStreetlight($"Streetlight_L_{z}", trackRoot.transform, new Vector3(-10.5f, 0f, z), matStreetlight, true);
                CreateStreetlight($"Streetlight_R_{z}", trackRoot.transform, new Vector3(10.5f, 0f, z), matStreetlight, false);
            }
        }

        private void CreateStreetlight(string name, Transform parent, Vector3 pos, Material mat, bool isLeft)
        {
            GameObject sl = new GameObject(name);
            sl.transform.SetParent(parent, false);
            sl.transform.position = pos;

            CreateBox("Pole", sl.transform, new Vector3(0f, 4f, 0f), new Vector3(0.25f, 8f, 0.25f), mat);
            float armX = isLeft ? 1.8f : -1.8f;
            CreateBox("Arm", sl.transform, new Vector3(armX * 0.5f, 7.8f, 0f), new Vector3(Mathf.Abs(armX), 0.2f, 0.2f), mat);
            CreateBox("Fixture", sl.transform, new Vector3(armX, 7.6f, 0f), new Vector3(0.8f, 0.15f, 0.4f), mat);

            GameObject lightObj = new GameObject("LightPoint");
            lightObj.transform.SetParent(sl.transform, false);
            lightObj.transform.localPosition = new Vector3(armX, 7.4f, 0f);

            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1f, 0.88f, 0.65f);
            pLight.range = 28f;
            pLight.intensity = 2.2f;
            pLight.shadows = LightShadows.None;
        }

        private GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, Quaternion? rot = null)
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

        private GameObject CreatePlane(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
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

        private Material CreateMaterial(Shader s, Color color, float smoothness, float metallic)
        {
            Material mat = new Material(s);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        private Material CreateEmissiveMaterial(Shader s, Color baseCol, Color emitCol)
        {
            Material mat = CreateMaterial(s, baseCol, 0.9f, 0f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emitCol);
            return mat;
        }

        private void BuildRuntimeHUD()
        {
            GameObject canvasObj = new GameObject("InGameHUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            HUDController hud = canvasObj.AddComponent<HUDController>();
            hud.SetTargetVehicle(carController);

            MobileDrivingUI mobileUI = canvasObj.AddComponent<MobileDrivingUI>();

            // Speedometer Center-Bottom
            GameObject speedPanel = new GameObject("SpeedPanel");
            speedPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform rtSpeed = speedPanel.AddComponent<RectTransform>();
            rtSpeed.anchorMin = new Vector2(0.5f, 0.05f);
            rtSpeed.anchorMax = new Vector2(0.5f, 0.05f);
            rtSpeed.pivot = new Vector2(0.5f, 0.5f);
            rtSpeed.anchoredPosition = new Vector2(0f, 65f);
            rtSpeed.sizeDelta = new Vector2(280f, 100f);

            Text speedTxt = CreateUIText("SpeedText", speedPanel.transform, "0 KM/H", 38, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            speedTxt.rectTransform.sizeDelta = new Vector2(280f, 60f);

            Text gearTxt = CreateUIText("GearText", speedPanel.transform, "N", 24, FontStyle.Bold, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleCenter);
            gearTxt.rectTransform.anchoredPosition = new Vector2(0f, -32f);

            // Title Banner
            Text titleTxt = CreateUIText("TitleText", canvasObj.transform, "GAMEIUM // VEHICLE TEST", 18, FontStyle.Bold, new Color(0.95f, 0.85f, 0.2f), TextAnchor.UpperLeft);
            titleTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.95f);
            titleTxt.rectTransform.anchorMax = new Vector2(0.04f, 0.95f);
            titleTxt.rectTransform.anchoredPosition = new Vector2(160f, -20f);
            titleTxt.rectTransform.sizeDelta = new Vector2(400f, 40f);

            // Controls Banner
            Text helpTxt = CreateUIText("HelpText", canvasObj.transform, "W/S: Drive | A/D: Steer | SPACE: Handbrake | R: Reset | C: Camera", 14, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperLeft);
            helpTxt.rectTransform.anchorMin = new Vector2(0.04f, 0.95f);
            helpTxt.rectTransform.anchorMax = new Vector2(0.04f, 0.95f);
            helpTxt.rectTransform.anchoredPosition = new Vector2(240f, -50f);
            helpTxt.rectTransform.sizeDelta = new Vector2(600f, 35f);

            // Touch Controls for Android Mobile
            BuildTouchButtons(canvasObj.transform, mobileUI);
        }

        private void BuildTouchButtons(Transform canvasParent, MobileDrivingUI mobileUI)
        {
            // Steer Left
            CreateTouchButton("Btn_SteerLeft", canvasParent, new Vector2(0.08f, 0.16f), new Vector2(90f, 90f), "<",
                () => mobileUI.OnLeftDown(), () => mobileUI.OnLeftUp());

            // Steer Right
            CreateTouchButton("Btn_SteerRight", canvasParent, new Vector2(0.22f, 0.16f), new Vector2(90f, 90f), ">",
                () => mobileUI.OnRightDown(), () => mobileUI.OnRightUp());

            // Throttle
            CreateTouchButton("Btn_Throttle", canvasParent, new Vector2(0.92f, 0.22f), new Vector2(95f, 95f), "GAS",
                () => mobileUI.OnThrottleDown(), () => mobileUI.OnThrottleUp(), new Color(0.15f, 0.75f, 0.35f, 0.75f));

            // Brake
            CreateTouchButton("Btn_Brake", canvasParent, new Vector2(0.80f, 0.16f), new Vector2(85f, 85f), "BRAKE",
                () => mobileUI.OnBrakeDown(), () => mobileUI.OnBrakeUp(), new Color(0.85f, 0.25f, 0.25f, 0.75f));

            // Handbrake
            CreateTouchButton("Btn_Handbrake", canvasParent, new Vector2(0.92f, 0.46f), new Vector2(75f, 65f), "DRIFT",
                () => mobileUI.OnHandbrakeDown(), () => mobileUI.OnHandbrakeUp(), new Color(0.95f, 0.65f, 0.15f, 0.75f));

            // Reset
            CreateTouchButton("Btn_Reset", canvasParent, new Vector2(0.92f, 0.88f), new Vector2(75f, 45f), "RESET",
                () => mobileUI.OnResetClicked(), null, new Color(0.35f, 0.35f, 0.45f, 0.75f));
        }

        private GameObject CreateTouchButton(
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

        private Text CreateUIText(string name, Transform parent, string text, int fontSize, FontStyle style, Color color, TextAnchor anchor)
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
    }
}

#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.UI
{
    public enum MenuScreen
    {
        Title,
        RouteSelect,
        Garage
    }

    /// <summary>
    /// Master Frontend Manager for GAMEIUM.
    /// Manages the Title Screen, 3D Interactive Garage Showcase (turntable, 5 vehicle configs,
    /// dynamic stat bars, vehicle selection persistence), and Route Selection with track cards.
    /// Auto-generates its 3D environment, showroom lighting, and full UI hierarchy if run standalone.
    /// </summary>
    [ExecuteAlways]
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Databases & Configs")]
        [SerializeField] private VehicleDatabase vehicleDatabase;

        [Header("Showroom 3D References")]
        [SerializeField] private Transform turntableTransform;
        [SerializeField] private UnityEngine.Camera showroomCamera;

        [Header("State")]
        [SerializeField] private MenuScreen currentScreen = MenuScreen.Title;
        [SerializeField] private int currentVehicleIndex = 0;
        [SerializeField] private string selectedRouteScene = "DowntownCircuit";

        // UI Panel Roots
        private GameObject titlePanel;
        private GameObject routeSelectPanel;
        private GameObject garagePanel;

        // Garage UI Elements
        private Text carNameText;
        private Text carRoleBadgeText;
        private Text carDescText;
        private Text statSpeedText;
        private Text statAccelText;
        private Text statHandlingText;
        private Text statBrakingText;
        private Image statSpeedBar;
        private Image statAccelBar;
        private Image statHandlingBar;
        private Image statBrakingBar;
        private Text selectButtonText;

        // Current 3D car in garage
        private GameObject currentCarInstance;

        private void Awake()
        {
            ResolveVehicleDatabase();
            SetupShowroomEnvironment();
            EnsureUIHierarchy();
            LoadSelectedVehiclePreference();
            ShowScreen(MenuScreen.Title);
        }

        private void Start()
        {
            UpdateGarageVehicleVisual();
        }

        private void Update()
        {
            // Spin turntable smoothly
            if (turntableTransform != null)
            {
                turntableTransform.Rotate(Vector3.up * 16f * Time.deltaTime, Space.World);
            }
        }

        private void ResolveVehicleDatabase()
        {
            if (vehicleDatabase == null)
            {
#if UNITY_EDITOR
                vehicleDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<VehicleDatabase>(
                    "Assets/_Project/ScriptableObjects/Vehicles/VehicleDatabase.asset");
#endif
            }
        }

        private void LoadSelectedVehiclePreference()
        {
            string savedId = PlayerPrefs.GetString("SelectedVehicleId", "car_regular");
            if (vehicleDatabase != null && vehicleDatabase.Vehicles != null)
            {
                for (int i = 0; i < vehicleDatabase.Vehicles.Count; i++)
                {
                    if (vehicleDatabase.Vehicles[i] != null &&
                        string.Equals(vehicleDatabase.Vehicles[i].vehicleId, savedId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        currentVehicleIndex = i;
                        return;
                    }
                }
            }
            currentVehicleIndex = 0;
        }

        #region 3D Showroom Setup

        private void SetupShowroomEnvironment()
        {
            // 1. Showroom Camera
            if (showroomCamera == null)
            {
                showroomCamera = UnityEngine.Camera.main;
                if (showroomCamera == null)
                {
                    GameObject camObj = new GameObject("ShowroomCamera");
                    camObj.tag = "MainCamera";
                    showroomCamera = camObj.AddComponent<UnityEngine.Camera>();
                    camObj.AddComponent<AudioListener>();
                }
            }

            showroomCamera.transform.position = new Vector3(0f, 1.8f, -5.8f);
            showroomCamera.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            showroomCamera.backgroundColor = new Color(0.04f, 0.06f, 0.10f); // Deep midnight indigo
            showroomCamera.clearFlags = CameraClearFlags.SolidColor;

            // 2. Turntable Platform
            if (turntableTransform == null)
            {
                Transform existingTt = transform.Find("GarageTurntable");
                if (existingTt != null)
                {
                    turntableTransform = existingTt;
                }
                else
                {
                    GameObject ttObj = new GameObject("GarageTurntable");
                    ttObj.transform.SetParent(transform, false);
                    ttObj.transform.position = new Vector3(0f, 0f, 0f);
                    turntableTransform = ttObj.transform;

                    // Platform Disc
                    GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    disc.name = "PlatformDisc";
                    disc.transform.SetParent(ttObj.transform, false);
                    disc.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                    disc.transform.localScale = new Vector3(7.5f, 0.1f, 7.5f);

                    Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    Material discMat = new Material(litShader);
                    discMat.SetColor("_BaseColor", new Color(0.12f, 0.14f, 0.18f));
                    discMat.SetFloat("_Smoothness", 0.85f);
                    discMat.SetFloat("_Metallic", 0.75f);
                    disc.GetComponent<Renderer>().sharedMaterial = discMat;

                    // Glowing Accent Ring
                    GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    ring.name = "AccentRing";
                    ring.transform.SetParent(ttObj.transform, false);
                    ring.transform.localPosition = new Vector3(0f, -0.09f, 0f);
                    ring.transform.localScale = new Vector3(7.8f, 0.08f, 7.8f);
                    Material ringMat = new Material(litShader);
                    ringMat.SetColor("_BaseColor", new Color(0.15f, 0.65f, 1.0f));
                    ringMat.EnableKeyword("_EMISSION");
                    ringMat.SetColor("_EmissionColor", new Color(0.2f, 1.2f, 2.4f));
                    ring.GetComponent<Renderer>().sharedMaterial = ringMat;
                }
            }

            // 3. Studio Spotlighting
            if (transform.Find("StudioLights") == null)
            {
                GameObject lightsRoot = new GameObject("StudioLights");
                lightsRoot.transform.SetParent(transform, false);

                // Overhead Center Soft Spotlight
                GameObject centerSpot = new GameObject("CenterSpotlight");
                centerSpot.transform.SetParent(lightsRoot.transform, false);
                centerSpot.transform.position = new Vector3(0f, 6.5f, 0f);
                centerSpot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                Light lCenter = centerSpot.AddComponent<Light>();
                lCenter.type = LightType.Spot;
                lCenter.color = new Color(1f, 0.96f, 0.90f);
                lCenter.intensity = 3.2f;
                lCenter.range = 14f;
                lCenter.spotAngle = 60f;

                // Front Key Light
                GameObject keyLight = new GameObject("KeyLight");
                keyLight.transform.SetParent(lightsRoot.transform, false);
                keyLight.transform.position = new Vector3(-3.5f, 3.2f, -4f);
                keyLight.transform.LookAt(Vector3.up * 0.5f);
                Light lKey = keyLight.AddComponent<Light>();
                lKey.type = LightType.Spot;
                lKey.color = new Color(0.9f, 0.95f, 1f);
                lKey.intensity = 2.4f;
                lKey.range = 10f;
                lKey.spotAngle = 55f;

                // Rim Backlight (Warm Gold)
                GameObject rimLight = new GameObject("RimLight");
                rimLight.transform.SetParent(lightsRoot.transform, false);
                rimLight.transform.position = new Vector3(3.5f, 2.5f, 3.5f);
                rimLight.transform.LookAt(Vector3.up * 0.5f);
                Light lRim = rimLight.AddComponent<Light>();
                lRim.type = LightType.Spot;
                lRim.color = new Color(1f, 0.78f, 0.25f);
                lRim.intensity = 2.8f;
                lRim.range = 10f;
                lRim.spotAngle = 65f;
            }
        }

        #endregion

        #region UI Architecture

        private void EnsureUIHierarchy()
        {
            if (titlePanel != null && routeSelectPanel != null && garagePanel != null) return;

            Transform canvasTransform = transform.Find("Canvas_MainMenu");
            GameObject canvasObj;
            if (canvasTransform != null)
            {
                canvasObj = canvasTransform.gameObject;
            }
            else
            {
                canvasObj = new GameObject("Canvas_MainMenu");
                canvasObj.transform.SetParent(transform, false);
                Canvas c = canvasObj.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Title Screen Panel
            titlePanel = BuildTitlePanel(canvasObj.transform, font);

            // 2. Route Selection Panel
            routeSelectPanel = BuildRouteSelectPanel(canvasObj.transform, font);

            // 3. Garage Showcase Panel
            garagePanel = BuildGaragePanel(canvasObj.transform, font);
        }

        private GameObject BuildTitlePanel(Transform parent, Font font)
        {
            GameObject panel = new GameObject("Panel_Title");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Brand Title & Subtitle
            CreateText(panel.transform, "Title_Main", "GAMEIUM",
                new Vector2(0.5f, 0.82f), new Vector2(900f, 90f), 72, FontStyle.Bold, new Color(1f, 0.85f, 0.15f), font);

            CreateText(panel.transform, "Subtitle", "AFRICAN COASTAL METROPOLIS AT NIGHT",
                new Vector2(0.5f, 0.74f), new Vector2(800f, 40f), 22, FontStyle.Normal, new Color(0.75f, 0.85f, 0.98f), font);

            // Menu Buttons (Left-Center Column)
            float startY = 60f;
            float stepY = -85f;

            CreateMenuButton(panel.transform, "Btn_Play", "RACE NOW", new Vector2(-420f, startY), font,
                new Color(0.12f, 0.65f, 0.35f), () => ShowScreen(MenuScreen.RouteSelect));

            CreateMenuButton(panel.transform, "Btn_Garage", "GARAGE / VEHICLES", new Vector2(-420f, startY + stepY), font,
                new Color(0.12f, 0.45f, 0.85f), () => ShowScreen(MenuScreen.Garage));

            CreateMenuButton(panel.transform, "Btn_GateA", "SAMPLE STREET (GATE A)", new Vector2(-420f, startY + stepY * 2), font,
                new Color(0.25f, 0.30f, 0.40f), () => SceneManager.LoadScene("SampleStreet_GateA"));

            CreateMenuButton(panel.transform, "Btn_Exit", "EXIT GAME", new Vector2(-420f, startY + stepY * 3), font,
                new Color(0.35f, 0.18f, 0.20f), Application.Quit);

            return panel;
        }

        private GameObject BuildRouteSelectPanel(Transform parent, Font font)
        {
            GameObject panel = new GameObject("Panel_RouteSelect");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent backdrop
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.05f, 0.08f, 0.88f);

            // Title
            CreateText(panel.transform, "Title", "SELECT ROUTE",
                new Vector2(0.5f, 0.88f), new Vector2(600f, 60f), 52, FontStyle.Bold, Color.white, font);

            // Track Cards (Horizontal Row)
            float[] cardX = new float[] { -540f, -180f, 180f, 540f };
            string[] trackNames = new string[] { "Downtown Circuit", "Waterfront Run", "Market District", "Industrial Port" };
            string[] trackSpecs = new string[] { 
                "1.6 km | 2 Laps\nAfrican Coastal Metropolis\n[PLAYABLE NOW]", 
                "2.2 km | 2 Laps\nHigh-Speed Ocean Sweep\n[PLAYABLE NOW]", 
                "1.4 km | Technical Chicanes\nBustling Market District\n[PLAYABLE NOW]", 
                "1.8 km | Shipping Docks\nHarbor Container Circuit\n[PLAYABLE NOW]" 
            };
            string[] sceneTargets = new string[] { "DowntownCircuit", "WaterfrontRun", "MarketDistrict", "IndustrialPort" };
            bool[] isPlayable = new bool[] { true, true, true, true };

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                BuildTrackCard(panel.transform, cardX[i], trackNames[i], trackSpecs[i], isPlayable[i], font, () =>
                {
                    if (isPlayable[idx])
                    {
                        selectedRouteScene = sceneTargets[idx];
                        SceneManager.LoadScene(selectedRouteScene);
                    }
                });
            }

            // Back Button (Bottom-Left)
            CreateMenuButton(panel.transform, "Btn_Back", "< BACK", new Vector2(-650f, -440f), font,
                new Color(0.25f, 0.30f, 0.38f), () => ShowScreen(MenuScreen.Title));

            return panel;
        }

        private void BuildTrackCard(Transform parent, float posX, string title, string specs, bool playable, Font font, UnityEngine.Events.UnityAction onClick)
        {
            GameObject cardObj = new GameObject("TrackCard_" + title);
            cardObj.transform.SetParent(parent, false);
            RectTransform rt = cardObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(posX, 20f);
            rt.sizeDelta = new Vector2(320f, 440f);

            Image img = cardObj.AddComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

            // Header Banner
            GameObject banner = new GameObject("Banner");
            banner.transform.SetParent(cardObj.transform, false);
            RectTransform bRt = banner.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, 0.65f);
            bRt.anchorMax = new Vector2(1f, 1f);
            bRt.sizeDelta = Vector2.zero;
            Image bImg = banner.AddComponent<Image>();
            bImg.color = playable ? new Color(0.12f, 0.45f, 0.85f, 0.85f) : new Color(0.20f, 0.22f, 0.28f, 0.85f);

            CreateText(cardObj.transform, "Name", title, new Vector2(0.5f, 0.82f), new Vector2(300f, 50f), 24, FontStyle.Bold, Color.white, font);
            CreateText(cardObj.transform, "Specs", specs, new Vector2(0.5f, 0.42f), new Vector2(280f, 160f), 19, FontStyle.Normal, playable ? new Color(0.8f, 0.9f, 1f) : new Color(0.6f, 0.65f, 0.72f), font);

            // Select & Launch Button
            GameObject btnObj = new GameObject("Btn_Race");
            btnObj.transform.SetParent(cardObj.transform, false);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.12f);
            btnRt.anchorMax = new Vector2(0.5f, 0.12f);
            btnRt.sizeDelta = new Vector2(240f, 54f);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = playable ? new Color(0.12f, 0.65f, 0.35f) : new Color(0.22f, 0.25f, 0.30f);
            Button btn = btnObj.AddComponent<Button>();
            if (playable)
            {
                btn.onClick.AddListener(onClick);
            }
            else
            {
                btn.interactable = false;
            }

            string btnLabel = playable ? "RACE NOW" : "COMING SOON";
            Color btnTextColor = playable ? Color.white : new Color(0.55f, 0.60f, 0.68f);
            CreateText(btnObj.transform, "Label", btnLabel, new Vector2(0.5f, 0.5f), new Vector2(240f, 50f), 18, FontStyle.Bold, btnTextColor, font);
        }

        private GameObject BuildGaragePanel(Transform parent, Font font)
        {
            GameObject panel = new GameObject("Panel_Garage");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Vehicle Selector Header (Top-Center)
            CreateText(panel.transform, "GarageTitle", "VEHICLE SHOWROOM",
                new Vector2(0.5f, 0.92f), new Vector2(600f, 50f), 42, FontStyle.Bold, Color.white, font);

            // Left / Right Vehicle Switch Arrows
            CreateArrowButton(panel.transform, "Btn_PrevCar", "<", new Vector2(-460f, 0f), font, () => SwitchVehicle(-1));
            CreateArrowButton(panel.transform, "Btn_NextCar", ">", new Vector2(460f, 0f), font, () => SwitchVehicle(1));

            // Right Info Card (Vehicle Specs & Stat Bars)
            GameObject cardObj = new GameObject("VehicleInfoCard");
            cardObj.transform.SetParent(panel.transform, false);
            RectTransform cardRt = cardObj.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(1f, 0.5f);
            cardRt.anchorMax = new Vector2(1f, 0.5f);
            cardRt.pivot = new Vector2(1f, 0.5f);
            cardRt.anchoredPosition = new Vector2(-40f, 0f);
            cardRt.sizeDelta = new Vector2(460f, 620f);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.06f, 0.08f, 0.14f, 0.88f);

            // Vehicle Name & Role Badge
            carNameText = CreateText(cardObj.transform, "Name", "Cheetah GT",
                new Vector2(0.5f, 0.88f), new Vector2(420f, 50f), 32, FontStyle.Bold, new Color(1f, 0.85f, 0.15f), font);

            carRoleBadgeText = CreateText(cardObj.transform, "Role", "[BALANCED SPORTS COUPE]",
                new Vector2(0.5f, 0.80f), new Vector2(420f, 30f), 18, FontStyle.Bold, new Color(0.35f, 0.85f, 1f), font);

            carDescText = CreateText(cardObj.transform, "Desc", "Balanced handling, forgiving drift angle.",
                new Vector2(0.5f, 0.70f), new Vector2(420f, 60f), 17, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), font);

            // Stat Bars
            float barStartY = 0.56f;
            float barStepY = -0.09f;

            BuildStatBar(cardObj.transform, "TopSpeed", "TOP SPEED", new Vector2(0.5f, barStartY), font, out statSpeedText, out statSpeedBar);
            BuildStatBar(cardObj.transform, "Accel", "ACCELERATION", new Vector2(0.5f, barStartY + barStepY), font, out statAccelText, out statAccelBar);
            BuildStatBar(cardObj.transform, "Handling", "HANDLING", new Vector2(0.5f, barStartY + barStepY * 2), font, out statHandlingText, out statHandlingBar);
            BuildStatBar(cardObj.transform, "Braking", "BRAKING", new Vector2(0.5f, barStartY + barStepY * 3), font, out statBrakingText, out statBrakingBar);

            // Select Car Button
            GameObject selectBtnObj = new GameObject("Btn_SelectVehicle");
            selectBtnObj.transform.SetParent(cardObj.transform, false);
            RectTransform selRt = selectBtnObj.AddComponent<RectTransform>();
            selRt.anchorMin = new Vector2(0.5f, 0.10f);
            selRt.anchorMax = new Vector2(0.5f, 0.10f);
            selRt.sizeDelta = new Vector2(380f, 60f);

            Image selImg = selectBtnObj.AddComponent<Image>();
            selImg.color = new Color(0.12f, 0.65f, 0.35f);
            Button selBtn = selectBtnObj.AddComponent<Button>();
            selBtn.onClick.AddListener(SelectCurrentVehicle);

            selectButtonText = CreateText(selectBtnObj.transform, "Label", "SELECT VEHICLE",
                new Vector2(0.5f, 0.5f), new Vector2(380f, 50f), 22, FontStyle.Bold, Color.white, font);

            // Back to Menu Button (Bottom-Left)
            CreateMenuButton(panel.transform, "Btn_BackToMenu", "← MAIN MENU", new Vector2(-650f, -440f), font,
                new Color(0.25f, 0.30f, 0.38f), () => ShowScreen(MenuScreen.Title));

            return panel;
        }

        private void BuildStatBar(Transform parent, string name, string label, Vector2 anchor, Font font, out Text valText, out Image fillBar)
        {
            GameObject container = new GameObject("Stat_" + name);
            container.transform.SetParent(parent, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(400f, 40f);

            CreateText(container.transform, "Label", label, new Vector2(0f, 0.5f), new Vector2(160f, 30f), 16, FontStyle.Bold, new Color(0.75f, 0.85f, 0.95f), font, TextAnchor.MiddleLeft);
            valText = CreateText(container.transform, "Value", "80", new Vector2(1f, 0.5f), new Vector2(50f, 30f), 16, FontStyle.Bold, Color.white, font, TextAnchor.MiddleRight);

            // Bar Background
            GameObject bgObj = new GameObject("BarBg");
            bgObj.transform.SetParent(container.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.42f, 0.3f);
            bgRt.anchorMax = new Vector2(0.85f, 0.7f);
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.18f, 0.25f);

            // Bar Fill
            GameObject fillObj = new GameObject("BarFill");
            fillObj.transform.SetParent(bgObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.8f, 1f);
            fillRt.sizeDelta = Vector2.zero;
            fillBar = fillObj.AddComponent<Image>();
            fillBar.color = new Color(0.18f, 0.65f, 1.0f);
        }

        private Text CreateText(Transform parent, string name, string content, Vector2 anchor, Vector2 size, int fontSize, FontStyle style, Color color, Font font, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;

            Text txt = obj.AddComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            return txt;
        }

        private Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Font font, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(340f, 65f);

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            CreateText(btnObj.transform, "Label", label, new Vector2(0.5f, 0.5f), new Vector2(340f, 60f), 22, FontStyle.Bold, Color.white, font);

            return btn;
        }

        private void CreateArrowButton(Transform parent, string name, string label, Vector2 pos, Font font, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(75f, 75f);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.24f, 0.85f);

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            CreateText(btnObj.transform, "Label", label, new Vector2(0.5f, 0.5f), new Vector2(70f, 70f), 32, FontStyle.Bold, new Color(1f, 0.85f, 0.2f), font);
        }

        #endregion

        #region Navigation & Vehicle Switching

        public void ShowScreen(MenuScreen screen)
        {
            currentScreen = screen;
            if (titlePanel != null) titlePanel.SetActive(screen == MenuScreen.Title);
            if (routeSelectPanel != null) routeSelectPanel.SetActive(screen == MenuScreen.RouteSelect);
            if (garagePanel != null) garagePanel.SetActive(screen == MenuScreen.Garage);

            // Turntable car is visible primarily in Title and Garage
            if (turntableTransform != null)
            {
                turntableTransform.gameObject.SetActive(screen != MenuScreen.RouteSelect);
            }
        }

        public void SwitchVehicle(int delta)
        {
            if (vehicleDatabase == null || vehicleDatabase.Vehicles == null || vehicleDatabase.Vehicles.Count == 0) return;

            currentVehicleIndex = (currentVehicleIndex + delta + vehicleDatabase.Vehicles.Count) % vehicleDatabase.Vehicles.Count;
            UpdateGarageVehicleVisual();
        }

        private void UpdateGarageVehicleVisual()
        {
            if (vehicleDatabase == null || vehicleDatabase.Vehicles == null || vehicleDatabase.Vehicles.Count == 0) return;

            VehicleConfig cfg = vehicleDatabase.Vehicles[currentVehicleIndex];
            if (cfg == null) return;

            // 1. Respawn car on turntable
            if (currentCarInstance != null)
            {
                DestroyImmediate(currentCarInstance);
            }

            if (turntableTransform != null)
            {
                currentCarInstance = CarModelBuilder.BuildProceduralCar("ShowcaseCar_" + cfg.displayName, cfg);
                currentCarInstance.transform.SetParent(turntableTransform, false);
                currentCarInstance.transform.localPosition = new Vector3(0f, 0.18f, 0f);
                currentCarInstance.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);

                // Disable physics on showcase car so it doesn't fall or drift
                Rigidbody rb = currentCarInstance.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }

            // 2. Update UI Card details
            if (carNameText != null) carNameText.text = cfg.displayName;
            if (carRoleBadgeText != null)
            {
                carRoleBadgeText.text = $"[{cfg.role.ToString().ToUpper()}]";
                carRoleBadgeText.color = cfg.role switch
                {
                    VehicleRole.HypercarSpeed => new Color(1f, 0.35f, 0.15f),
                    VehicleRole.ClassicMuscle => new Color(0.95f, 0.75f, 0.2f),
                    VehicleRole.TunerDrift => new Color(0.85f, 0.25f, 1f),
                    VehicleRole.StreetRally => new Color(0.25f, 0.85f, 0.4f),
                    _ => new Color(0.35f, 0.85f, 1f)
                };
            }

            if (carDescText != null) carDescText.text = cfg.description;

            // Stat Numbers & Bars
            UpdateStatElement(statSpeedText, statSpeedBar, cfg.statTopSpeed);
            UpdateStatElement(statAccelText, statAccelBar, cfg.statAcceleration);
            UpdateStatElement(statHandlingText, statHandlingBar, cfg.statHandling);
            UpdateStatElement(statBrakingText, statBrakingBar, cfg.statBraking);

            // Check if current car is already active selected
            string savedId = PlayerPrefs.GetString("SelectedVehicleId", "car_regular");
            bool isCurrentSelected = string.Equals(cfg.vehicleId, savedId, System.StringComparison.OrdinalIgnoreCase);
            if (selectButtonText != null)
            {
                selectButtonText.text = isCurrentSelected ? "✔ SELECTED" : "SELECT VEHICLE";
            }
        }

        private void UpdateStatElement(Text text, Image fillBar, int rating)
        {
            if (text != null) text.text = rating.ToString();
            if (fillBar != null)
            {
                RectTransform rt = fillBar.rectTransform;
                rt.anchorMax = new Vector2(Mathf.Clamp01(rating / 100f), 1f);
            }
        }

        public void SelectCurrentVehicle()
        {
            if (vehicleDatabase == null || vehicleDatabase.Vehicles == null || vehicleDatabase.Vehicles.Count == 0) return;

            VehicleConfig cfg = vehicleDatabase.Vehicles[currentVehicleIndex];
            if (cfg != null)
            {
                PlayerPrefs.SetString("SelectedVehicleId", cfg.vehicleId);
                PlayerPrefs.Save();

                if (selectButtonText != null)
                {
                    selectButtonText.text = "✔ SELECTED";
                }
            }
        }

        #endregion
    }
}


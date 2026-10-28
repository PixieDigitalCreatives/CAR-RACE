using UnityEngine;
using UnityEngine.UI;
using ThegameioRacing.Race;

namespace ThegameioRacing.UI
{
    /// <summary>
    /// In-race pause menu with Resume, Restart, and Main Menu navigation.
    /// Auto-generates UI elements if not wired in prefab/scene.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            EnsureMenuStructure();
            if (menuRoot != null) menuRoot.SetActive(false);
        }

        private void Start()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnStateChanged += HandleStateChanged;
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void EnsureMenuStructure()
        {
            if (menuRoot == null)
            {
                GameObject panel = new GameObject("Pause_Panel");
                panel.transform.SetParent(transform, false);
                RectTransform rt = panel.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                Image bg = panel.AddComponent<Image>();
                bg.color = new Color(0.04f, 0.06f, 0.10f, 0.88f); // Dark translucent backdrop

                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Title
                GameObject titleObj = new GameObject("Title_Text");
                titleObj.transform.SetParent(panel.transform, false);
                RectTransform titleRt = titleObj.AddComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0.5f, 0.75f);
                titleRt.anchorMax = new Vector2(0.5f, 0.75f);
                titleRt.anchoredPosition = Vector2.zero;
                titleRt.sizeDelta = new Vector2(500f, 80f);
                Text titleText = titleObj.AddComponent<Text>();
                titleText.font = font;
                titleText.text = "PAUSED";
                titleText.fontSize = 58;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = new Color(1f, 0.82f, 0.12f);

                // Resume Button
                resumeButton = CreateButton(panel.transform, "Btn_Resume", "RESUME", new Vector2(0f, 40f), font, new Color(0.12f, 0.55f, 0.95f));
                // Restart Button
                restartButton = CreateButton(panel.transform, "Btn_Restart", "RESTART RACE", new Vector2(0f, -50f), font, new Color(0.25f, 0.30f, 0.38f));
                // Menu Button
                menuButton = CreateButton(panel.transform, "Btn_Menu", "MAIN MENU", new Vector2(0f, -140f), font, new Color(0.25f, 0.30f, 0.38f));

                menuRoot = panel;
            }
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Font font, Color bgColor)
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
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.25f;
            cb.pressedColor = bgColor * 0.85f;
            btn.colors = cb;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            Text txt = textObj.AddComponent<Text>();
            txt.font = font;
            txt.text = label;
            txt.fontSize = 24;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }

        private void HandleStateChanged(RaceState state)
        {
            if (menuRoot != null)
            {
                menuRoot.SetActive(state == RaceState.Paused);
            }
        }

        public void OnResumeClicked()
        {
            RaceManager.Instance?.TogglePause();
        }

        public void OnRestartClicked()
        {
            RaceManager.Instance?.RestartRace();
        }

        public void OnMenuClicked()
        {
            RaceManager.Instance?.LoadScene("MainMenu");
        }
    }
}

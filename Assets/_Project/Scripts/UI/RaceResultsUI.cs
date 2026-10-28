using UnityEngine;
using UnityEngine.UI;
using ThegameioRacing.Race;

namespace ThegameioRacing.UI
{
    /// <summary>
    /// Post-race podium & results screen displaying player position,
    /// race time, best lap, full leaderboard standings, and restart/menu options.
    /// Auto-generates UI elements if not wired in prefab/scene.
    /// </summary>
    public class RaceResultsUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject resultsRoot;
        [SerializeField] private Text headerTitleText;
        [SerializeField] private Text playerRankText;
        [SerializeField] private Text playerTimeText;
        [SerializeField] private Text playerBestLapText;
        [SerializeField] private Text leaderboardTableText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            EnsureResultsStructure();
            if (resultsRoot != null) resultsRoot.SetActive(false);
        }

        private void Start()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnPlayerFinished += HandlePlayerFinished;
            }

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.OnPlayerFinished -= HandlePlayerFinished;
            }
        }

        private void EnsureResultsStructure()
        {
            if (resultsRoot == null)
            {
                GameObject panel = new GameObject("Results_Panel");
                panel.transform.SetParent(transform, false);
                RectTransform rt = panel.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;

                Image bg = panel.AddComponent<Image>();
                bg.color = new Color(0.03f, 0.05f, 0.09f, 0.94f); // Rich dark midnight backdrop

                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Header Title
                headerTitleText = CreateText(panel.transform, "Title_Text", new Vector2(0.5f, 0.88f), new Vector2(600f, 70f), 54, FontStyle.Bold, Color.white, font);
                headerTitleText.text = "RACE FINISHED";

                // Player Rank
                playerRankText = CreateText(panel.transform, "Rank_Text", new Vector2(0.5f, 0.77f), new Vector2(500f, 65f), 48, FontStyle.Bold, new Color(1f, 0.84f, 0.12f), font);
                playerRankText.text = "1st PLACE";

                // Time Metrics
                playerTimeText = CreateText(panel.transform, "Time_Text", new Vector2(0.5f, 0.68f), new Vector2(500f, 40f), 24, FontStyle.Normal, Color.white, font);
                playerTimeText.text = "Total Time: 00:00.00";

                playerBestLapText = CreateText(panel.transform, "BestLap_Text", new Vector2(0.5f, 0.62f), new Vector2(500f, 40f), 22, FontStyle.Normal, new Color(0.35f, 0.85f, 1f), font);
                playerBestLapText.text = "Best Lap: 00:00.00";

                // Leaderboard Table
                leaderboardTableText = CreateText(panel.transform, "Leaderboard_Text", new Vector2(0.5f, 0.40f), new Vector2(650f, 200f), 20, FontStyle.Normal, new Color(0.85f, 0.90f, 0.95f), font);
                leaderboardTableText.alignment = TextAnchor.UpperCenter;

                // Action Buttons
                restartButton = CreateButton(panel.transform, "Btn_Restart", "PLAY AGAIN", new Vector2(-150f, -380f), font, new Color(0.12f, 0.65f, 0.35f));
                menuButton = CreateButton(panel.transform, "Btn_Menu", "MAIN MENU", new Vector2(150f, -380f), font, new Color(0.25f, 0.30f, 0.38f));

                resultsRoot = panel;
            }
        }

        private Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, FontStyle style, Color color, Font font)
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
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            return txt;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 pos, Font font, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(260f, 60f);

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
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }

        private void HandlePlayerFinished(RacerProgress playerProgress)
        {
            if (playerProgress == null) return;

            if (resultsRoot != null) resultsRoot.SetActive(true);

            int rank = playerProgress.finalRank;
            string suffix = GetOrdinalSuffix(rank);

            if (headerTitleText != null)
            {
                headerTitleText.text = rank == 1 ? "VICTORY!" : "RACE FINISHED";
                headerTitleText.color = rank == 1 ? new Color(1f, 0.85f, 0.2f) : Color.white;
            }

            if (playerRankText != null)
            {
                playerRankText.text = $"{rank}{suffix} PLACE";
                playerRankText.color = rank switch
                {
                    1 => new Color(1f, 0.84f, 0.12f),  // Gold
                    2 => new Color(0.85f, 0.85f, 0.88f), // Silver
                    3 => new Color(0.82f, 0.52f, 0.28f), // Bronze
                    _ => new Color(0.5f, 0.8f, 1f)      // Blue
                };
            }

            if (playerTimeText != null)
            {
                playerTimeText.text = $"Total Time: {FormatTime(playerProgress.totalRaceTime)}";
            }

            if (playerBestLapText != null)
            {
                string bestLap = playerProgress.bestLapTime < 9999f ? FormatTime(playerProgress.bestLapTime) : "--:--.--";
                playerBestLapText.text = $"Best Lap: {bestLap}";
            }

            // Build Leaderboard Table Text
            if (leaderboardTableText != null && RaceManager.Instance?.Checkpoints != null)
            {
                var standings = RaceManager.Instance.Checkpoints.Leaderboard;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("FINAL STANDINGS");
                sb.AppendLine("----------------------------------------");

                for (int i = 0; i < standings.Count; i++)
                {
                    var r = standings[i];
                    string carName = r.vehicle != null ? r.vehicle.gameObject.name.Replace("(Clone)", "") : "Racer";
                    string timeStr = r.isFinished ? FormatTime(r.totalRaceTime) : "DNF";
                    sb.AppendLine($"#{i + 1}  {carName,-20}  {timeStr}");
                }

                leaderboardTableText.text = sb.ToString();
            }
        }

        private string GetOrdinalSuffix(int number)
        {
            if (number <= 0) return "";
            return (number % 100) switch
            {
                11 or 12 or 13 => "th",
                _ => (number % 10) switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                }
            };
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            float seconds = timeInSeconds % 60f;
            return $"{minutes:00}:{seconds:05.2f}";
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

#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Vehicle Target")]
        [SerializeField] private VehicleController targetVehicle;

        [Header("Telemetry UI Elements")]
        [SerializeField] private Text speedText;
        [SerializeField] private Text gearText;
        [SerializeField] private Text carNameText;
        [SerializeField] private Image rpmFillBar;
        [SerializeField] private GameObject handbrakeAlert;

        [Header("Race Progression Elements")]
        [SerializeField] private Text lapText;
        [SerializeField] private Text positionText;
        [SerializeField] private Text lapTimeText;
        [SerializeField] private GameObject wrongWayBanner;

        [Header("Info / Controls Help")]
        [SerializeField] private Text controlsHelpText;

        private void Awake()
        {
            EnsureHUDStructure();
        }

        private void Start()
        {
            if (targetVehicle == null)
            {
                targetVehicle = FindAnyObjectByType<VehicleController>();
            }

            if (controlsHelpText != null)
            {
                controlsHelpText.text = "<b>W/UP:</b> Throttle  |  <b>S/DOWN:</b> Brake/Rev  |  <b>A/D:</b> Steer\n<b>SPACE:</b> Handbrake  |  <b>R:</b> Reset  |  <b>C:</b> Change Camera";
            }
        }

        private void EnsureHUDStructure()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Car Name Header
            if (carNameText == null)
            {
                carNameText = CreateTextElement("CarName_Text", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(35f, -30f), new Vector2(500f, 32f), 18, TextAnchor.MiddleLeft, new Color(0.6f, 0.8f, 1f, 0.85f), defaultFont);
            }

            // 2. Position Rank Display
            if (positionText == null)
            {
                positionText = CreateTextElement("Position_Text", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(35f, -85f), new Vector2(220f, 75f), 44, TextAnchor.MiddleLeft, new Color(1f, 0.84f, 0.12f), defaultFont);
                positionText.text = "<size=44><b>1</b></size>st";
            }

            // 3. Lap Counter Display
            if (lapText == null)
            {
                lapText = CreateTextElement("Lap_Text", new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(35f, -145f), new Vector2(240f, 45f), 24, TextAnchor.MiddleLeft, Color.white, defaultFont);
                lapText.text = "LAP <size=32><b>1</b></size>/2";
            }

            // 4. Lap Time Display
            if (lapTimeText == null)
            {
                lapTimeText = CreateTextElement("LapTime_Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -40f), new Vector2(380f, 50f), 28, TextAnchor.MiddleCenter, new Color(0.9f, 0.95f, 1f), defaultFont);
                lapTimeText.text = "TIME: 00:00.00";
            }

            // 5. Speedometer & Gear Display
            if (speedText == null)
            {
                speedText = CreateTextElement("Speed_Text", new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(-155f, 75f), new Vector2(220f, 75f), 46, TextAnchor.MiddleRight, Color.white, defaultFont);
                speedText.text = "0 <size=24>KM/H</size>";
            }

            if (gearText == null)
            {
                gearText = CreateTextElement("Gear_Text", new Vector2(1f, 0f), new Vector2(1f, 0f),
                    new Vector2(-45f, 75f), new Vector2(75f, 75f), 50, TextAnchor.MiddleCenter, new Color(0.35f, 0.85f, 1f), defaultFont);
                gearText.text = "N";
            }

            // 6. Wrong Way Banner
            if (wrongWayBanner == null)
            {
                GameObject wwObj = new GameObject("WrongWay_Banner");
                wwObj.transform.SetParent(transform, false);
                RectTransform rt = wwObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 120f);
                rt.sizeDelta = new Vector2(460f, 80f);

                Image bg = wwObj.AddComponent<Image>();
                bg.color = new Color(0.85f, 0.08f, 0.08f, 0.78f);

                Text wwText = CreateTextElement("WrongWay_Label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(440f, 70f), 38, TextAnchor.MiddleCenter, Color.white, defaultFont);
                wwText.transform.SetParent(wwObj.transform, false);
                wwText.text = "[!] WRONG WAY - TURN BACK [!]";
                wwText.fontStyle = FontStyle.Bold;

                wrongWayBanner = wwObj;
                wrongWayBanner.SetActive(false);
            }

            // 7. Controls Help text
            if (controlsHelpText == null)
            {
                controlsHelpText = CreateTextElement("ControlsHelp_Text", new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(35f, 35f), new Vector2(650f, 50f), 14, TextAnchor.LowerLeft, new Color(0.7f, 0.82f, 0.95f, 0.75f), defaultFont);
            }
        }

        private Text CreateTextElement(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 sizeDelta, int fontSize, TextAnchor alignment, Color color, Font font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.fontStyle = FontStyle.Bold;

            // Add subtle shadow for legibility over bright lights
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        public void SetTargetVehicle(VehicleController vehicle)
        {
            targetVehicle = vehicle;
        }

        public void BindVehicle(VehicleController vehicle)
        {
            SetTargetVehicle(vehicle);
        }

        private void Update()
        {
            if (targetVehicle == null)
            {
                targetVehicle = FindAnyObjectByType<VehicleController>();
                if (targetVehicle == null) return;
            }

            float speed = targetVehicle.CurrentSpeedKmh;
            if (speedText != null)
            {
                speedText.text = $"{Mathf.RoundToInt(speed)} <size=24>KM/H</size>";
            }

            if (gearText != null)
            {
                if (targetVehicle.IsReversing)
                {
                    gearText.text = "R";
                    gearText.color = new Color(1f, 0.35f, 0.35f);
                }
                else if (speed < 1f)
                {
                    gearText.text = "N";
                    gearText.color = Color.white;
                }
                else
                {
                    gearText.text = targetVehicle.CurrentGear.ToString();
                    gearText.color = new Color(0.35f, 0.85f, 1f);
                }
            }

            if (carNameText != null && targetVehicle.Config != null)
            {
                carNameText.text = $"GAMEIUM  //  {targetVehicle.Config.displayName.ToUpper()}";
            }

            if (rpmFillBar != null)
            {
                rpmFillBar.fillAmount = Mathf.Clamp01(targetVehicle.EngineRpm / 7500f);
            }

            if (handbrakeAlert != null)
            {
                handbrakeAlert.SetActive(targetVehicle.IsHandbraking);
            }

            // Race progression updates
            if (Race.RaceManager.Instance != null && Race.RaceManager.Instance.Checkpoints != null)
            {
                var cpManager = Race.RaceManager.Instance.Checkpoints;
                var progress = cpManager.GetRacerProgress(targetVehicle);
                if (progress != null)
                {
                    if (lapText != null)
                    {
                        int currentLap = Mathf.Min(progress.currentLap, cpManager.TotalLaps);
                        lapText.text = $"LAP <size=32><b>{currentLap}</b></size>/{cpManager.TotalLaps}";
                    }

                    if (positionText != null)
                    {
                        int rank = cpManager.GetVehiclePosition(targetVehicle);
                        string suffix = (rank == 1) ? "st" : (rank == 2) ? "nd" : (rank == 3) ? "rd" : "th";
                        positionText.text = $"<size=44><b>{rank}</b></size>{suffix}";
                        positionText.color = rank switch
                        {
                            1 => new Color(1f, 0.85f, 0.12f),
                            2 => new Color(0.85f, 0.85f, 0.9f),
                            3 => new Color(0.85f, 0.55f, 0.3f),
                            _ => Color.white
                        };
                    }

                    if (lapTimeText != null)
                    {
                        int minutes = Mathf.FloorToInt(progress.currentLapTime / 60f);
                        float seconds = progress.currentLapTime % 60f;
                        lapTimeText.text = $"TIME: {minutes:00}:{seconds:05.2f}";
                    }

                    if (wrongWayBanner != null)
                    {
                        wrongWayBanner.SetActive(progress.isWrongWay);
                    }
                }
            }
        }
    }
}

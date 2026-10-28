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
        [SerializeField] private Image throttleFillBar;
        [SerializeField] private Image brakeFillBar;
        [SerializeField] private GameObject handbrakeAlert;

        [Header("Info / Controls Help")]
        [SerializeField] private Text controlsHelpText;

        private void Start()
        {
            if (targetVehicle == null)
            {
                targetVehicle = FindFirstObjectByType<VehicleController>();
            }

            if (controlsHelpText != null)
            {
                controlsHelpText.text = "<b>W/UP:</b> Throttle  |  <b>S/DOWN:</b> Brake/Rev  |  <b>A/D:</b> Steer\n<b>SPACE:</b> Handbrake  |  <b>R:</b> Reset  |  <b>C:</b> Change Camera";
            }
        }

        public void SetTargetVehicle(VehicleController vehicle)
        {
            targetVehicle = vehicle;
        }

        private void Update()
        {
            if (targetVehicle == null)
            {
                targetVehicle = FindFirstObjectByType<VehicleController>();
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
                carNameText.text = $"THEGAMEIO RACING  //  {targetVehicle.Config.displayName.ToUpper()}";
            }

            if (rpmFillBar != null)
            {
                rpmFillBar.fillAmount = Mathf.Clamp01(targetVehicle.EngineRpm / 7500f);
            }

            if (handbrakeAlert != null)
            {
                handbrakeAlert.SetActive(targetVehicle.IsHandbraking);
            }
        }
    }
}

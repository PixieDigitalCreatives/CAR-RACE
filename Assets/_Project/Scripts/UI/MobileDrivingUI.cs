using UnityEngine;
using UnityEngine.EventSystems;
using ThegameioRacing.Input;

namespace ThegameioRacing.UI
{
    public class MobileDrivingUI : MonoBehaviour
    {
        [Header("Target Input Reader")]
        [SerializeField] private VehicleInputReader inputReader;

        private bool leftPressed;
        private bool rightPressed;
        private bool throttlePressed;
        private bool brakePressed;
        private bool handbrakePressed;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = FindAnyObjectByType<VehicleInputReader>();
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                inputReader = FindAnyObjectByType<VehicleInputReader>();
                if (inputReader == null) return;
            }

            float steer = 0f;
            if (leftPressed) steer -= 1f;
            if (rightPressed) steer += 1f;

            inputReader.SetMobileSteer(steer);
            inputReader.SetMobileThrottle(throttlePressed ? 1f : 0f);
            inputReader.SetMobileBrake(brakePressed ? 1f : 0f);
            inputReader.SetMobileHandbrake(handbrakePressed);
        }

        public void BindInputReader(VehicleInputReader reader)
        {
            inputReader = reader;
        }

        public void OnLeftDown() => leftPressed = true;
        public void OnLeftUp() => leftPressed = false;

        public void OnRightDown() => rightPressed = true;
        public void OnRightUp() => rightPressed = false;

        public void OnThrottleDown() => throttlePressed = true;
        public void OnThrottleUp() => throttlePressed = false;

        public void OnBrakeDown() => brakePressed = true;
        public void OnBrakeUp() => brakePressed = false;

        public void OnHandbrakeDown() => handbrakePressed = true;
        public void OnHandbrakeUp() => handbrakePressed = false;

        public void OnResetClicked()
        {
            if (inputReader != null) inputReader.TriggerMobileReset();
        }
    }
}

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ThegameioRacing.Input
{
    /// <summary>
    /// Reads inputs from Keyboard, Gamepad, and Mobile On-Screen Controls,
    /// feeding them into IVehicleInput for the VehicleController.
    /// </summary>
    public class VehicleInputReader : MonoBehaviour, IVehicleInput
    {
        [Header("Runtime State")]
        [SerializeField] private float steerInput;
        [SerializeField] private float throttleInput;
        [SerializeField] private float brakeInput;
        [SerializeField] private bool handbrakeInput;
        [SerializeField] private bool resetTriggered;

        [Header("Smoothing")]
        [SerializeField] private float keyboardSteerSmoothing = 10f;
        [SerializeField] private bool inputEnabled = true;
        private float targetSteer;

        // Interface implementation
        public float SteerInput => steerInput;
        public float ThrottleInput => throttleInput;
        public float BrakeInput => brakeInput;
        public bool HandbrakeInput => handbrakeInput;
        public bool ResetTriggered => resetTriggered;

        // Mobile on-screen overrides
        private float mobileSteer;
        private float mobileThrottle;
        private float mobileBrake;
        private bool mobileHandbrake;
        private bool mobileReset;

        private void Update()
        {
            float rawSteer = 0f;
            float rawThrottle = 0f;
            float rawBrake = 0f;
            bool rawHandbrake = false;
            bool rawReset = false;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) rawSteer -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) rawSteer += 1f;

                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) rawThrottle = 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) rawBrake = 1f;

                if (kb.spaceKey.isPressed) rawHandbrake = true;
                if (kb.rKey.wasPressedThisFrame) rawReset = true;
            }

            var gp = Gamepad.current;
            if (gp != null)
            {
                float stickX = gp.leftStick.x.ReadValue();
                if (Mathf.Abs(stickX) > 0.1f) rawSteer = stickX;

                float rightTrigger = gp.rightTrigger.ReadValue();
                if (rightTrigger > 0.05f) rawThrottle = rightTrigger;

                float leftTrigger = gp.leftTrigger.ReadValue();
                if (leftTrigger > 0.05f) rawBrake = leftTrigger;

                if (gp.buttonSouth.isPressed || gp.rightShoulder.isPressed) rawHandbrake = true;
                if (gp.buttonNorth.wasPressedThisFrame) rawReset = true;
            }
#else
            rawSteer = UnityEngine.Input.GetAxis("Horizontal");
            float vert = UnityEngine.Input.GetAxis("Vertical");
            if (vert > 0f) rawThrottle = vert;
            else if (vert < 0f) rawBrake = -vert;

            rawHandbrake = UnityEngine.Input.GetKey(KeyCode.Space);
            rawReset = UnityEngine.Input.GetKeyDown(KeyCode.R);
#endif

            // Blend with Mobile UI if touched
            if (Mathf.Abs(mobileSteer) > 0.05f) rawSteer = mobileSteer;
            if (mobileThrottle > 0.05f) rawThrottle = Mathf.Max(rawThrottle, mobileThrottle);
            if (mobileBrake > 0.05f) rawBrake = Mathf.Max(rawBrake, mobileBrake);
            if (mobileHandbrake) rawHandbrake = true;
            if (mobileReset)
            {
                rawReset = true;
                mobileReset = false;
            }

            if (!inputEnabled)
            {
                steerInput = 0f;
                throttleInput = 0f;
                brakeInput = 1f; // Apply brakes when locked at grid
                handbrakeInput = true;
                resetTriggered = false;
                return;
            }

            targetSteer = Mathf.Clamp(rawSteer, -1f, 1f);
            steerInput = Mathf.MoveTowards(steerInput, targetSteer, Time.deltaTime * keyboardSteerSmoothing);
            throttleInput = Mathf.Clamp01(rawThrottle);
            brakeInput = Mathf.Clamp01(rawBrake);
            handbrakeInput = rawHandbrake;
            resetTriggered = rawReset;
        }

        public void SetInputEnabled(bool enabled) => inputEnabled = enabled;

        #region Mobile UI Injection
        public void SetMobileSteer(float value) => mobileSteer = Mathf.Clamp(value, -1f, 1f);
        public void SetMobileThrottle(float value) => mobileThrottle = Mathf.Clamp01(value);
        public void SetMobileBrake(float value) => mobileBrake = Mathf.Clamp01(value);
        public void SetMobileHandbrake(bool value) => mobileHandbrake = value;
        public void TriggerMobileReset() => mobileReset = true;
        #endregion
    }
}

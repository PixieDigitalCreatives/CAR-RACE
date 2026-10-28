using System;
using UnityEngine;
using ThegameioRacing.Input;

namespace ThegameioRacing.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private VehicleConfig config;

        [Header("Wheel Colliders")]
        [SerializeField] private WheelCollider frontLeftCollider;
        [SerializeField] private WheelCollider frontRightCollider;
        [SerializeField] private WheelCollider rearLeftCollider;
        [SerializeField] private WheelCollider rearRightCollider;

        [Header("Visual Wheel Meshes")]
        [SerializeField] private Transform frontLeftVisual;
        [SerializeField] private Transform frontRightVisual;
        [SerializeField] private Transform rearLeftVisual;
        [SerializeField] private Transform rearRightVisual;

        [Header("Input Source")]
        [SerializeField] private MonoBehaviour inputSourceBehaviour;
        private IVehicleInput inputSource;

        [Header("Audio / FX Hooks")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private TrailRenderer[] tireSkidTrails;

        // Cached components
        private Rigidbody rb;

        // Telemetry
        public float CurrentSpeedKmh { get; private set; }
        public float ForwardSpeedMps { get; private set; }
        public float EngineRpm { get; private set; }
        public int CurrentGear { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsReversing { get; private set; }
        public bool IsHandbraking { get; private set; }
        public float OverallSlip { get; private set; }

        public VehicleConfig Config => config;

        public void SetEngineAudio(AudioSource source)
        {
            engineAudioSource = source;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (inputSourceBehaviour != null && inputSourceBehaviour is IVehicleInput vi)
            {
                inputSource = vi;
            }
            else
            {
                inputSource = GetComponent<IVehicleInput>();
            }

            ApplyConfig();
        }

        public void SetConfig(VehicleConfig newConfig)
        {
            config = newConfig;
            ApplyConfig();
        }

        public void SetInputSource(IVehicleInput newInput)
        {
            inputSource = newInput;
        }

        public void ApplyConfig()
        {
            if (config == null || rb == null) return;

            rb.mass = config.mass;
            rb.centerOfMass = config.centerOfMassOffset;

            ConfigureWheelFriction(frontLeftCollider);
            ConfigureWheelFriction(frontRightCollider);
            ConfigureWheelFriction(rearLeftCollider);
            ConfigureWheelFriction(rearRightCollider);
        }

        private void ConfigureWheelFriction(WheelCollider wc)
        {
            if (wc == null || config == null) return;

            // Suspension
            JointSpring spring = wc.suspensionSpring;
            spring.spring = config.springRate;
            spring.damper = config.damperRate;
            spring.targetPosition = config.targetPosition;
            wc.suspensionSpring = spring;
            wc.suspensionDistance = config.suspensionDistance;

            // Forward Friction
            WheelFrictionCurve forward = wc.forwardFriction;
            forward.extremumSlip = config.forwardExtremumSlip;
            forward.extremumValue = config.forwardExtremumValue;
            forward.asymptoteSlip = config.forwardAsymptoteSlip;
            forward.asymptoteValue = config.forwardAsymptoteValue;
            forward.stiffness = config.forwardStiffness;
            wc.forwardFriction = forward;

            // Sideways Friction
            WheelFrictionCurve sideways = wc.sidewaysFriction;
            sideways.extremumSlip = config.sidewaysExtremumSlip;
            sideways.extremumValue = config.sidewaysExtremumValue;
            sideways.asymptoteSlip = config.sidewaysAsymptoteSlip;
            sideways.asymptoteValue = config.sidewaysAsymptoteValue;
            sideways.stiffness = config.sidewaysStiffness;
            wc.sidewaysFriction = sideways;
        }

        private void Update()
        {
            UpdateVisualWheels();
            UpdateTelemetryAndAudio();

            if (inputSource != null && inputSource.ResetTriggered)
            {
                ResetVehicle();
            }
        }

        private void FixedUpdate()
        {
            if (config == null) return;

            CalculateSpeed();
            ProcessSteering();
            ProcessMotorAndBrakes();
            ApplyAntiRollBars();
            ApplyAerodynamicDownforce();
        }

        private void CalculateSpeed()
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 vel = rb.linearVelocity;
#else
            Vector3 vel = rb.velocity;
#endif
            ForwardSpeedMps = Vector3.Dot(vel, transform.forward);
            CurrentSpeedKmh = vel.magnitude * 3.6f;

            bool flG = frontLeftCollider != null && frontLeftCollider.isGrounded;
            bool frG = frontRightCollider != null && frontRightCollider.isGrounded;
            bool rlG = rearLeftCollider != null && rearLeftCollider.isGrounded;
            bool rrG = rearRightCollider != null && rearRightCollider.isGrounded;
            IsGrounded = flG || frG || rlG || rrG;
        }

        private void ProcessSteering()
        {
            float steerInput = (inputSource != null) ? inputSource.SteerInput : 0f;

            // Speed-sensitive steering angle
            float speedRatio = Mathf.Clamp01(CurrentSpeedKmh / Mathf.Max(config.topSpeedKmh, 10f));
            float targetAngle = Mathf.Lerp(config.maxSteerAngle, config.highSpeedSteerAngle, speedRatio) * steerInput;

            if (frontLeftCollider != null)
                frontLeftCollider.steerAngle = Mathf.MoveTowards(frontLeftCollider.steerAngle, targetAngle, config.steerSpeed * 20f * Time.fixedDeltaTime);
            if (frontRightCollider != null)
                frontRightCollider.steerAngle = Mathf.MoveTowards(frontRightCollider.steerAngle, targetAngle, config.steerSpeed * 20f * Time.fixedDeltaTime);
        }

        private void ProcessMotorAndBrakes()
        {
            float throttle = (inputSource != null) ? inputSource.ThrottleInput : 0f;
            float brake = (inputSource != null) ? inputSource.BrakeInput : 0f;
            bool handbrake = (inputSource != null) && inputSource.HandbrakeInput;
            IsHandbraking = handbrake;

            // Determine forward vs reverse
            if (ForwardSpeedMps < 0.5f && brake > 0.1f)
            {
                IsReversing = true;
            }
            else if (throttle > 0.1f)
            {
                IsReversing = false;
            }

            float currentMotorTorque = 0f;
            float currentFootBrakeTorque = 0f;

            if (!IsReversing)
            {
                // Forward drive
                if (CurrentSpeedKmh < config.topSpeedKmh)
                {
                    currentMotorTorque = throttle * config.motorTorque;
                }
                currentFootBrakeTorque = brake * config.brakeTorque;
            }
            else
            {
                // Reverse drive
                if (CurrentSpeedKmh < config.maxReverseSpeedKmh)
                {
                    currentMotorTorque = -brake * config.reverseTorque;
                }
                currentFootBrakeTorque = throttle * config.brakeTorque;
            }

            // Apply motor torque based on drivetrain
            float frontTorque = 0f;
            float rearTorque = 0f;

            switch (config.drivetrain)
            {
                case DrivetrainType.AllWheelDrive:
                    frontTorque = currentMotorTorque * 0.45f;
                    rearTorque = currentMotorTorque * 0.55f;
                    break;
                case DrivetrainType.FrontWheelDrive:
                    frontTorque = currentMotorTorque * 0.95f;
                    break;
                case DrivetrainType.RearWheelDrive:
                    rearTorque = currentMotorTorque * 0.95f;
                    break;
            }

            if (frontLeftCollider != null) frontLeftCollider.motorTorque = frontTorque;
            if (frontRightCollider != null) frontRightCollider.motorTorque = frontTorque;
            if (rearLeftCollider != null) rearLeftCollider.motorTorque = rearTorque;
            if (rearRightCollider != null) rearRightCollider.motorTorque = rearTorque;

            // Apply foot brake
            if (frontLeftCollider != null) frontLeftCollider.brakeTorque = currentFootBrakeTorque * 0.6f;
            if (frontRightCollider != null) frontRightCollider.brakeTorque = currentFootBrakeTorque * 0.6f;
            if (rearLeftCollider != null) rearLeftCollider.brakeTorque = currentFootBrakeTorque * 0.4f;
            if (rearRightCollider != null) rearRightCollider.brakeTorque = currentFootBrakeTorque * 0.4f;

            // Handbrake overrides rear wheels and adjusts slip
            if (handbrake)
            {
                if (rearLeftCollider != null) rearLeftCollider.brakeTorque = config.handbrakeTorque;
                if (rearRightCollider != null) rearRightCollider.brakeTorque = config.handbrakeTorque;

                ApplyHandbrakeDriftFriction(rearLeftCollider, true);
                ApplyHandbrakeDriftFriction(rearRightCollider, true);
            }
            else
            {
                ApplyHandbrakeDriftFriction(rearLeftCollider, false);
                ApplyHandbrakeDriftFriction(rearRightCollider, false);
            }
        }

        private void ApplyHandbrakeDriftFriction(WheelCollider wc, bool handbraking)
        {
            if (wc == null || config == null) return;

            WheelFrictionCurve sideways = wc.sidewaysFriction;
            sideways.stiffness = handbraking ? (config.sidewaysStiffness * config.handbrakeDriftSlipFactor) : config.sidewaysStiffness;
            wc.sidewaysFriction = sideways;
        }

        private void ApplyAntiRollBars()
        {
            ApplyAntiRoll(frontLeftCollider, frontRightCollider, config.frontAntiRoll);
            ApplyAntiRoll(rearLeftCollider, rearRightCollider, config.rearAntiRoll);
        }

        private void ApplyAntiRoll(WheelCollider left, WheelCollider right, float stiffness)
        {
            if (left == null || right == null) return;

            float travelL = 1.0f;
            float travelR = 1.0f;

            bool groundedL = left.GetGroundHit(out WheelHit hitL);
            if (groundedL)
            {
                travelL = (-left.transform.InverseTransformPoint(hitL.point).y - left.radius) / left.suspensionDistance;
            }

            bool groundedR = right.GetGroundHit(out WheelHit hitR);
            if (groundedR)
            {
                travelR = (-right.transform.InverseTransformPoint(hitR.point).y - right.radius) / right.suspensionDistance;
            }

            float antiRollForce = (travelL - travelR) * stiffness;

            if (groundedL)
                rb.AddForceAtPosition(left.transform.up * -antiRollForce, left.transform.position);
            if (groundedR)
                rb.AddForceAtPosition(right.transform.up * antiRollForce, right.transform.position);
        }

        private void ApplyAerodynamicDownforce()
        {
            float speed = Mathf.Max(0f, ForwardSpeedMps);
            float downforce = speed * speed * config.downforceCoefficient * 0.05f;
            rb.AddForce(-transform.up * downforce);
        }

        private void UpdateVisualWheels()
        {
            SyncWheelMesh(frontLeftCollider, frontLeftVisual);
            SyncWheelMesh(frontRightCollider, frontRightVisual);
            SyncWheelMesh(rearLeftCollider, rearLeftVisual);
            SyncWheelMesh(rearRightCollider, rearRightVisual);
        }

        private void SyncWheelMesh(WheelCollider collider, Transform visual)
        {
            if (collider == null || visual == null) return;

            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            visual.position = pos;
            visual.rotation = rot;
        }

        private void UpdateTelemetryAndAudio()
        {
            if (config == null) return;

            // Simulated RPM and gear calculations
            float speedRatio = Mathf.Clamp01(CurrentSpeedKmh / Mathf.Max(config.topSpeedKmh, 10f));
            int gears = Mathf.Max(config.gearCount, 4);
            float gearRange = 1f / gears;
            CurrentGear = Mathf.Min(gears, Mathf.FloorToInt(speedRatio / gearRange) + 1);

            float speedInGear = (speedRatio % gearRange) / gearRange;
            EngineRpm = Mathf.Lerp(1000f, 7500f, speedInGear);

            if (engineAudioSource != null)
            {
                float targetPitch = Mathf.Lerp(0.8f, 2.2f, EngineRpm / 7500f);
                engineAudioSource.pitch = Mathf.MoveTowards(engineAudioSource.pitch, targetPitch, Time.deltaTime * 3f);
            }

            // Skid trail check
            float totalSlip = 0f;
            int count = 0;
            CheckWheelSlip(frontLeftCollider, ref totalSlip, ref count);
            CheckWheelSlip(frontRightCollider, ref totalSlip, ref count);
            CheckWheelSlip(rearLeftCollider, ref totalSlip, ref count);
            CheckWheelSlip(rearRightCollider, ref totalSlip, ref count);
            OverallSlip = count > 0 ? (totalSlip / count) : 0f;

            bool emitSkid = OverallSlip > 0.35f || IsHandbraking;
            if (tireSkidTrails != null)
            {
                foreach (var trail in tireSkidTrails)
                {
                    if (trail != null) trail.emitting = emitSkid && IsGrounded;
                }
            }
        }

        private void CheckWheelSlip(WheelCollider wc, ref float totalSlip, ref int count)
        {
            if (wc != null && wc.GetGroundHit(out WheelHit hit))
            {
                totalSlip += Mathf.Abs(hit.sidewaysSlip) + Mathf.Abs(hit.forwardSlip);
                count++;
            }
        }

        public void ResetVehicle()
        {
            Vector3 currentPos = transform.position;
            Vector3 currentEuler = transform.eulerAngles;

            // Clear pitch and roll, lift slightly
            transform.position = currentPos + Vector3.up * 1.0f;
            transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        public void SetupComponents(
            WheelCollider flC, WheelCollider frC, WheelCollider rlC, WheelCollider rrC,
            Transform flV, Transform frV, Transform rlV, Transform rrV,
            TrailRenderer[] trails = null)
        {
            frontLeftCollider = flC;
            frontRightCollider = frC;
            rearLeftCollider = rlC;
            rearRightCollider = rrC;

            frontLeftVisual = flV;
            frontRightVisual = frV;
            rearLeftVisual = rlV;
            rearRightVisual = rrV;

            if (trails != null)
            {
                tireSkidTrails = trails;
            }

            ApplyConfig();
        }
    }
}

using UnityEngine;

namespace ThegameioRacing.Vehicle
{
    public enum DrivetrainType
    {
        RearWheelDrive,
        FrontWheelDrive,
        AllWheelDrive
    }

    public enum VehicleRole
    {
        StarterBalanced,
        TunerDrift,
        HypercarSpeed,
        ClassicMuscle,
        StreetRally
    }

    [CreateAssetMenu(fileName = "VehicleConfig_", menuName = "THEGAMEIO Racing/Vehicle Config", order = 1)]
    public class VehicleConfig : ScriptableObject
    {
        [Header("Identity")]
        public string vehicleId = "car_regular";
        public string displayName = "Regular";
        [TextArea(2, 4)]
        public string description = "Balanced everyday sports car, easy to drive.";
        public VehicleRole role = VehicleRole.StarterBalanced;
        public Texture2D previewCard;

        [Header("Displayed Stat Ratings (0 - 100)")]
        [Range(0, 100)] public int statTopSpeed = 65;
        [Range(0, 100)] public int statAcceleration = 60;
        [Range(0, 100)] public int statHandling = 75;
        [Range(0, 100)] public int statBraking = 70;

        [Header("Chassis & Mass")]
        [Tooltip("Mass in kg")]
        public float mass = 1350f;
        [Tooltip("Center of mass offset. Lowering Y reduces roll and prevents rolling over.")]
        public Vector3 centerOfMassOffset = new Vector3(0f, -0.45f, 0.05f);
        [Tooltip("Aerodynamic downforce multiplier at speed.")]
        public float downforceCoefficient = 45f;

        [Header("Engine & Transmission")]
        public DrivetrainType drivetrain = DrivetrainType.AllWheelDrive;
        [Tooltip("Maximum engine motor torque in Nm.")]
        public float motorTorque = 1800f;
        [Tooltip("Maximum speed in km/h.")]
        public float topSpeedKmh = 210f;
        [Tooltip("Torque applied in reverse.")]
        public float reverseTorque = 1000f;
        [Tooltip("Maximum reverse speed in km/h.")]
        public float maxReverseSpeedKmh = 45f;
        [Tooltip("Simulated gear count for engine audio/RPM.")]
        public int gearCount = 6;

        [Header("Braking & Handbrake")]
        [Tooltip("Footbrake torque in Nm.")]
        public float brakeTorque = 4000f;
        [Tooltip("Handbrake torque applied to rear wheels in Nm.")]
        public float handbrakeTorque = 6000f;
        [Tooltip("Sideways friction multiplier when handbraking (allows drifting).")]
        [Range(0.1f, 1f)] public float handbrakeDriftSlipFactor = 0.35f;

        [Header("Steering")]
        [Tooltip("Maximum steering angle at zero speed (degrees).")]
        public float maxSteerAngle = 34f;
        [Tooltip("Steering angle at top speed (degrees).")]
        public float highSpeedSteerAngle = 14f;
        [Tooltip("How fast steering wheels turn towards input target.")]
        public float steerSpeed = 8f;

        [Header("Anti-Roll Bars")]
        [Tooltip("Front anti-roll bar stiffness.")]
        public float frontAntiRoll = 6000f;
        [Tooltip("Rear anti-roll bar stiffness.")]
        public float rearAntiRoll = 5000f;

        [Header("Suspension (WheelCollider)")]
        public float suspensionDistance = 0.22f;
        public float springRate = 38000f;
        public float damperRate = 4200f;
        [Range(0f, 1f)] public float targetPosition = 0.5f;

        [Header("Tire Friction")]
        public float forwardExtremumSlip = 0.4f;
        public float forwardExtremumValue = 1.0f;
        public float forwardAsymptoteSlip = 0.8f;
        public float forwardAsymptoteValue = 0.75f;
        public float forwardStiffness = 1.1f;

        public float sidewaysExtremumSlip = 0.35f;
        public float sidewaysExtremumValue = 1.0f;
        public float sidewaysAsymptoteSlip = 0.7f;
        public float sidewaysAsymptoteValue = 0.75f;
        public float sidewaysStiffness = 1.2f;

        [Header("Visual Styling")]
        public Color primaryColor = new Color(0.12f, 0.45f, 0.95f);
        public Color accentColor = new Color(0.98f, 0.55f, 0.12f);
    }
}

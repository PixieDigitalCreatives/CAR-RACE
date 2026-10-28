using UnityEngine;
using ThegameioRacing.Input;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.AI
{
    /// <summary>
    /// Autonomous racing driver implementing IVehicleInput. Navigates along
    /// WaypointNode tracks with obstacle avoidance, speed regulation, and error recovery.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class AIRacingController : MonoBehaviour, IVehicleInput
    {
        [Header("Path Navigation")]
        [SerializeField] private WaypointNode currentWaypoint;
        [SerializeField] private float waypointReachRadius = 8.5f;
        [SerializeField] private float steeringSensitivity = 0.15f;

        [Header("Aggression & Rubber-Banding")]
        [Range(0.7f, 1.3f)]
        [SerializeField] private float skillFactor = 1.0f;
        [SerializeField] private bool enableRubberBanding = true;
        [SerializeField] private Transform playerTransform;

        [Header("Obstacle Avoidance")]
        [SerializeField] private float sensorLength = 22f;
        [SerializeField] private float sideSensorLength = 14f;
        [SerializeField] private float sideSensorAngle = 20f;
        [SerializeField] private LayerMask obstacleLayers = ~0;

        // IVehicleInput state
        private float steerInput;
        private float throttleInput;
        private float brakeInput;
        private bool handbrakeInput;
        private bool resetTriggered;
        private bool inputEnabled = true;

        public float SteerInput => steerInput;
        public float ThrottleInput => throttleInput;
        public float BrakeInput => brakeInput;
        public bool HandbrakeInput => handbrakeInput;
        public bool ResetTriggered => resetTriggered;

        private VehicleController vehicle;
        private float stuckTimer = 0f;
        private float reverseTimer = 0f;
        private bool isReversing = false;

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
            vehicle.SetInputSource(this);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }
        }

        public void SetTargetWaypoint(WaypointNode waypoint)
        {
            currentWaypoint = waypoint;
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
            {
                steerInput = 0f;
                throttleInput = 0f;
                brakeInput = 1f;
                handbrakeInput = true;
            }
        }

        private void Update()
        {
            if (!inputEnabled || currentWaypoint == null)
            {
                if (!inputEnabled)
                {
                    throttleInput = 0f;
                    brakeInput = 1f;
                    handbrakeInput = true;
                }
                return;
            }

            // Check if reached current waypoint
            Vector3 toWp = currentWaypoint.transform.position - transform.position;
            toWp.y = 0f;
            if (toWp.sqrMagnitude < waypointReachRadius * waypointReachRadius)
            {
                if (currentWaypoint.NextNode != null)
                {
                    currentWaypoint = currentWaypoint.NextNode;
                }
            }

            // Handle stuck recovery
            float speed = vehicle.CurrentSpeedKmh;
            if (throttleInput > 0.5f && speed < 3f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 2.2f)
                {
                    isReversing = true;
                    reverseTimer = 1.4f;
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = Mathf.Max(0f, stuckTimer - Time.deltaTime);
            }

            if (isReversing)
            {
                reverseTimer -= Time.deltaTime;
                steerInput = 0.5f;
                throttleInput = 0f;
                brakeInput = 0.8f; // Reverse in automatic transmission
                handbrakeInput = false;

                if (reverseTimer <= 0f)
                {
                    isReversing = false;
                }
                return;
            }

            // Steering calculation
            Vector3 localWp = transform.InverseTransformPoint(currentWaypoint.transform.position);
            float targetSteer = Mathf.Clamp(localWp.x * steeringSensitivity, -1f, 1f);

            // Obstacle Avoidance Sensors
            float avoidance = CalculateAvoidance();
            targetSteer = Mathf.Clamp(targetSteer + avoidance, -1f, 1f);
            steerInput = Mathf.MoveTowards(steerInput, targetSteer, Time.deltaTime * 8f);

            // Speed & Throttle/Brake regulation
            float targetSpeed = currentWaypoint.TargetSpeedKmh * skillFactor;

            // Rubber-banding (tune speed based on distance to player)
            if (enableRubberBanding && playerTransform != null)
            {
                float playerDistZ = transform.position.z - playerTransform.position.z;
                if (playerDistZ > 45f) // AI far ahead: ease off
                {
                    targetSpeed *= 0.88f;
                }
                else if (playerDistZ < -45f) // AI far behind: catch up
                {
                    targetSpeed *= 1.15f;
                }
            }

            if (speed < targetSpeed * 0.96f)
            {
                throttleInput = 1f;
                brakeInput = 0f;
                handbrakeInput = false;
            }
            else if (speed > targetSpeed * 1.06f)
            {
                throttleInput = 0f;
                brakeInput = Mathf.Clamp01((speed - targetSpeed) / 18f);
                handbrakeInput = (speed > targetSpeed * 1.35f && Mathf.Abs(steerInput) > 0.6f);
            }
            else
            {
                throttleInput = 0.55f;
                brakeInput = 0f;
                handbrakeInput = false;
            }
        }

        private float CalculateAvoidance()
        {
            float avoidanceOffset = 0f;
            Vector3 origin = transform.position + Vector3.up * 0.6f;

            // Center forward ray
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hitC, sensorLength, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                if (hitC.transform.root != transform.root)
                {
                    avoidanceOffset += (hitC.normal.x > 0f) ? 0.75f : -0.75f;
                }
            }

            // Left angled ray
            Quaternion leftRot = Quaternion.AngleAxis(-sideSensorAngle, Vector3.up);
            Vector3 leftDir = leftRot * transform.forward;
            if (Physics.Raycast(origin, leftDir, out RaycastHit hitL, sideSensorLength, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                if (hitL.transform.root != transform.root)
                {
                    avoidanceOffset += 0.55f; // Steer right
                }
            }

            // Right angled ray
            Quaternion rightRot = Quaternion.AngleAxis(sideSensorAngle, Vector3.up);
            Vector3 rightDir = rightRot * transform.forward;
            if (Physics.Raycast(origin, rightDir, out RaycastHit hitR, sideSensorLength, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                if (hitR.transform.root != transform.root)
                {
                    avoidanceOffset -= 0.55f; // Steer left
                }
            }

            return avoidanceOffset;
        }

        private void OnDrawGizmosSelected()
        {
            if (currentWaypoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + Vector3.up * 1f, currentWaypoint.transform.position);
            }
        }
    }
}

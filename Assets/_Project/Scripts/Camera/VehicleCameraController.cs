using UnityEngine;
using ThegameioRacing.Vehicle;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ThegameioRacing.Camera
{
    public class VehicleCameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            ChaseClose,
            ChaseFar,
            HoodBumper
        }

        [Header("Target & Mode")]
        [SerializeField] private Transform target;
        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private CameraMode mode = CameraMode.ChaseClose;

        [Header("Close Chase Settings")]
        [SerializeField] private float closeDistance = 5.2f;
        [SerializeField] private float closeHeight = 2.1f;
        [SerializeField] private float closeLookAhead = 2.5f;

        [Header("Far Chase Settings")]
        [SerializeField] private float farDistance = 7.5f;
        [SerializeField] private float farHeight = 3.2f;
        [SerializeField] private float farLookAhead = 4.0f;

        [Header("Hood View Settings")]
        [SerializeField] private Vector3 hoodOffset = new Vector3(0f, 1.25f, 0.4f);

        [Header("Damping & Physics")]
        [SerializeField] private float positionDamping = 12f;
        [SerializeField] private float rotationDamping = 10f;
        [SerializeField] private float lookAtDamping = 15f;

        [Header("Speed Dynamics")]
        [SerializeField] private float baseFov = 60f;
        [SerializeField] private float maxFov = 76f;
        [SerializeField] private float fovDamping = 4f;

        [Header("Collision Avoidance")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float cameraCollisionRadius = 0.25f;

        private UnityEngine.Camera cam;
        private Vector3 smoothedLookAt;
        private Vector3 currentVelocity;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null) cam = UnityEngine.Camera.main;

            if (target != null && vehicleController == null)
            {
                vehicleController = target.GetComponent<VehicleController>();
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                vehicleController = target.GetComponent<VehicleController>();
                smoothedLookAt = target.position + target.up * 1.2f;
            }
        }

        public void CycleCameraMode()
        {
            mode = (CameraMode)(((int)mode + 1) % 3);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.cKey.wasPressedThisFrame)
            {
                CycleCameraMode();
            }
#else
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                CycleCameraMode();
            }
#endif
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float currentSpeedKmh = (vehicleController != null) ? vehicleController.CurrentSpeedKmh : 0f;
            float topSpeed = (vehicleController != null && vehicleController.Config != null) ? vehicleController.Config.topSpeedKmh : 200f;
            float speedRatio = Mathf.Clamp01(currentSpeedKmh / Mathf.Max(topSpeed, 50f));

            // Dynamic FOV
            if (cam != null)
            {
                float targetFov = Mathf.Lerp(baseFov, maxFov, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovDamping);
            }

            if (mode == CameraMode.HoodBumper)
            {
                Vector3 hoodPos = target.TransformPoint(hoodOffset);
                transform.position = Vector3.Lerp(transform.position, hoodPos, Time.deltaTime * 25f);
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * 25f);
                return;
            }

            float dist = (mode == CameraMode.ChaseFar) ? farDistance : closeDistance;
            float height = (mode == CameraMode.ChaseFar) ? farHeight : closeHeight;
            float lookAheadDist = (mode == CameraMode.ChaseFar) ? farLookAhead : closeLookAhead;

            // Target position behind vehicle oriented along its forward vector
            Vector3 targetEuler = target.eulerAngles;
            Quaternion desiredRotation = Quaternion.Euler(0f, targetEuler.y, 0f);

            Vector3 desiredPosition = target.position - (desiredRotation * Vector3.forward * dist) + (Vector3.up * height);

            // Collision check against track geometry / buildings
            Vector3 targetCenter = target.position + Vector3.up * (height * 0.5f);
            Vector3 rayDir = (desiredPosition - targetCenter).normalized;
            float desiredDist = Vector3.Distance(targetCenter, desiredPosition);

            if (Physics.SphereCast(targetCenter, cameraCollisionRadius, rayDir, out RaycastHit hit, desiredDist, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                desiredPosition = targetCenter + rayDir * Mathf.Max(1.5f, hit.distance - cameraCollisionRadius);
            }

            // Smooth position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionDamping);

            // Dynamic look-ahead point
            Vector3 lookTarget = target.position + Vector3.up * 1.1f + target.forward * (lookAheadDist * (0.8f + speedRatio * 0.5f));
            smoothedLookAt = Vector3.Lerp(smoothedLookAt, lookTarget, Time.deltaTime * lookAtDamping);

            transform.LookAt(smoothedLookAt);
        }
    }
}

using UnityEngine;
using ThegameioRacing.Vehicle;

namespace ThegameioRacing.Race
{
    /// <summary>
    /// Trigger checkpoint along the racing line. Enforces route progression,
    /// wrong-way detection, and split-timing metrics.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int index = 0;
        [SerializeField] private bool isFinishLine = false;

        private CheckpointManager manager;

        public int Index => index;
        public bool IsFinishLine => isFinishLine;

        public void Initialize(int checkpointIndex, bool finishLine, CheckpointManager checkpointManager)
        {
            index = checkpointIndex;
            isFinishLine = finishLine;
            manager = checkpointManager;

            BoxCollider col = GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            VehicleController vehicle = other.GetComponentInParent<VehicleController>();
            if (vehicle == null) return;

            // Check travel direction relative to track forward
            float alignment = Vector3.Dot(vehicle.transform.forward, transform.forward);
            bool isWrongWay = alignment < -0.25f;

            if (manager != null)
            {
                manager.OnVehicleHitCheckpoint(vehicle, this, isWrongWay);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isFinishLine ? new Color(1f, 0.85f, 0f, 0.45f) : new Color(0f, 0.8f, 1f, 0.3f);
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.center, col.size);
                Gizmos.color = isFinishLine ? Color.yellow : Color.cyan;
                Gizmos.DrawWireCube(col.center, col.size);

                // Draw forward pointing arrow
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(col.center, Vector3.forward * (col.size.z + 2f));
            }
        }
    }
}

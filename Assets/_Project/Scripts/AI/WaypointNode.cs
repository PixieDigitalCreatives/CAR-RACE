using UnityEngine;

namespace ThegameioRacing.AI
{
    /// <summary>
    /// Represents a single waypoint node along the AI racing spline,
    /// defining path position, target speed, and width limits.
    /// </summary>
    public class WaypointNode : MonoBehaviour
    {
        [Header("Waypoint Data")]
        [SerializeField] private float targetSpeedKmh = 160f;
        [SerializeField] private float corridorWidth = 6f;
        [SerializeField] private WaypointNode nextNode;

        public float TargetSpeedKmh => targetSpeedKmh;
        public float CorridorWidth => corridorWidth;
        public WaypointNode NextNode
        {
            get => nextNode;
            set => nextNode = value;
        }

        public void SetParameters(float speedKmh, float width, WaypointNode next)
        {
            targetSpeedKmh = speedKmh;
            corridorWidth = width;
            nextNode = next;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.8f);

            if (nextNode != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, nextNode.transform.position);

                // Draw corridor width bar
                Vector3 toNext = (nextNode.transform.position - transform.position).normalized;
                Vector3 side = Vector3.Cross(Vector3.up, toNext) * (corridorWidth * 0.5f);
                Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.4f);
                Gizmos.DrawLine(transform.position - side, transform.position + side);
            }
        }
    }
}


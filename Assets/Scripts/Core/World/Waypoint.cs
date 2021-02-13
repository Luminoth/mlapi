using UnityEngine;

// TODO: this doesn't belong in Core, just merge it up to the Game version
namespace pdxpartyparrot.Core.World
{
    public class Waypoint : MonoBehaviour
    {
        [SerializeField]
        private Waypoint _nextWaypoint;

        public Waypoint NextWaypoint => _nextWaypoint;

        [SerializeField]
        private float _cooldown;

        public float Cooldown => _cooldown;

        public bool HasCooldown => _cooldown > 0.0f;

        public Transform Transform { get; private set; }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Transform = transform;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1);
        }

        #endregion
    }
}

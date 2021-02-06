using pdxpartyparrot.Core.World;

using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    public class PlatformWaypoint : Waypoint
    {
        public PlatformWaypoint NextPlatformWaypoint => (PlatformWaypoint)NextWaypoint;

        [SerializeField]
        private float _cooldown;

        public float Cooldown => _cooldown;
    }
}

using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    [RequireComponent(typeof(Collider))]
    public class WorldBoundary3D : WorldBoundary
    {
        #region Unity Lifecycle

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollisionEnter(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleCollisionExit(other.gameObject);
        }

        #endregion
    }
}

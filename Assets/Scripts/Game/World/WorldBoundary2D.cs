using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldBoundary2D : WorldBoundary
    {
        #region Unity Lifecycle

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollisionEnter(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            HandleCollisionExit(other.gameObject);
        }

        #endregion
    }
}

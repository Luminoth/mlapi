using pdxpartyparrot.Core;
using pdxpartyparrot.Game.State;

using UnityEngine;
using UnityEngine.AI;

namespace pdxpartyparrot.Game.World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class StaticObstacle : MonoBehaviour
    {
        private Collider _collider;

        private NavMeshObstacle _obstacle;

        #region Unity Lifecycle

        private void Awake()
        {
            gameObject.layer = GameStateManager.Instance.GameManager.GameData.WorldLayer;

            _collider = GetComponent<Collider>();
            _collider.sharedMaterial = PartyParrotManager.Instance.FrictionlessMaterial;

            _obstacle = GetComponent<NavMeshObstacle>();
            _obstacle.carving = true;
        }

        #endregion
    }
}

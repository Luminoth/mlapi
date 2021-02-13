using pdxpartyparrot.Core.Effects;

using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    [RequireComponent(typeof(Collider))]
    public abstract class Teleporter : MonoBehaviour
    {
        [SerializeField]
        private Teleporter _exit;

        [SerializeField]
        private Transform _exitPoint;

        protected Transform ExitPoint => _exitPoint;

        #region Effects

        [SerializeField]
        private EffectTrigger _enterEffect;

        [SerializeField]
        private EffectTrigger _exitEffect;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            GetComponent<Collider>().isTrigger = true;

            // only allow rotating around the y axis
            // TODO: make this a set of flags like how it is on Rigidbody
            Vector3 rot = _exitPoint.eulerAngles;
            rot.y = 0.0f;
            rot.z = 0.0f;
            _exitPoint.eulerAngles = rot;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if(!CanTeleport(other.gameObject)) {
                return;
            }

            if(null != _enterEffect) {
                _enterEffect.Trigger(() => {
                    _exit.Teleport(other.gameObject, this);
                });
            } else {
                _exit.Teleport(other.gameObject, this);
            }
        }

        #endregion

        protected abstract bool CanTeleport(GameObject gameObject);

        private void Teleport(GameObject gameObject, Teleporter source)
        {
            if(null != _exitEffect) {
                _exitEffect.Trigger(() => {
                    OnTeleport(gameObject, source);
                });
            } else {
                OnTeleport(gameObject, source);
            }
        }

        // called on the destination teleporter
        protected virtual void OnTeleport(GameObject gameObject, Teleporter source)
        {
        }
    }
}

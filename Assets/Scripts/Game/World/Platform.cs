using pdxpartyparrot.Core;
using pdxpartyparrot.Core.Time;
using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Game.State;

using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    [RequireComponent(typeof(Collider))]
    public abstract class Platform : MonoBehaviour
    {
        [SerializeField]
        private PlatformWaypoint _initialWaypoint;

        [SerializeField]
        private float _speed = 5.0f;

        [SerializeField]
        [ReadOnly]
        private PlatformWaypoint _nextWaypoint;

        [SerializeReference]
        [ReadOnly]
        private ITimer _cooldown;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            GetComponent<Collider>().isTrigger = true;

            _cooldown = TimeManager.Instance.AddTimer();

            SetWaypoint(_initialWaypoint);
        }

        protected virtual void OnDestroy()
        {
            if(TimeManager.HasInstance) {
                TimeManager.Instance.RemoveTimer(_cooldown);
                _cooldown = null;
            }
        }

        protected virtual void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            if(null == _nextWaypoint || _cooldown.IsRunning) {
                return;
            }

            if(PartyParrotManager.Instance.IsPaused || null == GameStateManager.Instance.GameManager || !GameStateManager.Instance.GameManager.IsGameReady || GameStateManager.Instance.GameManager.IsGameOver) {
                return;
            }

            transform.MoveTowards(_nextWaypoint.transform.position, _speed * dt);

            if(Vector3.Distance(transform.position, _nextWaypoint.transform.position) < float.Epsilon) {
                _cooldown.Start(_nextWaypoint.Cooldown);

                transform.position = _nextWaypoint.transform.position;
                SetWaypoint(_nextWaypoint.NextPlatformWaypoint);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            OnEnterPlatform(other.gameObject);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            OnExitPlatform(other.gameObject);
        }

        #endregion

        private void SetWaypoint(PlatformWaypoint waypoint)
        {
            _nextWaypoint = waypoint;

            if(null == _nextWaypoint) {
                Debug.Log("No next waypoint, stopping");
                return;
            }
        }

        protected abstract void OnEnterPlatform(GameObject gameObject);

        protected abstract void OnExitPlatform(GameObject gameObject);
    }
}

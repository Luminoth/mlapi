using System;

using pdxpartyparrot.Core;
using pdxpartyparrot.Core.Time;
using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Core.World;
using pdxpartyparrot.Game.State;

using UnityEngine;

namespace pdxpartyparrot.Game.World
{
    // TODO: this hasn't been fully tested
    // TODO: this needs to also support moving actors through their Movement component
    // TODO: this needs to also support moving via rigidbody rather than transform
    public class WaypointFollower : MonoBehaviour
    {
        #region Events

        public event EventHandler<EventArgs> RouteBeginEvent;

        public event EventHandler<EventArgs> RouteCompleteEvent;

        public event EventHandler<EventArgs> DepartureEvent;

        public event EventHandler<EventArgs> ArrivalEvent;

        #endregion

        [SerializeField]
        private Waypoint _initialWaypoint;

        [SerializeField]
        private float _speed = 5.0f;

        [SerializeField]
        [ReadOnly]
        private Waypoint _nextWaypoint;

        [SerializeReference]
        [ReadOnly]
        private ITimer _cooldown;

        private Transform _transform;

        protected bool CanMove => !PartyParrotManager.Instance.IsPaused
            && null != GameStateManager.Instance.GameManager && GameStateManager.Instance.GameManager.IsGameReady && !GameStateManager.Instance.GameManager.IsGameOver
            && null != _nextWaypoint && !_cooldown.IsRunning;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _transform = transform;

            _cooldown = TimeManager.Instance.AddTimer();
        }

        protected virtual void Start()
        {
            RouteBeginEvent?.Invoke(this, EventArgs.Empty);

            SetWaypoint(_initialWaypoint);
        }

        protected virtual void OnDestroy()
        {
            if(TimeManager.HasInstance) {
                TimeManager.Instance.RemoveTimer(_cooldown);
            }
            _cooldown = null;
        }

        protected virtual void FixedUpdate()
        {
            if(!CanMove) {
                return;
            }

            float dt = Time.fixedDeltaTime;

            _transform.MoveTowards(_nextWaypoint.Transform.position, _speed * dt);

            if(Vector3.Distance(_transform.position, _nextWaypoint.Transform.position) < float.Epsilon) {
                if(_nextWaypoint.HasCooldown) {
                    _cooldown.Start(_nextWaypoint.Cooldown);
                }

                _transform.position = _nextWaypoint.Transform.position;

                ArrivalEvent?.Invoke(this, EventArgs.Empty);

                SetWaypoint(_nextWaypoint.NextWaypoint);
            }
        }

        #endregion

        private void SetWaypoint(Waypoint waypoint)
        {
            _nextWaypoint = waypoint;

            if(null == _nextWaypoint) {
                RouteCompleteEvent?.Invoke(this, EventArgs.Empty);
            } else {
                // TODO: we actually only wnat to send this
                // when our cooldown (if we have one) is up
                DepartureEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

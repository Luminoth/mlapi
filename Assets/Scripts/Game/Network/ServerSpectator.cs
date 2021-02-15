#if ENABLE_SERVER_SPECTATOR
#pragma warning disable 0618    // disable obsolete warning for now

using JetBrains.Annotations;

using pdxpartyparrot.Core;
using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Core.Util;

using pdxpartyparrot.Game.Camera;

using UnityEngine;
using UnityEngine.InputSystem;

#if USE_NETWORKING
using UnityEngine.Networking;
#elif USE_MLAPI
using MLAPI;
#endif

// TODO: fix input

namespace pdxpartyparrot.Game.Network
{
#if USE_NETWORKING
    [RequireComponent(typeof(NetworkIdentity))]
#elif USE_MLAPI
    [RequireComponent(typeof(NetworkedObject))]
#endif
    public sealed class ServerSpectator : MonoBehaviour
    {
        private const string InvertLookYKey = "serverspectator.invertlooky";

        private bool InvertLookY
        {
            get => PartyParrotManager.Instance.GetBool(InvertLookYKey);
            set => PartyParrotManager.Instance.SetBool(InvertLookYKey, value);
        }

        [SerializeField]
        private float _mouseSensitivity = 0.5f;

        [SerializeField]
        [ReadOnly]
        private Vector3 _lastControllerMove;

        [SerializeField]
        [ReadOnly]
        private Vector3 _lastControllerLook;

        [CanBeNull]
        private ServerSpectatorViewer _viewer;

        #region Unity Lifecycle

        private void Awake()
        {
            InitViewer();
        }

        private void OnDestroy()
        {
            if(ViewerManager.HasInstance) {
                ViewerManager.Instance.ReleaseViewer(_viewer);
            }
            _viewer = null;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;

            Quaternion rotation = null != _viewer ? Quaternion.AngleAxis(_viewer.transform.localEulerAngles.y, Vector3.up) : transform.rotation;
            transform.position = Vector3.Lerp(transform.position, transform.position + (rotation * _lastControllerMove), dt * 20.0f);
        }

        #endregion

        private void InitViewer()
        {
            _viewer = ViewerManager.Instance.AcquireViewer<ServerSpectatorViewer>();
            if(null == _viewer) {
                Debug.LogWarning("Unable to acquire server spectator viewer!");
                return;
            }
            _viewer.Initialize(this);
        }

        private bool IsInputAllowed(InputAction.CallbackContext ctx)
        {
            // no input unless we have focus
            if(!Application.isFocused) {
                return false;
            }

            // TODO: this probably doesn't handle multiple keyboards/mice
            return true;
        }

        #region IServerSpectatorActions

        public void OnMoveForward(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(_lastControllerMove.x, _lastControllerMove.y, context.performed ? 1.0f : 0.0f);
        }

        public void OnMoveBackward(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(_lastControllerMove.x, _lastControllerMove.y, context.performed ? -1.0f : 0.0f);
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(context.performed ? -1.0f : 0.0f, _lastControllerMove.y, _lastControllerMove.z);
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(context.performed ? 1.0f : 0.0f, _lastControllerMove.y, _lastControllerMove.z);
        }

        public void OnMoveUp(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(_lastControllerMove.x, context.performed ? 1.0f : 0.0f, _lastControllerMove.z);
        }

        public void OnMoveDown(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            _lastControllerMove = new Vector3(_lastControllerMove.x, context.performed ? -1.0f : 0.0f, _lastControllerMove.z);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            if(context.canceled) {
                _lastControllerLook = Vector3.zero;

                //FollowTarget.LastLookAxes = _lastControllerLook;
            } else {
                Vector2 axes = context.ReadValue<Vector2>();
                axes.y *= InvertLookY ? -1 : 1;

                bool isMouse = context.control.device is Mouse;
                if(isMouse) {
                    axes *= _mouseSensitivity;
                }

                _lastControllerLook = new Vector3(axes.x, axes.y, 0.0f);
            }
        }

        #endregion
    }
}
#endif

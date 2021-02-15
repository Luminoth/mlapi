#if ENABLE_SERVER_SPECTATOR
#pragma warning disable 0618    // disable obsolete warning for now

using JetBrains.Annotations;

using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game.Camera;

using UnityEngine;

#if USE_NETWORKING
using UnityEngine.Networking;
#elif USE_MLAPI
using MLAPI;
#endif

// TODO: move to Core.Network
// TODO: implement look behavior
namespace pdxpartyparrot.Game.Network
{
#if USE_NETWORKING
    [RequireComponent(typeof(NetworkIdentity))]
#elif USE_MLAPI
    [RequireComponent(typeof(NetworkedObject))]
#endif
    public sealed class ServerSpectator : MonoBehaviour
    {
        [SerializeField]
        private ServerSpectatorInputHandler _inputHandler;

        public ServerSpectatorInputHandler ServerSpectatorInputHandler => _inputHandler;

        [CanBeNull]
        private ServerSpectatorViewer _viewer;

        [SerializeField]
        private float _speed = 20.0f;

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
            transform.position = Vector3.Lerp(transform.position, transform.position + (rotation * _inputHandler.LastMove), dt * _speed);
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
    }
}
#endif

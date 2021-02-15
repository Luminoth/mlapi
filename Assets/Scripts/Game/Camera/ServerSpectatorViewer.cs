#if ENABLE_SERVER_SPECTATOR
using Cinemachine;

using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game.Network;

using UnityEngine.Assertions;

namespace pdxpartyparrot.Game.Camera
{
    //[RequireComponent(typeof(Cinemachine3rdPersonFollow))]
    public sealed class ServerSpectatorViewer : CinemachineViewer
    {
        private Cinemachine3rdPersonFollow _follow;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            _follow = GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            Assert.IsNotNull(_follow, "Set Body to 3rd Person Follow");
        }

        #endregion

        public void Initialize(ServerSpectator owner)
        {
            LookAt(owner.transform);
            Follow(owner.transform);
        }
    }
}
#endif

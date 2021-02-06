using Cinemachine;

using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game.Data;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Game.Camera
{
    [RequireComponent(typeof(Cinemachine3rdPersonFollow))]
    public class FollowCamera3D : CinemachineViewer, IPlayerViewer
    {
        public Viewer Viewer => this;

        private Cinemachine3rdPersonFollow _follow;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            _follow = GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            Assert.IsNotNull(_follow);
        }

        #endregion

        public virtual void Initialize(GameData gameData)
        {
            Viewer.Set3D(gameData.FoV);
        }

        public void FollowTarget(GameObject target)
        {
            LookAt(target.transform);
            Follow(target.transform);
        }
    }
}

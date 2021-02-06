using Cinemachine;

using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game.Data;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Game.Camera
{
    [RequireComponent(typeof(CinemachineFramingTransposer))]
    public class FollowCamera25D : CinemachineViewer, IPlayerViewer
    {
        public Viewer Viewer => this;

        private CinemachineFramingTransposer _transposer;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            _transposer = GetCinemachineComponent<CinemachineFramingTransposer>();
            Assert.IsNotNull(_transposer);
        }

        #endregion

        public virtual void Initialize(GameData gameData)
        {
            Viewer.Set3D(gameData.FoV);
        }

        public void FollowTarget(GameObject target)
        {
            // LookAt must be empty
            Follow(target.transform);
        }
    }
}

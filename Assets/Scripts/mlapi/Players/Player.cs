using System;

using pdxpartyparrot.Core.World;
using pdxpartyparrot.Game.Characters.Players;
using pdxpartyparrot.mlapi.Camera;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.mlapi.Players
{
    public sealed class Player : Player3D
    {
        public PlayerBehavior GamePlayerBehavior => (PlayerBehavior)PlayerBehavior;

        private GameViewer PlayerGameViewer => (GameViewer)Viewer;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            Assert.IsTrue(PlayerInputHandler is PlayerInputHandler);

            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        #endregion

        public override void Initialize(Guid id)
        {
            base.Initialize(id);

            Assert.IsTrue(PlayerBehavior is PlayerBehavior);
        }

        protected override bool InitializeLocalPlayer(Guid id)
        {
            if(!base.InitializeLocalPlayer(id)) {
                return false;
            }

            PlayerViewer = GameManager.Instance.Viewer;

            return true;
        }

        #region Spawn

        public override bool OnSpawn(SpawnPoint spawnpoint)
        {
            if(!base.OnSpawn(spawnpoint)) {
                return false;
            }

            PlayerGameViewer.FollowTarget(gameObject);

            return true;
        }

        public override bool OnReSpawn(SpawnPoint spawnpoint)
        {
            if(!base.OnReSpawn(spawnpoint)) {
                return false;
            }

            PlayerGameViewer.FollowTarget(gameObject);

            return true;
        }

        public override void OnDeSpawn()
        {
            PlayerGameViewer.FollowTarget(null);

            base.OnDeSpawn();
        }

        #endregion
    }
}

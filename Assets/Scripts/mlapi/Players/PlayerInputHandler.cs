using pdxpartyparrot.Game.Players.Input;
using pdxpartyparrot.mlapi.Data.Players;

using UnityEngine.Assertions;

namespace pdxpartyparrot.mlapi.Players
{
    public sealed class PlayerInputHandler : ThirdPersonPlayerInputHandler
    {
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            Assert.IsTrue(PlayerInputData is PlayerInputData);
            Assert.IsTrue(Player is Player);
        }

        #endregion
    }
}

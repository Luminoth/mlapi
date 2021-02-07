using pdxpartyparrot.Core.Data.Actors.Components;
using pdxpartyparrot.mlapi.Data.Players;

using UnityEngine.Assertions;

namespace pdxpartyparrot.mlapi.Players
{
    public sealed class PlayerBehavior : Game.Characters.Players.PlayerBehavior
    {
        public PlayerBehaviorData GamePlayerBehaviorData => (PlayerBehaviorData)PlayerBehaviorData;

        public override void Initialize(ActorBehaviorComponentData behaviorData)
        {
            Assert.IsTrue(Owner is Player);
            Assert.IsTrue(behaviorData is PlayerBehaviorData);

            base.Initialize(behaviorData);
        }
    }
}

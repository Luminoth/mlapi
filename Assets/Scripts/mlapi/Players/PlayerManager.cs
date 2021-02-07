using pdxpartyparrot.Game.Players;
using pdxpartyparrot.mlapi.Data.Players;

namespace pdxpartyparrot.mlapi.Players
{
    public sealed class PlayerManager : PlayerManager<PlayerManager>
    {
        public PlayerData GamePlayerData => (PlayerData)PlayerData;
    }
}

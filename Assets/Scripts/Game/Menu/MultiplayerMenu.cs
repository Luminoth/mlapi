#if USE_NETWORKING || USE_MLAPI
using pdxpartyparrot.Game.State;

namespace pdxpartyparrot.Game.Menu
{
    public sealed class MultiplayerMenu : MenuPanel
    {
        #region Event Handlers

        public void OnHost()
        {
            // TODO: what if we want a different main game state?
            GameStateManager.Instance.StartHost(GameStateManager.Instance.GameManager.GameData.MainGameStatePrefab);
        }

        public void OnJoin()
        {
            // TODO: what if we want a different main game state?
            GameStateManager.Instance.StartJoin(GameStateManager.Instance.GameManager.GameData.MainGameStatePrefab);
        }

        #endregion
    }
}
#endif

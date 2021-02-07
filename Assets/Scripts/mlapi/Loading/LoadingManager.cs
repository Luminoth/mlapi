using pdxpartyparrot.Game.Loading;
using pdxpartyparrot.mlapi.Players;
using pdxpartyparrot.mlapi.UI;

using UnityEngine;

namespace pdxpartyparrot.mlapi.Loading
{
    public sealed class LoadingManager : LoadingManager<LoadingManager>
    {
        [Space(10)]

        #region Manager Prefabs

        [Header("Project Manager Prefabs")]

        [SerializeField]
        private GameManager _gameManagerPrefab;

        [SerializeField]
        private GameUIManager _gameUiManagerPrefab;

        [SerializeField]
        private PlayerManager _playerManager;

        #endregion

        protected override void CreateManagers()
        {
            base.CreateManagers();

            GameManager.CreateFromPrefab(_gameManagerPrefab, ManagersContainer);
            GameUIManager.CreateFromPrefab(_gameUiManagerPrefab, ManagersContainer);
            PlayerManager.CreateFromPrefab(_playerManager, ManagersContainer);
        }
    }
}

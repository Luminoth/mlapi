using JetBrains.Annotations;

using pdxpartyparrot.Core.DebugMenu;
using pdxpartyparrot.Core.UI;
using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Game.State;

using UnityEngine;

namespace pdxpartyparrot.Game.Menu
{
    public abstract class MainMenu : MenuPanel
    {
#if USE_NETWORKING || USE_MLAPI
        #region Multiplayer

        [SerializeField]
        [CanBeNull]
        private MultiplayerMenu _multiplayerMenu;

        #endregion
#endif

        #region High Scores

        [SerializeField]
        [CanBeNull]
        private HighScoresMenu _highScoresPanel;

        #endregion

        #region Credits

        [SerializeField]
        private CreditsMenu _creditsPanel;

        #endregion

        #region Character Select

        [SerializeField]
        [CanBeNull]
        private CharacterSelectMenu _characterSelectPanel;

        #endregion

        private DebugMenuNode _debugMenuNode;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

#if USE_NETWORKING || USE_MLAPI
            if(null != _multiplayerMenu) {
                _multiplayerMenu.gameObject.SetActive(false);
            }
#endif

            if(null != _highScoresPanel) {
                _highScoresPanel.gameObject.SetActive(false);
            }

            if(null != _creditsPanel) {
                _creditsPanel.gameObject.SetActive(false);
            }

            if(null != _characterSelectPanel) {
                _characterSelectPanel.gameObject.SetActive(false);
            }

            InitDebugMenu();
        }

        protected override void OnDestroy()
        {
            DestroyDebugMenu();

            base.OnDestroy();
        }

        #endregion

        public void ShowHighScores()
        {
            OnHighScores();
        }

        #region Event Handlers

        public virtual void OnStart()
        {
            if(null != _characterSelectPanel) {
                Owner.PushPanel(_characterSelectPanel);
            } else {
                Debug.Log("TODO: override MainMenu and handle OnStart()!");
            }
        }

#if USE_NETWORKING || USE_MLAPI
        public void OnMultiplayer()
        {
            Owner.PushPanel(_multiplayerMenu);
        }
#endif

        public void OnHighScores()
        {
            Owner.PushPanel(_highScoresPanel);
        }

        public void OnCredits()
        {
            Owner.PushPanel(_creditsPanel);
        }

        public void OnQuitGame()
        {
            UnityUtil.Quit();
        }

        #endregion

        private void InitDebugMenu()
        {
            _debugMenuNode = DebugMenuManager.Instance.AddNode(() => "Menu");
            _debugMenuNode.RenderContentsAction = () => {
                if(GUIUtils.LayoutButton("Local")) {
                    GameStateManager.Instance.StartLocal(GameStateManager.Instance.GameManager.GameData.MainGameStatePrefab);
                    return;
                }

#if USE_NETWORKING || USE_MLAPI
                GUILayout.BeginVertical("Multiplayer", GUI.skin.box);
                // TODO: these take in the main game state now
                if(GUIUtils.LayoutButton("Host")) {
                    GameStateManager.Instance.StartHost(GameStateManager.Instance.GameManager.GameData.MainGameStatePrefab);
                    return;
                }

                if(GUIUtils.LayoutButton("Join")) {
                    GameStateManager.Instance.StartJoin(GameStateManager.Instance.GameManager.GameData.MainGameStatePrefab);
                    return;
                }
                GUILayout.EndVertical();
#endif
            };
        }

        private void DestroyDebugMenu()
        {
            if(DebugMenuManager.HasInstance) {
                DebugMenuManager.Instance.RemoveNode(_debugMenuNode);
            }
            _debugMenuNode = null;
        }
    }
}

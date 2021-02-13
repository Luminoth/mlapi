using JetBrains.Annotations;

using pdxpartyparrot.Core;
using pdxpartyparrot.Core.Audio;
using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Game.State;

using UnityEngine;

namespace pdxpartyparrot.Game.Menu
{
    public sealed class PauseMenu : MenuPanel
    {
        #region Settings

        [SerializeField]
        [CanBeNull]
        private SettingsMenu _settingsMenu;

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            if(!HasInitialSelection) {
                Debug.LogWarning("Pause menu missing initial selection");
            }

            if(null != _settingsMenu) {
                _settingsMenu.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Event Handlers

        public void OnSettings()
        {
            Owner.PushPanel(_settingsMenu);
        }

        public void OnResume()
        {
            PartyParrotManager.Instance.TogglePause();
        }

        public override void OnBack()
        {
            // TODO: this auto-fires if escape is used both for pausing and canceling
            // so for now disable it and just rely on OnResume()
            //PartyParrotManager.Instance.TogglePause();
        }

        public void OnExitMainMenu()
        {
            // stop all audio so when it unducks it doesn't blast all weird
            AudioManager.Instance.StopAllAudio();

            GameStateManager.Instance.TransitionToInitialStateAsync();

            PartyParrotManager.Instance.TogglePause();
        }

        public void OnQuitGame()
        {
            UnityUtil.Quit();
        }

        #endregion
    }
}

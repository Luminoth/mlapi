using UnityEngine;

namespace pdxpartyparrot.Game.State
{
    // TODO: this needs to be abstract because we need to deal with viewer configuration
    public abstract class SceneTester : MainGameState
    {
        [SerializeField]
        private string[] _testScenes;

        public string[] TestScenes => _testScenes;

        protected override bool InitializeServer()
        {
            if(!base.InitializeServer()) {
                Debug.LogWarning("Failed to initialize server!");
                return false;
            }

            GameStateManager.Instance.GameManager.StartGameServer();

            return true;
        }

        protected override bool InitializeClient()
        {
            // need to init the viewer(s) before we start spawning players
            // so that they have a viewer to attach to
            InitViewers();

            if(!base.InitializeClient()) {
                Debug.LogWarning("Failed to initialize client!");
                return false;
            }

            GameStateManager.Instance.GameManager.StartGameClient();

            return true;
        }

        public abstract void InitViewers();

        public void SetScene(string sceneName)
        {
            CurrentSceneName = sceneName;
        }
    }
}

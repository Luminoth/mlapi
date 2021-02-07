using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game;
using pdxpartyparrot.mlapi.Camera;
using pdxpartyparrot.mlapi.Data;

using UnityEngine;

namespace pdxpartyparrot.mlapi
{
    public sealed class GameManager : GameManager<GameManager>
    {
        public GameViewer Viewer { get; private set; }

        public GameData GameGameData => (GameData)GameData;

        public void InitViewer()
        {
            Viewer = ViewerManager.Instance.AcquireViewer<GameViewer>();
            if(null == Viewer) {
                Debug.LogWarning("Unable to acquire game viewer!");
                return;
            }
            Viewer.Initialize(GameGameData);
        }
    }
}

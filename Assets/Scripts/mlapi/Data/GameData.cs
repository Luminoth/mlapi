using System;

using pdxpartyparrot.mlapi.Camera;

using UnityEngine;

namespace pdxpartyparrot.mlapi.Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "pdxpartyparrot/mlapi/Data/Game Data")]
    [Serializable]
    public sealed class GameData : Game.Data.GameData
    {
        public GameViewer GameViewerPrefab => (GameViewer)ViewerPrefab;
    }
}

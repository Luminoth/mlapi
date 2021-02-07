#pragma warning disable 0618    // disable obsolete warning for now

using System;

using JetBrains.Annotations;

using pdxpartyparrot.Core.Network;
using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Game.UI;

using UnityEngine;

#if USE_NETWORKING
using UnityEngine.Networking;
#endif

namespace pdxpartyparrot.Game.State
{
    public sealed class NetworkConnectState : SubGameState
    {
        public enum ConnectType
        {
            Local,
#if USE_NETWORKING || USE_MLAPI
            Server,
            Client
#endif
        }

#if USE_NETWORKING || USE_MLAPI
        [SerializeField]
        [CanBeNull]
        private NetworkConnectUI _networkConnectUIPrefab;

        [CanBeNull]
        private NetworkConnectUI _networkConnectUI;
#endif

        [SerializeField]
        [ReadOnly]
        private GameState _gameStatePrefab;

        private Action<GameState> _gameStateInit;

        [SerializeField]
        [ReadOnly]
        private ConnectType _connectType = ConnectType.Local;

        public void Initialize(ConnectType connectType, GameState gameStatePrefab, Action<GameState> gameStateInit = null)
        {
            _connectType = connectType;
            _gameStatePrefab = gameStatePrefab;
            _gameStateInit = gameStateInit;
        }

        public void Cancel()
        {
#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.DiscoverStop();
#endif

#if USE_NETWORKING || USE_MLAPI
            switch(_connectType) {
            case ConnectType.Local:
                Core.Network.NetworkManager.Instance.StopHost();
                break;
            case ConnectType.Server:
                Core.Network.NetworkManager.Instance.StopServer();
                break;
            case ConnectType.Client:
                Core.Network.NetworkManager.Instance.StopClient();
                break;
            }
#endif

            GameStateManager.Instance.TransitionToInitialStateAsync();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            // TODO: OnEnter is called after DoEnter() so the UI
            // isn't updating the status correctly (it's disabled)

#if USE_NETWORKING || USE_MLAPI
            if(null != GameStateManager.Instance.GameUIManager) {
                _networkConnectUI = GameStateManager.Instance.GameUIManager.InstantiateUIPrefab(_networkConnectUIPrefab);
                if(null != _networkConnectUI) {
                    _networkConnectUI.Initialize(this);
                }
            } else {
                Debug.LogWarning("GameUIManager missing!");
            }
#endif
        }

        protected override void DoEnter()
        {
            base.DoEnter();

            switch(_connectType) {
            case ConnectType.Local:
                StartSinglePlayer();
                break;
#if USE_NETWORKING || USE_MLAPI
            case ConnectType.Server:
                StartServer();
                break;
            case ConnectType.Client:
                StartClient();
                break;
#endif
            }
        }

        protected override void DoExit()
        {
#if USE_NETWORKING || USE_MLAPI
            if(null != _networkConnectUI) {
                Destroy(_networkConnectUI.gameObject);
            }
            _networkConnectUI = null;

            if(Core.Network.NetworkManager.HasInstance) {
#if USE_NETWORKING
                Core.Network.NetworkManager.Instance.Discovery.ReceivedBroadcastEvent -= ReceivedBroadcastEventHandler;
                Core.Network.NetworkManager.Instance.DiscoverStop();
#endif

                Core.Network.NetworkManager.Instance.ServerConnectEvent -= ServerConnectEventHandler;
                Core.Network.NetworkManager.Instance.ClientConnectEvent -= ClientConnectEventHandler;
#if USE_NETWORKING
                Core.Network.NetworkManager.Instance.ClientSceneChangedEvent -= ClientSceneChangedEventHandler;
#endif
            }
#endif

            base.DoExit();
        }

        private void StartSinglePlayer()
        {
#if USE_NETWORKING || USE_MLAPI
            Core.Network.NetworkManager.Instance.ServerConnectEvent += ServerConnectEventHandler;

#if USE_MLAPI
            Core.Network.NetworkManager.Instance.StartHost();
#else
            GameStateManager.Instance.NetworkClient = Core.Network.NetworkManager.Instance.StartHost();
#endif
            if(!Core.Network.NetworkManager.Instance.IsClientActive()) {
                SetStatus("Unable to start network host!");
            }
#else

            ServerConnectEventHandler(this, null);
#endif
        }

#if USE_NETWORKING || USE_MLAPI
        private void StartServer()
        {
            Core.Network.NetworkManager.Instance.ServerConnectEvent += ServerConnectEventHandler;

#if USE_MLAPI
            Core.Network.NetworkManager.Instance.StartServer();
#else
            if(!Core.Network.NetworkManager.Instance.StartServer()) {
                SetStatus("Unable to start network server!");
                return;
            }
#endif

#if USE_NETWORKING
            if(!Core.Network.NetworkManager.Instance.DiscoverServer()) {
                SetStatus("Unable to start network server discovery!");
                return;
            }
#endif

            SetStatus("Waiting for players...");
        }

        private void StartClient()
        {
#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.Discovery.ReceivedBroadcastEvent += ReceivedBroadcastEventHandler;

            if(!Core.Network.NetworkManager.Instance.DiscoverClient()) {
                SetStatus("Unable to start network client discovery!");
                return;
            }
#endif

            SetStatus("Searching for server...");
        }
#endif

        private void SetStatus(string status)
        {
            Debug.Log(status);

#if USE_NETWORKING || USE_MLAPI
            if(null != _networkConnectUI) {
                _networkConnectUI.SetStatus(status);
            }
#endif
        }

        #region Event Handlers

        private void ServerConnectEventHandler(object sender, EventArgs args)
        {
#if USE_NETWORKING || USE_MLAPI
#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.DiscoverStop();
#endif

            SetStatus("Client connected, transitioning to game state...");
#else
            SetStatus("Transitioning to game state...");
#endif

            Core.Network.NetworkManager.Instance.ServerChangeScene(_gameStatePrefab.CurrentSceneName);

            GameStateManager.Instance.TransitionStateAsync(_gameStatePrefab, _gameStateInit);
        }

#if USE_NETWORKING || USE_MLAPI
        private void ClientConnectEventHandler(object sender, EventArgs args)
        {
#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.DiscoverStop();
#endif

#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.ClientSceneChangedEvent += ClientSceneChangedEventHandler;
#endif

            SetStatus("Connected, waiting for server...");
        }

        private void ClientSceneChangedEventHandler(object sender, ClientSceneEventArgs args)
        {
            SetStatus("Server ready, transitioning to game state...");

            GameStateManager.Instance.TransitionStateAsync(_gameStatePrefab, _gameStateInit);
        }

        private void ReceivedBroadcastEventHandler(object sender, ReceivedBroadcastEventArgs args)
        {
#if USE_NETWORKING
            Core.Network.NetworkManager.Instance.Discovery.ReceivedBroadcastEvent -= ReceivedBroadcastEventHandler;
            Core.Network.NetworkManager.Instance.DiscoverStop();
#endif

            SetStatus($"Found server at {args.EndPoint}, connecting...");
            Core.Network.NetworkManager.Instance.SetClientConnection(args.EndPoint.Address.ToString(), args.EndPoint.Port);
            Core.Network.NetworkManager.Instance.ClientConnectEvent += ClientConnectEventHandler;

#if USE_MLAPI
            Core.Network.NetworkManager.Instance.StartClient();
#else
            GameStateManager.Instance.NetworkClient = Core.Network.NetworkManager.Instance.StartClient();
            if(null == GameStateManager.Instance.NetworkClient) {
                SetStatus("Unable to start network client!");
                return;
            }
#endif
        }
#endif

        #endregion
    }
}

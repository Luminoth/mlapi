#pragma warning disable 0618    // disable obsolete warning for now

using System;

using JetBrains.Annotations;

using pdxpartyparrot.Core.DebugMenu;
using pdxpartyparrot.Core.Util;

using UnityEngine;

#if USE_NETWORKING
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
#elif USE_MLAPI
using MLAPI;
using MLAPI.Configuration;
using MLAPI.Transports.Tasks;
using MLAPI.Transports.UNET;
#endif

namespace pdxpartyparrot.Core.Network
{
#if USE_NETWORKING
    // TODO: HLAPI is deprecated so this needs to be replaced
    // https://bitbucket.org/Unity-Technologies/networking
    [RequireComponent(typeof(NetworkManagerHUD))]
    [RequireComponent(typeof(NetworkDiscovery))]
    public sealed class NetworkManager : UnityEngine.Networking.NetworkManager
#elif USE_MLAPI
    public sealed class NetworkManager : NetworkingManager
#else
    public sealed class NetworkManager : SingletonBehavior<NetworkManager>
#endif
    {
        #region Events

#if USE_NETWORKING
        public event EventHandler<EventArgs> ServerStartEvent;
        public event EventHandler<EventArgs> ServerStopEvent;
        public event EventHandler<EventArgs> ServerConnectEvent;
        public event EventHandler<EventArgs> ServerDisconnectEvent;
#elif USE_MLAPI
        public event EventHandler<EventArgs> ServerStartEvent;
        public event EventHandler<EventArgs> ServerConnectEvent;
#endif

        public event EventHandler<EventArgs> ServerChangeSceneEvent;
        public event EventHandler<EventArgs> ServerChangedSceneEvent;
        public event EventHandler<ServerAddPlayerEventArgs> ServerAddPlayerEvent;

#if USE_NETWORKING || USE_MLAPI
        public event EventHandler<EventArgs> ClientConnectEvent;
        public event EventHandler<EventArgs> ClientDisconnectEvent;
        public event EventHandler<ClientSceneEventArgs> ClientSceneChangeEvent;
        public event EventHandler<ClientSceneEventArgs> ClientSceneChangedEvent;
#endif

        #endregion

        #region Messages

#if USE_NETWORKING
        public class CustomMsgType
        {
            public const short SceneChange = MsgType.Highest + 1;
            public const short SceneChanged = MsgType.Highest + 2;

            // NOTE: always last, always highest
            public const short Highest = MsgType.Highest + 3;
        }
#endif

        #endregion

        #region Singleton

#if USE_NETWORKING
        public static NetworkManager Instance => (NetworkManager)singleton;

        public static bool HasInstance => null != Instance;
#elif USE_MLAPI
        public static NetworkManager Instance => (NetworkManager)Singleton;

        public static bool HasInstance => null != Instance;
#endif

        #endregion

        [SerializeField]
        private bool _enableCallbackLogging = true;

#if USE_NETWORKING
        private NetworkManagerHUD _hud;

        public NetworkDiscovery Discovery { get; private set; }
#else
        [SerializeField]
        [ReadOnly]
        [CanBeNull]
        private GameObject m_PlayerPrefab;

        [CanBeNull]
        public GameObject playerPrefab
        {
            get => m_PlayerPrefab;
            set => m_PlayerPrefab = value;
        }
#endif

        #region Unity Lifecycle

#if USE_NETWORKING
        // TODO: whenever this becomes a thing...
        /*protected override void Awake()
        {
            base.Awake();

            Initialize();
        }*/

        private void Start()
        {
            Initialize();
        }
#elif USE_MLAPI
        private void Awake()
        {
            Initialize();
        }
#else
        private void Start()
        {
            Initialize();
        }
#endif

        #endregion

        private void Initialize()
        {
#if USE_NETWORKING
            _hud = GetComponent<NetworkManagerHUD>();
            _hud.showGUI = false;

            Discovery = GetComponent<NetworkDiscovery>();
            Discovery.useNetworkManager = true;
            Discovery.showGUI = false;
            Discovery.enabled = PartyParrotManager.Instance.Config.Network.Discovery.Enable;

            autoCreatePlayer = false;
#elif USE_MLAPI
            NetworkConfig.ConnectionApproval = true;
            NetworkConfig.CreatePlayerPrefab = false;
            NetworkConfig.ForceSamePrefabs = true;
            NetworkConfig.UsePrefabSync = false;
            NetworkConfig.EnableMessageBuffering = true;

            // if MLAPI ever supports additive scene loading,
            // we could look into supporting this
            NetworkConfig.EnableSceneManagement = false;
            NetworkConfig.AllowRuntimeSceneChanges = true;

            ConnectionApprovalCallback += ApprovalCheckEventHandler;
            OnServerStarted += ServerStartedEventHandler;
            OnClientConnectedCallback += ClientConnectedEventHandler;
            OnClientDisconnectCallback += ClientDisconnectEventHandler;
#endif

            InitDebugMenu();
        }

        public void SetClientConnection(string address, int port)
        {
#if USE_NETWORKING
            networkAddress = address;
            networkPort = port;
#elif USE_MLAPI
            // TODO: don't assume UnetTransport
            UnetTransport transport = (UnetTransport)NetworkConfig.NetworkTransport;
            transport.ConnectAddress = address;
            transport.ConnectPort = port;
#endif
        }

        public bool IsServerActive()
        {
#if USE_NETWORKING
            return NetworkServer.active;
#elif USE_MLAPI
            // TODO: not sure if this has the same meaning?
            return IsServer || IsHost;
#else
            return true;
#endif
        }

        public bool IsClientActive()
        {
#if USE_NETWORKING
            return NetworkClient.active;
#elif USE_MLAPI
            // TODO: not sure if this has the same meaning?
            return IsClient || IsHost;
#else
            return true;
#endif
        }

        #region Network Prefabs

#if USE_MLAPI
        public void RegisterNetworkPrefab<T>(T networkPrefab, bool isPlayer = false) where T : NetworkedBehaviour
#else
        public void RegisterNetworkPrefab<T>(T networkPrefab, bool isPlayer = false) where T : NetworkBehaviour
#endif
        {
            Debug.Log($"[NetworkManager]: Registering network prefab '{networkPrefab.name}'");
#if USE_NETWORKING
            ClientScene.RegisterPrefab(networkPrefab.gameObject);
#elif USE_MLAPI
            NetworkConfig.NetworkedPrefabs.Add(new NetworkedPrefab {
                Prefab = networkPrefab.gameObject,
                PlayerPrefab = isPlayer,
            });
#else
            Debug.LogWarning($"[NetworkManager]: Not registering network prefab {networkPrefab.name}");
#endif
        }

#if USE_MLAPI
        public void UnregisterNetworkPrefab<T>(T networkPrefab) where T : NetworkedBehaviour
#else
        public void UnregisterNetworkPrefab<T>(T networkPrefab) where T : NetworkBehaviour
#endif
        {
            Debug.Log($"[NetworkManager]: Unregistering network prefab '{networkPrefab.name}'");
#if USE_NETWORKING
            ClientScene.UnregisterPrefab(networkPrefab.gameObject);
#elif USE_MLAPI
            NetworkConfig.NetworkedPrefabs.RemoveAll(x => x.Prefab == networkPrefab);
#else
            Debug.LogWarning($"[NetworkManager]: Not unregistering network prefab {networkPrefab.name}");
#endif
        }

        [CanBeNull]
#if USE_MLAPI
        public T SpawnNetworkPrefab<T>(T networkPrefab) where T : NetworkedBehaviour
#else
        public T SpawnNetworkPrefab<T>(T networkPrefab) where T : NetworkBehaviour
#endif
        {
            if(!IsServerActive()) {
                Debug.LogWarning("[NetworkManager]: Cannot spawn network prefab without an active server!");
                return null;
            }

            Debug.Log($"[NetworkManager]: Spawning network prefab '{networkPrefab.name}'");

            T obj = Instantiate(networkPrefab);
            if(null == obj) {
                return null;
            }

            SpawnNetworkObject(obj);
            return obj;
        }

        [CanBeNull]
#if USE_MLAPI
        public T SpawnNetworkPrefab<T>(T networkPrefab, Transform parent) where T : NetworkedBehaviour
#else
        public T SpawnNetworkPrefab<T>(T networkPrefab, Transform parent) where T : NetworkBehaviour
#endif
        {
            T obj = SpawnNetworkPrefab(networkPrefab);
            if(null == obj) {
                return null;
            }
            obj.transform.SetParent(parent, true);
            return obj;
        }

#if USE_MLAPI
        public void SpawnNetworkObject<T>([NotNull] T networkObject) where T : NetworkedBehaviour

#else
        public void SpawnNetworkObject<T>([NotNull] T networkObject) where T : NetworkBehaviour
#endif
        {
#if USE_NETWORKING
            NetworkServer.Spawn(networkObject.gameObject);
#elif USE_MLAPI
            networkObject.NetworkedObject.Spawn();
#else
            Debug.LogWarning($"[NetworkManager]: Not spawning network object {networkObject.name}");
#endif
        }

#if USE_MLAPI
        public void DeSpawnNetworkObject<T>([NotNull] T networkObject) where T : NetworkedBehaviour
#else
        public void DeSpawnNetworkObject<T>([NotNull] T networkObject) where T : NetworkBehaviour
#endif
        {
#if USE_NETWORKING
            NetworkServer.UnSpawn(networkObject.gameObject);
#elif USE_MLAPI
            networkObject.NetworkedObject.UnSpawn();
#else
            Debug.LogWarning($"[NetworkManager]: Not despawning network object {networkObject.name}");
#endif
        }

#if USE_MLAPI
        public void DestroyNetworkObject<T>([CanBeNull] T networkObject) where T : NetworkedBehaviour

#else
        public void DestroyNetworkObject<T>([CanBeNull] T networkObject) where T : NetworkBehaviour
#endif
        {
            if(null == networkObject) {
                return;
            }

            if(!IsServerActive()) {
                Debug.LogWarning("[NetworkManager]: Cannot destroy network object without an active server!");
                return;
            }

#if USE_NETWORKING
            Debug.Log($"[NetworkManager]: Destroying network object '{networkObject.name}'");
            NetworkServer.Destroy(networkObject.gameObject);
#elif USE_MLAPI
            Debug.LogWarning($"TODO: Destroy network object {networkObject.name}");
#else
            Debug.LogWarning($"[NetworkManager]: Not destroying network object {networkObject.name}");
#endif
        }

        #endregion

        #region Player Prefab

        public void RegisterPlayerPrefab<T>(T prefab) where T : NetworkActor
        {
            Debug.Log($"[NetworkManager]: Registering player prefab '{prefab.name}'");

            // TODO: warn if already set?
            playerPrefab = prefab.gameObject;
#if USE_NETWORKING || USE_MLAPI
            RegisterNetworkPrefab(prefab, true);
#endif
        }

        public void UnregisterPlayerPrefab()
        {
            Debug.Log($"[NetworkManager]: Unregistering player prefab '{playerPrefab.name}'");

#if USE_NETWORKING
            UnregisterNetworkPrefab(playerPrefab.GetComponent<NetworkBehaviour>());
#elif USE_MLAPI
            UnregisterNetworkPrefab(playerPrefab.GetComponent<NetworkedBehaviour>());

#endif

            // TODO: warn if not set?
            playerPrefab = null;
        }

        public T SpawnPlayer<T>(int controllerId, NetworkConnection conn) where T : NetworkActor
        {
            if(!IsServerActive()) {
                Debug.LogWarning("[NetworkManager]: Cannot spawn player prefab without an active server!");
                return null;
            }

            if(null == playerPrefab) {
                Debug.LogWarning("[NetworkManager]: Player prefab not registered!");
                return null;
            }

            GameObject player = Instantiate(playerPrefab);
            if(null == player) {
                Debug.LogError("Failed to spawn player!");
                return null;
            }
            player.name = $"Player {controllerId}";

            // call this instead of NetworkServer.Spawn()
#if USE_NETWORKING
            NetworkServer.AddPlayerForConnection(conn, player, controllerId);
#elif USE_MLAPI
            NetworkedObject no = player.GetComponent<NetworkedObject>();
            no.SpawnAsPlayerObject(conn.ClientId);
#endif
            return player.GetComponent<T>();
        }

        public T SpawnPlayer<T>(int controllerId, NetworkConnection conn, Transform parent) where T : NetworkActor
        {
            T player = SpawnPlayer<T>(controllerId, conn);
            if(null == player) {
                return null;
            }
            player.transform.SetParent(parent, true);
            return player;
        }

        public void DestroyPlayer(NetworkConnection conn)
        {
            if(!IsServerActive()) {
                Debug.LogWarning("Cannot despawn players without an active server!");
                return;
            }

#if USE_NETWORKING
            NetworkServer.DestroyPlayersForConnection(conn);
#elif USE_MLAPI
            Debug.LogWarning($"TODO: destroy player (?)");
#endif
        }

        #endregion

        #region Discovery

#if USE_NETWORKING
        private bool InitDiscovery()
        {
            Discovery.broadcastPort = PartyParrotManager.Instance.Config.Network.Discovery.Port;
            return Discovery.Initialize();
        }

        public bool DiscoverServer()
        {
            if(!Discovery.enabled) {
                return true;
            }

            Debug.Log("[NetworkManager]: Starting server discovery");

            if(!InitDiscovery()) {
                return false;
            }

            return Discovery.StartAsServer();
        }

        public bool DiscoverClient()
        {
            if(!Discovery.enabled) {
                return true;
            }

            Debug.Log("[NetworkManager]: Starting client discovery");

            if(!InitDiscovery()) {
                return false;
            }

            return Discovery.StartAsClient();
        }

        public void DiscoverStop()
        {
            if(Discovery.running) {
                Debug.Log("[NetworkManager]: Stopping discovery");
                Discovery.StopBroadcast();
            }

            // TODO: see if this stops the "host id out of bound" error
            // on the client without breaking anything else
            /*if(NetworkTransport.IsBroadcastDiscoveryRunning()) {
                Debug.Log("[NetworkManager]: Removing broadcast host");
                NetworkTransport.StopBroadcastDiscovery();
                NetworkTransport.RemoveHost(0);
            }*/
        }
#endif

        #endregion

#if USE_NETWORKING
        public override NetworkClient StartHost()
        {
            Debug.Log("[NetworkManager]: Starting host");

            maxConnections = PartyParrotManager.Instance.Config.Network.Server.MaxConnections;
            NetworkClient networkClient = base.StartHost();
            if(null == networkClient) {
                return null;
            }

            InitClient(networkClient);
            return networkClient;
        }

        public new bool StartServer()
        {
            maxConnections = PartyParrotManager.Instance.Config.Network.Server.MaxConnections;
            networkAddress = PartyParrotManager.Instance.Config.Network.Server.NetworkAddress;
            networkPort = PartyParrotManager.Instance.Config.Network.Server.Port;

            if(PartyParrotManager.Instance.Config.Network.Server.BindIp()) {
                serverBindAddress = PartyParrotManager.Instance.Config.Network.Server.NetworkAddress;
                serverBindToIP = true;

                Debug.Log($"[NetworkManager]: Binding to address {serverBindAddress}");
            }

            Debug.Log($"[NetworkManager]: Listening for clients on {networkAddress}:{networkPort}");
            return base.StartServer();
        }

        public new NetworkClient StartClient()
        {
            Debug.Log($"[NetworkManager]: Connecting client to {networkAddress}:{networkPort}");
            NetworkClient networkClient = base.StartClient();
            if(null == networkClient) {
                return null;
            }

            InitClient(networkClient);
            return networkClient;
        }

        private void InitClient(NetworkClient networkClient)
        {
            networkClient.RegisterHandler(CustomMsgType.SceneChange, OnClientCustomSceneChange);
            networkClient.RegisterHandler(CustomMsgType.SceneChanged, OnClientCustomSceneChanged);
        }
#elif USE_MLAPI
        public new SocketTasks StartHost(Vector3? position = null, Quaternion? rotation = null, bool? createPlayerObject = null, ulong? prefabHash = null, System.IO.Stream payloadStream = null)
        {
            Debug.Log("[NetworkManager]: Starting host");

            // TODO: don't assume UnetTransport
            UnetTransport transport = (UnetTransport)NetworkConfig.NetworkTransport;
            transport.MaxConnections = PartyParrotManager.Instance.Config.Network.Server.MaxConnections;

            return base.StartHost(position, rotation, createPlayerObject, prefabHash, payloadStream);
        }

        public new void StopHost()
        {
            Debug.Log("[NetworkManager]: Stopping host");

            base.StopHost();
        }

        public new SocketTasks StartServer()
        {
            Debug.Log("[NetworkManager]: Starting server");

            // TODO: don't assume UnetTransport
            UnetTransport transport = (UnetTransport)NetworkConfig.NetworkTransport;
            transport.MaxConnections = PartyParrotManager.Instance.Config.Network.Server.MaxConnections;

            transport.ConnectAddress = PartyParrotManager.Instance.Config.Network.Server.NetworkAddress;
            transport.ServerListenPort = PartyParrotManager.Instance.Config.Network.Server.Port;

            transport.SupportWebsocket = PartyParrotManager.Instance.Config.Network.Server.UseWebSockets;
            transport.ServerWebsocketListenPort = PartyParrotManager.Instance.Config.Network.Server.WebSocketPort;

            Debug.Log($"[NetworkManager]: Listening for clients on {transport.ConnectAddress}:{transport.ServerListenPort}");
            return base.StartServer();
        }

        public new void StopServer()
        {
            Debug.Log("[NetworkManager]: Stopping server");

            base.StopServer();
        }

        public new SocketTasks StartClient()
        {
            Debug.Log("[NetworkManager]: Starting client");

            // TODO: don't assume UnetTransport
            UnetTransport transport = (UnetTransport)NetworkConfig.NetworkTransport;

            Debug.Log($"[NetworkManager]: Connecting client to {transport.ConnectAddress}:{transport.ConnectPort}");
            return base.StartClient();
        }

        public SocketTasks StartClient(byte[] connectionData)
        {
            NetworkConfig.ConnectionData = connectionData;

            return StartClient();
        }

        public new void StopClient()
        {
            Debug.Log("[NetworkManager]: Stopping client");

            base.StopClient();
        }
#else
        public NetworkClient StartHost()
        {
            Debug.Log("[NetworkManager]: Starting host");

            return new NetworkClient();
        }

        public void StopHost()
        {
            Debug.Log("[NetworkManager]: Stopping host");
        }

        public bool StartServer()
        {
            Debug.Log("[NetworkManager]: Starting server");

            return true;
        }

        public void StopServer()
        {
            Debug.Log("[NetworkManager]: Stopping server");
        }

        public NetworkClient StartClient(byte[] connectionData = null)
        {
            Debug.Log("[NetworkManager]: Starting client");

            return new NetworkClient();
        }

        public void StopClient()
        {
            Debug.Log("[NetworkManager]: Stopping client");
        }
#endif

        public void Stop()
        {
            if(IsServerActive() && IsClientActive()) {
                StopHost();
            } else if(IsServerActive()) {
                StopServer();
            } else if(IsClientActive()) {
                StopClient();
            }
        }

#if USE_MLAPI
        public void LocalClientReady()
#else
        public void LocalClientReady(NetworkConnection conn)
#endif
        {
#if USE_NETWORKING
            if(null == conn || conn.isReady) {
                return;
            }

            Debug.Log($"[NetworkManager]: Local client ready!");

            ClientScene.Ready(conn);
#elif USE_MLAPI
            Debug.LogWarning($"TODO: local client ready");
#endif
        }

        public void AddLocalPlayer(int playerControllerId)
        {
            Debug.Log($"[NetworkManager]: Adding local player {playerControllerId}!");

#if USE_NETWORKING
            ClientScene.AddPlayer(playerControllerId);
#elif USE_MLAPI
            Debug.LogWarning($"TODO: add local player {playerControllerId}");
#else
            ServerAddPlayerEvent?.Invoke(this, new ServerAddPlayerEventArgs(new NetworkConnection((ulong)playerControllerId), playerControllerId));
#endif
        }

#if USE_NETWORKING
        public override void ServerChangeScene(string sceneName)
#else
        public void ServerChangeScene(string sceneName)
#endif
        {
#if USE_NETWORKING
            Debug.Log($"[NetworkManager]: Server changing to scene '{sceneName}'...");

            NetworkServer.SetAllClientsNotReady();
            networkSceneName = sceneName;
#elif USE_MLAPI
            Debug.LogWarning($"TODO: server change scene");
#endif

            ServerChangeSceneEvent?.Invoke(this, EventArgs.Empty);

            BroadcastSceneChange(sceneName);
        }

        private void BroadcastSceneChange(string sceneName)
        {
#if USE_NETWORKING
            StringMessage msg = new StringMessage(networkSceneName);
            NetworkServer.SendToAll(CustomMsgType.SceneChange, msg);
#elif USE_MLAPI
            Debug.LogWarning("TODO: broadcast scene change");
#endif
        }

        private void BroadcastSceneChanged(string sceneName)
        {
#if USE_NETWORKING
            StringMessage msg = new StringMessage(networkSceneName);
            NetworkServer.SendToAll(CustomMsgType.SceneChanged, msg);
#elif USE_MLAPI
            Debug.LogWarning("TODO: broadcast scene changed");
#endif
        }

        public void ServerChangedScene()
        {
            if(!IsServerActive()) {
                return;
            }

            ServerChangedSceneEvent?.Invoke(this, EventArgs.Empty);

#if USE_NETWORKING
            NetworkServer.SpawnObjects();
            OnServerSceneChanged(networkSceneName);
#elif USE_MLAPI
            Debug.LogWarning("TODO: server changed scene");
            //OnServerSceneChanged(networkSceneName);
#endif
        }

#if USE_NETWORKING

        #region Server Callbacks

        // host is started
        public override void OnStartHost()
        {
            CallbackLog("OnStartHost()");

            base.OnStartHost();
        }

        // host is stopped
        public override void OnStopHost()
        {
            CallbackLog("OnStopHost()");

            base.OnStopHost();
        }

        // server / host is started
        public override void OnStartServer()
        {
            CallbackLog("OnStartServer()");

            base.OnStartServer();

            ServerStartEvent?.Invoke(this, EventArgs.Empty);
        }

        // server / host is stopped
        public override void OnStopServer()
        {
            CallbackLog("OnStopServer()");

            ServerStopEvent?.Invoke(this, EventArgs.Empty);

            base.OnStopServer();
        }

        // server - client connect
        public override void OnServerConnect(NetworkConnection conn)
        {
            CallbackLog($"OnServerConnect({conn})");

            base.OnServerConnect(conn);

            ServerConnectEvent?.Invoke(this, EventArgs.Empty);
        }

        // server - client disconnect
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            CallbackLog($"OnServerDisconnect({conn})");

            ServerDisconnectEvent?.Invoke(this, EventArgs.Empty);

            base.OnServerDisconnect(conn);
        }

        // server - client adds player
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            CallbackLog($"OnServerAddPlayer({conn}, {playerControllerId})");

            ServerAddPlayerEvent?.Invoke(this, new ServerAddPlayerEventArgs(conn, playerControllerId));

            // NOTE: do not call the base method
        }

        // server - client removes player
        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            CallbackLog($"OnServerRemovePlayer({conn}, {player})");

            base.OnServerRemovePlayer(conn, player);
        }

        // server - client ready
        public override void OnServerReady(NetworkConnection conn)
        {
            CallbackLog($"OnServerReady({conn})");

            base.OnServerReady(conn);
        }

        // server - scene loaded (server initiated)
        public override void OnServerSceneChanged(string sceneName)
        {
            CallbackLog($"OnServerSceneChanged({sceneName})");

            BroadcastSceneChanged(sceneName);
        }

        #endregion

        #region Client Callbacks

        // client is started
        public override void OnStartClient(NetworkClient networkClient)
        {
            CallbackLog($"OnStartClient({networkClient})");

            base.OnStartClient(networkClient);
        }

        // client is stopped
        public override void OnStopClient()
        {
            CallbackLog("OnStopClient()");

            base.OnStopClient();
        }

        // client - client connect
        public override void OnClientConnect(NetworkConnection conn)
        {
            CallbackLog($"OnClientConnect({conn})");

            ClientConnectEvent?.Invoke(this, EventArgs.Empty);

            // NOTE: do not call the base method
        }

        // client - client disconnect
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            CallbackLog($"OnClientDisconnect({conn})");

            base.OnClientDisconnect(conn);

            ClientDisconnectEvent?.Invoke(this, EventArgs.Empty);
        }

        // client - scene loaded (server initiated)
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            CallbackLog($"OnClientSceneChanged({conn})");

            base.OnClientSceneChanged(conn);
        }

        // custom message
        public void OnClientCustomSceneChange(NetworkMessage netMsg)
        {
            CallbackLog($"OnClientCustomSceneChange({netMsg})");

            string sceneName = netMsg.reader.ReadString();
            ClientSceneChangeEvent?.Invoke(this, new ClientSceneEventArgs(sceneName));
        }

        // custom message
        public void OnClientCustomSceneChanged(NetworkMessage netMsg)
        {
            CallbackLog($"OnClientCustomSceneChanged({netMsg})");

            string sceneName = netMsg.reader.ReadString();
            ClientSceneChangedEvent?.Invoke(this, new ClientSceneEventArgs(sceneName));
        }

        #endregion

#elif USE_MLAPI

        #region Callbacks

        private void ApprovalCheckEventHandler(byte[] connectionData, ulong clientId, ConnectionApprovedDelegate callback)
        {
            CallbackLog($"Approving connection {clientId}");

            // TODO: actually verify the connection
            bool approve = true;

            callback(false, null, approve, null, null);
        }

        private void ServerStartedEventHandler()
        {
            CallbackLog("Server started");

            ServerStartEvent?.Invoke(this, EventArgs.Empty);

            // for whatever reason, OnClientConnectedCallback isn't invoked when running as a host
            if(IsHost) {
                ClientConnectedEventHandler(ServerClientId);
            }
        }

        private void ClientConnectedEventHandler(ulong clientId)
        {
            CallbackLog($"Client {clientId} connect");

            if(IsServer) {
                ServerConnectEvent?.Invoke(this, EventArgs.Empty);
            }

            if(IsClient) {
                ClientConnectEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ClientDisconnectEventHandler(ulong clientId)
        {
            CallbackLog($"Client {clientId} disconnect");

            ClientDisconnectEvent?.Invoke(this, EventArgs.Empty);
        }

        // server - scene loaded (server initiated)
        public void OnServerSceneChanged(string sceneName)
        {
            CallbackLog($"OnServerSceneChanged({sceneName})");

            BroadcastSceneChanged(sceneName);
        }

        // custom message
        public void OnClientCustomSceneChange(string sceneName)
        {
            CallbackLog($"OnClientCustomSceneChange({sceneName})");

            ClientSceneChangeEvent?.Invoke(this, new ClientSceneEventArgs(sceneName));
        }

        // custom message
        public void OnClientCustomSceneChanged(string sceneName)
        {
            CallbackLog($"OnClientCustomSceneChanged({sceneName})");

            ClientSceneChangedEvent?.Invoke(this, new ClientSceneEventArgs(sceneName));
        }

        #endregion

#endif

        private void CallbackLog(string message)
        {
            if(!_enableCallbackLogging) {
                return;
            }
            Debug.Log($"[NetworkManager]: {message}");
        }

        private void InitDebugMenu()
        {
            DebugMenuNode debugMenuNode = DebugMenuManager.Instance.AddNode(() => "Core.NetworkManager");
            debugMenuNode.RenderContentsAction = () => {
#if USE_NETWORKING
                if(_hud.enabled) {
                    _hud.showGUI = GUILayout.Toggle(_hud.showGUI, "Show Network HUD GUI");
                }

                if(Discovery.enabled) {
                    Discovery.showGUI = GUILayout.Toggle(Discovery.showGUI, "Show Network Discovery GUI");
                }
#endif

                _enableCallbackLogging = GUILayout.Toggle(_enableCallbackLogging, "Callback Logging");
            };
        }
    }
}

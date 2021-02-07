using System;
using System.IO;
using System.Net;

using UnityEngine;

namespace pdxpartyparrot.Core
{
    [Serializable]
    public class Config
    {
        [Serializable]
        public struct NetworkConfig
        {
#if USE_NETWORKING
            [Serializable]
            public struct DiscoveryConfig
            {
                [SerializeField]
                private bool enable;

                public bool Enable => enable;

                [SerializeField]
                private int port;

                public int Port => port <= 0 ? 4777 : port;
            }
#endif

#if USE_NETWORKING || USE_MLAPI
            [Serializable]
            public struct ServerConfig
            {
                [SerializeField]
                private string networkAddress;

                // TODO: this should only be true if it's an ip
                public bool BindIp()
                {
                    return IPAddress.TryParse(networkAddress, out _);
                }

                public string NetworkAddress => networkAddress;

                [SerializeField]
                private int port;

                public int Port => port <= 0 ? 7777 : port;

                [SerializeField]
                private bool _useWebSockets;

                public bool UseWebSockets => _useWebSockets;

                [SerializeField]
                private int webSocketPort;

                public int WebSocketPort => webSocketPort <= 0 ? 8887 : webSocketPort;

                [SerializeField]
                private int maxConnections;

                public int MaxConnections => maxConnections <= 0 ? 1 : maxConnections;
            }
#endif

#if USE_NETWORKING
            [SerializeField]
            private DiscoveryConfig discovery;

            public DiscoveryConfig Discovery => discovery;
#endif

#if USE_NETWORKING || USE_MLAPI
            [SerializeField]
            private ServerConfig server;

            public ServerConfig Server => server;
#endif
        }

#if USE_NETWORKING || USE_MLAPI
        [SerializeField]
        private NetworkConfig network;

        public NetworkConfig Network => network;
#endif

        public void Load(string path, string fileName)
        {
            string configPath = Path.Combine(path, fileName);
            if(!File.Exists(configPath)) {
                Debug.LogWarning($"Missing config {configPath}!");
                return;
            }

            Debug.Log($"Loading config from {configPath}...");

            string configJson = File.ReadAllText(configPath);
            JsonUtility.FromJsonOverwrite(configJson, this);
        }
    }
}

#pragma warning disable 0618    // disable obsolete warning for now

using System;

#if USE_NETWORKING
using UnityEngine.Networking;
#endif

namespace pdxpartyparrot.Core.Network
{
    public sealed class ServerAddPlayerEventArgs : EventArgs
    {
        public NetworkConnection NetworkConnection { get; }

        public int PlayerControllerId { get; }

        public ServerAddPlayerEventArgs(NetworkConnection conn, int playerControllerId)
        {
            NetworkConnection = conn;
            PlayerControllerId = playerControllerId;
        }
    }
}

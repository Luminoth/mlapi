#if USE_MLAPI
using MLAPI.Connection;

namespace pdxpartyparrot.Core.Network
{
    public class NetworkClient : NetworkedClient
    {
    }
}
#elif !USE_NETWORKING
namespace pdxpartyparrot.Core.Network
{
    public class NetworkClient
    {
        protected NetworkConnection m_Connection;

        public NetworkConnection connection => m_Connection;
    }
}
#endif

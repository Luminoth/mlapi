#if !USE_NETWORKING
namespace pdxpartyparrot.Core.Network
{
    public class NetworkConnection
    {
        public ulong ClientId { get; private set; }

        public NetworkConnection(ulong clientId)
        {
            ClientId = clientId;
        }
    }
}
#endif

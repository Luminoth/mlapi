using UnityEngine.Networking;

namespace pdxpartyparrot.Core.Network
{
    public static class UnityWebRequestExtensions
    {
        public static bool IsHttpError(this UnityWebRequest www)
        {
            return www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
        }
    }
}

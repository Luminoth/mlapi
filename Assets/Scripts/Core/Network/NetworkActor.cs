#pragma warning disable 0618    // disable obsolete warning for now

using pdxpartyparrot.Core.Actors;

#if USE_NETWORKING
using UnityEngine;
using UnityEngine.Networking;
#elif USE_MLAPI
using MLAPI;

using UnityEngine;
#endif

namespace pdxpartyparrot.Core.Network
{
    //[RequireComponent(typeof(Actor))]
#if USE_NETWORKING
    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(NetworkTransform))]
#elif USE_MLAPI
    [RequireComponent(typeof(NetworkedObject))]
#endif
    public abstract class NetworkActor : NetworkBehaviour
    {
#if USE_NETWORKING
        public NetworkIdentity NetworkIdentity { get; private set; }

        public NetworkTransform NetworkTransform { get; private set; }
#endif

        protected Actor Actor { get; private set; }

        #region Unity Lifecycle

        protected virtual void Awake()
        {
#if USE_NETWORKING
            NetworkIdentity = GetComponent<NetworkIdentity>();
            NetworkTransform = GetComponent<NetworkTransform>();
#endif

            Actor = GetComponent<Actor>();
        }

        #endregion
    }
}

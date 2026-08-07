using Unity.Netcode;
using UnityEngine;

namespace Frantic.Networking
{
    public abstract class FranticNetworkObject : NetworkBehaviour
    {
        protected bool IsServerOwner => IsServer && !IsHost;

        protected bool IsHostOwner => IsHost;

        protected bool IsLocalOwner => IsOwner;

        protected void RequireServer()
        {
            if (!IsServer)
            {
                Debug.LogError($"[Network] {nameof(FranticNetworkObject)} requires server authority");
            }
        }

        protected void RequireOwner()
        {
            if (!IsOwner)
            {
                Debug.LogError($"[Network] {nameof(FranticNetworkObject)} requires owner authority");
            }
        }
    }
}

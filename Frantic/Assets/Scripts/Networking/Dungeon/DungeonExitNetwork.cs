using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class DungeonExitNetwork : FranticNetworkObject
    {
        [SerializeField]
        private float _interactionRange = 2f;

        private void Update()
        {
            if (!IsOwner) return;

            if (InputManager.Interact)
            {
                CheckInteraction();
            }
        }

        private void CheckInteraction()
        {
            var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, _interactionRange);

            foreach (var collider in nearbyObjects)
            {
                if (collider.CompareTag("Player"))
                {
                    Debug.Log("[Exit] Player reached dungeon exit, completing run");
                    CompleteDungeonServerRpc();
                    return;
                }
            }
        }

        [ServerRpc]
        private void CompleteDungeonServerRpc()
        {
            if (!IsServer) return;

            Debug.Log("[Exit] Completing dungeon, returning to hub");
            GameManager.Instance?.CompleteDungeon();
        }
    }
}

using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class InteractionNetwork : FranticNetworkObject
    {
        [SerializeField]
        private float _interactionRange = 2f;

        [SerializeField]
        private string _interactionTag = "DungeonEntrance";

        private void Update()
        {
            if (!IsOwner) return;

            if (InputManager.Interact)
            {
                Debug.Log($"[Interaction] Interact pressed, checking range {_interactionRange}");
                CheckInteraction();
            }
        }

        private void CheckInteraction()
        {
            var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, _interactionRange);
            Debug.Log($"[Interaction] Found {nearbyObjects.Length} colliders in range");

            foreach (var collider in nearbyObjects)
            {
                Debug.Log($"[Interaction] Collider: {collider.name} tag={collider.tag}");
                if (collider.CompareTag(_interactionTag))
                {
                    Debug.Log($"[Interaction] Found {collider.name}, requesting ready");
                    var playerNetwork = GetComponent<PlayerNetwork>();
                    playerNetwork?.InteractServerRpc();
                    return;
                }
            }

            Debug.LogWarning("[Interaction] No dungeon entrance found in range");
        }
    }
}

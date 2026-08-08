using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (InputManager.Interact)
            {
                Debug.Log("[Interaction] Interact pressed, checking nearby objects...");
                CheckInteraction();
            }
        }

        private void CheckInteraction()
        {
            var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, _interactionRange);
            Debug.Log($"[Interaction] Found {nearbyObjects.Length} colliders in range");

            foreach (var collider in nearbyObjects)
            {
                Debug.Log($"[Interaction] Collider: {collider.gameObject.name}, Tag: {collider.gameObject.tag}");
                if (collider.CompareTag(_interactionTag))
                {
                    Debug.Log("[Interaction] DungeonEntrance found, calling Interact");
                    var playerNetwork = GetComponent<PlayerNetwork>();
                    if (playerNetwork != null)
                    {
                        Debug.Log("[Interaction] PlayerNetwork found, calling Interact()");
                        playerNetwork.Interact();
                    }
                    else
                    {
                        Debug.LogError("[Interaction] PlayerNetwork NOT found on this GameObject! Components: " + string.Join(", ", GetComponents<Component>().Select(c => c.GetType().Name)));
                    }
                    return;
                }
            }
        }
    }
}

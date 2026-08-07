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
                CheckInteraction();
            }
        }

        private void CheckInteraction()
        {
            var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, _interactionRange);

            foreach (var collider in nearbyObjects)
            {
                if (collider.CompareTag(_interactionTag))
                {
                    GameManager.Instance?.RequestReadyServerRpc(OwnerClientId);
                    return;
                }
            }
        }
    }
}

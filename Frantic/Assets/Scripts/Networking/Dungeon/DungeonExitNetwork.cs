using UnityEngine;

namespace Frantic.Networking
{
    public class DungeonExitNetwork : MonoBehaviour
    {
        [SerializeField]
        private float _interactionRange = 2f;

        private bool _hasTriggered;

        private void Update()
        {
            if (!FranticNetworkManager.Instance.IsHost) return;
            if (_hasTriggered) return;

            CheckInteraction();
        }

        private void CheckInteraction()
        {
            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

            foreach (var player in players)
            {
                var distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < _interactionRange)
                {
                    _hasTriggered = true;
                    GameManager.Instance?.CompleteDungeon();
                    return;
                }
            }
        }
    }
}

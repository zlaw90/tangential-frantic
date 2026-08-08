using UnityEngine;

namespace Frantic.Networking
{
    public class DungeonExitNetwork : MonoBehaviour
    {
        [SerializeField]
        private float _interactionRange = 5f;

        private float _spawnTime;
        private bool _hasTriggered;
        private Collider2D _exitCollider;

        private void Awake()
        {
            _spawnTime = Time.time;
            _exitCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            if (_exitCollider != null)
            {
                var playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer != -1)
                {
                    Physics2D.IgnoreLayerCollision(playerLayer, gameObject.layer, true);
                }
            }
        }

        private void Update()
        {
            if (_hasTriggered) return;
            if (Time.time - _spawnTime < 5f) return;

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

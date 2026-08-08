using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class EnemyNetwork : FranticNetworkObject
    {
        [SerializeField]
        private int _maxHealth = 50;

        [SerializeField]
        private float _moveSpeed = 2f;

        [SerializeField]
        private float _attackRange = 1.5f;

        [SerializeField]
        private int _attackDamage = 10;

        [SerializeField]
        public GameObject _lootPrefab;

        [SerializeField]
        private float _lootDropChance = 0.5f;

        private NetworkVariable<int> _networkHealth = new NetworkVariable<int>();

        private Transform _targetPlayer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _networkHealth.Value = _maxHealth;
            Debug.Log($"[Enemy] Spawned with {_maxHealth} HP");

            FindNearestPlayer();
        }

        private void Update()
        {
            if (!IsServer) return;

            if (_targetPlayer != null)
            {
                MoveTowardsTarget();
            }
        }

        private void FindNearestPlayer()
        {
            var players = FindObjectsOfType<PlayerNetwork>();
            if (players.Length == 0) return;

            float nearestDistance = float.MaxValue;
            foreach (var player in players)
            {
                var distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    _targetPlayer = player.transform;
                }
            }
        }

        private void MoveTowardsTarget()
        {
            if (_targetPlayer == null) return;

            var direction = (_targetPlayer.position - transform.position).normalized;
            transform.position += new Vector3(direction.x, direction.y, 0f) * _moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _targetPlayer.position) < _attackRange)
            {
                AttackPlayer();
            }
        }

        private void AttackPlayer()
        {
            var playerHealth = _targetPlayer.GetComponent<PlayerHealthNetwork>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageServerRpc(_attackDamage);
            }
        }

        [ServerRpc]
        public void TakeDamageServerRpc(int damage)
        {
            if (!IsServer) return;

            if (damage <= 0) return;

            var currentHealth = _networkHealth.Value;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            _networkHealth.Value = currentHealth;

            Debug.Log($"[Enemy] Took {damage} damage, health: {currentHealth}/{_maxHealth}");

            if (currentHealth <= 0)
            {
                DieServerRpc();
            }
        }

        [ServerRpc]
        private void DieServerRpc()
        {
            Debug.Log("[Enemy] Enemy died");

            if (Random.value < _lootDropChance && _lootPrefab != null)
            {
                SpawnLoot();
            }

            if (NetworkObject != null)
            {
                NetworkObject.Despawn();
            }

            GameObject.Destroy(gameObject);
        }

        private void SpawnLoot()
        {
            var loot = GameObject.Instantiate(_lootPrefab, transform.position, Quaternion.identity);
            var networkObject = loot.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                networkObject = loot.AddComponent<NetworkObject>();
            }
            networkObject.Spawn(true);

            Debug.Log($"[Spawn] Loot dropped at {transform.position}");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;

            var playerHealth = collision.GetComponent<PlayerHealthNetwork>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageServerRpc(_attackDamage);
            }
        }
    }
}

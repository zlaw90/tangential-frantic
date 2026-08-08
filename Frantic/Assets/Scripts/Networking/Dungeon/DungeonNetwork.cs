using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

namespace Frantic.Networking
{
    public class DungeonNetwork : NetworkBehaviour
    {
        [SerializeField]
        public GameObject _roomPrefab;

        [SerializeField]
        public GameObject _enemyPrefab;



        [SerializeField]
        private int _minRooms = 5;

        [SerializeField]
        private int _maxRooms = 10;

        private readonly HashSet<Vector3Int> _placedRooms = new();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                GenerateDungeonServerRpc();
            }
        }

        [ServerRpc]
        private void GenerateDungeonServerRpc()
        {
            Debug.Log("[Dungeon] Generating dungeon");

            _placedRooms.Clear();
            int roomCount = Random.Range(_minRooms, _maxRooms + 1);

            var center = Vector3Int.zero;
            PlaceRoom(center);

            for (int i = 1; i < roomCount; i++)
            {
                var direction = GetRandomDirection();
                var nextRoom = _placedRooms.Count > 0 ? _placedRooms.ToArray()[_placedRooms.Count - 1] + direction : Vector3Int.right;

                if (!_placedRooms.Contains(nextRoom))
                {
                    PlaceRoom(nextRoom);
                    _placedRooms.Add(nextRoom);

                    SpawnEnemiesForRoom(nextRoom);
                }
                else
                {
                    i--;
                }
            }

            Debug.Log($"[Dungeon] Generated {roomCount} rooms");
        }

        private void PlaceRoom(Vector3Int position)
        {
            if (_roomPrefab == null)
            {
                Debug.LogWarning("[Dungeon] No room prefab assigned");
                return;
            }

            var room = GameObject.Instantiate(_roomPrefab, new Vector3(position.x, position.y, 0f), Quaternion.identity);
            var networkObject = room.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                networkObject = room.AddComponent<NetworkObject>();
            }
            networkObject.Spawn(true);

            Debug.Log($"[Spawn] Room at {position}");
        }

        private void SpawnEnemiesForRoom(Vector3Int roomPosition)
        {
            var enemyCount = Random.Range(2, 5);
            var playerCount = FranticNetworkManager.Instance.ConnectedClientsList.Count;
            var difficultyMultiplier = playerCount switch
            {
                2 => 1.5f,
                3 => 2f,
                4 => 2.5f,
                _ => 1f
            };

            for (int i = 0; i < Mathf.CeilToInt(enemyCount * difficultyMultiplier); i++)
            {
                var offset = new Vector3(Random.Range(-4f, 4f), Random.Range(-4f, 4f), 0f);
                var spawnPosition = new Vector3(roomPosition.x, roomPosition.y, 0f) + offset;

                SpawnEnemy(spawnPosition);
            }
        }

        private void SpawnEnemy(Vector3 position)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogWarning("[Dungeon] No enemy prefab assigned");
                return;
            }

            var enemy = GameObject.Instantiate(_enemyPrefab, position, Quaternion.identity);
            var networkObject = enemy.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                networkObject = enemy.AddComponent<NetworkObject>();
            }
            networkObject.Spawn(true);

            Debug.Log($"[Spawn] Enemy at {position}");
        }

        private Vector3Int GetRandomDirection()
        {
            int direction = Random.Range(0, 4);
            return direction switch
            {
                0 => Vector3Int.right,
                1 => Vector3Int.left,
                2 => Vector3Int.up,
                _ => Vector3Int.down
            };
        }


    }
}

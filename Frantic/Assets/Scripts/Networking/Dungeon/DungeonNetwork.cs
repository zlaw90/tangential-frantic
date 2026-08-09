using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Frantic.Networking
{
    public class DungeonNetwork : MonoBehaviour
    {
        [SerializeField]
        public GameObject _roomPrefab;

        [SerializeField]
        public GameObject _enemyPrefab;

        [SerializeField]
        public GameObject _playerPrefab;

        [SerializeField]
        public GameObject _exitPrefab;

        [SerializeField]
        public Map _map;

        [SerializeField]
        private float _roomSpacing = 24f;

        [Header("Dungeon Size By Cell Count")]
        public int width = 32;
        public int height = 18;

        [Header("Player Spawn Cell Position")]
        public int playerSpawnX = -1;
        public int playerSpawnY = -1;

        [Header("Exit Cell Position")]
        public int exitSpawnX = -1;
        public int exitSpawnY = -1;

        private readonly HashSet<Vector3Int> _placedRooms = new();

        private void Start()
        {
            GenerateDungeon();

            SpawnPlayer();

            SpawnEnemies();
        }

        public void GenerateDungeon()
        {
            RandomizePlayerSpawnLocationIfNeeded();
            RandomizeExitLocationIfNeeded();

            Debug.Log("[Dungeon] Generating dungeon");
            _map.Generate(width, height, exitSpawnX, exitSpawnY, playerSpawnX, playerSpawnY);

            SpawnExit();
        }

        private void RandomizePlayerSpawnLocationIfNeeded()
        {
            if (playerSpawnX < 0 || playerSpawnY < 0)
            {
                Debug.Log("[Dungeon] Randomizing player spawn location");
                bool hasDefinedExit = exitSpawnX >= 0 && exitSpawnY >= 0;
                int minXDif = Mathf.FloorToInt((float)width / 4);
                int minYDif = Mathf.FloorToInt((float)height / 4);
                do
                {
                    playerSpawnX = Random.Range(0, width);
                    playerSpawnY = Random.Range(0, height);
                } while (hasDefinedExit && (exitSpawnX < minXDif || exitSpawnY < minYDif));
            }
        }
        private void RandomizeExitLocationIfNeeded()
        {
            if (exitSpawnX < 0 || exitSpawnY < 0)
            {
                Debug.Log("[Dungeon] Randomizing exit location");
                int minXDif = Mathf.FloorToInt((float)width / 4);
                int minYDif = Mathf.FloorToInt((float)height / 4);
                do
                {
                    exitSpawnX = Random.Range(0, width);
                    exitSpawnY = Random.Range(0, height);
                } while (exitSpawnX < minXDif || exitSpawnY < minYDif);
            }
        }

        private void SpawnPlayer()
        {
            if (_playerPrefab != null)
            {
                Instantiate(_playerPrefab, _map.GetCoordinatesFromCellPosition(playerSpawnX, playerSpawnY), Quaternion.identity);
                Debug.Log("[Dungeon] Player spawned");
            }
            else
            {
                Debug.LogWarning("[Dungeon] No player prefab assigned");
            }

            var hud = FindAnyObjectByType<HUD>();
            if (hud == null)
            {
                Debug.LogWarning("[Dungeon] No HUD found in scene!");
            }
        }

        private void SpawnEnemies()
        {
            int enemyCount = 3 * Mathf.FloorToInt(Mathf.Log(width * height));

            var occupiedCells = new List<(int x, int y)>(enemyCount + 1)
            {
                (playerSpawnX, playerSpawnY)
            };

            for (int i = 0; i < enemyCount; i++)
            {
                int enemyX;
                int enemyY;
                do
                {
                    enemyX = Random.Range(0, width);
                    enemyY = Random.Range(0, height);
                } while (occupiedCells.Any(t => t.x == enemyX && t.y == enemyY));

                occupiedCells.Add((enemyX, enemyY));
                SpawnEnemy(_map.GetCoordinatesFromCellPosition(enemyX, enemyY));
            }
        }

        private void SpawnEnemy(Vector3 position)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogWarning("[Dungeon] No enemy prefab assigned");
                return;
            }

            var enemyInstance = Instantiate(_enemyPrefab, position, Quaternion.identity);
            Debug.Log($"[Spawn] Enemy at {position}");
        }

        private void SpawnExit()
        {
           
            if (_exitPrefab != null)
            {
                var exitPosition = _map.GetCoordinatesFromCellPosition(exitSpawnX, exitSpawnY);
                var exitInstance = Instantiate(_exitPrefab, exitPosition, Quaternion.identity);
                exitInstance.name = "DungeonExit";
                Debug.Log($"[Spawn] Exit at {exitPosition}");
                return;
            }

            Debug.LogWarning("[Dungeon] No exit prefab assigned");
        }

        // private Vector3 FindSafeExitPosition()
        // {
        //     if (_placedRooms.Count == 0)
        //     {
        //         return new Vector3(0f, -20f, 0f);
        //     }

        //     var roomHalfSize = 10f;

        //     for (int attempt = 0; attempt < 200; attempt++)
        //     {
        //         var angle = Random.value * Mathf.PI * 2f;
        //         var radius = Random.Range(30f, 60f);
        //         var candidate = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

        //         bool insideAnyRoom = false;
        //         foreach (var room in _placedRooms)
        //         {
        //             var roomCenter = new Vector3(room.x * _roomSpacing, room.y * _roomSpacing, 0f);
        //             var dx = Mathf.Abs(candidate.x - roomCenter.x);
        //             var dy = Mathf.Abs(candidate.y - roomCenter.y);
        //             if (dx < roomHalfSize && dy < roomHalfSize)
        //             {
        //                 insideAnyRoom = true;
        //                 break;
        //             }
        //         }

        //         if (!insideAnyRoom)
        //         {
        //             Debug.Log($"[Exit] Safe position found: {candidate}");
        //             return candidate;
        //         }
        //     }

        //     Debug.LogWarning("[Exit] Could not find safe position, using fallback");
        //     return new Vector3(0f, -40f, 0f);
        // }

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

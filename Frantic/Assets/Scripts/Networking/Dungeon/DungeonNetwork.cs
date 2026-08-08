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
        private int _minRooms = 5;

        [SerializeField]
        private int _maxRooms = 10;

        [SerializeField]
        private float _roomSpacing = 24f;

        private readonly HashSet<Vector3Int> _placedRooms = new();

        private void Awake()
        {
            SpawnPlayer();
            GenerateDungeon();
        }

        public void GenerateDungeon()
        {
            Debug.Log("[Dungeon] Generating dungeon");

            _placedRooms.Clear();
            int roomCount = Random.Range(_minRooms, _maxRooms + 1);

            var center = Vector3Int.zero;
            PlaceRoom(center);

            for (int i = 1; i < roomCount; i++)
            {
                var direction = GetRandomDirection();
                var lastRoom = _placedRooms.ToList()[_placedRooms.Count - 1];
                var nextRoom = lastRoom + direction;

                if (!_placedRooms.Contains(nextRoom))
                {
                    PlaceRoom(nextRoom);
                    if (nextRoom != Vector3Int.zero)
                    {
                        SpawnEnemiesForRoom(nextRoom);
                    }
                }
                else
                {
                    i--;
                }
            }

            SpawnExit();
            Debug.Log($"[Dungeon] Generated {roomCount} rooms");
        }

        private void SpawnPlayer()
        {
            if (_playerPrefab != null)
            {
                Instantiate(_playerPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                Debug.Log("[Dungeon] Player spawned");
            }
            else
            {
                Debug.LogWarning("[Dungeon] No player prefab assigned");
            }

            var hud = FindObjectOfType<HUD>();
            if (hud == null)
            {
                Debug.LogWarning("[Dungeon] No HUD found in scene!");
            }
        }

        private void PlaceRoom(Vector3Int position)
        {
            if (_roomPrefab == null)
            {
                Debug.LogWarning("[Dungeon] No room prefab assigned");
                return;
            }

            var worldPos = new Vector3(position.x * _roomSpacing, position.y * _roomSpacing, 0f);
            var roomInstance = Instantiate(_roomPrefab, worldPos, Quaternion.identity);
            _placedRooms.Add(position);

            Debug.Log($"[Spawn] Room at {worldPos}");
        }

        private void SpawnEnemiesForRoom(Vector3Int roomPosition)
        {
            var enemyCount = Random.Range(2, 5);
            var difficultyMultiplier = 1f;

            for (int i = 0; i < Mathf.CeilToInt(enemyCount * difficultyMultiplier); i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(6f, 10f);
                var offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                var spawnPosition = new Vector3(roomPosition.x * _roomSpacing, roomPosition.y * _roomSpacing, 0f) + offset;

                float distToPlayer = Vector3.Distance(spawnPosition, Vector3.zero);
                if (distToPlayer >= 10f)
                {
                    SpawnEnemy(spawnPosition);
                }
                else
                {
                    i--;
                }
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
            var exitPosition = FindSafeExitPosition();

            if (_exitPrefab != null)
            {
                var exitInstance = Instantiate(_exitPrefab, exitPosition, Quaternion.identity);
                exitInstance.name = "DungeonExit";
                Debug.Log($"[Spawn] Exit at {exitPosition}");
                return;
            }

            Debug.LogWarning("[Dungeon] No exit prefab assigned, creating fallback exit");
            var fallbackExit = new GameObject("DungeonExit");
            fallbackExit.transform.position = exitPosition;
            var exitCollider = fallbackExit.AddComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector2(3f, 2f);
            fallbackExit.AddComponent<DungeonExitNetwork>();
            var spriteRenderer = fallbackExit.AddComponent<SpriteRenderer>();
            var whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.yellow);
            whiteTexture.Apply();
            spriteRenderer.sprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            Debug.Log($"[Spawn] Fallback exit at {exitPosition}");
        }

        private Vector3 FindSafeExitPosition()
        {
            if (_placedRooms.Count == 0)
            {
                return new Vector3(0f, -20f, 0f);
            }

            var roomHalfSize = 10f;

            for (int attempt = 0; attempt < 200; attempt++)
            {
                var angle = Random.value * Mathf.PI * 2f;
                var radius = Random.Range(30f, 60f);
                var candidate = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

                bool insideAnyRoom = false;
                foreach (var room in _placedRooms)
                {
                    var roomCenter = new Vector3(room.x * _roomSpacing, room.y * _roomSpacing, 0f);
                    var dx = Mathf.Abs(candidate.x - roomCenter.x);
                    var dy = Mathf.Abs(candidate.y - roomCenter.y);
                    if (dx < roomHalfSize && dy < roomHalfSize)
                    {
                        insideAnyRoom = true;
                        break;
                    }
                }

                if (!insideAnyRoom)
                {
                    Debug.Log($"[Exit] Safe position found: {candidate}");
                    return candidate;
                }
            }

            Debug.LogWarning("[Exit] Could not find safe position, using fallback");
            return new Vector3(0f, -40f, 0f);
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

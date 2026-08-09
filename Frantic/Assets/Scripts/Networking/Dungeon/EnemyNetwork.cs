using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Frantic.Networking
{
    public class EnemyNetwork : FranticNetworkObject
    {
        [SerializeField]
        private int _maxHealth = 50;

        [SerializeField]
        private float _moveSpeed = 3f;

        [SerializeField]
        private float _attackRange = 1.2f;

        [SerializeField]
        private int _attackDamage = 5;

        [SerializeField]
        public GameObject _lootPrefab;

        [SerializeField]
        private float _lootDropChance = 0.5f;

        [SerializeField]
        private float _healthBarWidth = 0.8f;

        [SerializeField]
        private float _healthBarHeight = 0.06f;

        [SerializeField]
        private float _healthBarOffsetY = 1.0f;

        private float _seekDistanceThreshold = 5.0f;

        public EnemyState State { get { return _state; } }

        private float _mapScale;
        private int _currentHealth;
        private Transform _targetPlayer;
        private float _lastRetargetTime;
        private float _lastAttackTime;
        private const float RETARGET_INTERVAL = 1f;
        private const float ATTACK_COOLDOWN = 0.5f;
        private Image _healthBarImage;
        private GameObject _healthBarGO;
        private NavMeshAgent _navMeshAgent;
        private EnemyState _state = EnemyState.Idle;
        private Animator _animator;


        private void Awake()
        {
            _currentHealth = _maxHealth;
            _lastAttackTime = -999f;
            _animator = GetComponent<Animator>();
            CreateHealthBar();
        }

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.updateUpAxis = false;

            var map = FindFirstObjectByType<Map>();
            _mapScale = map.scale;
            _seekDistanceThreshold *= _mapScale;
        }

        private void CreateHealthBar()
        {
            _healthBarGO = new GameObject("HealthBar");
            _healthBarGO.transform.SetParent(transform);

            var canvas = _healthBarGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10;

            var rectTransform = _healthBarGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(_healthBarWidth, _healthBarHeight);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var backgroundGO = new GameObject("Background");
            backgroundGO.transform.SetParent(_healthBarGO.transform);
            var bgRect = backgroundGO.AddComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(_healthBarWidth + 0.02f, _healthBarHeight + 0.02f);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            var bgImage = backgroundGO.AddComponent<Image>();
            bgImage.color = Color.black;
            bgImage.raycastTarget = false;

            var healthBarInnerGO = new GameObject("HealthBar");
            healthBarInnerGO.transform.SetParent(_healthBarGO.transform);
            var innerRect = healthBarInnerGO.AddComponent<RectTransform>();
            innerRect.sizeDelta = new Vector2(_healthBarWidth, _healthBarHeight);
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            _healthBarImage = healthBarInnerGO.AddComponent<Image>();
            _healthBarImage.color = Color.green;

            UpdateHealthBarPosition();
        }

        private void UpdateHealthBarPosition()
        {
            if (_healthBarGO != null)
            {
                _healthBarGO.transform.localPosition = new Vector3(0f, _healthBarOffsetY, 0f);
            }
        }

        private void UpdateHealthBarColor()
        {
            if (_healthBarImage == null || _healthBarGO == null) return;

            float healthPercent = (float)_currentHealth / _maxHealth;
            _healthBarImage.color = Color.Lerp(Color.red, Color.green, healthPercent);

            var innerRect = _healthBarImage.GetComponent<RectTransform>();
            if (innerRect != null)
            {
                innerRect.localScale = new Vector3(healthPercent, 1f, 1f);
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case EnemyState.Idle:      Behavior_Idle();      break;
                case EnemyState.Seeking:   Behavior_Seeking();   break;
                case EnemyState.Attacking: Behavior_Attacking(); break;
                case EnemyState.Dead:      Behavior_Dead();      break;
            }
        }


        private void Behavior_Idle()
        {
            float nearestPlayerDistance = FindNearestPlayer();
            if (nearestPlayerDistance < _seekDistanceThreshold)
            {
                _state = EnemyState.Seeking;
            }
            else
            {
                _animator.SetBool("attacking", false);
                _animator.SetBool("moving", false);
                _navMeshAgent.isStopped = true;
            }
        }
        private void Behavior_Seeking()
        {
            float nearestPlayerDistance = FindNearestPlayer();
            if (nearestPlayerDistance > _seekDistanceThreshold)
            {
                _animator.SetBool("moving", false);
                _state = EnemyState.Idle;
            }
            else
            {
                _animator.SetBool("moving", true);
                _navMeshAgent.SetDestination(_targetPlayer.position);
                _navMeshAgent.speed = _moveSpeed;
                _navMeshAgent.isStopped = false;
                if (nearestPlayerDistance <= _attackRange)
                {
                    _state = EnemyState.Attacking;
                }
            }
        }
        private void Behavior_Attacking()
        {
            float nearestPlayerDistance = FindNearestPlayer();
            if (nearestPlayerDistance > _attackRange)
            {
                _animator.SetBool("attacking", false);
                _state = EnemyState.Seeking;
            }
            else
            {
                _animator.SetBool("attacking", true);
                _navMeshAgent.SetDestination(_targetPlayer.position);
                _navMeshAgent.speed = _moveSpeed;
                _navMeshAgent.isStopped = false;
                AttackPlayer();
            }
        }
        private void Behavior_Dead()
        {
            _navMeshAgent.isStopped = true;
            _animator.SetBool("attacking", false);
            _animator.SetBool("moving", false);
        }


        private float FindNearestPlayer()
        {
            Transform newTarget = null;

            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

            var path = new NavMeshPath();
            float nearestDistance = float.MaxValue;
            foreach (var player in players)
            {
                if (_navMeshAgent.CalculatePath(player.transform.position, path))
                {
                    var distance = GetPathDistance(path.corners, nearestDistance);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        newTarget = player.transform;
                    }
                }
            }

            _targetPlayer = newTarget;
            return nearestDistance;
        }


        private void AttackPlayer()
        {
            if (Time.time - _lastAttackTime < ATTACK_COOLDOWN) return;

            var playerHealth = _targetPlayer.GetComponent<PlayerHealthNetwork>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_attackDamage);
                _lastAttackTime = Time.time;
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            UpdateHealthBarColor();

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            _state = EnemyState.Dead;

            if (Random.value < _lootDropChance && _lootPrefab != null)
            {
                SpawnLoot();
            }

            if (_healthBarGO != null)
            {
                Destroy(_healthBarGO);
            }

            Destroy(gameObject);
        }

        private void SpawnLoot()
        {
            Instantiate(_lootPrefab, transform.position, Quaternion.identity);
        }


        /// <summary>
        /// Calculate the full distance along a path.
        /// </summary>
        private float GetPathDistance(Vector3[] pathCorners)
        {
            float distance = 0.0f;
            for (int i = 1; i < pathCorners.Length; i++)
            {
                distance += Vector3.Distance(pathCorners[i - 1], pathCorners[i]);
            }
            return distance;
        }
        /// <summary>
        /// Calculate the distance along a path, quiting once the distance reaches the specified limit.
        /// Use this when you only want to identify if a path is longer than a known length.
        /// </summary>
        private float GetPathDistance(Vector3[] pathCorners, float limit)
        {
            float distance = 0.0f;
            int i = 1;
            while (i < pathCorners.Length && distance < limit)
            {
                distance += Vector3.Distance(pathCorners[i - 1], pathCorners[i]);
                i++;
            }
            return distance;
        }

    }


    public enum EnemyState
    {
        Idle,
        Seeking,
        Attacking,
        Dead
    }

}

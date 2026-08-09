using UnityEngine;
using UnityEngine.UI;

namespace Frantic.Networking
{
    public class EnemyNetwork : FranticNetworkObject
    {
        [SerializeField]
        private int _maxHealth = 50;

        [SerializeField]
        private float _moveSpeed = 2f;

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

        private int _currentHealth;
        private Transform _targetPlayer;
        private float _lastRetargetTime;
        private float _lastAttackTime;
        private const float RETARGET_INTERVAL = 1f;
        private const float ATTACK_COOLDOWN = 0.5f;
        private Image _healthBarImage;
        private GameObject _healthBarGO;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _lastAttackTime = -999f;
            FindNearestPlayer();
            CreateHealthBar();
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
            if (_targetPlayer != null)
            {
                MoveTowardsTarget();
            }

            if (Time.time - _lastRetargetTime > RETARGET_INTERVAL)
            {
                FindNearestPlayer();
                _lastRetargetTime = Time.time;
            }

            UpdateHealthBarColor();
        }

        private void FindNearestPlayer()
        {
            var players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);
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
                Debug.Log("Enemy is attacking the player!");
                AttackPlayer();
            }
           
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
    }
}

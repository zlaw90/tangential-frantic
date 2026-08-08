using UnityEngine;

namespace Frantic.Networking
{
    public class ProjectileNetwork : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 20f;

        [SerializeField]
        private int _damage = 10;

        [SerializeField]
        private float _lifetime = 3f;

        private Vector3 _direction;
        private GameObject _owner;
        private float _spawnTime;

        public void Initialize(Vector3 direction, GameObject owner = null)
        {
            _direction = direction.normalized;
            _owner = owner;
            var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            Destroy(gameObject, _lifetime);
            _spawnTime = Time.time;
        }

        private void Update()
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Time.time - _spawnTime < 0.05f)
            {
                return;
            }

            if (collision.gameObject == _owner)
            {
                return;
            }

            var enemyHealth = collision.gameObject.GetComponent<EnemyNetwork>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            var playerHealth = collision.gameObject.GetComponent<PlayerHealthNetwork>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}

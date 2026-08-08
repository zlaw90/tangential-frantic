using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class ProjectileNetwork : NetworkBehaviour
    {
        [SerializeField]
        private float _speed = 20f;

        [SerializeField]
        private int _damage = 10;

        [SerializeField]
        private float _lifetime = 3f;

        private Vector3 _direction;
        private GameObject _owner;

        public void Initialize(Vector3 direction, GameObject owner = null)
        {
            _direction = direction.normalized;
            _owner = owner;
            var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            Destroy(gameObject, _lifetime);
        }

        private void Update()
        {
            if (!IsServer) return;

            transform.position += _direction * _speed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;

            var playerHealth = collision.GetComponent<PlayerHealthNetwork>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageServerRpc(_damage);
                Destroy(gameObject);
                return;
            }

            var enemyHealth = collision.GetComponent<EnemyNetwork>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageServerRpc(_damage);
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject);
        }
    }
}

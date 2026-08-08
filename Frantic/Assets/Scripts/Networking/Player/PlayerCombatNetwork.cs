using System;
using UnityEngine;

namespace Frantic.Networking
{
    public class PlayerCombatNetwork : FranticNetworkObject
    {
        public event Action OnWeaponFired;

        [SerializeField]
        private Transform _weaponTip;

        [SerializeField]
        private float _fireCooldown = 0.25f;

        [SerializeField]
        private int _maxAmmo = 30;

        [SerializeField]
        private float _fireRange = 20f;

        [SerializeField]
        public GameObject _projectilePrefab;

        private float _lastFireTime;
        private int _currentAmmo;

        private void Awake()
        {
            _currentAmmo = _maxAmmo;
            if (_weaponTip == null)
            {
                _weaponTip = transform;
                _weaponTip.position += Vector3.up * 0.5f;
            }
        }

        public int CurrentAmmo => _currentAmmo;

        public void FireWeapon(Vector2 direction)
        {
            if (Time.time - _lastFireTime < _fireCooldown) return;

            if (_currentAmmo <= 0) return;

            _currentAmmo--;
            _lastFireTime = Time.time;

            if (_weaponTip != null)
            {
                var worldDirection = new Vector3(direction.x, direction.y, 0f);
                SpawnProjectile(_weaponTip.position, worldDirection);
            }

            OnWeaponFired?.Invoke();
        }

        public void Reload()
        {
            if (_currentAmmo == _maxAmmo) return;

            _currentAmmo = _maxAmmo;
        }

        public void AddAmmo(int amount)
        {
            _currentAmmo = Mathf.Min(_maxAmmo, _currentAmmo + amount);
        }

        private void SpawnProjectile(Vector3 position, Vector3 direction)
        {
            if (_projectilePrefab != null)
            {
                var spawnPos = position + direction.normalized * 0.5f;
                var projectile = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
                var projectileNetwork = projectile.GetComponent<ProjectileNetwork>();
                if (projectileNetwork != null)
                {
                    projectileNetwork.Initialize(direction, gameObject);
                }
            }
        }
    }
}

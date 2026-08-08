using System;
using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class PlayerCombatNetwork : FranticNetworkObject
    {
        public event Action OnWeaponFired;

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
        private NetworkVariable<int> _networkAmmo = new NetworkVariable<int>();

        public int CurrentAmmo => _networkAmmo.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentAmmo = _maxAmmo;
            _networkAmmo.Value = _maxAmmo;
        }

        [ServerRpc]
        public void FireWeaponServerRpc(Vector2 direction)
        {
            if (!IsServer) return;

            if (Time.time - _lastFireTime < _fireCooldown) return;

            if (_currentAmmo <= 0) return;

            _currentAmmo--;
            _networkAmmo.Value = _currentAmmo;
            _lastFireTime = Time.time;

            var player = GetComponent<PlayerNetwork>();
            if (player != null && player.weaponTip != null)
            {
                var worldDirection = new Vector3(direction.x, direction.y, 0f);
                SpawnProjectileServerRpc(player.weaponTip.position, worldDirection);
            }

            OnFireClientRpc();
            OnWeaponFired?.Invoke();
        }

        [ServerRpc]
        public void ReloadServerRpc()
        {
            if (!IsServer) return;
            if (_currentAmmo == _maxAmmo) return;

            _currentAmmo = _maxAmmo;
            _networkAmmo.Value = _currentAmmo;
            OnReloadedClientRpc();
        }

        [ServerRpc]
        public void AddAmmoServerRpc(int amount)
        {
            if (!IsServer) return;

            _currentAmmo = Mathf.Min(_maxAmmo, _currentAmmo + amount);
            _networkAmmo.Value = _currentAmmo;
            OnReloadedClientRpc();
        }

        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector3 position, Vector3 direction)
        {
            if (_projectilePrefab != null)
            {
                var projectile = Instantiate(_projectilePrefab, position, Quaternion.identity);
                var networkObject = projectile.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = projectile.AddComponent<NetworkObject>();
                }
                networkObject.Spawn(true);
                var projectileNetwork = projectile.GetComponent<ProjectileNetwork>();
                if (projectileNetwork == null)
                {
                    projectileNetwork = projectile.AddComponent<ProjectileNetwork>();
                }
                projectileNetwork.Initialize(direction, gameObject);
            }
        }

        [ClientRpc]
        private void OnFireClientRpc() { }

        [ClientRpc]
        private void OnReloadedClientRpc() { }
    }
}

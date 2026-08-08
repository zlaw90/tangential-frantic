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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _currentAmmo = _maxAmmo;
            _networkAmmo.Value = _maxAmmo;
            Debug.Log($"[Combat] Initialized with {_maxAmmo} ammo");
        }

        [ServerRpc]
        public void FireWeaponServerRpc(Vector2 direction)
        {
            if (!IsServer) return;

            if (Time.time - _lastFireTime < _fireCooldown)
            {
                Debug.LogWarning("[Combat] Fire rate exceeded");
                return;
            }

            if (_currentAmmo <= 0)
            {
                Debug.LogWarning("[Combat] Out of ammo");
                return;
            }

            _currentAmmo--;
            _networkAmmo.Value = _currentAmmo;
            _lastFireTime = Time.time;

            Debug.Log($"[Combat] Fired weapon, ammo: {_currentAmmo}/{_maxAmmo}");

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

            Debug.Log("[Combat] Reloaded");
            OnReloadedClientRpc();
        }

        [ServerRpc]
        private void SpawnProjectileServerRpc(Vector3 position, Vector3 direction)
        {
            Debug.Log($"[Combat] SpawnProjectileServerRpc called at {position} dir={direction}");

            if (_projectilePrefab != null)
            {
                var projectile = Instantiate(_projectilePrefab, position, Quaternion.identity);
                var networkObject = projectile.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = projectile.AddComponent<NetworkObject>();
                    Debug.Log("[Combat] Added NetworkObject to projectile");
                }
                networkObject.Spawn(true);
                var projectileNetwork = projectile.GetComponent<ProjectileNetwork>();
                if (projectileNetwork == null)
                {
                    projectileNetwork = projectile.AddComponent<ProjectileNetwork>();
                    Debug.Log("[Combat] Added ProjectileNetwork to projectile");
                }
                projectileNetwork.Initialize(direction, gameObject);
                Debug.Log($"[Spawn] Projectile spawned at {position}");
            }
            else
            {
                Debug.LogWarning("[Combat] No projectile prefab assigned, skipping spawn");
            }
        }

        [ClientRpc]
        private void OnFireClientRpc()
        {
            Debug.Log("[Combat] Fire effect (client)");
        }

        [ClientRpc]
        private void OnReloadedClientRpc()
        {
            Debug.Log("[Combat] Reloaded (client)");
        }
    }
}

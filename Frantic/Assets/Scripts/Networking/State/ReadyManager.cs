using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

namespace Frantic.Networking
{
    public class ReadyManager : NetworkBehaviour
    {
        private const float COUNTDOWN_DURATION = 3f;

        private readonly Dictionary<ulong, bool> _readyStates = new();
        private float _countdownTimer;
        private bool _countdownActive;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                Debug.Log("[Ready] Server initialized ready manager");
            }
        }

        public void SetReady(ulong clientId, bool ready)
        {
            if (!IsServer) return;

            _readyStates[clientId] = ready;
            var readyCount = _readyStates.Values.Count(v => v);
            var totalPlayers = FranticNetworkManager.Instance.ConnectedClientsList.Count;
            Debug.Log($"[Ready] Player {clientId} is {(ready ? "ready" : "not ready")} ({readyCount}/{totalPlayers} ready)");

            if (readyCount >= totalPlayers && totalPlayers > 0)
            {
                StartCountdown();
            }
        }

        [ServerRpc]
        public void StartCountdownServerRpc()
        {
            if (!IsServer) return;
            StartCountdown();
        }

        private void StartCountdown()
        {
            if (_countdownActive) return;

            _countdownActive = true;
            _countdownTimer = COUNTDOWN_DURATION;
            Debug.Log("[Ready] Countdown started");

            OnCountdownStartClientRpc();
        }

        private void Update()
        {
            if (!IsServer || !_countdownActive) return;

            _countdownTimer -= Time.deltaTime;

            if (_countdownTimer <= 0f)
            {
                Debug.Log("[Ready] Countdown finished, starting dungeon");
                var gameManager = GetComponent<GameManager>();
                gameManager?.StartDungeonServerRpc();
                _countdownActive = false;
            }
            else if (_countdownTimer <= 1f && _countdownTimer > Time.deltaTime)
            {
                OnCountdownTickClientRpc(1);
            }
        }

        [ClientRpc]
        private void OnCountdownStartClientRpc()
        {
            OnCountdownTickClientRpc(3);
        }

        [ClientRpc]
        private void OnCountdownTickClientRpc(int tick)
        {
            Debug.Log($"[Ready] Countdown: {tick}");
        }
    }
}

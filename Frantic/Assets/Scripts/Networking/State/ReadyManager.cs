using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Frantic.Networking
{
    public class ReadyManager : MonoBehaviour
    {
        private const float COUNTDOWN_DURATION = 3f;

        private readonly Dictionary<ulong, bool> _readyStates = new();
        private float _countdownTimer;
        private bool _countdownActive;

        public void SetReady(ulong clientId, bool ready)
        {
            if (!IsServer()) return;

            _readyStates[clientId] = ready;
            var readyCount = _readyStates.Values.Count(v => v);
            var totalPlayers = FranticNetworkManager.Instance?.ConnectedClientsList.Count ?? 0;
            Debug.Log($"[Ready] Player {clientId} is {(ready ? "ready" : "not ready")} ({readyCount}/{totalPlayers} ready)");

            if (readyCount >= totalPlayers && totalPlayers > 0)
            {
                StartCountdown();
            }
        }

        public void StartCountdown()
        {
            if (!IsServer()) return;

            if (_countdownActive) return;

            _countdownActive = true;
            _countdownTimer = COUNTDOWN_DURATION;
            Debug.Log("[Ready] Countdown started");

            Debug.Log("[Ready] Countdown: 3");
        }

        private void Update()
        {
            if (!IsServer() || !_countdownActive) return;

            _countdownTimer -= Time.deltaTime;

            if (_countdownTimer <= 0f)
            {
                Debug.Log("[Ready] Countdown finished, starting dungeon");
                var gameManager = GetComponent<GameManager>();
                gameManager?.StartDungeon();
                _countdownActive = false;
            }
            else if (_countdownTimer <= 1f && _countdownTimer > Time.deltaTime)
            {
                Debug.Log("[Ready] Countdown: 1");
            }
        }

        private bool IsServer()
        {
            return FranticNetworkManager.Instance != null && (FranticNetworkManager.Instance.IsHost || FranticNetworkManager.Instance.IsServer);
        }
    }
}

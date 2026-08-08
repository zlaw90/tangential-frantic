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
        }

        private void Update()
        {
            if (!IsServer() || !_countdownActive) return;

            _countdownTimer -= Time.deltaTime;

            if (_countdownTimer <= 0f)
            {
                GameManager.Instance?.StartDungeon();
                _countdownActive = false;
            }
        }

        private bool IsServer()
        {
            return FranticNetworkManager.Instance != null && (FranticNetworkManager.Instance.IsHost || FranticNetworkManager.Instance.IsServer);
        }

        public System.Collections.Generic.Dictionary<ulong, bool> GetReadyStates()
        {
            return _readyStates;
        }
    }
}

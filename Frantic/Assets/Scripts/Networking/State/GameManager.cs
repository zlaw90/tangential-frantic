using System;
using UnityEngine;

namespace Frantic.Networking
{
    public enum GameState
    {
        Hub,
        ReadyCountdown,
        Dungeon,
        Exit,
        Defeat
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameState _currentState = GameState.Hub;

        public event Action<GameState> OnStateChanged;

        public GameState CurrentState => _currentState;

        private void Awake()
        {
            Instance = this;
        }

        public void RequestReady(ulong callerId)
        {
            Debug.Log($"[GameManager] Player {callerId} readying up");

            if (_currentState != GameState.Hub) return;

            var readyManager = GetComponent<ReadyManager>();
            if (readyManager != null)
            {
                readyManager.SetReady(callerId, true);
            }
        }

        public void StartCountdown()
        {
            if (!IsServer()) return;

            Debug.Log("[GameManager] Starting countdown");
            _currentState = GameState.ReadyCountdown;
            OnStateChanged?.Invoke(_currentState);

            var readyManager = GetComponent<ReadyManager>();
            if (readyManager != null)
            {
                readyManager.StartCountdown();
            }
        }

        public void StartDungeon()
        {
            if (!IsServer()) return;

            Debug.Log("[GameManager] Starting dungeon generation");
            _currentState = GameState.Dungeon;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance?.LoadDungeonScene();
        }

        public void CompleteDungeon()
        {
            if (!IsServer()) return;

            Debug.Log("[GameManager] Dungeon completed");
            _currentState = GameState.Exit;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance?.LoadHubScene();
        }

        public void Defeat()
        {
            if (!IsServer()) return;

            Debug.Log("[GameManager] Team defeated");
            _currentState = GameState.Defeat;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance?.LoadHubScene();
        }

        private bool IsServer()
        {
            return FranticNetworkManager.Instance != null && (FranticNetworkManager.Instance.IsHost || FranticNetworkManager.Instance.IsServer);
        }
    }
}

using System;
using System.Linq;
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
            if (_currentState != GameState.Hub) return;

            var readyManager = FindObjectsByType<ReadyManager>(FindObjectsSortMode.None).FirstOrDefault();
            readyManager?.SetReady(callerId, true);
        }

        public void StartCountdown()
        {
            if (!IsServer()) return;

            _currentState = GameState.ReadyCountdown;
            OnStateChanged?.Invoke(_currentState);

            var readyManager = FindObjectsByType<ReadyManager>(FindObjectsSortMode.None).FirstOrDefault();
            readyManager?.StartCountdown();
        }

        public void StartDungeon()
        {
            if (!IsServer()) return;

            _currentState = GameState.Dungeon;
            OnStateChanged?.Invoke(_currentState);
            FranticNetworkManager.Instance?.LoadDungeonScene();
        }

        public void CompleteDungeon()
        {
            if (!IsServer()) return;

            _currentState = GameState.Exit;
            OnStateChanged?.Invoke(_currentState);
            FranticNetworkManager.Instance?.LoadHubScene();
        }

        public void Defeat()
        {
            if (!IsServer()) return;

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

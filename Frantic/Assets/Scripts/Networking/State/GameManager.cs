using System;
using UnityEngine;
using Unity.Netcode;

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

    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameState _currentState = GameState.Hub;

        public event Action<GameState> OnStateChanged;

        public GameState CurrentState => _currentState;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;
            Debug.Log($"[GameManager] Spawned on {(IsHost || IsServer ? "server" : "client")}");
        }

        [ServerRpc]
        public void RequestReadyServerRpc(ulong callerId, ServerRpcParams serverRpcParams = default)
        {
            if (!IsServer) return;

            Debug.Log($"[GameManager] Player {callerId} readying up");

            if (_currentState != GameState.Hub) return;

            var readyManager = GetComponent<ReadyManager>();
            if (readyManager != null)
            {
                readyManager.SetReady(callerId, true);
            }
        }

        [ServerRpc]
        public void StartCountdownServerRpc()
        {
            if (!IsServer) return;

            Debug.Log("[GameManager] Starting countdown");
            _currentState = GameState.ReadyCountdown;
            OnStateChanged?.Invoke(_currentState);

            var readyManager = GetComponent<ReadyManager>();
            if (readyManager != null)
            {
                readyManager.StartCountdownServerRpc();
            }
        }

        [ServerRpc]
        public void StartDungeonServerRpc()
        {
            if (!IsServer) return;

            Debug.Log("[GameManager] Starting dungeon generation");
            _currentState = GameState.Dungeon;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance.LoadDungeonScene();
        }

        [ServerRpc]
        public void CompleteDungeonServerRpc()
        {
            if (!IsServer) return;

            Debug.Log("[GameManager] Dungeon completed");
            _currentState = GameState.Exit;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance.LoadHubScene();
        }

        [ServerRpc]
        public void DefeatServerRpc()
        {
            if (!IsServer) return;

            Debug.Log("[GameManager] Team defeated");
            _currentState = GameState.Defeat;
            OnStateChanged?.Invoke(_currentState);

            FranticNetworkManager.Instance.LoadHubScene();
        }
    }
}

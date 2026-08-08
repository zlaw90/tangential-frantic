using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Frantic.Networking
{
    public enum GameState
    {
        Hub,
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

        public void RequestReady()
        {
            if (_currentState != GameState.Hub) return;

            StartDungeon();
        }

        public void StartDungeon()
        {
            _currentState = GameState.Dungeon;
            OnStateChanged?.Invoke(_currentState);
            Debug.Log("[GameManager] Loading Dungeon scene");
            SceneManager.LoadScene("Dungeon", LoadSceneMode.Single);
        }

        public void CompleteDungeon()
        {
            _currentState = GameState.Exit;
            OnStateChanged?.Invoke(_currentState);
            Debug.Log("[GameManager] Loading Hub scene");
            SceneManager.LoadScene("Hub", LoadSceneMode.Single);
        }

        public void Defeat()
        {
            _currentState = GameState.Defeat;
            OnStateChanged?.Invoke(_currentState);
            Debug.Log("[GameManager] Loading Hub scene (defeat)");
            SceneManager.LoadScene("Hub", LoadSceneMode.Single);
        }
    }
}

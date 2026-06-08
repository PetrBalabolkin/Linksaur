using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Linksaurus.Core
{
    public enum GameState { Menu, Playing, GameOver, Paused }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        public float InitialScrollSpeed = 5f;
        public float SpeedRampRate = 0.05f;   // units per second
        public float MaxScrollSpeed = 12f;

        [Header("State")]
        public GameState CurrentState = GameState.Menu;
        public float ScrollSpeed;
        public int CurrentScore;
        public int HighScore;
        
        private int _consecutiveConnections;
        private int _gamesPlayed;
        private GameState _previousState;

        [Header("Audio")]
        [SerializeField] private AudioClip _collectSound;
        [SerializeField] private AudioClip _hitSound;
        private AudioSource _audioSource;

        public static event Action OnScoreChanged;
        public static event Action OnGameOver;
        public static event Action OnGameStart;
        public static event Action OnGamePaused;
        public static event Action OnGameUnpaused;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("GameManager Instance set and marked DontDestroyOnLoad.");
            }
            else
            {
                Debug.LogWarning("Duplicate GameManager found. Destroying duplicate component.");
                Destroy(this);
                return;
            }

            _audioSource = GetComponent<AudioSource>();
            CurrentState = GameState.Menu;
            LoadData();
        }

        public void StartGame()
        {
            CurrentScore = 0;
            _consecutiveConnections = 0;
            ScrollSpeed = InitialScrollSpeed;
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;

            Debug.Log($"[GameManager] Game Started. Initial Speed: {ScrollSpeed:F2}");

            OnGameStart?.Invoke();
            OnScoreChanged?.Invoke();
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;

            _previousState = CurrentState;
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }

        public void UnpauseGame()
        {
            if (CurrentState != GameState.Paused) return;

            CurrentState = _previousState;
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke();
        }

        private float _speedLogTimer;

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                ScrollSpeed = Mathf.Min(ScrollSpeed + SpeedRampRate * Time.deltaTime, MaxScrollSpeed);

                _speedLogTimer += Time.deltaTime;
                if (_speedLogTimer >= 5f)
                {
                    Debug.Log($"[GameManager] Current Scroll Speed: {ScrollSpeed:F2} (Max: {MaxScrollSpeed})");
                    _speedLogTimer = 0f;
                }
            }
            else
            {
                _speedLogTimer = 0f;
            }

            // Back button on Android (Escape)
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (CurrentState == GameState.Playing)
                    PauseGame();
                else if (CurrentState == GameState.Paused)
                    UnpauseGame();
            }
        }

        public void AddConnections(int amount)
        {
            if (CurrentState != GameState.Playing) return;

            CurrentScore += amount;
            
            // Play collect sound if positive
            if (amount > 0 && _audioSource != null && _collectSound != null)
            {
                _audioSource.PlayOneShot(_collectSound);
            }

            // Combo logic
            if (amount > 0)
            {
                _consecutiveConnections++;
                if (_consecutiveConnections >= 5)
                {
                    CurrentScore += 5; // Bonus
                    _consecutiveConnections = 0;
                    Debug.Log("Combo Bonus! +5");
                }
            }
            else
            {
                _consecutiveConnections = 0;
            }

            OnScoreChanged?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (CurrentState == GameState.GameOver) return;

            CurrentState = GameState.GameOver;

            if (_audioSource != null && _hitSound != null)
            {
                _audioSource.PlayOneShot(_hitSound);
            }

            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
            }

            _gamesPlayed++;
            SaveManager.Save();
            if (Linksaurus.UI.LeaderboardManager.Instance != null)
            {
                Linksaurus.UI.LeaderboardManager.Instance.TryAddScore(CurrentScore);
            }

            OnGameOver?.Invoke();

            if (_gamesPlayed % 3 == 0)
            {
                AdManager.Instance.ShowInterstitialAd();
            }
        }

        private void LoadData()
        {
            SaveManager.Load();
            _gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
        }
    }
}

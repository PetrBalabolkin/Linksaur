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
        public float SpeedIncreaseAmount = 0.2f;
        public float MaxScrollSpeed = 12f;
        public float SpeedIncreaseInterval = 10f;

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
            }
            else
            {
                Destroy(gameObject);
            }

            _audioSource = GetComponent<AudioSource>();
            LoadData();
        }

        public void StartGame()
        {
            CurrentScore = 0;
            _consecutiveConnections = 0;
            ScrollSpeed = InitialScrollSpeed;
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;

            CancelInvoke(nameof(IncreaseSpeed));
            InvokeRepeating(nameof(IncreaseSpeed), SpeedIncreaseInterval, SpeedIncreaseInterval);

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

        private void Update()
        {
            // Back button on Android (Escape)
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (CurrentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (CurrentState == GameState.Paused)
                {
                    UnpauseGame();
                }
            }
        }

        private void IncreaseSpeed()
        {
            if (CurrentState != GameState.Playing) return;

            ScrollSpeed = Mathf.Min(ScrollSpeed + SpeedIncreaseAmount, MaxScrollSpeed);
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
            CancelInvoke(nameof(IncreaseSpeed));

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

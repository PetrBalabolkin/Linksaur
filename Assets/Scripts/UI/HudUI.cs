using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Linksaurus.Core;
using Linksaurus.Player;

namespace Linksaurus.UI
{
    public class HudUI : MonoBehaviour
    {
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _powerUpPanel;
        [SerializeField] private TextMeshProUGUI _powerUpLabel;
        [SerializeField] private Image _powerUpFill;

        [SerializeField] private Button _pauseButton;

        private PowerUpType? _currentPowerUp;
        private float _maxDuration;

        private void OnEnable()
        {
            GameManager.OnScoreChanged += UpdateScore;
            PowerUpManager.OnPowerUpChanged += HandlePowerUpChanged;
            GameManager.OnGameStart += Show;
            GameManager.OnGameOver += Hide;
        }

        private void OnDisable()
        {
            GameManager.OnScoreChanged -= UpdateScore;
            PowerUpManager.OnPowerUpChanged -= HandlePowerUpChanged;
            GameManager.OnGameStart -= Show;
            GameManager.OnGameOver -= Hide;
        }

        private void Start()
        {
            if (_pauseButton != null) _pauseButton.onClick.AddListener(TogglePause);
            
            if (_powerUpPanel != null) _powerUpPanel.SetActive(false);
            
            // Manual sync in case we missed the Start event or re-enabled
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState == GameState.Playing) Show();
                else Hide();
            }
            
            UpdateScore();
        }

        private void Show()
        {
            Debug.Log("HUD Showing");
            if (_hudPanel != null) _hudPanel.SetActive(true);
            else Debug.LogWarning("HUD Panel reference missing!");
        }

        private void Hide()
        {
            Debug.Log("HUD Hiding");
            if (_hudPanel != null) _hudPanel.SetActive(false);
        }

        private void TogglePause()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.UnpauseGame();
            }
            else
            {
                GameManager.Instance.PauseGame();
            }
        }

        private void UpdateScore()
        {
            if (_scoreText != null && GameManager.Instance != null)
            {
                _scoreText.text = $"Connections: {GameManager.Instance.CurrentScore}";
            }
        }

        private void HandlePowerUpChanged(PowerUpType? type, float duration)
        {
            if (type == null)
            {
                if (_powerUpPanel != null) _powerUpPanel.SetActive(false);
                _currentPowerUp = null;
            }
            else
            {
                if (_powerUpPanel != null) _powerUpPanel.SetActive(true);
                if (_powerUpLabel != null) _powerUpLabel.text = type.ToString().ToUpper();
                _currentPowerUp = type;
                _maxDuration = duration;
                if (_powerUpFill != null) _powerUpFill.fillAmount = 1f;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (_hudPanel != null && !_hudPanel.activeInHierarchy) Show();
            }

            if (_currentPowerUp != null && _maxDuration > 0)
            {
                float remaining = PowerUpManager.Instance.RemainingDuration;
                if (_powerUpFill != null) _powerUpFill.fillAmount = remaining / _maxDuration;
            }
        }
    }
}

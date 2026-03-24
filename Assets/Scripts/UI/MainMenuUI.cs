using UnityEngine;
using TMPro;
using Linksaurus.Core;
using UnityEngine.UI;

namespace Linksaurus.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;

        private void Start()
        {
            _playButton.onClick.AddListener(StartGame);
            UpdateHighScore();
            
            GameManager.OnGameStart += Hide;
            GameManager.OnGameOver += Show;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStart -= Hide;
            GameManager.OnGameOver -= Show;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (_panel != null && _panel.activeInHierarchy) Hide();
            }
        }

        private void StartGame()
        {
            GameManager.Instance.StartGame();
        }

        private void Show()
        {
            _panel.SetActive(true);
            UpdateHighScore();
        }

        private void Hide()
        {
            _panel.SetActive(false);
        }

        private void UpdateHighScore()
        {
            _highScoreText.text = $"Best: {GameManager.Instance.HighScore} connections";
        }
    }
}

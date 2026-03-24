using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Linksaurus.Core;
using Linksaurus.Player;
using System.Collections;

namespace Linksaurus.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private Button _shareButton;

        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_playAgainButton != null) _playAgainButton.onClick.AddListener(PlayAgain);
            if (_reviveButton != null) _reviveButton.onClick.AddListener(Revive);
            if (_shareButton != null) _shareButton.onClick.AddListener(Share);

            GameManager.OnGameOver += Show;
            GameManager.OnGameStart += Hide;
        }

        private void OnDestroy()
        {
            GameManager.OnGameOver -= Show;
            GameManager.OnGameStart -= Hide;
        }

        private void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = $"Score: {GameManager.Instance.CurrentScore}";
            if (_highScoreText != null) _highScoreText.text = $"Best: {GameManager.Instance.HighScore}";
            
            _panel.transform.localScale = Vector3.zero;
            StartCoroutine(ScalePanel(Vector3.one));
        }

        private void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void PlayAgain()
        {
            GameManager.Instance.StartGame();
        }

        private void Revive()
        {
            AdManager.Instance.ShowRewardedAd(() => 
            {
                _panel.SetActive(false);
                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    player.RevivePlayer();
                }
                Debug.Log("Player Revived!");
                _reviveButton.interactable = false;
            });
        }

        private void Share()
        {
            Debug.Log($"SHARE: I got {GameManager.Instance.CurrentScore} connections in Linksaurus!");
        }

        private IEnumerator ScalePanel(Vector3 targetScale)
        {
            float t = 0;
            Vector3 startScale = _panel.transform.localScale;
            while (t < 1)
            {
                t += Time.deltaTime * 2f;
                _panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            _panel.transform.localScale = targetScale;
        }
    }
}
